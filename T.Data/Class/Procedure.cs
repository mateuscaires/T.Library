using T.Common;
using T.Interfaces;
using System.Collections.Generic;
using System.Data;

namespace T.Infra.Data
{
    public class Procedure : IValidate
    {
        public Procedure(string name)
        {
            if (name.IsNullOrEmpty())
                name = string.Empty;

            Parameters = new List<ProcedureParameter>();

            name = name.Replace("[", string.Empty);
            name = name.Replace("]", string.Empty);

            string[] parts = name.Split('.');

            if (parts.Length == 2)
            {
                Schema = parts[0];
                Specific_Name = parts[1];
            }
            else
            {
                Specific_Name = parts[0];
            }
        }

        public Procedure(string name, string schema)
        {
            Parameters = new List<ProcedureParameter>();
            Specific_Name = name;
            Schema = schema;
        }

        public Procedure(DataTable procedureInfo)
        {
            Parameters = new List<ProcedureParameter>();

            if (procedureInfo.HasRows())
            {
                Specific_Name = procedureInfo.Rows[0].GetColumn("NAME");

                foreach (DataRow row in procedureInfo.Rows)
                {
                    ProcedureParameter param = new ProcedureParameter
                    {
                        Data_Type = row.GetColumn("DATA_TYPE"),
                        Ordinal_Position = row.GetColumn("ORDINAL_POSITION").ToInt32(),
                        Parameter_Name = row.GetColumn("PARAMETER_NAME"),
                        Parameter_Mode = row.GetColumn("PARAMETER_MODE") == "IN" ? ParameterDirection.Input : ParameterDirection.InputOutput
                    };

                    if(param.IsValid())
                        Parameters.Add(param);
                }
            }
        }

        public string Specific_Name { get; set; }
        public string Schema { get; set; }

        public List<ProcedureParameter> Parameters { get; set; }

        public bool Validate()
        {
            return !Specific_Name.IsNullOrEmpty();
        }

        public object GetParameterValue(string name)
        {
            if (Parameters.Empty())
                return null;

            foreach (ProcedureParameter item in Parameters)
            {
                if (item.Parameter_Name.Equals(name, System.StringComparison.InvariantCultureIgnoreCase))
                    return item.Parameter_Value;
            }

            return null;
        }
    }

    public class ProcedureParameter
    {
        public string Parameter_Name { get; set; }
        public ParameterDirection Parameter_Mode { get; set; }
        public int Ordinal_Position { get; set; }
        public string Data_Type { get; set; }
        public object Parameter_Value { get; set; }

        public bool IsValid()
        {
            return (!Parameter_Name.IsNullOrEmpty() && Ordinal_Position > 0 && !Data_Type.IsNullOrEmpty());
        }
    }
}
