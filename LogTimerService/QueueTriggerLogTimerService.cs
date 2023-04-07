using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Rasputin.LogTimerService
{
    public class QueueTriggerLogTimerService
    {
        [FunctionName("QueueTriggerLogTimerService")]
        public async Task RunAsync([ServiceBusTrigger("ms-logtimer", Connection = "rasputinServicebus")]string myQueueItem, ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");
            try {
                var logTimer = JsonSerializer.Deserialize<LogTimer>(myQueueItem, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                await DBHelper.InsertLogTimerAsync(logTimer, log);
            } catch(Exception ex) {
                log.LogError($"Queue insertion failed", ex);
                throw;
            }
        }

    }
}
