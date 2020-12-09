using Hashgraph.Portal.Components;
using Hashgraph.Portal.Services;
using Microsoft.AspNetCore.Components;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Hashgraph.Portal.Pages
{
    public partial class DeleteToken : ComponentBase
    {
        [Inject] public DefaultsService DefaultsService { get; set; }

        private Network _network = null;
        private DeleteTokenInput _input = new DeleteTokenInput();
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
                _output = await client.DeleteTokenAsync(_input.Token, ctx => ctx.Memo = _input.Memo?.Trim());
            });
        }
    }
    public class DeleteTokenInput
    {
        [Required(ErrorMessage = "Please select a Network Gateway Node.")]
        public Gateway Gateway { get; set; }
        [Required(ErrorMessage = "Please enter the Token that will pay the Transaction Fees.")]
        public Address Payer { get; set; }
        [Required(ErrorMessage = "Please enter the Token you wish to delete.")]
        public Address Token { get; set; }
        [MaxLength(100, ErrorMessage = "The memo field cannot exceed 100 characters.")]
        public string Memo { get; set; }
    }
}
