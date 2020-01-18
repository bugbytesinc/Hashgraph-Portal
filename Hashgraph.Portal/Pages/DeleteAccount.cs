using Hashgraph.Portal.Components;
using Hashgraph.Portal.Services;
using Microsoft.AspNetCore.Components;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Hashgraph.Portal.Pages
{
    public partial class DeleteAccount : ComponentBase
    {
        [Inject] public DefaultsService DefaultsService { get; set; }

        private Network _network = null;
        private DeleteAccountInput _input = new DeleteAccountInput();
        private TransactionReceipt _output = null;

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
                _output = await client.DeleteAccountAsync(_input.DeleteAddress, _input.TransferToAddress);
            });
        }
    }
    public class DeleteAccountInput
    {
        [Required] public Gateway Gateway { get; set; }
        [Required] public Address Payer { get; set; }
        [Required] public Address DeleteAddress { get; set; }
        [Required] public Address TransferToAddress { get; set; }
    }
}
