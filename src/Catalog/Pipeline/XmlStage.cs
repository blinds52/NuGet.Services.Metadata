﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NuGet.Services.Metadata.Catalog.Pipeline
{
    public class XmlStage : PackagePipelineStage
    {
        string _entryName;
        XDocument _transform;

        public XmlStage(string entryName, XDocument transform)
        {
            _entryName = entryName;
            _transform = transform;
        }

        public override bool Execute(PipelinePackage package, PackagePipelineContext context)
        {
            XDocument document = PackagePipelineHelpers.GetXmlZipArchiveEntry(package, context, _entryName);



            return true;
        }
    }
}
