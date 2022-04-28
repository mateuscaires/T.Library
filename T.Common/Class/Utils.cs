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

                try
                {
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
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            };

            if (!config.FilePath.IsNullOrEmpty())
            {
                try
                {
                    upload(config.FilePath);
                }
                catch (Exception ex)
                {
                    SftpFileUpload(config);
                }
            }

            if (config.FilesPath.HasItems())
            {
                foreach (string item in config.FilesPath)
                {
                    try
                    {
                        upload(item);
                    }
                    catch (Exception ex)
                    {
                        config.FilePath = item;
                        SftpFileUpload(config);
                    }
                }
            }
        }

        public static void SftpFileUpload(FTPConfig config)
        {
            try
            {
                ConnectionInfo connectionInfo = null;

                if (config.RSAKeyFilePath.HasText())
                {
                    connectionInfo = GetCertificateBasedConnection(config);
                }
                else
                {
                    connectionInfo = new ConnectionInfo(config.IP, config.Port, config.User, new PasswordAuthenticationMethod(config.User, config.Password));
                }

                using (SftpClient sftp = new SftpClient(connectionInfo))
                {
                    sftp.Connect();
                    sftp.ChangeDirectory(config.Directory);
                    FileInfo fi = new FileInfo(config.FilePath);

                    using (var uplfileStream = File.OpenRead(config.FilePath))
                    {
                        if (config.DeleteIfExists)
                        {
                            try
                            {
                                Renci.SshNet.Sftp.SftpFile file = sftp.Get(fi.Name);

                                if (file.HasValue())
                                    sftp.DeleteFile(file.FullName);
                            }
                            catch
                            {

                            }
                        }

                        sftp.UploadFile(uplfileStream, fi.Name, true);
                    }

                    sftp.Disconnect();
                }
            }
            catch (Exception ex)
            {
                WinSCPSftpFileUpload(config);
            }
        }

        public static void WinSCPSftpFileUpload(FTPConfig config)
        {
            if (!File.Exists(config.FilePath))
                return;

            var fi = new FileInfo(config.FilePath);

            var sessionOptions = new WinSCP.SessionOptions
            {
                Protocol = WinSCP.Protocol.Sftp,
                HostName = config.IP,
                UserName = config.User,
                PortNumber = config.Port,
                SshHostKeyFingerprint = config.KeyFingerPrint,
                SshPrivateKeyPath = config.RSAKeyFilePath,
            };

            using (WinSCP.Session session = new WinSCP.Session())
            {
                session.Open(sessionOptions);

                try
                {
                    using (var uplfileStream = File.OpenRead(config.FilePath))
                    {
                        session.PutFile(uplfileStream, string.Concat(config.Directory, @"\", fi.Name));
                    }
                }
                finally
                {
                    session.Close();
                }
            }
        }

        public static void WinSCPSFtpFileDownLoad(FTPConfig config, string outpath)
        {
            var sessionOptions = new WinSCP.SessionOptions
            {
                Protocol = WinSCP.Protocol.Sftp,
                HostName = config.IP,
                UserName = config.User,
                PortNumber = config.Port,
                SshHostKeyFingerprint = config.KeyFingerPrint,
                SshPrivateKeyPath = config.RSAKeyFilePath,
            };

            using (WinSCP.Session session = new WinSCP.Session())
            {
                session.Open(sessionOptions);

                try
                {
                    // Download files
                    WinSCP.TransferOptions transferOptions = new WinSCP.TransferOptions();

                    transferOptions.TransferMode = WinSCP.TransferMode.Binary;
                    
                    WinSCP.RemoteDirectoryInfo directory = session.ListDirectory(string.Concat("/", config.Directory));

                    foreach (WinSCP.RemoteFileInfo item in directory.Files)
                    {
                        if (item.Name.Contains(".."))
                            continue;

                        session.GetFileToDirectory(item.FullName, outpath, false, transferOptions);

                        session.RemoveFile(item.FullName);
                    }
                }
                finally
                {
                    session.Close();
                }
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

        private static ConnectionInfo GetCertificateBasedConnection(FTPConfig config)
        {
            ConnectionInfo connection;
            
            using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(config.RSAKeyFilePath)))
            {
                var file = new PrivateKeyFile(stream);
                var authMethod = new PrivateKeyAuthenticationMethod(config.User, file);

                connection = new ConnectionInfo(config.IP, config.Port, config.User, authMethod);
            }

            /*
            using (var stream = new FileStream(config.RSAKeyFilePath, FileMode.Open, FileAccess.Read))
            {
                var file = new PrivateKeyFile(stream);
                var authMethod = new PrivateKeyAuthenticationMethod(config.User, file);

                connection = new ConnectionInfo(config.IP, config.Port, config.User, authMethod);
            }
            */

            return connection;
        }
    }
}
