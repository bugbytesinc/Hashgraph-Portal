using Hashgraph.Portal.Components;
using Hashgraph.Portal.Services;
using Microsoft.AspNetCore.Components;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Hashgraph.Portal.Pages
{
    public partial class AccountRecords : ComponentBase
    {
        [Inject] public DefaultsService DefaultsService { get; set; }

        private Network _network = null;
        private AccountInfoInput _input = new AccountInfoInput();
        private TransactionRecord[] _output = null;

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
                _output = await client.GetAccountRecordsAsync(_input.Address);
            });
        }
    }
    public class AccountRecordsInput
    {
        [Required(ErrorMessage = "Please select a Network Gateway Node.")]
        public Gateway Gateway { get; set; }
        [Required(ErrorMessage = "Please enter the account that will pay the Query Transaction Fees.")]
        public Address Payer { get; set; }
        [Required(ErrorMessage = "Please enter the account you wish to Query.")]
        public Address Address { get; set; }
    }
}
