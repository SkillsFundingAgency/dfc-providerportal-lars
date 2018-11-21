using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dfc.ProviderPortal.Lars.Common.DependencyInjection;
using Dfc.ProviderPortal.Lars.Common.Files.Delimited;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

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

            try
            {
                using (var client = new DocumentClient(new Uri(settings.Value.EndpointUri), settings.Value.PrimaryKey))
                {
                    await CreateDatabaseIfNotExistsAsync(settings.Value, client);
                    await CreateOrRecreateDocumentCollectionAsync(settings.Value, client, name);
                    await ImportFileContentAsync(settings.Value, client, name, myBlob);
                }
            }
            catch (Exception e)
            {
                log.LogError(e, e.StackTrace);
            }
        }

        internal static async Task CreateDatabaseIfNotExistsAsync(LarsCosmosDbCollectionSettings settings, DocumentClient client)
        {
            await client.CreateDatabaseIfNotExistsAsync(new Database { Id = settings.DatabaseId });
        }

        internal static async Task CreateOrRecreateDocumentCollectionAsync(LarsCosmosDbCollectionSettings settings, DocumentClient client, string fileName)
        {
            if (TryGetCollectionId(settings, fileName, out string collectionId))
            {
                var collection = client
                    .CreateDocumentCollectionQuery(UriFactory.CreateDatabaseUri(settings.DatabaseId))
                    .ToArray()
                    .FirstOrDefault(c => c.Id == collectionId);

                if (collection != null)
                {
                    await client.DeleteDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(settings.DatabaseId, collectionId));
                }

                await client.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(settings.DatabaseId), new DocumentCollection { Id = collectionId });
            }
        }

        internal static bool TryGetCollectionId(LarsCosmosDbCollectionSettings settings, string fileName, out string collectionId)
        {
            collectionId = string.Empty;

            if (fileName == settings.CollectionSettings.Category.FileName) collectionId = settings.CollectionSettings.Category.CollectionId;
            if (fileName == settings.CollectionSettings.LearningDelivery.FileName) collectionId = settings.CollectionSettings.LearningDelivery.CollectionId;
            if (fileName == settings.CollectionSettings.LearningDeliveryCategory.FileName) collectionId = settings.CollectionSettings.LearningDeliveryCategory.CollectionId;
            if (fileName == settings.CollectionSettings.AwardOrgCode.FileName) collectionId = settings.CollectionSettings.AwardOrgCode.CollectionId;
            if (fileName == settings.CollectionSettings.SectorSubjectAreaTier1.FileName) collectionId = settings.CollectionSettings.SectorSubjectAreaTier1.CollectionId;
            if (fileName == settings.CollectionSettings.SectorSubjectAreaTier2.FileName) collectionId = settings.CollectionSettings.SectorSubjectAreaTier2.CollectionId;
            if (fileName == settings.CollectionSettings.MINotionalNVQLevelv2.FileName) collectionId = settings.CollectionSettings.MINotionalNVQLevelv2.CollectionId;

            return !string.IsNullOrEmpty(collectionId);
        }

        internal static async Task ImportFileContentAsync(LarsCosmosDbCollectionSettings settings, DocumentClient client, string fileName, Stream stream)
        {
            if (TryGetCollectionId(settings, fileName, out string collectionId))
            {
                var headings = new List<string>();

                using (var reader = new StreamReader(stream))
                {
                    var lineNumber = 0;

                    foreach (var line in DelimitedFileReader.ReadLines(reader, new DelimitedFileSettings(true)))
                    {
                        lineNumber++;

                        if (lineNumber == 1)
                        {
                            headings = line.Fields.Select(f => CleanHeading(f.Value)).ToList();
                            continue;
                        }

                        var dict = new Dictionary<string, object>();

                        foreach (var field in line.Fields)
                        {
                            dict[headings[field.Number - 1]] = field.Value;
                        }

                        var doc = dict.ToExpandoObject();

                        await client.CreateDocumentAsync(
                            UriFactory.CreateDocumentCollectionUri(settings.DatabaseId, collectionId),
                            doc);
                    }
                }
            }
        }

        internal static string CleanHeading(string heading)
        {
            var rgx = new Regex("[^a-zA-Z0-9_]");
            var cleaned = rgx.Replace(heading, string.Empty);

            if (Regex.IsMatch(cleaned, @"^\d"))
            {
                return $"_{cleaned}";
            }

            return cleaned;
        }

        internal static ExpandoObject ToExpandoObject(this IDictionary<string, object> dictionary)
        {
            var expando = new ExpandoObject();
            var expandoDic = (IDictionary<string, object>)expando;

            foreach (var kvp in dictionary)
            {
                if (kvp.Value is IDictionary<string, object>)
                {
                    var expandoValue = ((IDictionary<string, object>)kvp.Value).ToExpandoObject();
                    expandoDic.Add(kvp.Key, expandoValue);
                }
                else if (kvp.Value is ICollection)
                {
                    var itemList = new List<object>();

                    foreach (var item in (ICollection)kvp.Value)
                    {
                        if (item is IDictionary<string, object>)
                        {
                            var expandoItem = ((IDictionary<string, object>)item).ToExpandoObject();
                            itemList.Add(expandoItem);
                        }
                        else
                        {
                            itemList.Add(item);
                        }
                    }

                    expandoDic.Add(kvp.Key, itemList);
                }
                else
                {
                    expandoDic.Add(kvp);
                }
            }

            return expando;
        }
    }
}
