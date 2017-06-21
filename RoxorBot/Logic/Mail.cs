using System;

namespace RoxorBot.Logic
{
    public static class Mail
    {
        public static void sendMail()
        {
            try
            {
                //TODO: fix mail
                //MailMessage mail = new MailMessage(Properties.Settings.Default.twitch_login + "@gmail.com", "horato@seznam.cz");
                //SmtpClient client = new SmtpClient("smtp.gmail.com", 25);
                //client.DeliveryMethod = SmtpDeliveryMethod.Network;
                //client.EnableSsl = true;
                //client.UseDefaultCredentials = false;
                //client.Credentials = new NetworkCredential(Properties.Settings.Default.twitch_login + "@gmail.com", Properties.Settings.Default.twitch_oauth);
                //mail.Subject = "Log: RoXoRk0Bot from " + DateTime.Now.ToString("dd MMM yyyy HH:mm:ss");
                //mail.Body = "";
                //mail.Attachments.Add(new Attachment(Path.Combine(WriteToLog.ExecutingDirectory, "Logs", WriteToLog.LogfileName)));
                //client.Send(mail);
            }
            catch (Exception) { }
        }
    }
}
