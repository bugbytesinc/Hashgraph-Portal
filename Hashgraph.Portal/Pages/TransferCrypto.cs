using Hashgraph.Portal.Components;
using Hashgraph.Portal.Models;
using Hashgraph.Portal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Hashgraph.Portal.Pages
{
    public partial class TransferCrypto : ComponentBase
    {
        [Inject] public DefaultsService DefaultsService { get; set; }

        private Network _network = null;
        private EditContext _editContext = null;
        private ValidationMessageStore _validationMessages = null;
        private TransferCryptoInput _input = new TransferCryptoInput();
        private TransactionReceipt _output = null;

        protected override void OnInitialized()
        {
            _input.Gateway = DefaultsService.Gateway;
            _input.Payer = DefaultsService.Payer;
            _input.CryptoTransfers = new CryptoTransferList();
            _editContext = new EditContext(_input);
            _editContext.OnValidationRequested += OnValidationRequested;
            _validationMessages = new ValidationMessageStore(_editContext);
            base.OnInitialized();
        }

        private void OnValidationRequested(object sender, ValidationRequestedEventArgs e)
        {
            _validationMessages.Clear();
            if (_input.CryptoTransfers is null && _input.TokenTransfers.Count == 0)
            {
                _validationMessages.Add(new FieldIdentifier(_input, string.Empty), "Please add a Crypto or Token Transfer.");
            }
        }

        protected async Task HandleValidSubmit()
        {
            _output = null;
            await _network.ExecuteAsync(_input.Gateway, _input.Payer, async client =>
            {
                var tokenTransfers = _input.TokenTransfers.ToTransferList().ToArray();
                _output = await client.TransferAsync(new TransferParams
                {
                    CryptoTransfers = _input.CryptoTransfers?.ToTransferDictionary(),
                    TokenTransfers = tokenTransfers.Length > 0 ? tokenTransfers : null
                },
                ctx => ctx.Memo = _input.Memo?.Trim());
            });
        }

        private void AddCryptoTransfer()
        {
            if (_input.CryptoTransfers == null)
            {
                _input.CryptoTransfers = new CryptoTransferList();
            }
            _editContext.NotifyValidationStateChanged();
        }

        private void AddTokenTransferGroup()
        {
            _input.TokenTransfers.Add(new TokenTransferGroup());
            _editContext.Validate();
        }

        private void RemoveTokenTransferGroup(TokenTransferGroup group)
        {
            _input.TokenTransfers.Remove(group);
            _editContext.NotifyValidationStateChanged();
        }

        private void RemoveCryptoTransfers()
        {
            _input.CryptoTransfers = null;
            _editContext.NotifyValidationStateChanged();
        }
    }
    public class TransferCryptoInput
    {
        [Required(ErrorMessage = "Please select a Network Gateway Node.")]
        public Gateway Gateway { get; set; }
        [Required(ErrorMessage = "Please enter the account that will pay the Transaction Fees.")]
        public Address Payer { get; set; }
        public CryptoTransferList CryptoTransfers { get; set; }
        public TokenTransferList TokenTransfers { get; } = new TokenTransferList();
        [MaxLength(100, ErrorMessage = "The memo field cannot exceed 100 characters.")]
        public string Memo { get; set; }
    }
}
