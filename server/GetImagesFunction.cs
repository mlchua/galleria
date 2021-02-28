using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace mchua.dev.galleria
{
    public static class GetImagesFunction
    {
        [FunctionName("GetImages")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            [CosmosDB(
                databaseName: "galleria",
                collectionName: "metadata",
                ConnectionStringSetting = "CosmosDBConnection")] IDocumentClient cosmosdb,
            ILogger log)
        {
            List<ImageMetadata> metadata =
                cosmosdb.CreateDocumentQuery<ImageMetadata>(
                    UriFactory.CreateDocumentCollectionUri("galleria", "metadata"),
                    new FeedOptions { MaxItemCount = 25, EnableCrossPartitionQuery = true })
                        .OrderByDescending(d => d.UploadTimestamp)
                        .ToList<ImageMetadata>();

            string json = JsonConvert.SerializeObject(metadata);
            return new OkObjectResult(json);
        }
    }
}
