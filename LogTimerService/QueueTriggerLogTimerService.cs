using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Data.SqlClient;
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
                await InsertLogTimerAsync(logTimer, log);
            } catch(Exception ex) {
                log.LogError($"Queue insertion failed", ex);
                throw;
            }
        }

        private async Task InsertLogTimerAsync(LogTimer logTimer, ILogger log)
        {
            var str = Environment.GetEnvironmentVariable("sqldb_connection");
            string query = "INSERT INTO LogTimer (Id, Queue, SentTimestamp, ReceiveTimestamp) VALUES (@Id, @Queue,@SentTimestamp,@ReceiveTimestamp)";
            using (SqlConnection connection = new SqlConnection(str))
            {
                connection.Open();
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", logTimer.Id);
                    command.Parameters.AddWithValue("@Queue", logTimer.Queue);
                    command.Parameters.AddWithValue("@SentTimestamp", logTimer.SentTimestamp);
                    command.Parameters.AddWithValue("@ReceiveTimestamp", logTimer.ReceiveTimestamp);
                    var countInserted = await command.ExecuteNonQueryAsync();
                    log.LogDebug($"Inserted into LogTimer[{countInserted}]");
                }
            }
        }
    }
}
