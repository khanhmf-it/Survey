using System.Net.Mail;

namespace SURVEY.Model.Common
{
    public class EmailSettings
    {
        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public string SenderEmail { get; set; }
        public string SenderName { get; set; }
        public string Password { get; set; }
    }
    public class EmailForm
    {
        public string toEmail { get; set; }
        public string subject { get; set; }
        public string content { get; set; }
    }
    public class EmailFormNetMail
    {
        //public string title { get; set; }
        //public string mail_from { get; set; }
        public string code_master_mail { get; set; }
        public string mail_to { get; set; }
        public string mail_cc { get; set; }
        public string mail_bcc { get; set; }
        //public string body { get; set; }
        //public int priority { get; set; }
    }
    public class EmailFormNetMailCustom
    {
        public string title { get; set; }
        public string mail_from { get; set; }
        public string mail_to { get; set; }
        public string mail_cc { get; set; }
        public string mail_bcc { get; set; }
        public string body { get; set; }
    }

    public class EmailFormNetMailCustomSendMultiAttachFile
    {
        public string title { get; set; }
        public string mail_from { get; set; }
        public string mail_to { get; set; }
        public string mail_cc { get; set; }
        public string mail_bcc { get; set; }
        public List<string> attachmentPaths { get; set; }
        public string body { get; set; }
    }
    public static class EmailSender
    {
        public static bool sendEmailNotify(string title, string mail_from, string mail_to, string mail_cc, string mail_bcc, string body, int priority)
        {
            bool blresult = true;

            try
            {
                using MailMessage msg = new MailMessage();
                msg.From = new MailAddress(mail_from);

                if (!string.IsNullOrWhiteSpace(mail_to))
                {
                    var arrTo = mail_to.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string item_to in arrTo)
                    {
                        var t = item_to?.Trim();
                        if (!string.IsNullOrEmpty(t))
                            msg.To.Add(new MailAddress(t));
                    }
                }

                if (!string.IsNullOrWhiteSpace(mail_cc))
                {
                    var arrCc = mail_cc.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string item_cc in arrCc)
                    {
                        var c = item_cc?.Trim();
                        if (!string.IsNullOrEmpty(c))
                            msg.CC.Add(new MailAddress(c));
                    }
                }

                if (!string.IsNullOrWhiteSpace(mail_bcc))
                {
                    var arrBcc = mail_bcc.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string item_bcc in arrBcc)
                    {
                        var b = item_bcc?.Trim();
                        if (!string.IsNullOrEmpty(b))
                            msg.Bcc.Add(new MailAddress(b));
                    }
                }

                if (priority == 1)
                {
                    msg.Priority = MailPriority.High;
                }

                msg.Subject = title;
                msg.Body = body;
                msg.IsBodyHtml = true;

                using (SmtpClient emailClient = new SmtpClient("smtp.brother.co.jp", 25))
                {
                    emailClient.UseDefaultCredentials = true;
                    emailClient.Send(msg);
                }
            }
            catch (Exception)
            {
                blresult = false;
            }

            return blresult;
        }
        // send mail gửi file 
        public static async Task<GenericResponse<bool>> SendEmailNotifyCustomSendMultiAttachFileAsync(EmailFormNetMailCustomSendMultiAttachFile emailForm)
        {
            var result = new GenericResponse<bool>();
            if (emailForm == null)
            {
                result.Success = false;
                result.Message = "Không nhận được dữ liệu input!";
                return result;
            }
            try
            {
                using MailMessage msg = new MailMessage();
                msg.From = new MailAddress(emailForm.mail_from);
                var toEmails = emailForm.mail_to?.Split(new char[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
                foreach (string item_to in toEmails)
                {
                    if (!string.IsNullOrWhiteSpace(item_to))
                    {
                        msg.To.Add(new MailAddress(item_to.Trim()));
                    }
                }
                if (emailForm.mail_cc != null && emailForm.mail_cc.Length > 0)
                {
                    var ccEmails = emailForm.mail_cc.Split(new char[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string item_cc in ccEmails)
                    {
                        if (!string.IsNullOrWhiteSpace(item_cc))
                        {
                            msg.CC.Add(new MailAddress(item_cc.Trim()));
                        }
                    }
                }

                if (!string.IsNullOrEmpty(emailForm.mail_bcc))
                {
                    var bccEmails = emailForm.mail_bcc.Split(new char[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string item_bcc in bccEmails)
                    {
                        if (!string.IsNullOrWhiteSpace(item_bcc))
                        {
                            msg.Bcc.Add(new MailAddress(item_bcc.Trim()));
                        }
                    }
                }

                if (emailForm.attachmentPaths != null && emailForm.attachmentPaths.Any())
                {
                    foreach (var path in emailForm.attachmentPaths)
                    {
                        if (!string.IsNullOrWhiteSpace(path))
                        {
                            msg.Attachments.Add(new Attachment(path));
                        }
                    }
                }

                msg.Subject = emailForm.title;
                msg.Body = emailForm.body;
                msg.IsBodyHtml = true;

                using (System.Net.Mail.SmtpClient emailClient = new System.Net.Mail.SmtpClient("smtp.brother.co.jp", 25))
                {
                    emailClient.UseDefaultCredentials = true;

                    await emailClient.SendMailAsync(msg);
                }
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "Message:" + ex.Message + "\nStackTrace:" + ex.StackTrace;
            }

            return result;
        }
    }
}
