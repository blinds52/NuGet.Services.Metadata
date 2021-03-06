﻿using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Ng
{
    public class StorageAccessHandler : DelegatingHandler
    {
        private readonly string _catalogBaseAddress;
        private readonly string _storageBaseAddress;

        public StorageAccessHandler(string catalogBaseAddress, string storageBaseAddress)
            : base(new HttpClientHandler())
        {
            _catalogBaseAddress = catalogBaseAddress;
            _storageBaseAddress = storageBaseAddress;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string requestUri = request.RequestUri.AbsoluteUri;

            if (requestUri.StartsWith(_catalogBaseAddress))
            {
                string newRequestUri = _storageBaseAddress + requestUri.Substring(_catalogBaseAddress.Length);
                request.RequestUri = new Uri(newRequestUri);
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
