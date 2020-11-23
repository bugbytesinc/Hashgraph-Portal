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
        private AccountBalances _output = null;

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
                _output = await client.GetAccountBalancesAsync(_input.Address);
            });
        }
    }
    public class AccountBalanceInput
    {
        [Required(ErrorMessage ="Please select a Network Gateway Node.")] 
        public Gateway Gateway { get; set; }

        [Required(ErrorMessage ="Please enter the address of the account to query.")]
        public Address Address { get; set; }
    }
}
