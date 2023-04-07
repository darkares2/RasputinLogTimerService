using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Rasputin.LogTimerService
{
    public class ServiceBusDLQTriggerApiRouter
    {
        [FunctionName("ServiceBusDLQTriggerApiRouter")]
        public async Task RunAsync([ServiceBusTrigger("api-router/$DeadLetterQueue", Connection = "rasputinServicebus")]Microsoft.Azure.ServiceBus.Message myQueueItem, ILogger log)
        {
            var body = Encoding.UTF8.GetString(myQueueItem.Body);
            log.LogInformation($"api-router/$DeadLetterQueue: {body}");
            var message = JsonSerializer.Deserialize<Message>(body, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            var reason = myQueueItem.UserProperties["DeadLetterReason"].ToString();
            LogTimer logTimer = DLQHelper.CreateLogTimerFromMessage(message, reason);
            await DBHelper.InsertLogTimerAsync(logTimer, log);
        }
    }
}
