using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.ProviderPortal.Lars.Functions.ImportCsv
{
    public class LarsCosmosDbCollectionSettings
    {
        public string EndpointUri { get; set; }
        public string PrimaryKey { get; set; }
        public string DatabaseId { get; set; }
        public CollectionSettings CollectionSettings { get; set; }
    }
}
