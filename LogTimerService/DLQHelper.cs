
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
            return new LogTimer() {
                Id = Guid.Parse(idHeader.Fields["GUID"]),
                Queue = current.Fields["Name"],
                SentTimestamp = current.Fields.ContainsKey("Timestamp") ? DateTime.ParseExact(current.Fields["Timestamp"], "yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal) : new DateTime(621355968000000000L, DateTimeKind.Utc),
                ReceiveTimestamp = DateTime.UtcNow,
                ProcesMs = 0
            };
        }
    }
}