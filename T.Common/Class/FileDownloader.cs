using System;
using System.ComponentModel;
using System.IO;
using System.Net;

namespace T.Common
{
    public sealed class FileDownloader
    {
        private readonly string _url;

        private readonly string _pathToSave;

        public event CallBack<int> OnProgress;

        public event CallBack<bool> OnCompleted;

        public event CallBack<Exception> OnError;

        public FileDownloader()
        {

        }

        public FileDownloader(string url, string pathToSave)
        {
            _url = url;
            _pathToSave = pathToSave;
        }

        private void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            OnProgress?.Invoke(e.ProgressPercentage);
        }

        private void DownloadCompleted(object sender, AsyncCompletedEventArgs args)
        {
            OnCompleted?.Invoke(!args.Cancelled);
        }

        public void DownloadFile()
        {
            DownloadFile(_url, _pathToSave);
        }

        public void DownloadFile(string url, string pathToSave)
        {
            try
            {
                string directory = Path.GetDirectoryName(pathToSave);

                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                if (File.Exists(pathToSave))
                {
                    File.Delete(pathToSave);
                }

                using (WebClient client = new WebClient())
                {
                    Uri ur = new Uri(url);

                    //client.cr

                    client.DownloadProgressChanged += DownloadProgressChanged;

                    client.DownloadFileCompleted += DownloadCompleted;

                    client.DownloadFileAsync(ur, pathToSave);
                }
            }
            catch (Exception e)
            {
                OnError?.Invoke(e);
            }
        }
        
        public byte[] GetFile(string url)
        {
            byte[] buffer = new byte[0];
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                req.Method = "GET";

                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
                
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                
                WebResponse response = req.GetResponse();
                Stream stream = response.GetResponseStream();
                
                buffer = new byte[response.ContentLength];

                response.GetResponseStream().Read(buffer, 0, (int)response.ContentLength);

                return buffer;

            }
            catch (Exception e)
            {
                OnError?.Invoke(e);
            }

            return buffer;
        }
    }

    public class FileSystemBase
    {
        public string ID { get; set; }

        public string Upload_Location { get; set; }

        public string Name { get; set; }
    }
}
