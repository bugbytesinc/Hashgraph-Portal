using Hashgraph.Portal.Components;
using Hashgraph.Portal.Services;
using Microsoft.AspNetCore.Components;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Hashgraph.Portal.Pages
{
    public partial class AccountBalance : ComponentBase
    {
        [Inject] public DefaultsService DefaultsService { get; set; }

        private Network _network = null;
        private AccountBalanceInput _input = new AccountBalanceInput();
        private ulong? _output = null;

        protected override void OnInitialized()
        {
            _input.Gateway = DefaultsService.Gateway;
            base.OnInitialized();
        }

        protected async Task HandleValidSubmit()
        {
            _output = null;
            await _network.ExecuteAsync(_input.Gateway, null, async client =>
            {
                _output = await client.GetAccountBalanceAsync(_input.Address);
            });
        }
    }
    public class AccountBalanceInput
    {
        [Required] public Gateway Gateway { get; set; }
        [Required] public Address Address { get; set; }
    }
}
