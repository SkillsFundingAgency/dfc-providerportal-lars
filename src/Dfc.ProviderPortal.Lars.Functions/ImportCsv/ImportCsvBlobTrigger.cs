using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dfc.ProviderPortal.Lars.Common.DependencyInjection;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dfc.ProviderPortal.Lars.Functions.ImportCsv
{
    public static class ImportCsvBlobTrigger
    {
        [FunctionName("ImportCsvBlobTrigger")]
        public static async Task Run(
            [BlobTrigger("%import-csv-blob-trigger-container%/{name}", Connection = "")]Stream myBlob,
            string name,
            ILogger log,
            [Inject] IOptions<LarsCosmosDbCollectionSettings> settings)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");

            await CreateDatabaseIfNotExistsAsync(settings.Value);
            await CreateOrRecreateDocumentCollectionAsync(settings.Value, name);
        }

        internal static async Task CreateDatabaseIfNotExistsAsync(LarsCosmosDbCollectionSettings settings)
        {
            using (var client = new DocumentClient(new Uri(settings.EndpointUri), settings.PrimaryKey))
            {
                await client.CreateDatabaseIfNotExistsAsync(new Database { Id = settings.DatabaseId });
            }
        }

        internal static async Task CreateOrRecreateDocumentCollectionAsync(LarsCosmosDbCollectionSettings settings, string fileName)
        {
            if (TryGetCollectionId(settings, fileName, out string collectionId))
            {
                using (var client = new DocumentClient(new Uri(settings.EndpointUri), settings.PrimaryKey))
                {
                    var collection = client
                        .CreateDocumentCollectionQuery(UriFactory.CreateDatabaseUri(settings.DatabaseId))
                        .ToArray()
                        .FirstOrDefault(c => c.Id == collectionId);

                    if (collection != null)
                    {
                        var deleteResponse = await client.DeleteDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(settings.DatabaseId, collectionId));
                    }

                    var createResponse = await client.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(settings.DatabaseId), new DocumentCollection { Id = collectionId });
                }
            }
        }

        internal static bool TryGetCollectionId(LarsCosmosDbCollectionSettings settings, string fileName, out string collectionId)
        {
            collectionId = string.Empty;

            if (fileName == settings.CollectionSettings.Category.FileName) collectionId = settings.CollectionSettings.Category.CollectionId;
            if (fileName == settings.CollectionSettings.LearningDelivery.FileName) collectionId = settings.CollectionSettings.LearningDelivery.CollectionId;
            if (fileName == settings.CollectionSettings.LearningDeliveryCategory.FileName) collectionId = settings.CollectionSettings.LearningDeliveryCategory.CollectionId;
            if (fileName == settings.CollectionSettings.AwardOrgCode.FileName) collectionId = settings.CollectionSettings.AwardOrgCode.CollectionId;
            if (fileName == settings.CollectionSettings.SubjectSectorTier1.FileName) collectionId = settings.CollectionSettings.SubjectSectorTier1.CollectionId;
            if (fileName == settings.CollectionSettings.SubjectSectorTier2.FileName) collectionId = settings.CollectionSettings.SubjectSectorTier2.CollectionId;
            if (fileName == settings.CollectionSettings.MINotionalNVQLevelv2.FileName) collectionId = settings.CollectionSettings.MINotionalNVQLevelv2.CollectionId;

            return !string.IsNullOrEmpty(collectionId);
        }
    }
}
