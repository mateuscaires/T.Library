
using System.ComponentModel;

namespace T.Common
{
    public class Layout
    {
        public enum RegisterType
        {
            Header = 1,
            Detail = 2,
            Trailer = 3
        }
    }

    public enum Fluxo
    {
        [AmbientValue("E")]
        Entrada = 0,
        [AmbientValue("S")]
        Saida = 1
    }

    public enum Lado
    {
        [AmbientValue("E")]
        Esquerdo = 0,
        [AmbientValue("D")]
        Direito = 1
    }

    public enum CommandType
    {
        [AmbientValue("SELECT ")]
        Select,
        [AmbientValue("INSERT ")]
        Insert,
        [AmbientValue("UPDATE ")]
        Update,
        [AmbientValue("DELETE ")]
        Delete,
        [AmbientValue("ANY")]
        Command,
    }

    public enum AlertType
    {
        Alert,
        Info,
        Error,
        Success
    }

    public enum Bank
    {
        Bradesco = 237,
        Brasil = 1,
        Caixa_Economica = 104,
        HSBC = 399,
        Itau = 341,
        Real = 356,
        Santander = 33,
    }

    public enum FlagType
    {
        No = 0,
        Yes = 1,
        All = 3
    }

    public enum FileType
    {
        TXT = 1,
        CSV = 2,
        XLS = 3,
        XLSX = 3,
        PDF = 4
    }

    public enum AuthType
    {
        SQL = 1,
        FTP = 2
    }

    public enum Validators
    {
        NENHUM = 0,
        CPF = 1,
        CNPJ = 2,
        EMAIL = 3,
        NUMERO = 4,
        DATA = 5,
        TEXTO = 6,
        BIT = 7
    }
}