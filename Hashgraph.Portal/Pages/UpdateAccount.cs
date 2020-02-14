using Hashgraph.Portal.Components;
using Hashgraph.Portal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Hashgraph.Portal.Pages
{
    public partial class UpdateAccount : ComponentBase
    {
        [Inject] public DefaultsService DefaultsService { get; set; }

        private Network _network = null;
        private EditContext _editContext = null;
        private ValidationMessageStore _validationMessages = null;
        private UpdateAccountInput _input = new UpdateAccountInput();
        private TransactionReceipt _output = null;

        protected override void OnInitialized()
        {
            _input.Gateway = DefaultsService.Gateway;
            _input.Payer = DefaultsService.Payer;
            _editContext = new EditContext(_input);
            _editContext.OnValidationRequested += OnValidationRequested;
            _validationMessages = new ValidationMessageStore(_editContext);
            base.OnInitialized();
        }
        private void OnValidationRequested(object sender, ValidationRequestedEventArgs e)
        {
            _validationMessages.Clear();
            bool somethingIsSelected = false;
            if (_input.UpdateEndorsement)
            {
                somethingIsSelected = true;
                if (_input.Endorsement == null)
                {
                    AddIfNoOtherErrors(nameof(_input.Endorsement), "Please enter a a new Endorsement.");
                }
            }
            if (_input.UpdateSendThresholdCreateRecord)
            {
                somethingIsSelected = true;
                if (!_input.SendThresholdCreateRecord.HasValue)
                {
                    AddIfNoOtherErrors(nameof(_input.SendThresholdCreateRecord), "Please enter a Send Threshold Value (tℏ)");
                }
            }
            else
            {
                _input.SendThresholdCreateRecord = null;
            }
            if (_input.UpdateReceiveThresholdCreateRecord)
            {
                somethingIsSelected = true;
                if (!_input.ReceiveThresholdCreateRecord.HasValue)
                {
                    AddIfNoOtherErrors(nameof(_input.ReceiveThresholdCreateRecord), "Please enter a Receive Threshold Value (tℏ)");
                }
            }
            if (_input.UpdateReceiveSignatureRequired)
            {
                somethingIsSelected = true;
            }
            if (!somethingIsSelected)
            {
                _validationMessages.Add(new FieldIdentifier(_input, string.Empty), "Nothing has been selected to change.");
            }
        }

        private void AddIfNoOtherErrors(string fieldName, string message)
        {
            var field = _editContext.Field(fieldName);
            if (!_editContext.GetValidationMessages(field).Any())
            {
                _validationMessages.Add(field, message);
            }
        }

        protected async Task HandleValidSubmit()
        {
            _output = null;
            await _network.ExecuteAsync(_input.Gateway, _input.Payer, async client =>
            {
                var updateParams = new UpdateAccountParams
                {
                    Address = _input.Address
                };
                if (_input.UpdateEndorsement)
                {
                    updateParams.Endorsement = _input.Endorsement;
                }
                if (_input.UpdateSendThresholdCreateRecord)
                {
                    updateParams.SendThresholdCreateRecord = (ulong)_input.SendThresholdCreateRecord.Value;
                }
                if (_input.UpdateReceiveThresholdCreateRecord)
                {
                    updateParams.ReceiveThresholdCreateRecord = (ulong)_input.ReceiveThresholdCreateRecord.Value;
                }
                if (_input.UpdateReceiveSignatureRequired)
                {
                    updateParams.RequireReceiveSignature = _input.ReceiveSignatureRequired;
                }
                _output = await client.UpdateAccountAsync(updateParams, ctx => ctx.Memo = _input?.Memo.Trim());
            });
        }
    }
    public class UpdateAccountInput
    {
        [Required(ErrorMessage = "Please select a Network Gateway Node.")]
        public Gateway Gateway { get; set; }
        [Required(ErrorMessage = "Please enter the account that will pay the Update Transaction Fees.")]
        public Address Payer { get; set; }
        [Required(ErrorMessage = "Please enter the account you wish to update.")]
        public Address Address { get; set; }
        public bool UpdateEndorsement { get; set; }
        public Endorsement Endorsement { get; set; }
        public bool UpdateSendThresholdCreateRecord { get; set; }
        [Range(1, ulong.MaxValue / 2)]
        public long? SendThresholdCreateRecord { get; set; }
        public bool UpdateReceiveThresholdCreateRecord { get; set; }
        [Range(1, ulong.MaxValue / 2)]
        public long? ReceiveThresholdCreateRecord { get; set; }
        public bool UpdateReceiveSignatureRequired { get; set; }
        public bool ReceiveSignatureRequired { get; set; }
        [MaxLength(100, ErrorMessage = "The memo field cannot exceed 100 characters.")]
        public string Memo { get; set; }
    }
}
