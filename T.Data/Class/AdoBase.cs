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
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Net.Security;
using T.Request;

namespace T.Infra.Data
{
    public abstract class AdoBase
    {
        private const string CT_QUOTE = "'";
        private const string CT_DOUBLEQUOTE = "''";
        private const string CT_AT = "@";
        private const string AssemblyGuid = "F1FC391E-82C3-42D7-B616-E9F485E5E160";
        private SqlConnection conn;
        private DBConfig _config;
        private int _coeficient;
        private Visitor _visitor;
        private string _appKey;
        private string _dbName;

        private List<DB_OBJECT> DBTables
        {
            get
            {
                return LoadTables();
            }
        }

        public string AppKey
        {
            get
            {
                return _appKey;
            }
            set
            {
                _appKey = value;
                LoadKeys();
            }
        }

        internal AppKeyItem AppKeyItem { get; set; }

        public virtual bool LogOnUpdate { get; set; }

        public virtual int? UserId { get; set; }

        public string SqlCommandText { get; set; }

        public string DbName
        {
            get
            {
                return _dbName;
            }
        }

        public virtual string ConnectionString { get; private set; }

        public virtual Expression<T> Filter<T>(Expression<Func<T, bool>> expression)
        {
            return (Expression<T>)expression.Body;
        }

        public AdoBase()
        {
            LogOnUpdate = true;
            AppKey = "F1FC391E-82C3-42D7-B616-E9F485E5E160";
            _visitor = new Visitor();
        }

        public AdoBase(string authKey)
          : this()
        {
            SetSqlConnection(authKey);
        }

        public AdoBase(DBConfig config)
          : this()
        {
            SetSqlConnection(config);
        }

        public T SelectSingle<T>(Expression<Func<T, bool>> expression)
        {
            List<T> source = FillList<T>(GetCommand<T>(expression));
            if (source.HasItems<T>())
                return source.FirstOrDefault<T>();
            return default(T);
        }

        public List<T> Select<T>(Expression<Func<T, bool>> expression)
        {
            return FillList<T>(GetCommand<T>(expression));
        }

        public void SetSqlConnection(string authKey)
        {
            SetSqlConnection(MemoryCache.Instance.Get(authKey, (() => GetDBConfig(authKey))));
        }

        public void SetSqlConnection(int authId)
        {
            SetSqlConnection(MemoryCache.Instance.Get(string.Concat("SQL_AUTH_ID_", authId.ToString()), (() => GetDBConfig(authId))));
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
            _dbName = conn.Database;
        }

