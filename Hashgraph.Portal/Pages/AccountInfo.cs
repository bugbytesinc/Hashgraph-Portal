using Hashgraph.Portal.Components;
using Hashgraph.Portal.Services;
using Microsoft.AspNetCore.Components;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Hashgraph.Portal.Pages
{
    public partial class AccountInfo : ComponentBase
    {
        [Inject] public DefaultsService DefaultsService { get; set; }

        private Network _network = null;
        private AccountInfoInput _input = new AccountInfoInput();
        private Hashgraph.AccountInfo _output = null;

        protected override void OnInitialized()
        {
            _input.Payer = DefaultsService.Payer;
            _input.Gateway = DefaultsService.Gateway;
            base.OnInitialized();
        }
        protected async Task HandleValidSubmit()
        {
            _output = null;
            await _network.ExecuteAsync(_input.Gateway, _input.Payer, async client =>
            {
                _output = await client.GetAccountInfoAsync(_input.Address);
            });
        }
    }
    public class AccountInfoInput
    {
        [Required] public Gateway Gateway { get; set; }
        [Required] public Address Address { get; set; }
        [Required] public Address Payer { get; set; }
    }
}
