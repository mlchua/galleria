using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.Storage.Blob;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace mchua.dev.galleria
{
    public static class ImageUploadFunction
    {
        [FunctionName("ImageUpload")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            [Blob("images", FileAccess.Write)] CloudBlobContainer container,
            [CosmosDB(
                databaseName: "galleria",
                collectionName: "metadata",
                ConnectionStringSetting = "CosmosDBConnection")] IDocumentClient cosmosdb,
            ILogger log)
        {
            try
            {
                IFormCollection form = await req.ReadFormAsync();
                IFormFile file = req.Form.Files["file"];

                await container.CreateIfNotExistsAsync();

                using (Stream stream = file.OpenReadStream())
                {
                    string hash = ComputeHash(stream);
                    stream.Position = 0;

                    if (await ImageExists(hash, cosmosdb))
                    {
                        return new OkObjectResult("File already exists");
                    }

                    ImageMetadata metadata = new ImageMetadata()
                    {
                        ImageId = hash,
                        OriginalName = file.FileName,
                        UploadTimestamp = DateTimeOffset.UtcNow,
                    };
                    await InsertMetadata(metadata, cosmosdb, log);

                    await UploadOriginalFile(stream, hash, file.FileName, container);

                    return new OkObjectResult(file.FileName + " - " + file.Length.ToString() + " - " + hash);
                }
            }
            catch (Exception ex)
            {
                log.LogWarning("Unhandled exception occured on image upload: {exception}.", ex);
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        private static async Task UploadOriginalFile(Stream stream, string hash, string filename, CloudBlobContainer container)
        {
            CloudBlockBlob blob = container.GetBlockBlobReference($"{hash}/{filename}");
            await blob.UploadFromStreamAsync(stream);
        }

        private static async Task InsertMetadata(ImageMetadata metadata, IDocumentClient cosmosdb, ILogger log)
        {
            JObject jobject = JObject.FromObject(metadata);

            log.LogInformation($"Inserting metadata for image '{metadata.ImageId}' into cosmos db.");

            Uri uri = UriFactory.CreateDocumentCollectionUri("galleria", "metadata");
            ResourceResponse<Document> response = await cosmosdb.CreateDocumentAsync(uri, jobject);

            log.LogInformation($"Inserting metadata into cosmos db took '{response.RequestCharge}' RUs.");
        }

        private static async Task<bool> ImageExists(string hash, IDocumentClient cosmosdb)
        {
            Uri uri = UriFactory.CreateDocumentUri("galleria", "metadata", "metadata");
            try
            {
                await cosmosdb.ReadDocumentAsync(uri.ToString(), new RequestOptions() { PartitionKey = new PartitionKey(hash) });
                return true;
            }
            catch (DocumentClientException ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    return false;
                }
                throw ex;
            }
        }

        private static string ComputeHash(Stream stream)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(stream);
                StringBuilder builder = new StringBuilder();
                foreach (byte b in hash)
                {
                    builder.AppendFormat("{0:x2}", b);
                }
                return builder.ToString();
            }
        }
    }
}
