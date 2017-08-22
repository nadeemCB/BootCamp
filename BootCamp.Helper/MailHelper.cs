using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace BootCamp.Helper
{
    public class MailHelper
    {
        private const int Timeout = 180000;
        private readonly string _host;
        private readonly int _port;
        private readonly string _user;
        private readonly string _sender;
        private readonly string _pass;
        private readonly bool _ssl;

        public string Recipient { get; set; }
        public string RecipientCC { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string AttachmentFile { get; set; }

        public MailHelper()
        {
            //MailServer - Represents the SMTP Server
            _host = ConfigurationManager.AppSettings["MailServer"];
            //Port- Represents the port number
            _port = int.Parse(ConfigurationManager.AppSettings["Port"]);
            //MailAuthUser and MailAuthPass - Used for Authentication for sending email
            _sender = ConfigurationManager.AppSettings["MailSender"];
            _user = ConfigurationManager.AppSettings["MailAuthUser"];
            _pass = ConfigurationManager.AppSettings["MailAuthPass"];
            _ssl = Convert.ToBoolean(ConfigurationManager.AppSettings["EnableSSL"]);
        }

        public void Send()
        {
            MailMessage message = null;
            SmtpClient smtp = null;
            Attachment att = null;
            try
            {
                message = new MailMessage(_sender, Recipient, Subject, Body) { IsBodyHtml = true };
                if (RecipientCC != null)
                {
                    message.CC.Add(RecipientCC);
                }
                smtp = new SmtpClient(_host, _port);

                if (!String.IsNullOrEmpty(AttachmentFile))
                {
                    if (File.Exists(AttachmentFile))
                    {
                        att = new Attachment(AttachmentFile);
                        message.Attachments.Add(att);
                    }
                }

                if (_user.Length > 0 && _pass.Length > 0)
                {
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential(_user, _pass);
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.EnableSsl = _ssl;
                }

                smtp.Send(message);
            }

            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if(message!=null)
                    message.Dispose();
                if(smtp !=null)
                    smtp.Dispose();
                if (att != null)
                    att.Dispose();
            }
        }
    }
}
