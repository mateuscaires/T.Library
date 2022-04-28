using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace T.Mail
{
    public static class Mail
    {
        public static bool Send(string to, string assunto, string mensagem)
        {
            List<MailAddress> mailList = new List<MailAddress>();
            mailList.Add(new MailAddress(to));
            return Send(mailList, assunto, mensagem, null);
        }
        
        public static bool Send(string to, string cc, string assunto, string mensagem)
        {
            List<MailAddress> mailList = new List<MailAddress>();
            mailList.Add(new MailAddress(to));
            return Send(mailList, cc, assunto, mensagem, null);
        }
        
        public static bool Send(IEnumerable<string> to, string assunto, string mensagem)
        {
            return Send(to, assunto, mensagem, null);
        }

        public static bool Send(string to, string assunto, string mensagem, List<Attachment> anexos)
        {
            List<MailAddress> mailList = new List<MailAddress>();
            mailList.Add(new MailAddress(to));
            return Send(mailList, assunto, mensagem, anexos);
        }
        
        public static bool Send(List<MailAddress> mailList, string assunto, string mensagem)
        {
            return Send(mailList, string.Empty, assunto, mensagem, null);
        }
        
        public static bool Send(IEnumerable<string> to, string assunto, string mensagem, List<Attachment> anexos)
        {
            if (to.Empty())
                return false;

            List<MailAddress> mailList = new List<MailAddress>();

            foreach (string item in to)
            {
                if (item.IsNullOrEmpty() || !item.IsEmail())
                    continue;

                mailList.Add(new MailAddress(item));
            }

            return Send(mailList, string.Empty, assunto, mensagem, anexos);
        }
        
        public static bool Send(List<MailAddress> mailList, string assunto, string mensagem, List<Attachment> anexos)
        {
            return Send(mailList, string.Empty, assunto, mensagem, anexos);
        }
        
        public static bool Send(List<MailAddress> mailList, string cc, string assunto, string mensagem, List<Attachment> anexos)
        {
            List<MailAddress> mail = new List<MailAddress>();
            if (!string.IsNullOrEmpty(cc))
                mail.Add(new MailAddress(cc));
            return Send(mailList, mail, assunto, mensagem, anexos);
        }
        
        public static bool Send(List<MailAddress> mailList, List<MailAddress> mailListcc, string assunto, string msg, List<Attachment> anexos)
        {
            try
            {
                string host = Config.GetValueKey(Constants.Mail.Host);
                string port = Config.GetValueKey(Constants.Mail.Port);
                string mailFrom = Config.GetValueKey(Constants.Mail.From);
                string user = Config.GetValueKey(Constants.Mail.CredentialUser);
                string pass = Config.GetValueKey(Constants.Mail.CredentinalPass);
                string displayName = Config.GetValueKey(Constants.Mail.DisplayName);

                if (displayName.IsNullOrEmpty())
                    displayName = user;

                MailAddress from = null;

                MailMessage message = null;

                //Corpo de Email
                StringBuilder stbBody = new StringBuilder();

                if (!msg.Contains("<html>"))
                {
                    stbBody.Append("<html>");
                    stbBody.Append("<body>");
                    stbBody.Append(msg).Append("<br><br>");
                    stbBody.Append("</body>");
                    stbBody.Append("</html>");
                }
                else
                {
                    stbBody.Append(msg);
                }

                from = new MailAddress(mailFrom, displayName);

                message = new MailMessage();

                if (mailList != null && mailList.Count > 0)
                {
                    foreach (var item in mailList)
                    {
                        message.To.Add(item);
                    }
                }

                if (mailListcc != null && mailListcc.Count > 0)
                {
                    foreach (var item in mailListcc)
                    {
                        message.CC.Add(item);
                    }
                }
                message.From = from;
                message.IsBodyHtml = true;
                message.Subject = assunto;
                message.Body = stbBody.ToString();

                if (anexos != null)
                {
                    foreach (Attachment anexo in anexos)
                    {
                        message.Attachments.Add(anexo);
                    }
                }

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                SmtpClient sc = new SmtpClient();
                sc.Host = host;
                sc.Port = port.ToInt32();
                sc.UseDefaultCredentials = false;
                sc.Credentials = new NetworkCredential(user, pass);
                sc.Timeout = 60000;
                sc.DeliveryMethod = SmtpDeliveryMethod.Network;
                sc.EnableSsl = true;
                sc.Send(message);

                return true;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
    }
}
