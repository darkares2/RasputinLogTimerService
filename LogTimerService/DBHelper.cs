using System;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace Rasputin.LogTimerService
{
    public class DBHelper {
        public static async Task InsertLogTimerAsync(LogTimer logTimer, ILogger log)
        {
            var connectionString = Environment.GetEnvironmentVariable("sqldb_connection");
            string query = @"
                    MERGE LogTimer AS target
                    USING (VALUES (@Id, @Queue, @SentTimestamp, @ReceiveTimestamp, @ProcessMs)) AS source (Id, Queue, SentTimestamp, ReceiveTimestamp, ProcessMs)
                    ON target.Id = source.Id AND target.SentTimestamp = source.SentTimestamp
                    WHEN MATCHED THEN
                        UPDATE SET target.Queue = source.Queue, target.ReceiveTimestamp = source.ReceiveTimestamp, target.ProcessMs = source.ProcessMs
                    WHEN NOT MATCHED THEN
                        INSERT (Id, Queue, SentTimestamp, ReceiveTimestamp, ProcessMs)
                        VALUES (source.Id, source.Queue, source.SentTimestamp, source.ReceiveTimestamp, source.ProcessMs);";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // open the connection
                await connection.OpenAsync();

                // create a SqlCommand object for the merge statement
                SqlCommand command = new SqlCommand(query, connection);

                // add parameters to the SqlCommand object
                command.Parameters.AddWithValue("@Id", logTimer.Id);
                command.Parameters.AddWithValue("@Queue", logTimer.Queue);
                command.Parameters.AddWithValue("@SentTimestamp", logTimer.SentTimestamp);
                command.Parameters.AddWithValue("@ReceiveTimestamp", logTimer.ReceiveTimestamp);
                command.Parameters.AddWithValue("@ProcessMs", logTimer.ProcesMs);

                // execute the merge statement
                int rowsAffected = await command.ExecuteNonQueryAsync();

                log.LogDebug($"Inserted into LogTimer[{rowsAffected}]");
            }

        }

    }
}