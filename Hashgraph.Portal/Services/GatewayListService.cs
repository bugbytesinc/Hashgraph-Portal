#pragma warning disable CA1819
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hashgraph.Portal.Services
{
    public class GatewayListService
    {
        private Dictionary<string, Gateway[]> _gateways;
        public async Task<Dictionary<string, Gateway[]>> GetNetworkGateways()
        {
            await EnsureLoadedAsync();
            return _gateways;
        }

        public bool IsMainNetwork(Gateway gateway)
        {
            if (_gateways != null)
            {
                if (_gateways.TryGetValue("Main", out Gateway[] list))
                {
                    return list.Contains(gateway);
                }
            }
            return false;
        }

        private Task EnsureLoadedAsync()
        {
            if (_gateways is null)
            {
                // Todo, potential local storage when editing
                // the list is available
                //if (await _storage.ContainKeyAsync("gateways"))
                //{
                //    _gateways = await _storage.GetItemAsync<Dictionary<string, Gateway[]>>("gateways");
                //    return;
                //}
                _gateways = new Dictionary<string, Gateway[]>
                {
                    {
                        "Main",
                        new []
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
                        }
                    },
                    {
                        "Test",
                        new []
                        {
                            new Gateway("0.testnet.hedera.com:50211", 0, 0, 3),
                            new Gateway("1.testnet.hedera.com:50211", 0, 0, 4),
                            new Gateway("2.testnet.hedera.com:50211", 0, 0, 5),
                            new Gateway("3.testnet.hedera.com:50211", 0, 0, 6)
                        }
                    },
                    {
                        "Preview",
                        new []
                        {
                            new Gateway("0.previewnet.hedera.com:50211", 0, 0, 3),
                            new Gateway("1.previewnet.hedera.com:50211", 0, 0, 4),
                            new Gateway("2.previewnet.hedera.com:50211", 0, 0, 5),
                            new Gateway("3.previewnet.hedera.com:50211", 0, 0, 6)
                        }
                    }
                };
            }
            return Task.FromResult(0);
        }
    }
}
