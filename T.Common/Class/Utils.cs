using T.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Renci.SshNet;

namespace T.Common
{
    public class Utils
    {
        private const string CT_Slach = "/";
        private const string CT_BackSlach = @"\";
        private const string CT_FtpPrefix = "ftp://";

        public static void SaveFile(string fileName, string content)
        {
            SaveFile(fileName, content, GetEncoding());
        }

        public static void SaveFile(string fileName, string[] content)
        {
            SaveFile(fileName, content, GetEncoding());
        }

        public static void SaveFile(string fileName, string content, Encoding encoding)
        {
            if (fileName.IsNullOrEmpty())
                return;

            if (File.Exists(fileName))
                File.AppendAllText(fileName, content);
            else
                File.WriteAllText(fileName, content, encoding);
        }

        public static void SaveFile(string fileName, string[] content, Encoding encoding)
        {
            if (fileName.IsNullOrEmpty())
                return;

            if (File.Exists(fileName))
                File.AppendAllLines(fileName, content, encoding);
            else
                File.WriteAllLines(fileName, content, encoding);
        }

        public static Encoding GetEncoding()
        {
            Encoding encoding = Encoding.GetEncoding(1252);
            return encoding;// Encoding.UTF8;// .GetEncoding("windows-1254");
        }

        /*
        public static void FtpFileUpload2(FTPConfig config)
        {
            SessionOptions sessionOptions = new SessionOptions
            {
                Protocol = Protocol.Sftp,
                HostName = config.IP,
                UserName = config.User,
                Password = config.Password,
                //FtpMode = FtpMode.Active,
                //FtpSecure = FtpSecure.Explicit,
                SshHostKeyFingerprint = "ssh-ed25519 255 29:92:36:82:ae:0c:3b:16:28:60:6c:d7:82:74:6d:df",
                //SessionOptions.TlsHostCertificateFingerprint o
                //SessionOptions.GiveUpSecurityAndAcceptAnyTlsHostCertificate is set,
                //but neither SessionOptions.FtpSecure nor SessionOptions.WebdavSecure is enabled nor is the protocol S3.
                //GiveUpSecurityAndAcceptAnyTlsHostCertificate = true,
                //GiveUpSecurityAndAcceptAnySshHostKey = true, WebdavSecure = false
            };

            string uri = string.Concat(config.IP.Contains(CT_FtpPrefix) ? string.Empty : CT_FtpPrefix, config.IP, config.IP.EndsWith(CT_Slach) ? string.Empty : CT_Slach);

            if (config.Directory.HasText())
            {
                uri = string.Concat(uri, string.Concat(config.Directory, config.Directory.EndsWith(CT_Slach) ? string.Empty : CT_Slach));
            }
                       
            string fileFtpPath = string.Empty;

            FtpWebRequest req = null;

            Action<string> setreq = (method) =>
            {
                req = (FtpWebRequest)WebRequest.Create(fileFtpPath);

                req.Credentials = new NetworkCredential(config.User, config.Password);
                req.Method = method;
                req.EnableSsl = config.EnableSsl;
                req.UsePassive = config.UsePassive;
            };

            Action<string> upload = (path) =>
            {
                if (!File.Exists(path))
                    return;

                FileInfo fi = new FileInfo(path);

                fileFtpPath = string.Concat(uri, fi.Name);

                setreq(WebRequestMethods.Ftp.DeleteFile);

                try
                {
                    FtpWebResponse response = (FtpWebResponse)req.GetResponse();
                    response.Close();
                }
                catch
                {

                }

                setreq(WebRequestMethods.Ftp.UploadFile);

                using (Stream fileStream = File.OpenRead(path))
                {
                    using (Stream ftpStream = req.GetRequestStream())
                    {
                        byte[] buffer = new byte[10240];
                        int read;
                        while ((read = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            ftpStream.Write(buffer, 0, read);
                        }
                    }
                }
            };

            if (config.FilesPath.HasItems())
            {
                foreach (string item in config.FilesPath)
                {
                    upload(item);
                }
            }

            if (!config.FilePath.IsNullOrEmpty())
            {
                //upload(config.FilePath);

                using (Session session = new Session())
                {
                    // Connect
                    session.Open(sessionOptions);

                    // Upload files
                    TransferOptions transferOptions = new TransferOptions();
                    transferOptions.TransferMode = TransferMode.Binary;

                    TransferOperationResult transferResult;
                    transferResult = session.PutFiles(config.FilePath, config.Directory, false, transferOptions);

                    // Throw on any error
                    transferResult.Check();

                    // Print results
                    foreach (TransferEventArgs transfer in transferResult.Transfers)
                    {
                        Console.WriteLine("Upload of {0} succeeded", transfer.FileName);
                    }
                }
            }
        }
        */

