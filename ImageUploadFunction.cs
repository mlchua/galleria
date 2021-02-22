using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.Storage.Blob;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace mchua.dev.galleria
{
    public static class ImageUploadFunction
    {
        [FunctionName("ImageUpload")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            [Blob("images", FileAccess.Write)] CloudBlobContainer container,
            ILogger log)
        {
            try
            {
                IFormCollection form = await req.ReadFormAsync();
                IFormFile file = req.Form.Files["file"];

                using (Stream stream = file.OpenReadStream())
                {
                    string hash = ComputeHash(stream);
                    stream.Position = 0;

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
            await container.CreateIfNotExistsAsync();
            CloudBlockBlob blob = container.GetBlockBlobReference($"{hash}/{filename}");
            await blob.UploadFromStreamAsync(stream);
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
