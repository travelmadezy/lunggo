﻿using Lunggo.Framework.HtmlTemplate;
using Lunggo.Framework.SharedModel;
using System;
using System.Collections.Generic;
using System.Linq;
using Mandrill;
using Mandrill.Models;
using Mandrill.Requests.Messages;

namespace Lunggo.Framework.Mail
{
    public partial class MailService {
        private class MandrillMailClient : MailClient
        {
            private static readonly MandrillMailClient ClientInstance = new MandrillMailClient();
            private bool _isInitialized;
            private MandrillApi _apiOfMandrill;
            private const bool ExposeRecipients = false;

            private enum RecipientType
            {
                To,
                Cc,
                Bcc
            }

            private MandrillMailClient()
            {
                
            }

            internal static MandrillMailClient GetClientInstance()
            {
                return ClientInstance;
            }

            internal override void Init(string apiKey)
            {
                if (!_isInitialized)
                {
                    _apiOfMandrill = new MandrillApi(apiKey);
                    _isInitialized = true;
                }
            }

            internal override void SendEmail<T>(T objectParam, MailModel mailModel, string type)
            {
                var emailMessage = GenerateMessage(objectParam, mailModel, type);
                var emailMessageRequest = new SendMessageRequest(emailMessage);
                Console.Write(emailMessageRequest.Message.Html);
                _apiOfMandrill.SendMessage(emailMessageRequest);
            }

            internal void SendPlainEmail(MailModel mailModel, string content)
            {
                var emailMessage = new EmailMessage
                {
                    PreserveRecipients = !ExposeRecipients,
                    Subject = mailModel.Subject,
                    FromEmail = mailModel.FromMail,
                    FromName = mailModel.FromName,
                    To = GenerateMessageAddressTo(mailModel),
                    Html = content
                };
                var emailMessageRequest = new SendMessageRequest(emailMessage);
                Console.Write(emailMessageRequest.Message.Html);
                _apiOfMandrill.SendMessage(emailMessageRequest);
            }

            private EmailMessage GenerateMessage<T>(T objectParam, MailModel mailModel, string type)
            {
                var emailMessage = new EmailMessage
                {
                    PreserveRecipients = !ExposeRecipients,
                    Subject = mailModel.Subject,
                    FromEmail = mailModel.FromMail,
                    FromName = mailModel.FromName,
                    To = GenerateMessageAddressTo(mailModel),
                    Html = HtmlTemplateService.GetInstance().GenerateTemplate(objectParam, type)
                };
                if (mailModel.ListFileInfo != null && mailModel.ListFileInfo.Count > 0)
                    emailMessage.Attachments = ConvertFileInfoToAttachmentFiles(mailModel.ListFileInfo);
                return emailMessage;
            }

            private IEnumerable<EmailAddress> GenerateMessageAddressTo(MailModel mailModel)
            {
                var addresses = new List<EmailAddress>();
                if (mailModel.RecipientList != null)
                    addresses.AddRange(GenerateRecipients(mailModel.RecipientList, RecipientType.To));
                if (mailModel.CcList != null)
                    addresses.AddRange(GenerateRecipients(mailModel.CcList, RecipientType.Cc));
                if (mailModel.BccList != null)
                    addresses.AddRange(GenerateRecipients(mailModel.BccList, RecipientType.Bcc));
                return addresses;

            }

            private static IEnumerable<EmailAddress> GenerateRecipients(IEnumerable<string> listAddress, RecipientType sendingType)
            {
                return listAddress.Select(address => new EmailAddress(address, address, sendingType.ToString().ToLower()));
            }

            private static IEnumerable<EmailAttachment> ConvertFileInfoToAttachmentFiles(List<FileInfo> files)
            {
                if (files == null || files.Count < 1)
                {
                    yield break;
                }
                foreach (var file in files)
                {
                    var base64OfAttachmentFile = Convert.ToBase64String(file.FileData, 0, file.FileData.Length);
                    var attachmentToSend = new EmailAttachment
                    {
                        Name = file.FileName,
                        Type = file.ContentType,
                        Content = base64OfAttachmentFile
                    };
                    yield return attachmentToSend;
                }

            }
        }

    }
}
