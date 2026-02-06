using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Next_V4.Util
{
    public class EnviaEmail
    {
        public static async Task<bool> Executar(string assunto, string mensagem, EmailConfigModel config )
        {
            try
            {
                MailMessage mail = new MailMessage()
                {
                    From = new MailAddress(config.UsuarioEmail, "Nova Vida TI - Next"),
                    Subject = assunto,
                    Body = mensagem,
                    IsBodyHtml = true,
                    Priority = MailPriority.High
                };

                mail.To.Add(new MailAddress(config.EmailDestino));

                if (!String.IsNullOrEmpty(config.EmailComCopia))
                    mail.CC.Add(new MailAddress(config.EmailComCopia));

                using(SmtpClient smtp = new SmtpClient(config.DominioPrimario, config.PortaPrimaria))
                {
                    smtp.Credentials = new NetworkCredential(config.UsuarioEmail, config.UsuarioSenha);
                    smtp.EnableSsl = true;
                    await smtp.SendMailAsync(mail);
                }

                return true;

            } catch(Exception ex)
            {
                return false;
            }
        }
    }

    public class EmailConfigModel
    {
        public String DominioPrimario { get; set; }
        public int PortaPrimaria { get; set; }
        public String UsuarioEmail { get; set; }
        public String UsuarioSenha { get; set; }
        public String EmailOrigem { get; set; }
        public String EmailDestino { get; set; }
        public String EmailComCopia { get; set; }
    }
}
