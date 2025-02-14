﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lunggo.Framework.BlobStorage;
using Lunggo.Framework.Config;
using RestSharp;

namespace Lunggo.WebJob.BookingAutomation
{
    public partial class Program
    {
        public static void Init()
        {
            InitConfigurationManager();
            InitBlobStorageService();
            InitData();
            InitSupplierName();
        }
        private static void InitConfigurationManager()
        {
            var configManager = ConfigManager.GetInstance();
            configManager.Init(@"");
        }

        public static void InitBlobStorageService()
        {
            var connString = ConfigManager.GetInstance().GetConfigValue("azureStorage", "connectionString");
            var blobStorageService = BlobStorageService.GetInstance();
            blobStorageService.Init(connString);
        }

        private static void InitData()
        {
            _apiUrl = ConfigManager.GetInstance().GetConfigValue("api", "apiUrl");
            _client = new RestClient(_apiUrl);
            isFirst = true;
            GetAuthAccess();
        }

        private static void InitSupplierName()
        {
            FlightSuppliers = new Dictionary<int, string>();
            FlightSuppliers.Add(0, "Mystifly");
            FlightSuppliers.Add(1, "AirAsia");
            FlightSuppliers.Add(2, "Citilink");
            FlightSuppliers.Add(3, "Sriwijaya");
            FlightSuppliers.Add(4, "LionAir");
            FlightSuppliers.Add(5, "Garuda");
        }
    }
}