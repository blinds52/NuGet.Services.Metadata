﻿using NuGet.Services.Metadata.Catalog;
using NuGet.Services.Metadata.Catalog.Persistence;
using NuGet.Services.Metadata.Catalog.WarehouseIntegration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CatalogTests
{
    class WarehouseCatalogTests
    {
        public static void Test0()
        {
            Storage storage = new FileStorage("http://localhost:8000/catalogmetricsstorage", @"c:\data\site\catalogmetricsstorage");

            SqlConnectionStringBuilder connStrBldr = new SqlConnectionStringBuilder();
            connStrBldr.IntegratedSecurity = true;
            connStrBldr.InitialCatalog = "TestSourceWarehouse";
            connStrBldr.DataSource = @"(LocalDB)\v11.0";

            string connectionString = connStrBldr.ToString();

            WarehouseHelper.CreateStatisticsCatalogAsync(storage, connectionString).Wait();
        }

        public static void Test1()
        {
            FileSystemEmulatorHandler handler = new FileSystemEmulatorHandler
            {
                BaseAddress = new Uri("http://localhost:8000"),
                RootFolder = @"c:\data\site",
                InnerHandler = new HttpClientHandler()
            };

            DateTime minDownloadTimeStamp = DateTime.Parse("2014-07-20");
            //DateTime minDownloadTimeStamp = DateTime.MinValue;

            StatsCountCollector collector = new StatsGreaterThanCountCollector(minDownloadTimeStamp);
            //StatsCountCollector collector = new StatsLessThanCountCollector(minDownloadTimeStamp);

            Uri index = new Uri("http://localhost:8000/stats/index.json");

            collector.Run(index, DateTime.MinValue, handler).Wait();

            Console.WriteLine("count = {0}", collector.Count);
        }

        public static void Test2()
        {
            SqlConnectionStringBuilder connStrBldr = new SqlConnectionStringBuilder();
            connStrBldr.IntegratedSecurity = true;
            connStrBldr.InitialCatalog = "TestSourceWarehouse";
            connStrBldr.DataSource = @"(LocalDB)\v11.0";

            string connectionString = connStrBldr.ToString();
            WarehouseHelper.PostToMetricsService(connectionString).Wait();
        }

        public static void Test3()
        {
            FileSystemEmulatorHandler handler = new FileSystemEmulatorHandler
            {
                BaseAddress = new Uri("http://localhost:8000"),
                RootFolder = @"c:\data\site",
                InnerHandler = new HttpClientHandler()
            };

            DateTime minDownloadTimeStamp = DateTime.Parse("2014-09-16");
            DateTime maxDownloadTimeStamp = DateTime.UtcNow;
            //DateTime minDownloadTimeStamp = DateTime.MinValue;

            StatsCountCollector collector = new StatsTimeRangeCountCollector(minDownloadTimeStamp, maxDownloadTimeStamp);

            Uri index = new Uri("http://localhost:8000/CatalogMetricsStorage/index.json");

            collector.Run(index, DateTime.MinValue, handler).Wait();

            Console.WriteLine("count = {0}", collector.Count);
            Console.WriteLine("Result Min Timestamp = {0}, Max Timestamp: {1}", collector.ResultMinTimestamp, collector.ResultMaxTimestamp);
        }
    }
}
