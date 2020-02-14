using Hashgraph.Portal.Components;
using Hashgraph.Portal.Models;
using Hashgraph.Portal.Services;
using Microsoft.AspNetCore.Components;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Hashgraph.Portal.Pages
{
    public partial class TransferCrypto : ComponentBase
    {
        [Inject] public DefaultsService DefaultsService { get; set; }

        private Network _network = null;
        private TransferCryptoInput _input = new TransferCryptoInput();
        private TransactionReceipt _output = null;

        protected override void OnInitialized()
        {
            _input.Gateway = DefaultsService.Gateway;
            _input.Payer = DefaultsService.Payer;
            _input.Transfers.To.Add(new CryptoTransfer());
            _input.Transfers.From.Add(new CryptoTransfer());
            base.OnInitialized();
        }

        protected async Task HandleValidSubmit()
        {
            _output = null;
            await _network.ExecuteAsync(_input.Gateway, _input.Payer, async client =>
            {
                _output = await client.TransferAsync(_input.Transfers.ToTransferDictionary(), ctx=>ctx.Memo = _input.Memo?.Trim());
            });
        }
    }
    public class TransferCryptoInput
    {
        [Required(ErrorMessage = "Please select a Network Gateway Node.")]
        public Gateway Gateway { get; set; }
        [Required(ErrorMessage = "Please enter the account that will pay the Transaction Fees.")]
        public Address Payer { get; set; }
        [Required(ErrorMessage = "Please enter transfer information.")]
        public CryptoTransferList Transfers { get; set; } = new CryptoTransferList();
        [MaxLength(100, ErrorMessage = "The memo field cannot exceed 100 characters.")]
        public string Memo { get; set; }
    }
}
