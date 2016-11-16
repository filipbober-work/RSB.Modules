﻿using System.Collections.Generic;

namespace RSB.Mail.SmtpSender.Models
{
    public class MailProperties
    {
        public string FromMail { get; set; }
        public string FromName { get; set; }
        public List<Recipient> Recipients { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}