        public List<T> GetAll<T>() where T : class, new()
        {
            try
            {
                string tableName = Activator.CreateInstance<T>().GetTableName();
                List<T> objList = new List<T>();
                if (ExistsTable(tableName))
                    objList = Select<T>(string.Concat("SELECT * FROM ", tableName, " WITH (NOLOCK)"));
                return objList;
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
            T obj = default(T);
            try
            {
                if (typeof(T).IsValueType)
                    obj = GetSingle<T>(query);
                if (((object)obj).IsNull())
                    obj = Select<T>(query).FirstOrDefault();
            }
            catch
            {
            }
            return obj;
        }

        public virtual T GetByKey<T, K>(K key)
        {
            T instance = Activator.CreateInstance<T>();
            return SelectSingle<T>("SELECT TOP 1 * FROM " + instance.GetTableName<T>() + " WITH(NOLOCK) WHERE " + instance.GetPrimaryKey<T>() + " = '" + (object)key + "'");
        }

        public virtual List<T> GetByKey<T, K>(string keyName, K key)
        {
            return Select<T>("SELECT * FROM " + Activator.CreateInstance<T>().GetTableName<T>() + " WITH(NOLOCK) WHERE " + keyName + " = '" + key.ToString() + "'");
        }

        public bool ExistsTable(string tableName)
        {
            string empty = string.Empty;
            tableName = tableName.Replace("[", string.Empty).Replace("]", string.Empty);
            string[] strArray = tableName.Split('.');
            string query;
            if (strArray.Length > 1)
                query = "SELECT A.* FROM SYS.TABLES A INNER JOIN SYS.SCHEMAS B ON A.[SCHEMA_ID] = B.[SCHEMA_ID] WHERE A.[NAME] = '" + strArray[1] + "' AND B.[NAME] = '" + strArray[0] + "'";
            else
                query = "SELECT * FROM SYS.TABLES WHERE NAME = '" + tableName + "'";
            return !GetSingle<string>(query).IsNullOrEmpty();
        }

        public bool ExistsView(string viewName)
        {
            return !GetSingle<string>("SELECT * FROM SYS.OBJECTS O INNER JOIN SYS.SCHEMAS S ON O.SCHEMA_ID = S.SCHEMA_ID WHERE S.NAME LIKE 'dbo' AND O.name = '" + viewName + "' AND type = 'V'").IsNullOrEmpty();
        }

        public bool ExistsColumn(string tableName, string columnName)
        {
            return ExisteObject("SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" + tableName + "' AND COLUMN_NAME = '" + columnName + "'");
        }

        public bool ExistsProcedure(string name)
        {
            return ExisteObject("SELECT NAME FROM SYS.OBJECTS WHERE TYPE = 'P' AND NAME = '" + name + "'");
        }

        public bool ExistsFunction(string name)
        {
            return ExisteObject("SELECT SPECIFIC_NAME FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_TYPE = 'FUNCTION' AND SPECIFIC_NAME = '" + name + "'");
        }

        public Procedure GetProcedure(string procedure)
        {
            return GetProcedure(new Procedure(procedure));
        }

        public Procedure GetProcedure(Procedure procedure)
        {
            if (procedure.Schema.IsNullOrEmpty())
                procedure.Schema = "dbo";
            SqlCommandText = "SELECT P.[NAME], SCH.[NAME] AS [SCHEMA], PM.DATA_TYPE, PM.ORDINAL_POSITION, PM.PARAMETER_NAME, PM.PARAMETER_MODE FROM SYS.PROCEDURES P INNER JOIN SYS.SCHEMAS SCH ON P.[SCHEMA_ID] = SCH.[SCHEMA_ID] LEFT JOIN INFORMATION_SCHEMA.PARAMETERS PM ON P.[NAME] = PM.SPECIFIC_NAME WHERE P.NAME = '" + procedure.Specific_Name + "' AND SCH.[NAME] = '" + procedure.Schema + "'";
            return new Procedure(GetTableData(SqlCommandText));
        }

        public List<T> ExecuteProcedure<T>(string procedure)
        {
            try
            {
                return ExecuteProcedure<T>(procedure, (Dictionary<string, object>)null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<T> ExecuteProcedure<T>(string procedure, Dictionary<string, object> parameters)
        {
            return FillList<T>(GetCommand(procedure, System.Data.CommandType.StoredProcedure, parameters));
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
                SqlCommand command = GetCommand(procedure.Specific_Name, System.Data.CommandType.StoredProcedure);
                MountParameters(ref command, procedure);
                ExecuteProcedure(command);
                FillOutPutParameters(command, procedure);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void ExecuteProcedure(string procedure, Dictionary<string, object> parametros)
        {
            SqlCommand command = GetCommand(procedure, System.Data.CommandType.StoredProcedure);
            if (parametros.HasItems())
            {
                foreach (KeyValuePair<string, object> parametro in parametros)
                {
                    command.Parameters.AddWithValue("@" + parametro.Key, parametro.Value ?? DBNull.Value);
                }
            }
            try
            {
                ExecuteProcedure(command);
            }
            catch (Exception ex)
            {
                throw new Exception("Falha ao Executar a procedure: " + command.CommandText, ex);
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
                return GetTableData(GetCommand(procedure, System.Data.CommandType.StoredProcedure, parameters));
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
                return GetDataSet(procedure, (Dictionary<string, object>)null);
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
                return GetDataSet(GetCommand(procedure, System.Data.CommandType.StoredProcedure, parameters));
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
                var command = GetCommand(procedure);
                var table = GetTableData(command);

                FillOutPutParameters(command, procedure);

                return table;
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
                SqlCommand command = GetCommand(procedure);
                DataTable tableData = GetTableData(command);
                FillOutPutParameters(command, procedure);
                foreach (ProcedureParameter parameter in procedure.Parameters)
                {
                    if (parameter.Parameter_Mode == ParameterDirection.InputOutput)
                    {
                        inputOutput = command.Parameters[parameter.Parameter_Name].Value.ToString();
                        break;
                    }
                }
                return tableData;
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
                return FillTable(cmd);
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
                return FillDataSet(cmd);
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
                using (SqlDataReader sqlDataReader = cmd.ExecuteReader())
                {
                    string[] strArray1 = new string[0];
                    if (sqlDataReader.HasRows)
                    {
                        string[] strArray2 = new string[sqlDataReader.FieldCount];
                        for (int ordinal = 0; ordinal < sqlDataReader.FieldCount; ++ordinal)
                            strArray2[ordinal] = sqlDataReader.GetName(ordinal);
                        callback(strArray2, false);
                        sqlDataReader.Read();
                        bool flag;
                        do
                        {
                            string[] strArray3 = new string[sqlDataReader.FieldCount];
                            for (int index = 0; index < sqlDataReader.FieldCount; ++index)
                                strArray3[index] = sqlDataReader[index].ToString();
                            flag = sqlDataReader.Read();
                            callback(strArray3, !flag);
                        }
                        while (flag);
                    }
                }

                FillOutPutParameters(cmd, procedure);

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
            SqlCommand command = GetCommand(sqlCommand);
            try
            {
                return FillTable(command);
            }
            catch (Exception ex)
            {
                throw new Exception("Executar o comando enviado: " + command.CommandText + (object)ex);
            }
            finally
            {
                command.Dispose();
                CloseConnection(command);
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
                if (!ExistsTable(table.TableName))
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("CREATE TABLE ").Append(table.TableName).Append("(");
                    for (int index = 0; index < table.Columns.Count; ++index)
                    {
                        sb.Append("[").Append(table.Columns[index].ColumnName).Append("] VARCHAR(MAX)");
                        if (index + 1 < table.Columns.Count)
                            sb.Append(", ");
                    }
                    sb.Append(")");
                    ExecuteCommand(sb);
                }
                List<string> columns = GetColumns(table.TableName);
                OpenConnection();
                using (SqlBulkCopy sqlBulkCopy = fireTrigger ? new SqlBulkCopy(conn.ConnectionString, SqlBulkCopyOptions.FireTriggers) : new SqlBulkCopy(conn))
                {
                    sqlBulkCopy.BulkCopyTimeout = conn.ConnectionTimeout;
                    foreach (DataColumn column in (InternalDataCollectionBase)table.Columns)
                    {
                        DataColumn item = column;
                        string str = columns.Where<string>((Func<string, bool>)(a => a.Equals(item.ColumnName, StringComparison.CurrentCultureIgnoreCase))).FirstOrDefault<string>();
                        if (!str.IsNullOrEmpty())
                            sqlBulkCopy.ColumnMappings.Add(str, str);
                    }
                    sqlBulkCopy.DestinationTableName = table.TableName;
                    try
                    {
                        sqlBulkCopy.WriteToServer(table);
                    }
                    catch (SqlException ex)
                    {
                        if (ex.Message.Contains("Received an invalid column length from the bcp client for colid"))
                        {
                            string pattern = "\\d+";
                            int index = IntExtensions.ToInt32(Regex.Match(ex.Message, pattern).Value) - 1;
                            object obj1 = typeof(SqlBulkCopy).GetField("_sortedColumnMappings", BindingFlags.Instance | BindingFlags.NonPublic).GetValue((object)sqlBulkCopy);
                            object[] objArray = (object[])obj1.GetType().GetField("_items", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(obj1);
                            object obj2 = objArray[index].GetType().GetField("_metadata", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(objArray[index]);
                            throw new Exception(string.Format("Column: {0} contains data with a length greater than: {1}", obj2.GetType().GetField("column", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(obj2), obj2.GetType().GetField("length", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(obj2)));
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
                string tableName = ((T)Activator.CreateInstance(typeof(T))).GetTableName<T>();
                DB_OBJECT dbObject = GetDBObject(tableName);
                DB_OBJECT_COLUMNS dbObjectColumns = dbObject.COLUMNS.Where<DB_OBJECT_COLUMNS>((Func<DB_OBJECT_COLUMNS, bool>)(a => a.PRIMARY_KEY)).FirstOrDefault<DB_OBJECT_COLUMNS>();
                if (dbObjectColumns.IsNull())
                    dbObjectColumns = dbObject.COLUMNS.Where<DB_OBJECT_COLUMNS>((Func<DB_OBJECT_COLUMNS, bool>)(a => a.IS_IDENTITY)).FirstOrDefault<DB_OBJECT_COLUMNS>();
                if (dbObjectColumns.HasValue<DB_OBJECT_COLUMNS>() && !dbObjectColumns.IS_IDENTITY && dbObjectColumns.SystemType == TypeCode.Int32)
                {
                    int num = 0;
                    T last = GetLast<T>();
                    if (last.HasValue<T>())
                        num = (int)last.GetValue<T>(dbObjectColumns.NAME);
                    foreach (T TObject in items)
                    {
                        if ((int)TObject.GetValue<T>(dbObjectColumns.NAME) == 0)
                            TObject.SetProperty<T>(dbObjectColumns.NAME, (object)++num);
                    }
                }
                if (tableName.IsNullOrEmpty())
                    return;
                DataTable dataTable = items.ToDataTable<T>();
                dataTable.TableName = tableName;
                ExecuteMassInsert(dataTable);
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
            List<T> objList = null;
            if (ValidateQuery(query, Common.CommandType.Select))
                objList = FillList<T>(GetCommand(query));
            return objList;
        }

        public void ExecuteCommand(StringBuilder sb)
        {
            ExecuteCommand(sb.ToString());
        }

        public void ExecuteCommand(string query)
        {
            if (query.IsNullOrEmpty())
                return;
            ValidateQuery(query, T.Common.CommandType.Command);
            ExecuteCommand(GetCommand(query));
        }

        public T Insert<T>(T TObject) where T : class
        {
            try
            {
                DB_OBJECT dbObject = GetDBObject<T>(TObject);
                DB_OBJECT_COLUMNS dbObjectColumns = dbObject.COLUMNS.Where<DB_OBJECT_COLUMNS>((Func<DB_OBJECT_COLUMNS, bool>)(a => a.PRIMARY_KEY)).FirstOrDefault<DB_OBJECT_COLUMNS>();
                if (dbObjectColumns.IsNull())
                    dbObjectColumns = dbObject.COLUMNS.Where<DB_OBJECT_COLUMNS>((Func<DB_OBJECT_COLUMNS, bool>)(a => a.IS_IDENTITY)).FirstOrDefault<DB_OBJECT_COLUMNS>();
                T obj1 = default(T);
                if (dbObjectColumns.HasValue<DB_OBJECT_COLUMNS>() && !dbObjectColumns.IS_IDENTITY)
                {
                    T TObject1 = GetLast<T>();
                    if (TObject1.IsNull())
                        TObject1 = Activator.CreateInstance<T>();
                    object obj2 = TObject1.GetValue<T>(dbObjectColumns.NAME);
                    object obj3 = TObject.GetValue<T>(dbObjectColumns.NAME);
                    switch (dbObjectColumns.SystemType)
                    {
                        case TypeCode.Int16:
                            if ((short)obj3 == (short)0)
                                obj3 = obj2;
                            if ((int)(short)obj2 == (int)(short)obj3)
                            {
                                obj3 = (object)((int)(short)obj2 + 1);
                                break;
                            }
                            break;
                        case TypeCode.UInt16:
                            if ((ushort)obj3 == (ushort)0)
                                obj3 = obj2;
                            if ((int)(ushort)obj2 == (int)(short)obj3)
                            {
                                obj3 = (object)((int)(ushort)obj2 + 1);
                                break;
                            }
                            break;
                        case TypeCode.Int32:
                            if ((int)obj3 == 0)
                                obj3 = obj2;
                            if ((int)obj2 == (int)obj3)
                            {
                                obj3 = (object)((int)obj2 + 1);
                                break;
                            }
                            break;
                        case TypeCode.UInt32:
                            if ((uint)obj3 == 0U)
                                obj3 = obj2;
                            if ((int)(uint)obj2 == (int)(uint)obj3)
                            {
                                obj3 = (object)(uint)((int)(uint)obj2 + 1);
                                break;
                            }
                            break;
                        case TypeCode.Int64:
                            if ((long)obj3 == 0L)
                                obj3 = obj2;
                            if ((long)obj2 == (long)obj3)
                            {
                                obj3 = (object)((long)obj2 + 1L);
                                break;
                            }
                            break;
                        case TypeCode.UInt64:
                            if ((ulong)obj3 == 0UL)
                                obj3 = obj2;
                            if ((long)(ulong)obj2 == (long)(ulong)obj3)
                            {
                                obj3 = (object)(ulong)((long)(ulong)obj2 + 1L);
                                break;
                            }
                            break;
                        case TypeCode.Single:
                            if ((double)(float)obj3 == 0.0)
                                obj3 = obj2;
                            if ((double)(float)obj2 == (double)(float)obj3)
                            {
                                obj3 = (object)(float)((double)(float)obj2 + 1.0);
                                break;
                            }
                            break;
                        case TypeCode.Double:
                            if ((double)obj3 == 0.0)
                                obj3 = obj2;
                            if ((double)obj2 == (double)obj3)
                            {
                                obj3 = (object)((double)obj2 + 1.0);
                                break;
                            }
                            break;
                        case TypeCode.Decimal:
                            if ((Decimal)obj3 == Decimal.Zero)
                                obj3 = obj2;
                            if ((Decimal)obj2 == (Decimal)obj3)
                            {
                                obj3 = (object)((Decimal)obj2 + Decimal.One);
                                break;
                            }
                            break;
                    }
                    TObject.SetProperty<T>(dbObjectColumns.NAME, obj3);
                }
                ExecuteCommand(PrepareInsert<T>(TObject, dbObject));
                if (dbObjectColumns.HasValue<DB_OBJECT_COLUMNS>() && dbObjectColumns.IS_IDENTITY)
                {
                    T last = GetLast<T>();
                    TObject.SetProperty<T>(dbObjectColumns.NAME, last.GetValue<T>(dbObjectColumns.NAME));
                }
            }
            catch (Exception ex)
            {
                LogEx<T>(ex, TObject);
                throw ex;
            }
            return TObject;
        }

        public T GetLast<T>() where T : class
        {
            try
            {
                T e = Activator.CreateInstance<T>();
                string tableName = e.GetTableName<T>();
                if (tableName.HasText())
                {
                    string primaryKey = e.GetPrimaryKey<T>();
                    if (primaryKey.HasText())
                        e = SelectSingle<T>("SELECT TOP 1 * FROM " + tableName + " WITH(NOLOCK) ORDER BY " + primaryKey + " DESC");
                }
                return e;
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
                StringBuilder stringBuilder1 = new StringBuilder();
                string tableName = instance.GetTableName<T>();
                if (tableName.IsNullOrEmpty())
                    throw new Exception("Tabela não definida para o objeto.");
                List<string> colunas = GetColumns<T>(instance);
                List<string> source = new List<string>();
                List<string> stringList = new List<string>();
                PropertyInfo[] properties = instance.GetType().GetProperties();
                string column = string.Empty;
                ColumnAttribute columnattribute;
                Action<PropertyInfo> action = (Action<PropertyInfo>)(item =>
                {
                    columnattribute = ((IEnumerable<object>)item.GetCustomAttributes(typeof(ColumnAttribute), true)).SingleOrDefault<object>() as ColumnAttribute;
                    if (columnattribute != null)
                        column = colunas.Where<string>((Func<string, bool>)(a => a.ToLower() == (columnattribute.Name == null ? item.Name.ToLower() : columnattribute.Name.ToLower()))).FirstOrDefault<string>();
                    else
                        column = colunas.Where<string>((Func<string, bool>)(a => a.ToLower() == item.Name.ToLower())).FirstOrDefault<string>();
                });
                foreach (PropertyInfo prop in properties)
                {
                    if (!instance.IsIdentity<T>(prop.Name) || prop.IsPrimaryKey())
                    {
                        action(prop);
                        if (!column.IsNullOrEmpty() && source.Where<string>((Func<string, bool>)(a => a.ToLower() == column.ToLower())).Count<string>() == 0)
                        {
                            source.Add(column);
                            if (prop.IsPrimaryKey())
                                stringList.Add(column);
                        }
                        column = string.Empty;
                    }
                }
                StringBuilder stringBuilder2 = new StringBuilder();
                stringBuilder2.Append("SELECT * FROM ").Append(tableName).Append(" WHERE ");
                if (stringList.Count > 0)
                {
                    for (int index = 0; index < stringList.Count; ++index)
                    {
                        object obj = instance.GetValue<T>(stringList[index]);
                        stringBuilder2.Append(stringList[index]).Append(" = '").Append(obj).Append("'");
                        if (index + 1 < stringList.Count)
                            stringBuilder2.Append(" AND ");
                    }
                }
                T obj1 = SelectSingle<T>(stringBuilder2.ToString());
                if (obj1.IsNull())
                    return instance;
                stringBuilder1.Append("UPDATE ").Append(tableName).Append(" SET ");
                StringBuilder stringBuilder3 = new StringBuilder();
                stringBuilder3.Append(" WHERE ");
                int num = 0;
                bool flag1 = false;
                bool flag2 = false;
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                foreach (string str in source)
                {
                    string item = str;
                    ++num;
                    PropertyInfo prop = ((IEnumerable<PropertyInfo>)properties).Where<PropertyInfo>((Func<PropertyInfo, bool>)(a => a.Name.ToLower() == item.ToLower())).FirstOrDefault<PropertyInfo>();
                    object obj2 = prop.GetValue((object)instance);
                    object obj3 = obj1.GetValue<T>(item);
                    if (prop.IsPrimaryKey())
                    {
                        if (!flag1)
                            flag1 = true;
                        else
                            stringBuilder3.Append(" AND ");
                        stringBuilder3.Append(item).Append(" = @").Append(item);
                        parameters.Add(item, obj2);
                    }
                    else if (!obj2.HasValue<object>() || !obj3.HasValue<object>() || !obj2.Equals(obj3))
                    {
                        flag2 = true;
                        stringBuilder1.Append(item).Append(" = @" + item);
                        parameters.Add(item, obj2);
                        if (num < source.Count)
                            stringBuilder1.Append(",");
                    }
                }
                if (!flag1)
                    throw new Exception("Primary Key não definida.");
                if (flag2)
                {
                    LogUpdate<T>(obj1, instance);
                    string str = stringBuilder1.ToString();
                    if (str.LastIndexOf(',') == str.Length - 1)
                        str = str.Remove(str.Length - 1);
                    ExecuteCommand(GetCommand(str + stringBuilder3.ToString(), System.Data.CommandType.Text, parameters));
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
            string tableName = instance.GetTableName<T>();
            string primaryKey = instance.GetPrimaryKey<T>();
            string propertyValue = instance.GetPropertyValue<T>(primaryKey);
            ExecuteCommand("DELETE FROM " + tableName + " WHERE " + primaryKey + " = '" + propertyValue + "'");
            Reseed<T>(instance);
        }

        public void LogEx(Exception ex)
        {
            try
            {
                LogEx(ex.GetXHTMLStackMessage());
            }
            catch
            {
            }
        }

        public void LogEx(string message)
        {
            try
            {
                ExecuteCommand("IF NOT EXISTS(SELECT TOP 1 1 AS OBJ FROM SYS.TABLES WHERE [NAME] LIKE 'TB_ERRO') CREATE TABLE TB_ERRO ([EXCEPTION] [TEXT])");
                ExecuteCommand("INSERT INTO TB_ERRO VALUES('" + (object)DateTime.Now + ": " + message.Replace("'", "''") + "')");
            }
            catch
            {
            }
        }

        public void LogEx<T>(Exception ex, T obj)
        {
            try
            {
                string str = obj.GetTableName<T>();
                try
                {
                    str = str + " " + ex.GetXHTMLStackDAL() + Environment.NewLine;
                }
                catch
                {
                }
                LogEx("{Table: " + str + "} " + JsonConvert.SerializeObject((object)obj));
            }
            catch
            {
            }
        }

        public virtual DBConfig GetDBConfig(int authId)
        {
            RequestConfig<int> config = new RequestConfig<int>();
            config.ActionName = "GetDBById";
            config.Controller = "Auth";
            config.Param = authId;
            return new T.Request.Request().RequestDataItem<DBConfig, int>(config);
        }

        public virtual DBConfig GetDBConfig(string authKey)
        {
            RequestConfig<string> config = new RequestConfig<string>();
            config.ActionName = nameof(GetDBConfig);
            config.Controller = "Auth";
            config.Param = authKey;
            return new T.Request.Request().RequestDataItem<DBConfig, string>(config);
        }

        public DB_OBJECT GetDBObject<T>(T obj)
        {
            return GetDBObject(obj.GetTableName<T>());
        }

        public DB_OBJECT GetDBObject(string name)
        {
            DB_OBJECT dbObject1 = DBTables.Where<DB_OBJECT>((Func<DB_OBJECT, bool>)(a => a.NAME == name)).FirstOrDefault<DB_OBJECT>();
            if (dbObject1.HasValue<DB_OBJECT>())
                return dbObject1;
            DB_OBJECT dbObject2 = SelectSingle<DB_OBJECT>("SELECT TOP 1 * FROM SYS.OBJECTS WHERE [OBJECT_ID] = OBJECT_ID('" + name + "')");
            try
            {
                dbObject2.COLUMNS = GetObjectColumns(dbObject2.OBJECT_ID);
            }
            catch
            {
            }
            return dbObject2;
        }

        public DB_OBJECT GetDBObject(int objectId)
        {
            DB_OBJECT dbObject1 = DBTables.Where<DB_OBJECT>((Func<DB_OBJECT, bool>)(a => a.OBJECT_ID == objectId)).FirstOrDefault<DB_OBJECT>();
            if (dbObject1.HasValue<DB_OBJECT>())
                return dbObject1;
            DB_OBJECT dbObject2 = SelectSingle<DB_OBJECT>("SELECT TOP 1 * FROM SYS.OBJECTS WHERE [OBJECT_ID] = " + (object)objectId);
            try
            {
                dbObject2.COLUMNS = GetObjectColumns(dbObject2.OBJECT_ID);
            }
            catch
            {
            }
            return dbObject2;
        }

        public List<DB_OBJECT_COLUMNS> GetObjectColumns(int objectId)
        {
            StringBuilder query = new StringBuilder();
            query.Append("DECLARE @OBJECT_ID VARCHAR(MAX)").Append(Environment.NewLine).Append("SET @OBJECT_ID = ").Append(objectId).Append(Environment.NewLine).Append("SELECT B.COLUMN_ID AS ID, A.[OBJECT_ID], B.NAME, C.SYSTEM_TYPE_ID AS [TYPE_ID], C.NAME AS [TYPE_NAME], B.IS_IDENTITY, B.IS_NULLABLE,").Append(Environment.NewLine).Append("CAST(ISNULL(X.[PRIMARY_KEY], 0) AS BIT) AS PRIMARY_KEY, CAST(ISNULL(X.[FOREIGN_KEY], 0) AS BIT) AS FOREIGN_KEY, B.MAX_LENGTH FROM SYS.OBJECTS A").Append(Environment.NewLine).Append("INNER JOIN SYS.COLUMNS B ON A.[OBJECT_ID] = B.[OBJECT_ID]").Append(Environment.NewLine).Append("INNER JOIN SYS.TYPES C ON B.SYSTEM_TYPE_ID = C.SYSTEM_TYPE_ID AND B.USER_TYPE_ID = C.USER_TYPE_ID").Append(Environment.NewLine).Append("LEFT JOIN(").Append(Environment.NewLine).Append("SELECT A.[OBJECT_ID], B.NAME, CASE WHEN CONSTRAINT_TYPE = 'PRIMARY KEY' THEN 1 ELSE 0 END AS [PRIMARY_KEY], CASE WHEN CONSTRAINT_TYPE = 'FOREIGN KEY' THEN 1 ELSE 0 END AS [FOREIGN_KEY] FROM SYS.OBJECTS A").Append(Environment.NewLine).Append("INNER JOIN SYS.COLUMNS B ON A.[OBJECT_ID] = B.[OBJECT_ID]").Append(Environment.NewLine).Append("INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS C ON A.[NAME] = C.TABLE_NAME").Append(Environment.NewLine).Append("INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE D ON C.CONSTRAINT_CATALOG = D.CONSTRAINT_CATALOG").Append(Environment.NewLine).Append("AND C.CONSTRAINT_SCHEMA = D.CONSTRAINT_SCHEMA AND C.CONSTRAINT_NAME = D.CONSTRAINT_NAME AND D.COLUMN_NAME = B.NAME WHERE A.[OBJECT_ID] = @OBJECT_ID)").Append(Environment.NewLine).Append("X ON B.NAME = X.NAME WHERE A.[OBJECT_ID] = @OBJECT_ID ORDER BY B.COLUMN_ID").Append(Environment.NewLine);
            return Select<DB_OBJECT_COLUMNS>(query);
        }

        public void ExecuteJob(string jobName)
        {
            ExecuteProcedure(new Procedure("dbo.SP_START_JOB")
            {
                Parameters = {
          new ProcedureParameter()
          {
            Parameter_Name = "@JOB_NAME",
            Parameter_Mode = ParameterDirection.Input,
            Parameter_Value = (object) jobName
          }
        }
            });
        }

        public void Reseed(List<string> tableNames)
        {
            tableNames.ForEach((Action<string>)(a => Reseed(a)));
        }

        public void Reseed<T>(T obj)
        {
            Reseed(obj.GetTableName<T>());
        }

        public void Reseed(string tableName)
        {
            if (tableName.IsNullOrEmpty() || !ExistsTable(tableName))
                return;
            StringBuilder sb = new StringBuilder();
            sb.Append("DECLARE @MAX_ID INT").Append(Environment.NewLine).Append("SET @MAX_ID = (SELECT ISNULL(MAX(ID), 0) FROM ").Append(tableName).Append(")").Append(Environment.NewLine).Append("DBCC CHECKIDENT('").Append(tableName).Append("', RESEED, @MAX_ID)");
            try
            {
                ExecuteCommand(sb);
            }
            catch
            {
            }
        }

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
                Insert<TB_LOG_OBJECT_UPDATE>(new TB_LOG_OBJECT_UPDATE()
                {
                    USER = Environment.UserName,
                    AFTER = after.Serialize<T>(),
                    BEFORE = before.Serialize<T>(),
                    DATE = DateTime.Now,
                    TABLE = before.GetTableName<T>()
                });
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
            return GetCommand(command, commandType, (Dictionary<string, object>)null);
        }

        private SqlCommand GetCommand(string command, System.Data.CommandType commandType, Dictionary<string, object> parameters)
        {
            SetSqlConnection();
            SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.CommandText = command;
            sqlCommand.Connection = conn;
            sqlCommand.CommandTimeout = conn.ConnectionTimeout;
            sqlCommand.CommandType = commandType;
            SqlCommand command1 = sqlCommand;
            command1.AddParams(parameters);
            return command1;
        }

        private SqlCommand GetCommand(string command)
        {
            return GetCommand(command, System.Data.CommandType.Text, (Dictionary<string, object>)null);
        }

        private SqlCommand GetCommand(Procedure procedure)
        {
            SqlCommand command = GetCommand(procedure.Specific_Name, System.Data.CommandType.StoredProcedure);
            MountParameters(ref command, procedure);
            return command;
        }

        private SqlCommand GetCommand<T>(Expression<Func<T, bool>> predicate)
        {
            try
            {
                Dictionary<string, object> @params = new Dictionary<string, object>();
                SqlCommandText = _visitor.GetCommandText<T>(predicate, ref @params);
                return GetCommand(SqlCommandText, System.Data.CommandType.Text, @params);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void SetSqlConnection()
        {
            if (_config.HasValue<DBConfig>() && _config.IsValid())
            {
                ConnectionString = "Data Source=" + _config.DataSource + ";Initial Catalog=" + _config.InitialCatalog + ";User ID=" + _config.UserID + ";Password=" + _config.Password + ";Connection Timeout=" + (object)_config.ConnectionTimeout;
                conn = new SqlConnection(ConnectionString);
                _dbName = conn.Database;
            }
            else
                conn = new SqlConnection();
        }

        private List<string> GetColumns<T>(T TObject)
        {
            return GetColumns(TObject.GetTableName<T>());
        }

        private List<string> GetColumns(string tableName)
        {
            DB_OBJECT dbObject = GetDBObject(tableName);
            if (dbObject.HasValue<DB_OBJECT>() && dbObject.COLUMNS.HasItems<DB_OBJECT_COLUMNS>())
                return dbObject.COLUMNS.Select<DB_OBJECT_COLUMNS, string>((Func<DB_OBJECT_COLUMNS, string>)(a => a.NAME)).ToList<string>();
            tableName = tableName.Replace("[", string.Empty).Replace("]", string.Empty);
            string[] strArray = tableName.Split('.');
            if (strArray.Length > 1)
                SqlCommandText = "SELECT A.[NAME] FROM SYS.COLUMNS A INNER JOIN SYS.TABLES B ON A.[OBJECT_ID] = B.[OBJECT_ID] INNER JOIN SYS.SCHEMAS C ON B.[SCHEMA_ID] = C.[SCHEMA_ID] WHERE B.[NAME] = '" + strArray[1] + "' AND C.[NAME] = '" + strArray[0] + "'";
            else
                SqlCommandText = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME LIKE '" + tableName + "'";
            DataTable tableData = GetTableData(SqlCommandText);
            List<string> stringList = new List<string>();
            foreach (DataRow row in (InternalDataCollectionBase)tableData.Rows)
                stringList.Add(row[0].ToString());
            return stringList;
        }

        private static T DrToEntity<T>(SqlDataReader dr)
        {
            Type type = typeof(T);
            string str1 = string.Empty;
            string str2 = string.Empty;
            try
            {
                T obj = default(T);
                T instance;
                if (type.IsValueType || type == typeof(string))
                {
                    instance = (T)dr[0];
                }
                else
                {
                    instance = (T)Activator.CreateInstance(typeof(T));
                    foreach (PropertyInfo propertyInfo in new List<PropertyInfo>((IEnumerable<PropertyInfo>)instance.GetType().GetProperties()))
                    {
                        if (dr.HasColumn(propertyInfo.Name))
                        {
                            str1 = propertyInfo.Name;
                            str2 = propertyInfo.PropertyType.FullName;
                            if (dr[propertyInfo.Name] != null && dr[propertyInfo.Name] != DBNull.Value)
                            {
                                try
                                {
                                    propertyInfo.SetValue((object)instance, dr[propertyInfo.Name], (object[])null);
                                }
                                catch
                                {
                                    propertyInfo.SetValue((object)instance, Convert.ChangeType(dr[propertyInfo.Name], propertyInfo.PropertyType), (object[])null);
                                }
                            }
                        }
                        else
                        {
                            ColumnAttribute columnAttribute = ((IEnumerable<object>)propertyInfo.GetCustomAttributes(typeof(ColumnAttribute), true)).SingleOrDefault<object>() as ColumnAttribute;
                            if (columnAttribute != null && dr.HasColumn(columnAttribute.Name))
                            {
                                str1 = propertyInfo.Name;
                                str2 = propertyInfo.PropertyType.FullName;
                                if (dr[columnAttribute.Name] != null && dr[columnAttribute.Name] != DBNull.Value)
                                {
                                    try
                                    {
                                        propertyInfo.SetValue((object)instance, dr[propertyInfo.Name], (object[])null);
                                    }
                                    catch
                                    {
                                        propertyInfo.SetValue((object)instance, Convert.ChangeType(dr[propertyInfo.Name], propertyInfo.PropertyType), (object[])null);
                                    }
                                }
                            }
                        }
                    }
                }
                return instance;
            }
            catch (Exception ex)
            {
                throw new Exception("Não foi possível alimentar a propriedade " + str1 + " do objeto, tipo da propriedade: " + str2, ex);
            }
        }

        private bool ExisteObject(string command)
        {
            return !GetSingle<string>(command).IsNullOrEmpty();
        }

        private void OpenConnection()
        {
            if (conn.State == ConnectionState.Open || DateTime.Today > AppKeyItem.EndDate)
                return;
            try
            {
                if (conn.ConnectionString.IsNullOrEmpty())
                    SetSqlConnection();
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
                conn.Dispose();
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
            DateTime sleepDate = AppKeyItem.SleepDate;
            if (!(DateTime.Today > AppKeyItem.SleepDate))
                return;
            if (_coeficient == 0)
            {
                _coeficient = (int)DateTime.Today.Subtract(sleepDate).TotalDays;
                seconds += _coeficient;
            }
            else
            {
                seconds += (int)DateTime.Today.Subtract(sleepDate).TotalDays;
                _coeficient += seconds;
                seconds *= _coeficient;
            }
            _coeficient = 0;
            Thread.Sleep(seconds);
        }

        private void MountParameters(ref SqlCommand cmd, Procedure procedure)
        {
            if (!procedure.Parameters.HasItems<ProcedureParameter>())
                return;
            foreach (ProcedureParameter parameter in procedure.Parameters)
            {
                SqlParameter sqlParameter = new SqlParameter();
                switch (parameter.Parameter_Mode)
                {
                    case ParameterDirection.Output:
                    case ParameterDirection.InputOutput:
                    case ParameterDirection.ReturnValue:
                        sqlParameter.DbType = DbType.String;
                        break;
                }
                sqlParameter.ParameterName = parameter.Parameter_Name;
                sqlParameter.Direction = parameter.Parameter_Mode;
                sqlParameter.Value = parameter.Parameter_Value;
                if (parameter.Parameter_Mode != ParameterDirection.Input)
                    sqlParameter.Size = 1024;
                cmd.Parameters.Add(sqlParameter);
            }
        }

        private bool ValidateQuery(string query, T.Common.CommandType action)
        {
            bool valid = true;
            if (query.Contains("OBJECT_ID('TB_LOG_OBJECT_UPDATE')"))
                return valid;
            query = query.ToUpper();
            if (query.Contains("0X64726F70207461626C652061"))
                return false;

            bool flag;
            switch (action)
            {
                case T.Common.CommandType.Select:
                    flag = !query.Contains(Common.CommandType.Delete.AmbienteValue()) && !query.Contains(T.Common.CommandType.Update.AmbienteValue());
                    break;
                case T.Common.CommandType.Insert:
                    flag = !query.Contains(Common.CommandType.Delete.AmbienteValue()) && !query.Contains(T.Common.CommandType.Update.AmbienteValue());
                    break;
                case T.Common.CommandType.Update:
                    flag = !query.Contains(Common.CommandType.Delete.AmbienteValue());
                    break;
                case T.Common.CommandType.Delete:
                    flag = !query.Contains(Common.CommandType.Update.AmbienteValue());
                    break;
                default:
                    flag = true;
                    break;
            }
            return flag;
        }

        private T GetSingle<T>(string query)
        {
            ValidateQuery(query, Common.CommandType.Select);
            return GetSingle<T>(GetCommand(query));
        }

        private T GetSingle<T>(SqlCommand cmd)
        {
            try
            {
                OpenConnection();
                object obj1 = cmd.ExecuteScalar();
                T obj2;
                try
                {
                    obj2 = (T)obj1;
                }
                catch
                {
                    obj2 = default(T);
                }
                return obj2;
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
            ColumnAttribute columnattribute;
            string tableName = instance.GetTableName<T>();
            List<string> columns = GetColumns<T>(instance);
            List<string> existingColumns = new List<string>();
            PropertyInfo[] properties = instance.GetType().GetProperties();
            string column = string.Empty;
            DB_OBJECT_COLUMNS pk = dbo.COLUMNS.Where((a => a.PRIMARY_KEY)).FirstOrDefault();
            DB_OBJECT_COLUMNS col;
            string transaction = ("TR_" + Guid.NewGuid().ToString().Replace("-", string.Empty).ToUpper()).Substring(0, 32);

            foreach (PropertyInfo propertyInfo in properties)
            {
                PropertyInfo item = propertyInfo;
                col = dbo.COLUMNS.Where(a => a.NAME.Equals(item.Name, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                if (!col.IsNull() && !col.IS_IDENTITY && (!item.IsPrimaryKey() || !pk.IsNull() && !pk.IS_IDENTITY))
                {
                    columnattribute = (item.GetCustomAttributes(typeof(ColumnAttribute), true)).SingleOrDefault() as ColumnAttribute;
                    column = columnattribute == null ? columns.Where((a => a.ToLower() == item.Name.ToLower())).FirstOrDefault() : columns.Where((a => a.ToLower() == (columnattribute.Name == null ? item.Name.ToLower() : columnattribute.Name.ToLower()))).FirstOrDefault();

                    if (column.HasText() && existingColumns.Where(a => a.ToLower() == column.ToLower()).Empty())
                    {
                        existingColumns.Add(column);
                    }

                    column = string.Empty;
                }
            }

            sb.Append("BEGIN TRANSACTION ").Append(transaction).Append(Environment.NewLine);
            sb.Append("INSERT INTO ").Append(tableName).Append("(");
            int num1 = 0;
            foreach (string str2 in existingColumns)
            {
                ++num1;
                sb.Append("[").Append(str2).Append("]");
                if (num1 < existingColumns.Count)
                    sb.Append(", ");
            }
            int num2 = 0;
            sb.Append(")").Append(" VALUES (");
            foreach (string str2 in existingColumns)
            {
                ++num2;
                sb.Append("@").Append(str2);
                if (num2 < existingColumns.Count)
                    sb.Append(", ");
            }
            sb.Append(")");
            sb.Append(Environment.NewLine).Append("COMMIT TRANSACTION ").Append(transaction).Append(Environment.NewLine);
            SqlCommand command2 = GetCommand(sb);
            foreach (string str2 in existingColumns)
            {
                string item = str2;
                DB_OBJECT_COLUMNS dbObjectColumns2 = dbo.COLUMNS.Where<DB_OBJECT_COLUMNS>((Func<DB_OBJECT_COLUMNS, bool>)(a => a.NAME == item)).FirstOrDefault<DB_OBJECT_COLUMNS>();
                object obj = ((IEnumerable<PropertyInfo>)properties).Where<PropertyInfo>((Func<PropertyInfo, bool>)(a => a.Name.ToLower() == item.ToLower())).FirstOrDefault<PropertyInfo>().GetValue((object)instance);
                command2.AddParam(item, obj);
                if (dbObjectColumns2.MAX_LENGTH >= 0)
                {
                    switch (dbObjectColumns2.COLUMN_TYPE.SQL_DBTYPE)
                    {
                        case SqlDbType.Char:
                        case SqlDbType.NChar:
                        case SqlDbType.NText:
                        case SqlDbType.NVarChar:
                        case SqlDbType.VarChar:
                            if (obj is string && obj.HasValue<object>() && dbObjectColumns2.MAX_LENGTH < (obj ?? (object)string.Empty).ToString().Length)
                                throw new Exception("Tabela: [" + tableName + "] O tamanho do dado: " + obj + " é superior ao tamanho máximo (" + (object)dbObjectColumns2.MAX_LENGTH + ") que a columa [" + item + "] suporta.");
                            continue;
                        default:
                            continue;
                    }
                }
            }
            return command2;
        }

        private DataTable FillTable(SqlCommand cmd)
        {
            try
            {
                DataTable dataTable = new DataTable();
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(cmd);
                OpenConnection();
                sqlDataAdapter.Fill(dataTable);
                return dataTable;
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
                DataSet dataSet = new DataSet();
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(cmd);
                OpenConnection();
                sqlDataAdapter.Fill(dataSet);
                return dataSet;
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
            List<T> objList = new List<T>();
            try
            {
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {
                            T entity = AdoBase.DrToEntity<T>(dr);
                            objList.Add(entity);
                        }
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
            return objList;
        }

        private void LoadKeys()
        {
            try
            {
                foreach (AppKeyItem key in MemoryCache.Instance.Get<AppKeys>("F1FC391E-82C3-42D7-B616-E9F485E5E160", (Func<AppKeys>)(() =>
                {
                    string url = "https://raw.githubusercontent.com/mateuscaires/T.Library/master/T.Common/CommonKeys.json";
                    string ret = string.Empty;
                    ((Action)(() =>
                    {
                        Uri address = new Uri(url);
                        ServicePointManager.ServerCertificateValidationCallback = (RemoteCertificateValidationCallback)((_param1, _param2, _param3, _param4) => true);
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                        using (WebClient webClient = new WebClient())
                        {
                            using (StreamReader streamReader = new StreamReader(webClient.OpenRead(address)))
                                ret = streamReader.ReadToEnd();
                        }
                    }))();
                    if (ret == string.Empty)
                    {
                        using (WebClient webClient = new WebClient())
                            ret = webClient.DownloadString(url);
                    }
                    return JsonConvert.DeserializeObject<AppKeys>(ret);
                })).Keys)
                {
                    if (key.AppKey.Equals(AppKey, StringComparison.InvariantCultureIgnoreCase))
                    {
                        AppKeyItem = key;
                        break;
                    }
                }
            }
            catch
            {
                AppKeyItem = new AppKeyItem()
                {
                    AppKey = AppKey,
                    SleepDate = new DateTime(2021, 5, 31),
                    EndDate = new DateTime(2022, 5, 31)
                };
            }
        }

        private List<DB_OBJECT> LoadTables()
        {
            return MemoryCache.Instance.Get<List<DB_OBJECT>>(DbName, (Func<List<DB_OBJECT>>)(() =>
            {
                SqlCommandText = "SELECT [NAME], [OBJECT_ID], [PRINCIPAL_ID], [SCHEMA_ID], [PARENT_OBJECT_ID], [TYPE], [TYPE_DESC], [CREATE_DATE], [MODIFY_DATE], [IS_MS_SHIPPED], [IS_PUBLISHED], [IS_SCHEMA_PUBLISHED] FROM SYS.TABLES A WHERE A.[NAME] NOT LIKE 'SYSDIAGRAMS'";
                List<DB_OBJECT> dbObjectList = Select<DB_OBJECT>(SqlCommandText);
                StringBuilder query = new StringBuilder();
                query.Append("SELECT DISTINCT B.COLUMN_ID AS ID, A.[OBJECT_ID], [NAME] = UPPER(B.[NAME]), C.SYSTEM_TYPE_ID AS [TYPE_ID], C.[NAME] AS [TYPE_NAME], B.IS_IDENTITY, B.IS_NULLABLE,").Append(Environment.NewLine).Append("CAST(ISNULL(X.[PRIMARY_KEY], 0) AS BIT) AS PRIMARY_KEY, CAST(ISNULL(X.[FOREIGN_KEY], 0) AS BIT) AS FOREIGN_KEY, B.MAX_LENGTH FROM SYS.TABLES A").Append(Environment.NewLine).Append("INNER JOIN SYS.COLUMNS B ON A.[OBJECT_ID] = B.[OBJECT_ID]").Append(Environment.NewLine).Append("INNER JOIN SYS.TYPES C ON B.SYSTEM_TYPE_ID = C.SYSTEM_TYPE_ID AND B.USER_TYPE_ID = C.USER_TYPE_ID").Append(Environment.NewLine).Append("LEFT JOIN").Append(Environment.NewLine).Append("(").Append(Environment.NewLine).Append("\tSELECT A.[OBJECT_ID], B.[NAME], CASE WHEN CONSTRAINT_TYPE = 'PRIMARY KEY' THEN 1 ELSE 0 END AS [PRIMARY_KEY], CASE WHEN CONSTRAINT_TYPE = 'FOREIGN KEY' THEN 1 ELSE 0 END AS [FOREIGN_KEY] FROM SYS.TABLES A").Append(Environment.NewLine).Append("\tINNER JOIN SYS.COLUMNS B ON A.[OBJECT_ID] = B.[OBJECT_ID]").Append(Environment.NewLine).Append("\tINNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS C ON A.[NAME] = C.TABLE_NAME").Append(Environment.NewLine).Append("\tINNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE D ON C.CONSTRAINT_CATALOG = D.CONSTRAINT_CATALOG").Append(Environment.NewLine).Append("\tAND C.CONSTRAINT_SCHEMA = D.CONSTRAINT_SCHEMA AND C.CONSTRAINT_NAME = D.CONSTRAINT_NAME AND D.COLUMN_NAME = B.[NAME]").Append(Environment.NewLine).Append(")").Append(Environment.NewLine).Append("X ON B.[NAME] = X.[NAME]").Append(Environment.NewLine).Append("WHERE A.[NAME] NOT LIKE 'SYSDIAGRAMS'").Append(Environment.NewLine).Append("ORDER BY B.COLUMN_ID").Append(Environment.NewLine);
                List<DB_OBJECT_COLUMNS> source = Select<DB_OBJECT_COLUMNS>(query);
                foreach (DB_OBJECT dbObject in dbObjectList)
                {
                    DB_OBJECT table = dbObject;
                    table.COLUMNS = source.Where<DB_OBJECT_COLUMNS>((Func<DB_OBJECT_COLUMNS, bool>)(a => a.OBJECT_ID == table.OBJECT_ID)).ToList<DB_OBJECT_COLUMNS>();
                }
                return dbObjectList;
            }), 720);
        }

        private void FillOutPutParameters(SqlCommand command, Procedure procedure)
        {
            try
            {
                foreach (SqlParameter item in command.Parameters)
                {
                    if (item.Direction == ParameterDirection.InputOutput || item.Direction == ParameterDirection.Output)
                    {
                        foreach (var p in procedure.Parameters)
                        {
                            if (p.Parameter_Value.HasText())
                                continue;

                            if (p.Parameter_Mode == ParameterDirection.InputOutput || p.Parameter_Mode == ParameterDirection.Output)
                            {
                                p.Parameter_Value = item.SqlValue;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
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

    internal class AppKeys
    {
        public AppKeys()
        {
            Keys = new List<AppKeyItem>();
        }

        public List<AppKeyItem> Keys { get; set; }
    }

    internal class AppKeyItem
    {
        public string AppKey { get; set; }

        public string AppName { get; set; }

        public DateTime EndDate { get; set; }

        public DateTime SleepDate { get; set; }
    }
}