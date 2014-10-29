﻿using Newtonsoft.Json.Linq;
using NuGet.Services.Metadata.Catalog;
using NuGet.Services.Metadata.Catalog.Collecting;
using NuGet.Services.Metadata.Catalog.Persistence;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDS.RDF;

namespace NuGet.Canton
{
    public class BatchRegistrationCollector : RegistrationCatalogCollector
    {
        public BatchRegistrationCollector(StorageFactory factory)
            : base(factory, 200)
        {

        }

        public async Task ProcessGraphs(CollectorHttpClient client, string packageId, IEnumerable<Uri> catalogPageUris, JObject context)
        {
            ConcurrentDictionary<string, IGraph> graphs = new ConcurrentDictionary<string, IGraph>();

            ParallelOptions options = new ParallelOptions();
            options.MaxDegreeOfParallelism = 8;

            var uris = catalogPageUris.ToArray();

            Parallel.ForEach(uris, options, uri =>
            {
                var task = client.GetGraphAsync(uri);
                task.Wait();

                if (!graphs.TryAdd(uri.AbsoluteUri, task.Result))
                {
                    throw new Exception("Duplicate graph: " + uri);
                }
            });

            await ProcessGraphs(new KeyValuePair<string, IDictionary<string, IGraph>>(packageId, graphs));
        }
    }
}
