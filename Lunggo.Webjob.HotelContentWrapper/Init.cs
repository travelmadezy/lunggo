﻿using Lunggo.ApCommon.Constant;
using Lunggo.ApCommon.Flight.Service;
using Lunggo.ApCommon.Hotel.Service;
using Lunggo.Framework.BlobStorage;
using Lunggo.Framework.Config;
using Lunggo.Framework.Database;
using Lunggo.Framework.HtmlTemplate;
using Lunggo.Framework.Mail;
using Lunggo.Framework.Queue;
using Lunggo.Framework.Redis;
using Lunggo.Framework.TableStorage;

namespace Lunggo.Webjob.HotelContentWrapper
{
    partial class Program
    {
        public static void Init()
        {
            InitConfigurationManager();
            InitDatabaseService();
            InitQueueService();
            InitRedisService();
            InitBlobStorageService();
            InitTableStorageService();
            InitHtmlTemplateService();
            InitMailService();
            InitHotelService();
        }

        public static void InitTableStorageService()
        {
            var connString = ConfigManager.GetInstance().GetConfigValue("azureStorage", "connectionString");
            var table = TableStorageService.GetInstance();
            table.Init(connString);
        }
        private static void InitConfigurationManager()
        {
            var configManager = ConfigManager.GetInstance();
            configManager.Init("");
        }

        private static void InitHotelService()
        {
            var hotel= HotelService.GetInstance();
            hotel.Init("");
        }

        private static void InitDatabaseService()
        {
            var connString = ConfigManager.GetInstance().GetConfigValue("db", "connectionString");
            var db = DbService.GetInstance();
            db.Init(connString);
        }

        private static void InitQueueService()
        {
            var connString = ConfigManager.GetInstance().GetConfigValue("azureStorage", "connectionString");
            var queue = QueueService.GetInstance();
            queue.Init(connString);
        }

        private static void InitMailService()
        {
            var apiKey = ConfigManager.GetInstance().GetConfigValue("mandrill", "apiKey");
            var mailService = MailService.GetInstance();
            mailService.Init(apiKey);
        }

        public static void InitHtmlTemplateService()
        {
            var htmlTemplateService = HtmlTemplateService.GetInstance();
            htmlTemplateService.Init();
        }

        public static void InitBlobStorageService()
        {
            var connString = ConfigManager.GetInstance().GetConfigValue("azureStorage", "connectionString");
            var blobStorageService = BlobStorageService.GetInstance();
            blobStorageService.Init(connString);
        }
        private static void InitRedisService()
        {
            var redisService = RedisService.GetInstance();
            redisService.Init(new RedisConnectionProperty[]
            {
                new RedisConnectionProperty
                {
                    ConnectionName = ApConstant.SearchResultCacheName,
                    ConnectionString = ConfigManager.GetInstance().GetConfigValue("redis", "searchResultCacheConnectionString")
                },
                
                new RedisConnectionProperty
                {
                    ConnectionName = ApConstant.MasterDataCacheName,
                    ConnectionString = ConfigManager.GetInstance().GetConfigValue("redis", "masterDataCacheConnectionString")
                }, 
                 
            });
        }
    }
}
