﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NuGet.Services.Metadata.Catalog.Test
{
    public class CheckLinksCollector : BatchCollector
    {
        public CheckLinksCollector(Uri index, Func<HttpMessageHandler> handlerFunc = null, int batchSize = 200)
            : base(index, handlerFunc, batchSize)
        {
        }

        protected override async Task<bool> OnProcessBatch(CollectorHttpClient client, IList<JObject> items, JObject context)
        {
            List<Task<string>> tasks = new List<Task<string>>();

            foreach (JObject item in items)
            {
                Uri itemUri = item["@id"].ToObject<Uri>();
                tasks.Add(client.GetStringAsync(itemUri));
            }

            await Task.WhenAll(tasks.ToArray());

            return true;
        }
    }
}
