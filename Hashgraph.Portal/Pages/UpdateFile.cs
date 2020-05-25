#pragma warning disable CA1819
using Hashgraph.Portal.Components;
using Hashgraph.Portal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Hashgraph.Portal.Pages
{
    public partial class UpdateFile : ComponentBase
    {
        [Inject] public DefaultsService DefaultsService { get; set; }

        private Network _network = null;
        private EditContext _editContext = null;
        private ValidationMessageStore _validationMessages = null;
        private UpdateFileInput _input = new UpdateFileInput();
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
                _input.UpdateAdministrators ||
                _input.UpdateContents;
            if (!somethingIsSelected)
            {
                _validationMessages.Add(new FieldIdentifier(_input, string.Empty), "Nothing has been selected to change.");
            }
        }

        protected async Task HandleValidSubmit()
        {
            _output = null;
            await _network.ExecuteAsync(_input.Gateway, _input.Payer, async client =>
            {
                var updateParams = new UpdateFileParams
                {
                    File = _input.File
                };
                if (_input.UpdateAdministrators)
                {
                    updateParams.Endorsements = _input.Administrators == null ? Array.Empty<Endorsement>() : _input.Administrators;
                }
                if (_input.UpdateContents)
                {
                    updateParams.Contents = _input.Contents;
                }
                _output = await client.UpdateFileAsync(updateParams, ctx => ctx.Memo = _input.Memo?.Trim());
            });
        }
    }
    public class UpdateFileInput
    {
        [Required(ErrorMessage = "Please select a Network Gateway Node.")]
        public Gateway Gateway { get; set; }
        [Required(ErrorMessage = "Please enter the Account that will pay the Update Transaction Fees.")]
        public Address Payer { get; set; }
        [Required(ErrorMessage = "Please enter the File you wish to update.")]
        public Address File { get; set; }
        public bool UpdateAdministrators { get; set; }
        public Endorsement[] Administrators { get; set; }
        public bool UpdateContents { get; set; }
        public ReadOnlyMemory<byte> Contents { get; set; }
        [MaxLength(100, ErrorMessage = "The memo field cannot exceed 100 characters.")]
        public string Memo { get; set; }
    }
}
