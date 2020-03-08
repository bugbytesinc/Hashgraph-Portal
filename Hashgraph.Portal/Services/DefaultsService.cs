using System;

namespace Hashgraph.Portal.Services
{
    public class DefaultsService
    {
        public Address Payer { get; set; }
        public Gateway Gateway { get; set; }
        public Client RootClient { get; set; }
        public long FeeLimit { get; set; }
        public TimeSpan TransactionDuration { get; set; }

        public DefaultsService()
        {
            RootClient = new Client();
            RootClient.Configure(ctx => {
                FeeLimit = ctx.FeeLimit;
                TransactionDuration = ctx.TransactionDuration;
            });
        }
    }
}
