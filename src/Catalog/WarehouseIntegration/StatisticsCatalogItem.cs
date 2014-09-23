﻿using Newtonsoft.Json.Linq;
using NuGet.Services.Metadata.Catalog.Maintenance;
using NuGet.Services.Metadata.Catalog.Persistence;
using System;
using VDS.RDF;

namespace NuGet.Services.Metadata.Catalog.WarehouseIntegration
{
    public class StatisticsCatalogItem : CatalogItem
    {
        static Uri CatalogItemType = new Uri("http://nuget.org/schema#PackageStatisticsPage");

        JArray _data;
        DateTime _minDownloadTimestamp;
        DateTime _maxDownloadTimestamp;
        Guid _itemGUID;

        public StatisticsCatalogItem(JArray data, DateTime minDownloadTimestamp, DateTime maxDownloadTimestamp)
        {
            _data = data;
            _minDownloadTimestamp = minDownloadTimestamp;
            _maxDownloadTimestamp = maxDownloadTimestamp;
            _itemGUID = Guid.NewGuid();
        }
        public override StorageContent CreateContent(CatalogContext context)
        {
            return new StringStorageContent(_data.ToString(), "application/json");
        }

        public override IGraph CreatePageContent(CatalogContext context)
        {
            Uri resourceUri = new Uri(GetBaseAddress() + GetRelativeAddress());

            Graph graph = new Graph();

            INode subject = graph.CreateUriNode(resourceUri);
            INode count = graph.CreateUriNode(new Uri("http://nuget.org/schema#count"));
            INode itemGUID = graph.CreateUriNode(new Uri("http://nuget.org/schema#itemGUID"));
            INode minDownloadTimestamp = graph.CreateUriNode(new Uri("http://nuget.org/schema#minDownloadTimestamp"));
            INode maxDownloadTimestamp = graph.CreateUriNode(new Uri("http://nuget.org/schema#maxDownloadTimestamp"));

            graph.Assert(subject, count, graph.CreateLiteralNode(_data.Count.ToString(), Schema.DataTypes.Integer));
            graph.Assert(subject, itemGUID, graph.CreateLiteralNode(_itemGUID.ToString()));
            graph.Assert(subject, minDownloadTimestamp, graph.CreateLiteralNode(_minDownloadTimestamp.ToString("O"), Schema.DataTypes.DateTime));
            graph.Assert(subject, maxDownloadTimestamp, graph.CreateLiteralNode(_maxDownloadTimestamp.ToString("O"), Schema.DataTypes.DateTime));

            return graph;
        }

        protected override string GetItemIdentity()
        {
            const string dateTimeFormat = "yyyy.MM.dd.HH.mm.ss";
            const string itemIdentityFormat = "{0}_TO_{1}";
            string itemIdentity = String.Format(itemIdentityFormat, _minDownloadTimestamp.ToString(dateTimeFormat), _maxDownloadTimestamp.ToString(dateTimeFormat));

            return itemIdentity;
        }

        public override Uri GetItemType()
        {
            return CatalogItemType;
        }
    }
}
