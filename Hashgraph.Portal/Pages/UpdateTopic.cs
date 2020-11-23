using Hashgraph.Portal.Components;
using Hashgraph.Portal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Hashgraph.Portal.Pages
{
    public partial class UpdateTopic : ComponentBase
    {
        [Inject] public DefaultsService DefaultsService { get; set; }

        private Network _network = null;
        private EditContext _editContext = null;
        private ValidationMessageStore _validationMessages = null;
        private UpdateTopicInput _input = new UpdateTopicInput();
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
            bool somethingIsSelected =
                _input.UpdateAdministrator ||
                _input.UpdateParticipant ||
                _input.UpdateRenewAccount;
            if (_input.UpdateDescription)
            {
                somethingIsSelected = true;
                if (string.IsNullOrWhiteSpace(_input.Description))
                {
                    AddIfNoOtherErrors(nameof(_input.Description), "Please enter a new description.");
                }
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
                var updateParams = new UpdateTopicParams
                {
                    Topic = _input.Topic
                };
                if (_input.UpdateDescription)
                {
                    updateParams.Memo = _input.Description?.Trim();
                }
                if (_input.UpdateAdministrator)
                {
                    updateParams.Administrator = _input.Administrator ?? Endorsement.None;
                }
                if (_input.UpdateParticipant)
                {
                    updateParams.Participant = _input.Participant ?? Endorsement.None;
                }
                if (_input.UpdateRenewAccount)
                {
                    updateParams.RenewAccount = _input.RenewAccount ?? Address.None;
                }
                _output = await client.UpdateTopicAsync(updateParams, ctx => ctx.Memo = _input.Memo?.Trim());
            });
        }
    }
    public class UpdateTopicInput
    {
        [Required(ErrorMessage = "Please select a Network Gateway Node.")]
        public Gateway Gateway { get; set; }
        [Required(ErrorMessage = "Please enter the Topic that will pay the Update Transaction Fees.")]
        public Address Payer { get; set; }
        [Required(ErrorMessage = "Please enter the Topic you wish to update.")]
        public Address Topic { get; set; }
        public bool UpdateDescription { get; set; }
        [MaxLength(100, ErrorMessage = "The short description/name cannot exceed 100 characters.")]
        public string Description { get; set; }
        public bool UpdateAdministrator { get; set; }
        public Endorsement Administrator { get; set; }
        public bool UpdateParticipant { get; set; }
        public Endorsement Participant { get; set; }
        public bool UpdateRenewAccount { get; set; }
        public Address RenewAccount { get; set; }
        [MaxLength(100, ErrorMessage = "The memo field cannot exceed 100 characters.")]
        public string Memo { get; set; }
    }
}
