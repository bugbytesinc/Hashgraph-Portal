using Hashgraph.Portal.Components;
using Hashgraph.Portal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hashgraph.Portal.Pages
{
    public partial class SendMessage : ComponentBase
    {
        [Inject] public DefaultsService DefaultsService { get; set; }

        private Network _network = null;
        private EditContext _editContext = null;
        private ValidationMessageStore _validationMessages = null;
        private SubmitMessageInput _input = new SubmitMessageInput();
        private SubmitMessageReceipt _output = null;

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
            if (_input.MessageIsHex)
            {
                try
                {
                    Hex.ToBytes(_input.Message);
                }
                catch (ArgumentException)
                {
                    AddIfNoOtherErrors(nameof(_input.Message), "Unable to parse message as hex.");
                }
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
                var message = _input.MessageIsHex ? Hex.ToBytes(_input.Message) : Encoding.UTF8.GetBytes(_input.Message);
                _output = await client.SubmitMessageAsync(_input.Topic, message, ctx => ctx.Memo = _input.Memo?.Trim());
            });
        }
    }
    public class SubmitMessageInput
    {
        [Required(ErrorMessage = "Please select a Network Gateway Node.")]
        public Gateway Gateway { get; set; }
        [Required(ErrorMessage = "Please enter the account that will pay the Transaction Fees.")]
        public Address Payer { get; set; }
        [Required(ErrorMessage = "Please enter the topic address.")]
        public Address Topic { get; set; }
        public bool MessageIsHex { get; set; }
        [Required(ErrorMessage = "Please enter the topic message.")]
        public string Message { get; set; }
        [MaxLength(100, ErrorMessage = "The memo field cannot exceed 100 characters.")]
        public string Memo { get; set; }
    }
}
