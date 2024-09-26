using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Transcriber
{
    public static class TranscriptionFunction
    {
        [FunctionName("TranscriptionFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            string url = req.Query["url"];
            string transcript = await TranscriptionService.GetTranscriptionAsync(url);            
            return new OkObjectResult(transcript);
        }
    }
}
