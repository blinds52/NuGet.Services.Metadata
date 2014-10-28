﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Canton;
using NuGet.Services.Metadata.Catalog.Persistence;
using Microsoft.WindowsAzure.Storage;

namespace NuGet.Canton.One
{
    class Program
    {
        static bool _run = true;

        /// <summary>
        /// Canton jobs that can only run as single instances.
        /// </summary>
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine(".exe <config path>");
                Environment.Exit(1);
            }

            Console.CancelKeyPress += Console_CancelKeyPress;

            CantonUtilities.Init();

            Config config = new Config(args[0]);

            CloudStorageAccount account = CloudStorageAccount.Parse(config.GetProperty("StorageConnectionString"));

            Queue<CantonJob> jobs = new Queue<CantonJob>();

            // set up the storage account
            jobs.Enqueue(new InitStorageJob(config));

            // read the gallery to find new packages
            // jobs.Enqueue(new QueueNewPackagesFromGallery(config, new AzureStorage(account, config.GetProperty("GalleryPageContainer"))));

            // tmp
            //jobs.Enqueue(new CatalogPageJob(config, new AzureStorage(account, config.GetProperty("tmp")), CantonConstants.GalleryPagesQueue));

            // commit pages to the catalog
            jobs.Enqueue(new CatalogPageCommitJob(config, new AzureStorage(account, config.GetProperty("CatalogContainer"))));

            // create registration blobs
            // jobs.Enqueue(new RegistrationJob(config, new AzureStorage(account, config.GetProperty("RegistrationContainer")), new AzureStorageFactory(account, config.GetProperty("RegistrationContainer"))));

            Stopwatch timer = new Stopwatch();

            // avoid flooding the gallery
            TimeSpan minWait = TimeSpan.FromSeconds(30);

            while (_run)
            {
                timer.Restart();
                CantonUtilities.RunJobs(jobs);

                TimeSpan waitTime = minWait.Subtract(timer.Elapsed);

                Console.WriteLine("Completed jobs in: " + timer.Elapsed);

                // min sleep
                Thread.Sleep(60 * 1000);

                if (waitTime.TotalSeconds > 0 && _run)
                {
                    Console.WriteLine("Sleeping: " + waitTime.TotalSeconds + "s");
                    Thread.Sleep(waitTime);
                }
            }
        }

        static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            if (_run)
            {
                Console.WriteLine("Ctrl+C caught in Main");
                e.Cancel = true;
                _run = false;
            }
            else
            {
                Console.WriteLine("2nd Ctrl+C caught, ignoring");
            }
        }
    }
}
