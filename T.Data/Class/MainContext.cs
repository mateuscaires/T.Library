using System;
using System.Data.Entity;
using System.Linq;
using T.Common;
using System.Data.Entity.Validation;
using System.Text;
using System.Reflection;
using System.Data;
using T.Entities;
using System.Data.Entity.Infrastructure;

namespace T.Infra.Data
{
    public class MainContext<TMainAdoBase> : DbContext where TMainAdoBase : AdoBase, new()
    {
        public TMainAdoBase ADO { get; set; }

        public MainContext() : base()
        {
            ADO = new TMainAdoBase();
        }

        public MainContext(string authKey) : base()
        {
            ADO = new TMainAdoBase();
            SetConnectionString();
        }

        public DataTable GetTableData(string query)
        {
            return ADO.GetTableData(query);
        }

        public int SaveChanges<TEntity>(TEntity obj) where TEntity : class
        {
            try
            {
                return SaveChanges();
            }
            catch (Exception ex)
            {
                ADO.LogEx(ex, obj);

                return 0;
            }
        }

        public void SetSqlConnection(string authKey)
        {
            ADO.SetSqlConnection(authKey);
            SetConnectionString();
        }

        public void SetSqlConnection(int authId)
        {
            ADO.SetSqlConnection(authId);
            SetConnectionString();
        }

        public void SetSqlConnection(DBConfig config)
        {
            ADO.SetSqlConnection(config);
            SetConnectionString();
        }

        private void SetConnectionString()
        {
            base.Database.Connection.ConnectionString = ADO.ConnectionString;
        }
    }

    public static class ExtensionsMethods
    {
        public static string GetXHTMLStackDAL(this Exception ex)
        {
            string message = string.Empty;
            StringBuilder sb = new StringBuilder();
            while (ex != null)
            {
                if (!(ex is TargetInvocationException))
                {
                    if (ex is DbEntityValidationException)
                    {
                        sb.Append(((DbEntityValidationException)ex).GetXHTMLStackMessageDbEntityValidation());
                        if (ex.InnerException != null)
                            sb.Append(" ").Append(Environment.NewLine);
                    }
                    else
                    {
                        sb.Append(ex.Message.Replace(Environment.NewLine, " "));
                        if (ex.InnerException != null)
                            sb.Append(" ").Append(Environment.NewLine);
                    }
                }

                ex = ex.InnerException;
            }

            return sb.ToString();
        }

        public static string GetXHTMLStackMessageDbEntityValidation(this DbEntityValidationException ex)
        {
            StringBuilder message = new StringBuilder();
            foreach (var eve in ex.EntityValidationErrors)
            {
                string valor = string.Empty;
                var objName = eve.Entry.Entity.GetType();
                var properties = objName.GetProperties();
                PropertyInfo property;

                message.Append(string.Format("Algumas propriedades do objeto \"{0}\" no estado \"{1}\" possui valores inválidos: ", objName.BaseType.Name, eve.Entry.State)).Append(" ");

                foreach (var ve in eve.ValidationErrors)
                {
                    property = properties.Where(a => a.Name == ve.PropertyName).FirstOrDefault();
                    if (property != null)
                    {
                        valor = property.GetValue(eve.Entry.Entity, null).ToString();
                    }

                    message.Append(string.Format("Propriedade: \"{0}\", Valor: \"{1}\", Erro: \"{2}\"", ve.PropertyName, valor, ve.ErrorMessage)).Append(" ");
                    valor = string.Empty;
                }
            }
            return message.ToString();
        }
    }
}
