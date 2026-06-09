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
}
