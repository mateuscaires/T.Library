using System;
using System.Collections.Generic;
using System.Data;

namespace T.Entities
{
    public class DB_OBJECT
    {
        private List<DB_OBJECT_COLUMNS> _columns;

        public DB_OBJECT()
        {
            _columns = new List<DB_OBJECT_COLUMNS>();
        }

        public string NAME { get; set; }
        public int OBJECT_ID { get; set; }
        public int PRINCIPAL_ID { get; set; }
        public int SCHEMA_ID { get; set; }
        public int PARENT_OBJECT_ID { get; set; }
        public string TYPE { get; set; }
        public string TYPE_DESC { get; set; }
        public DateTime CREATE_DATE { get; set; }
        public DateTime MODIFY_DATE { get; set; }
        public bool IS_MS_SHIPPED { get; set; }
        public bool IS_PUBLISHED { get; set; }
        public bool IS_SCHEMA_PUBLISHED { get; set; }

        public List<DB_OBJECT_COLUMNS> COLUMNS
        {
            get
            {
                return _columns;
            }
            set
            {
                _columns = value;
                _columns.ForEach(a => a.COLUMN_TYPE.TYPE_ID = a.TYPE_ID);
            }
        }
    }

    public class DB_OBJECT_COLUMNS
    {
        private int _typeId;

        public DB_OBJECT_COLUMNS()
        {
            COLUMN_TYPE = new DB_COLUMN_TYPE();
        }
        
        public TypeCode SystemType { get { return COLUMN_TYPE.SYSTEM_TYPE; } set { } }
        public SqlDbType SqlDataType { get { return COLUMN_TYPE.SQL_DBTYPE; } set { } }
        public int ID { get; set; }
        public int MAX_LENGTH { get; set; }
        public int OBJECT_ID { get; set; }
        public int TYPE_ID { get { return _typeId; } set { _typeId = value; COLUMN_TYPE.TYPE_ID = value; } }
        public string TYPE_NAME { get; set; }
        public string NAME { get; set; }
        public bool IS_IDENTITY { get; set; }
        public bool IS_NULLABLE { get; set; }
        public bool PRIMARY_KEY { get; set; }
        public string VALUE { get; set; }

        public DB_COLUMN_TYPE COLUMN_TYPE { get; set; }

        public string FormatInput(string val)
        {
            if(Quot)
            {
                val = string.Concat("'", val, "'");
            }
            else
            {
                if (val.ToDecimal() == 0)
                    return "0";
            }

            return val.Replace(",", ".");
        }
        
        public bool Quot
        {
            get
            {
                switch (SystemType)
                {
                    case TypeCode.Boolean:
                    case TypeCode.Int16:
                    case TypeCode.UInt16:
                    case TypeCode.Int32:
                    case TypeCode.UInt32:
                    case TypeCode.Int64:
                    case TypeCode.UInt64:
                    case TypeCode.Single:
                    case TypeCode.Double:
                    case TypeCode.Decimal:                    
                        return false;
                }

                return true;
            }
        }        
    }

    public class DB_COLUMN_TYPE
    {
        private int _typeId;

        public int TYPE_ID { get { return _typeId; } set { _typeId = value; SetSqlDbType(); } }

        public SqlDbType SQL_DBTYPE { get; set; }

        public TypeCode SYSTEM_TYPE { get; set; }
        
