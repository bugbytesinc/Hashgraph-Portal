using System;

namespace Hashgraph.Portal.Services
{
    public static class Util
    {
        private static readonly DateTime EPOCH = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private const long NanosPerTick = 1_000_000_000L / TimeSpan.TicksPerSecond;

        public static DateTime ToDate(long seconds, int nanos)
        {
            return EPOCH.AddTicks(seconds * TimeSpan.TicksPerSecond + nanos / NanosPerTick);
        }
        public static DateTime ToDate(Proto.TransactionID transaction)
        {
            return transaction == null ? DateTime.MinValue : ToDate(transaction.TransactionValidStart.Seconds, transaction.TransactionValidStart.Nanos);
        }
        public static DateTime ToDate(TxId txid)
        {
            return txid == null ? DateTime.MinValue : ToDate(txid.ValidStartSeconds, txid.ValidStartNanos);
        }
        public static TimeSpan ComputeRetryDelay(TimeSpan totalWaitTime, int maxRetryCount)
        {
            if(maxRetryCount < 1)
            {
                maxRetryCount = 1;
            }
            var result = TimeSpan.FromMilliseconds((2 * totalWaitTime.TotalMilliseconds) / (maxRetryCount * (maxRetryCount + 1)));
            return result.TotalMilliseconds > 10 ? result : TimeSpan.FromMilliseconds(10);
        }
    }
}
