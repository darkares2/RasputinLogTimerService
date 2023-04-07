
using System;
using System.Globalization;
using System.Linq;

namespace Rasputin.LogTimerService
{
    public class DLQHelper
    {
        internal static LogTimer CreateLogTimerFromMessage(Message message, string reason)
        {
            var current = message.Headers.FirstOrDefault(x => x.Name.Equals("current-queue-header"));
            current.Fields["Name"] = current.Fields["Name"] + $"-Error: {reason}";
            var idHeader = message.Headers.FirstOrDefault(x => x.Name.Equals("id-header"));
            Random random = new Random();

            var sentTimestamp = current.Fields.ContainsKey("Timestamp") ? DateTime.ParseExact(current.Fields["Timestamp"], "yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal) : new DateTime(621355968000000000L, DateTimeKind.Utc);
            // Since this dead-letter-queue item could overwrite important already logged errors we change the signature 
            int randomMillisecond = random.Next(1, 1000);
            sentTimestamp = sentTimestamp.AddMilliseconds(randomMillisecond);

            return new LogTimer() {
                Id = Guid.Parse(idHeader.Fields["GUID"]),
                Queue = current.Fields["Name"],
                SentTimestamp = sentTimestamp,
                ReceiveTimestamp = DateTime.UtcNow,
                ProcesMs = 0
            };
        }
    }
}