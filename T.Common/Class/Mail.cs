using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace T.Common
{
    public static class Mail
    {
        /// <summary>
        /// Enviar email
        /// </summary>
        /// <param name="to">Destinatário</param>
        /// <param name="assunto">Assunto</param>
        /// <param name="mensagem">Mensagem</param>
        /// <returns>Booleano indicando o sucesso ou não do envio</returns>
        public static bool Send(string to, string assunto, string mensagem)
        {
            List<MailAddress> mailList = new List<MailAddress>();
            mailList.Add(new MailAddress(to));
            return Send(mailList, assunto, mensagem, null);
        }

        /// <summary>
        /// Enviar email
        /// </summary>
        /// <param name="to">Destinatário</param>
        /// <param name="assunto">Assunto</param>
        /// <param name="mensagem">Mensagem</param>
        /// <returns>Booleano indicando o sucesso ou não do envio</returns>
        public static bool Send(string to, string cc, string assunto, string mensagem)
        {
            List<MailAddress> mailList = new List<MailAddress>();
            mailList.Add(new MailAddress(to));
            return Send(mailList, cc, assunto, mensagem, null);
        }

        /// <summary>
        /// Enviar email
        /// </summary>
        /// <param name="to">Destinatários</param>
        /// <param name="assunto">Assunto</param>
        /// <param name="mensagem">Mensagem</param>
        /// <param name="anexos">anexos</param>
        /// <returns>Booleano indicando o sucesso ou não do envio</returns>
        public static bool Send(IEnumerable<string> to, string assunto, string mensagem)
        {
            List<MailAddress> mailList = new List<MailAddress>();

            foreach (string item in to)
            {
                mailList.Add(new MailAddress(item));
            }

            return Send(mailList, assunto, mensagem, null);
        }

        /// <summary>
        /// Enviar email
        /// </summary>
        /// <param name="to">Destinatário</param>
        /// <param name="assunto">Assunto</param>
        /// <param name="mensagem">Mensagem</param>
        /// <param name="anexos">anexos</param>
        /// <returns>Booleano indicando o sucesso ou não do envio</returns>
        public static bool Send(string to, string assunto, string mensagem, List<Attachment> anexos)
        {
            List<MailAddress> mailList = new List<MailAddress>();
            mailList.Add(new MailAddress(to));
            return Send(mailList, assunto, mensagem, anexos);
        }

        /// <summary>
        /// Enviar email
        /// </summary>
        /// <param name="to">Lista de Destinatários</param>
        /// <param name="assunto">Assunto</param>
        /// <param name="mensagem">Mensagem</param>
        /// <returns>Booleano indicando o sucesso ou não do envio</returns>
        public static bool Send(List<MailAddress> mailList, string assunto, string mensagem)
        {
            return Send(mailList, string.Empty, assunto, mensagem, null);
        }

        /// <summary>
        /// Enviar email
        /// </summary>
        /// <param name="to">Lista de Destinatários</param>
        /// <param name="assunto">Assunto</param>
        /// <param name="mensagem">Mensagem</param>
        /// <param name="anexos">anexos</param>
        /// <returns>Booleano indicando o sucesso ou não do envio</returns>
        public static bool Send(IEnumerable<string> to, string assunto, string mensagem, List<Attachment> anexos)
        {
            List<MailAddress> mailList = new List<MailAddress>();

            foreach (string item in to)
            {
                if (item.IsNullOrEmpty() || !item.IsEmail())
                    continue;

                mailList.Add(new MailAddress(item));
            }

            return Send(mailList, string.Empty, assunto, mensagem, anexos);
        }

        /// <summary>
        /// Enviar email
        /// </summary>
        /// <param name="to">Lista de Destinatários</param>
        /// <param name="assunto">Assunto</param>
        /// <param name="mensagem">Mensagem</param>
        /// <param name="anexos">anexos</param>
        /// <returns>Booleano indicando o sucesso ou não do envio</returns>
        public static bool Send(List<MailAddress> mailList, string assunto, string mensagem, List<Attachment> anexos)
        {
            return Send(mailList, string.Empty, assunto, mensagem, anexos);
        }

        /// <summary>
        /// Enviar email
        /// </summary>
        /// <param name="to">Lista de Destinatários</param>
        /// <param name="cc">Com cópia</param>
        /// <param name="assunto">Assunto</param>
        /// <param name="mensagem">Mensagem</param>
        /// <param name="anexos">anexos</param>
        /// <returns>Booleano indicando o sucesso ou não do envio</returns>
        public static bool Send(List<MailAddress> mailList, string cc, string assunto, string mensagem, List<Attachment> anexos)
        {
            List<MailAddress> mail = new List<MailAddress>();
            if (!string.IsNullOrEmpty(cc))
                mail.Add(new MailAddress(cc));
            return Send(mailList, mail, assunto, mensagem, anexos);
        }

        /// <summary>
        /// Enviar email
        /// </summary>
        /// <param name="to">Lista de Destinatários</param>
        /// <param name="cc">Com cópia para a lista de pessoas</param>
        /// <param name="assunto">Assunto</param>
        /// <param name="mensagem">Mensagem</param>
        /// <param name="anexos">anexos</param>
        /// <returns>Booleano indicando o sucesso ou não do envio</returns>
        public static bool Send(List<MailAddress> mailList, List<MailAddress> mailListcc, string assunto, string msg, List<Attachment> anexos)
        {
            try
            {
                string host = Config.GetValueKey(Constants.Mail.EmailHost);
                string port = Config.GetValueKey(Constants.Mail.EmailPort);
                string name_from = Config.GetValueKey(Constants.Mail.EmailFrom);
                string credential_user = Config.GetValueKey(Constants.Mail.EmailCredentialUser);
                string credential_pass = Config.GetValueKey(Constants.Mail.EmailCredentinalPass);

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

                from = new MailAddress(name_from, credential_user);

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

                SmtpClient sc = new SmtpClient();
                sc.Host = host;
                sc.Port = port.ToInt32();
                sc.UseDefaultCredentials = false;
                sc.Credentials = new NetworkCredential(credential_user, credential_pass);
                sc.Timeout = 60000;
                sc.DeliveryMethod = SmtpDeliveryMethod.Network;
                sc.EnableSsl = true;
                sc.Send(message);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