        private void SetSqlDbType()
        {
            switch (TYPE_ID)
            {
                case 34: SQL_DBTYPE = SqlDbType.Image; break;
                case 35: SQL_DBTYPE = SqlDbType.Text; break;
                case 36: SQL_DBTYPE = SqlDbType.UniqueIdentifier; break;
                case 40: SQL_DBTYPE = SqlDbType.Date; break;
                case 41: SQL_DBTYPE = SqlDbType.Time; break;
                case 42: SQL_DBTYPE = SqlDbType.DateTime2; break;
                case 43: SQL_DBTYPE = SqlDbType.DateTimeOffset; break;
                case 48: SQL_DBTYPE = SqlDbType.TinyInt; break;
                case 52: SQL_DBTYPE = SqlDbType.SmallInt; break;
                case 56: SQL_DBTYPE = SqlDbType.Int; break;
                case 58: SQL_DBTYPE = SqlDbType.SmallDateTime; break;
                case 59: SQL_DBTYPE = SqlDbType.Real; break;
                case 60: SQL_DBTYPE = SqlDbType.Money; break;
                case 61: SQL_DBTYPE = SqlDbType.DateTime; break;
                case 62: SQL_DBTYPE =  SqlDbType.Float; break;
                //case 108: SQL_DBTYPE = SqlDbType.Decimal; break;//.numeric; break;
                //case "sql_variant": DataType =  SqlDbType.sql.sql_variant; break;
                case 99: SQL_DBTYPE = SqlDbType.NText; break;
                case 104: SQL_DBTYPE = SqlDbType.Bit; break;
                case 106: SQL_DBTYPE = SqlDbType.Decimal; break;
                case 108: SQL_DBTYPE = SqlDbType.Decimal; break;
                case 122: SQL_DBTYPE = SqlDbType.SmallMoney; break;
                case 127: SQL_DBTYPE = SqlDbType.BigInt; break;
                case 165: SQL_DBTYPE = SqlDbType.VarBinary; break;
                case 167: SQL_DBTYPE = SqlDbType.VarChar; break;
                case 173: SQL_DBTYPE = SqlDbType.Binary; break;
                case 175: SQL_DBTYPE = SqlDbType.Char; break;
                case 189: SQL_DBTYPE = SqlDbType.Timestamp; break;
                case 231: SQL_DBTYPE = SqlDbType.NVarChar; break;
                //case "sysname": DataType =  SqlDbType.sysname; break;
                case 239: SQL_DBTYPE = SqlDbType.NChar; break;
                //case "hierarchyid": DataType =  SqlDbType.hierarchyid; break;
                //case "geometry": DataType =  SqlDbType.geometry; break;
                //case "geography": DataType =  SqlDbType.geography; break;
                case 241: SQL_DBTYPE = SqlDbType.Xml; break;
                default: SQL_DBTYPE = 0; break;
            }

            SetSystemType();
        }

        private void SetSystemType()
        {
            switch (SQL_DBTYPE)
            {
                case SqlDbType.BigInt: SYSTEM_TYPE = TypeCode.Int64; break;
                case SqlDbType.Binary: SYSTEM_TYPE = TypeCode.Byte; break;
                case SqlDbType.Bit: SYSTEM_TYPE = TypeCode.Boolean; break;

                case SqlDbType.NChar:
                case SqlDbType.Char: SYSTEM_TYPE = TypeCode.Char; break;

                case SqlDbType.Date:
                case SqlDbType.DateTime2:
                case SqlDbType.SmallDateTime:
                case SqlDbType.DateTime: SYSTEM_TYPE = TypeCode.DateTime; break;

                case SqlDbType.Decimal:
                case SqlDbType.Real:
                case SqlDbType.SmallMoney:
                case SqlDbType.Money: SYSTEM_TYPE = TypeCode.Decimal; break;

                case SqlDbType.Float: SYSTEM_TYPE = TypeCode.Double; break;

                case SqlDbType.Int: SYSTEM_TYPE = TypeCode.Int32; break;

                case SqlDbType.NText:
                case SqlDbType.Text:
                case SqlDbType.VarChar:
                case SqlDbType.UniqueIdentifier:
                case SqlDbType.NVarChar: SYSTEM_TYPE = TypeCode.String; break;

                case SqlDbType.TinyInt:
                case SqlDbType.SmallInt: SYSTEM_TYPE = TypeCode.Int16; break;
                case SqlDbType.VarBinary: SYSTEM_TYPE = TypeCode.Byte; break;

                case SqlDbType.Image:
                case SqlDbType.Timestamp:
                case SqlDbType.Variant:
                case SqlDbType.Xml:
                case SqlDbType.Udt:
                case SqlDbType.Structured:
                case SqlDbType.Time:
                case SqlDbType.DateTimeOffset: SYSTEM_TYPE = 0; break;
            }
        }
    }

    public class TB_LOG_OBJECT_UPDATE
    {
        public int ID { get; set; }
        public string USER { get; set; }
        public string TABLE { get; set; }
        public string BEFORE { get; set; }
        public string AFTER { get; set; }
        public DateTime DATE { get; set; }
    }
}
