using Hashgraph.Portal.Components;
using Hashgraph.Portal.Services;
using Microsoft.AspNetCore.Components;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Hashgraph.Portal.Pages
{
    public partial class CreateAccount : ComponentBase
    {
        [Inject] public DefaultsService DefaultsService { get; set; }

        private Network _network = null;
        private CreateAccountInput _input = new CreateAccountInput();
        private AccountReceipt _output = null;

        protected override void OnInitialized()
        {
            _input.Gateway = DefaultsService.Gateway;
            _input.Payer = DefaultsService.Payer;
            base.OnInitialized();
        }

        protected async Task HandleValidSubmit()
        {
            _output = null;
            await _network.ExecuteAsync(_input.Gateway, _input.Payer, async client =>
            {
                var createParams = new CreateAccountParams
                {
                    Endorsement = _input.Endorsement,
                    InitialBalance = _input.InitialBalance.GetValueOrDefault(),
                    RequireReceiveSignature = _input.RequireReceiveSignature,
                    Proxy = _input.Proxy
                };
                if (_input.SendThresholdCreateRecord.HasValue)
                {
                    createParams.SendThresholdCreateRecord = _input.SendThresholdCreateRecord.Value;
                }
                if (_input.ReceiveThresholdCreateRecord.HasValue)
                {
                    createParams.ReceiveThresholdCreateRecord = _input.ReceiveThresholdCreateRecord.Value;
                }
                _output = await client.CreateAccountAsync(createParams);
            });
        }
    }
    public class CreateAccountInput
    {
        [Required] public Gateway Gateway { get; set; }
        [Required] public Address Payer { get; set; }
        [Required] public ulong? InitialBalance { get; set; }
        [Required] public Endorsement Endorsement { get; set; }
        public ulong? SendThresholdCreateRecord { get; set; }
        public ulong? ReceiveThresholdCreateRecord { get; set; }
        public bool RequireReceiveSignature { get; set; }
        public Address Proxy { get; set; }
    }
}
