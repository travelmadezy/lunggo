﻿namespace Lunggo.Framework.Mail
{
    public partial class MailService
    {
        private static readonly MailService Instance = new MailService();
        private bool _isInitialized;
        private static readonly SendGridMailClient Client = SendGridMailClient.GetInstance();
        //private static readonly MandrillMailClient Client = MandrillMailClient.GetClientInstance();

        private MailService()
        {
            
        }

        public void Init(string apiKey)
        {
            if (!_isInitialized)
            {
                Client.Init(apiKey);
                _isInitialized = true;
            }
        }

        public static MailService GetInstance()
        {
            return Instance;
        }

        public void SendEmail<T>(T objectParam, MailModel mailModel, string type)
        {
            Client.SendEmail(objectParam, mailModel, type);
        }

        //public void SendPlainEmail(MailModel mailModel, string content)
        //{
        //    Client.SendPlainEmail(mailModel, content);
        //}
    }
}