        public static void FtpFileUpload(FTPConfig config)
        {
            string uri = string.Concat(config.IP.Contains(CT_FtpPrefix) ? string.Empty : CT_FtpPrefix, config.IP, config.IP.EndsWith(CT_Slach) ? string.Empty : CT_Slach);

            if (config.Directory.HasText())
            {
                uri = string.Concat(uri, string.Concat(config.Directory, config.Directory.EndsWith(CT_Slach) ? string.Empty : CT_Slach));
            }

            string fileFtpPath = string.Empty;

            FtpWebRequest req = null;

            Action<string> setreq = (method) =>
            {
                req = (FtpWebRequest)WebRequest.Create(fileFtpPath);

                req.Credentials = new NetworkCredential(config.User, config.Password);
                req.Method = method;
                req.EnableSsl = config.EnableSsl;
                req.UsePassive = config.UsePassive;
            };

            Action<string> upload = (path) =>
            {
                if (!File.Exists(path))
                    return;

                FileInfo fi = new FileInfo(path);

                fileFtpPath = string.Concat(uri, fi.Name);

                setreq(WebRequestMethods.Ftp.DeleteFile);

                try
                {
                    FtpWebResponse response = (FtpWebResponse)req.GetResponse();
                    response.Close();
                }
                catch
                {

                }

                setreq(WebRequestMethods.Ftp.UploadFile);

                using (Stream fileStream = File.OpenRead(path))
                {
                    using (Stream ftpStream = req.GetRequestStream())
                    {
                        byte[] buffer = new byte[10240];
                        int read;
                        while ((read = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            ftpStream.Write(buffer, 0, read);
                        }
                    }
                }
            };

            if (config.FilesPath.HasItems())
            {
                foreach (string item in config.FilesPath)
                {
                    upload(item);
                }
            }

            if (!config.FilePath.IsNullOrEmpty())
            {
                try
                {
                    upload(config.FilePath);
                }
                catch
                {
                    SftpFileUpload(config);
                }
            }
        }

        public static void SftpFileUpload(FTPConfig config)
        {
            ConnectionInfo connectionInfo = new ConnectionInfo(config.IP, config.Port, config.User, new PasswordAuthenticationMethod(config.User, config.Password));
            
            using (SftpClient sftp = new SftpClient(connectionInfo))
            {
                sftp.Connect();
                sftp.ChangeDirectory(config.Directory);
                FileInfo fi = new FileInfo(config.FilePath);

                using (var uplfileStream = File.OpenRead(config.FilePath))
                {
                    sftp.UploadFile(uplfileStream, fi.Name, true);
                }

                sftp.Disconnect();
            }
        }

        public static void FtpFileDownLoad(FTPConfig config, string outpath)
        {
            if (!Directory.Exists(outpath))
            {
                try
                {
                    Directory.CreateDirectory(outpath);
                }
                catch
                {
                    return;
                }
            }

            string[] parts = null;

            string fileName;

            using (WebClient client = new WebClient())
            {
                client.Credentials = new NetworkCredential(config.User, config.Password);

                foreach (string item in config.FilesPath)
                {
                    parts = item.Split(CT_Slach[0]);
                    fileName = string.Concat(outpath, outpath.EndsWith(CT_BackSlach) ? string.Empty : CT_BackSlach, parts[parts.Length - 1]);

                    if (File.Exists(fileName))
                        continue;

                    client.DownloadFile(item.StartsWith(CT_FtpPrefix) ? item : string.Concat(CT_FtpPrefix, item), fileName);
                }
            }
        }

        public static List<string> FtpListFiles(FTPConfig config)
        {
            FtpWebRequest request;
            List<string> items = new List<string>();

            try
            {
                config.IP = config.IP.EndsWith(CT_Slach) ? config.IP : string.Concat(config.IP, CT_Slach);

                string uri = string.Concat(config.IP.StartsWith(CT_FtpPrefix) ? string.Empty : CT_FtpPrefix, config.IP);

                if (config.Directory.HasText())
                {
                    uri = string.Concat(uri, config.Directory);
                }
                request = WebRequest.Create(new Uri(uri)) as FtpWebRequest;

                request.Method = WebRequestMethods.Ftp.ListDirectory;
                request.UseBinary = true;
                request.UsePassive = true;
                request.KeepAlive = true;
                request.Credentials = new NetworkCredential(config.User, config.Password);
                request.EnableSsl = config.EnableSsl;
                request.UsePassive = config.UsePassive;

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                    {

                        string line = streamReader.ReadLine();
                        while (!string.IsNullOrEmpty(line))
                        {
                            items.Add(string.Concat(config.IP, line));
                            line = streamReader.ReadLine();
                        }
                    }
                }
            }
            catch
            {

            }

            return items;
        }
    }
}
