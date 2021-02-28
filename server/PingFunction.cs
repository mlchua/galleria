using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace mchua.dev.galleria
{
    /// <summary>
    /// Function that returns "pong" for "ping".
    /// </summary>
    public static class PingFunction
    {
        /// <summary>
        /// Returns "pong".
        /// </summary>
        /// <param name="req">Incoming request.</param>
        /// <param name="log">Logger.</param>
        /// <returns>Returns "pong".</returns>
        [FunctionName("Ping")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Processing incoming Ping request.");
            return new OkObjectResult("Pong");
        }
    }
}
