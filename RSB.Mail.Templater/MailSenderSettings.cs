﻿namespace RSB.Mail.Templater
{
    public class MailSenderSettings
    {
        public string TemplatesPath { get; set; }
        public string Hostname { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}