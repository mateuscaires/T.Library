using System;

namespace T.Common
{
    internal static class Constants
    {
        private static DateTime? _minDataBaseDate;
        public const string DateFormatString = "{0:dd/MM/yyyy}";
        public const string DataBaseDateFormatString = "yyyy-MM-dd";
        public const string DecimalFormat = "{0:C}";        
        public const string AdmUserId = "AdmUserId";
        public static DateTime MinDataBaseDate
        {
            get
            {
                if (_minDataBaseDate.IsNull())
                    _minDataBaseDate = new DateTime(1900, 1, 1);
                return _minDataBaseDate.Value;
            }
        }

        public struct DBConn
        {
            public const string DataSource = "Data Source";
            public const string InitialCatalog = "Initial Catalog";
            public const string UserID = "User ID";
            public const string Password = "Password";
        }

        public struct Mail
        {
            public const string EmailHost = "EmailHost";
            public const string EmailPort = "EmailPort";
            public const string EmailFrom = "EmailFrom";
            public const string EmailCredentialUser = "EmailCredentialUser";
            public const string EmailCredentinalPass = "EmailCredentinalPass";
        }
    }
}
