﻿using Newtonsoft.Json.Linq;
using NuGet.Services.Metadata.Catalog.Helpers;
using NuGet.Services.Metadata.Catalog.Persistence;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VDS.RDF;
using VDS.RDF.Query;
using NuGet.Versioning;

namespace NuGet.Services.Metadata.Catalog
{
    public static class RegistrationCatalogPackageDeleter 
    {
        private static bool allVersionsDelete = false;
        private static bool largeToSmall = false;
        private static int deleteCount = 0;

        public static async Task ProcessGraphs(
            string id,
            IDictionary<string, IGraph> sortedGraphs,
            StorageFactory storageFactory,
            Uri contentBaseAddress,
            int partitionSize,
            int packageCountThreshold)
        {
            try
            {
                Storage storage = storageFactory.Create(id.ToLowerInvariant());
                
                Uri resourceUri = storage.ResolveUri("index.json");
                string json = await storage.LoadString(resourceUri);
                
                int count = Utils.CountItems(json);

                //Determine if there is any delete All versions
                CollectorHttpClient httpClient = new CollectorHttpClient();
                foreach (var graph in sortedGraphs)
                {
                    JObject jsonContent = await httpClient.GetJObjectAsync(new Uri(graph.Key));
                    string version = (jsonContent["version"] == null) ? null : jsonContent["version"].ToString();
                    if (version == null)
                    {
                        allVersionsDelete = true;
                        break;
                    }
                    else
                    {
                        //Determine if the versions are actually available
                        string fileName = version + ".json";
                        if (storage.Exists(fileName))
                        {
                            deleteCount++;
                        }
                    }
                }

                int total = 0;
                if (!allVersionsDelete)
                {
                    if (deleteCount == 0)
                    {
                        return;
                    }
                    total = count - deleteCount;
                }
                
                if (count >= packageCountThreshold && total < packageCountThreshold)
                {
                    largeToSmall = true;
                }

                if (total >= packageCountThreshold)  //Large->Large
                {
                    await SaveLargeRegistration(storage, storageFactory.BaseAddress, sortedGraphs, json, contentBaseAddress, partitionSize);
                }
                else //Large->Small or Small->Small
                {
                    await SaveSmallRegistration(storage, storageFactory.BaseAddress, sortedGraphs, contentBaseAddress, partitionSize);
                }
                
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Process id = {0}", id), e);
            }
        }
              
        public static async Task SaveSmallRegistration(Storage storage, Uri registrationBaseAddress, IDictionary<string, IGraph> items, Uri contentBaseAddress, int partitionSize)
        {
            SingleGraphPersistence graphPersistence = new SingleGraphPersistence(storage);

            await graphPersistence.Initialize();
            IList<Uri> cleanUpList = new List<Uri>();

            await SaveRegistration(storage, registrationBaseAddress, items, cleanUpList, graphPersistence, contentBaseAddress, partitionSize);

            Uri resourceUri = storage.ResolveUri("index.json");
            string json = await storage.LoadString(resourceUri);

            // now the commit has happened the graphPersistence.Graph should contain all the data
            // if this wasn't  either 'Delete All Version' or 'Delete the only version available' case
            if (!allVersionsDelete && !String.IsNullOrEmpty(json))
            {
                JObject frame = (new CatalogContext()).GetJsonLdContext("context.Registration.json", graphPersistence.TypeUri);
                StorageContent content = new StringStorageContent(Utils.CreateJson(graphPersistence.Graph, frame), "application/json", "no-store");
                await storage.Save(graphPersistence.ResourceUri, content);
            }

            // because there were multiple files some might now be irrelevant
            if (!allVersionsDelete)
            {
                foreach (Uri uri in cleanUpList)
                {
                    if (uri != storage.ResolveUri("index.json"))
                    {
                        Console.WriteLine("DELETE: {0}", uri);
                        await storage.Delete(uri);
                    }
                }
            }
        }

        public static async Task SaveLargeRegistration(Storage storage, Uri registrationBaseAddress, IDictionary<string, IGraph> items, string existingRoot, Uri contentBaseAddress, int partitionSize)
        {
            if (existingRoot != null)
            {
                JToken compacted = JToken.Parse(existingRoot);
                AddExistingItems(Utils.CreateGraph(compacted), items);
            }

            IList<Uri> cleanUpList = new List<Uri>();

            await SaveRegistration(storage, registrationBaseAddress, items, cleanUpList, null, contentBaseAddress, partitionSize);
            
            // because there were multiple files some might now be irrelevant
            if (!allVersionsDelete)
            {
                foreach (Uri uri in cleanUpList)
                {
                    if (uri != storage.ResolveUri("index.json"))
                    {
                        Console.WriteLine("DELETE: {0}", uri);
                        await storage.Delete(uri);
                    }
                }
            }
        }

        public static void AddExistingItems(IGraph graph, IDictionary<string, IGraph> items)
        {
            TripleStore store = new TripleStore();
            store.Add(graph, true);

            string inlinePackageSparql = Utils.GetResource("sparql.SelectInlinePackage.rq");

            SparqlResultSet rows = SparqlHelpers.Select(store, inlinePackageSparql);
            foreach (SparqlResult row in rows)
            {
                string packageUri = ((IUriNode)row["catalogPackage"]).Uri.AbsoluteUri;
                items[packageUri] = graph;
            }
        }

        public static async Task SaveRegistration(Storage storage, Uri registrationBaseAddress, IDictionary<string, IGraph> items, IList<Uri> cleanUpList, SingleGraphPersistence graphPersistence, Uri contentBaseAddress, int partitionSize)
        {
            using (RegistrationCatalogDeleteWriter writer = new RegistrationCatalogDeleteWriter(storage, partitionSize, cleanUpList, graphPersistence))
            {
                foreach (KeyValuePair<string, IGraph> item in items)
                {
                   writer.Add(new RegistrationDeleteCatalogItem(new Uri(item.Key), item.Value, contentBaseAddress, registrationBaseAddress));
                }
                await writer.Commit(DateTime.UtcNow, null, largeToSmall);
            }
        }
    }
}
