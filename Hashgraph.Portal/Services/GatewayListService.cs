using System.Collections.ObjectModel;

namespace Hashgraph.Portal.Services
{
    public class GatewayListService
    {
        public ReadOnlyCollection<Gateway> MainNet { get { return _mainNet; } }
        public ReadOnlyCollection<Gateway> TestNet { get { return _testNet; } }

        private static ReadOnlyCollection<Gateway> _mainNet = new ReadOnlyCollection<Gateway>(new Gateway[]
            {
                new Gateway("35.237.200.180:50211", 0, 0, 3 ),
                new Gateway("35.186.191.247:50211", 0, 0, 4 ),
                new Gateway("35.192.2.25:50211",    0, 0, 5 ),
                new Gateway("35.199.161.108:50211", 0, 0, 6 ),
                new Gateway("35.203.82.240:50211",  0, 0, 7 ),
                new Gateway("35.236.5.219:50211",   0, 0, 8 ),
                new Gateway("35.197.192.225:50211", 0, 0, 9 ),
                new Gateway("35.242.233.154:50211", 0, 0, 10),
                new Gateway("35.240.118.96:50211",  0, 0, 11),
                new Gateway("35.204.86.32:50211",   0, 0, 12)
            });
        private static ReadOnlyCollection<Gateway> _testNet = new ReadOnlyCollection<Gateway>(new Gateway[]
            {
                new Gateway("0.testnet.hedera.com:50211", 0, 0, 3),
                new Gateway("1.testnet.hedera.com:50211", 0, 0, 4),
                new Gateway("2.testnet.hedera.com:50211", 0, 0, 5),
                new Gateway("3.testnet.hedera.com:50211", 0, 0, 6)
            });
    }
}
