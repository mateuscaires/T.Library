﻿
using System.Collections.Generic;

namespace T.Entities
{
    public class FTPConfig
    {
        public FTPConfig()
        {
            UsePassive = true;
            Port = 21;
        }

        public int Port { get; set; }
        public string IP { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string FilePath { get; set; }
        public string Directory { get; set; }
        public bool EnableSsl { get; set; }
        public bool UsePassive { get; set; }
        public List<string> FilesPath { get; set; }
    }
}
