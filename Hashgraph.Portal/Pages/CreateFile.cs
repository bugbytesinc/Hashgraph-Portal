#pragma warning disable CA1819
#pragma warning disable CA1031
using Hashgraph.Portal.Components;
using Hashgraph.Portal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hashgraph.Portal.Pages
{
    public partial class CreateFile : ComponentBase
    {
        [Inject] public DefaultsService DefaultsService { get; set; }
        [Inject] public ClipboardService ClipboardService { get; set; }

        private Network _network = null;
        private EditContext _editContext = null;
        private ValidationMessageStore _validationMessages = null;
        private CreateFileInput _input = new CreateFileInput();
        private FileReceipt _output = null;

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
            switch (_input.ContentEncoding)
            {
                case FileInputEncoding.Base64:
                    try
                    {
                        Convert.FromBase64String(_input.Content);
                    }
                    catch (FormatException ex)
                    {
                        AddIfNoOtherErrors(nameof(_input.Content), $"Unable to parse content as Base 64: {ex.Message}");
                    }
                    break;
                case FileInputEncoding.Hex:
                    try
                    {
                        Hex.ToBytes(_input.Content);
                    }
                    catch (ArgumentException ex)
                    {
                        AddIfNoOtherErrors(nameof(_input.Content), $"Unable to parse content as HEX: {ex.Message}");
                    }
                    break;
                default:
                    if (string.IsNullOrWhiteSpace(_input.Content))
                    {
                        AddIfNoOtherErrors(nameof(_input.Content), "File content is missing.");
                    }
                    break;
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
            ReadOnlyMemory<byte> contents;
            switch (_input.ContentEncoding)
            {
                case FileInputEncoding.Base64:
                    contents = Convert.FromBase64String(_input.Content);
                    break;
                case FileInputEncoding.Hex:
                    contents = Hex.ToBytes(_input.Content);
                    break;
                default:
                    contents = Encoding.UTF8.GetBytes(_input.Content);
                    break;
            }
            await _network.ExecuteAsync(_input.Gateway, _input.Payer, async client =>
            {
                var createParams = new CreateFileParams
                {
                    Expiration = DateTime.UtcNow.AddSeconds(7890000), // Default enforced by network at the moment
                    Endorsements = _input.Endorsements != null ? _input.Endorsements : Array.Empty<Endorsement>(),
                    Contents = contents
                };
                _output = await client.CreateFileAsync(createParams, ctx => ctx.Memo = _input.Memo?.Trim());
            });
        }
        private async Task PasteContentFromClipboard()
        {
            if (ClipboardService.Enabled)
            {
                _input.Content = await ClipboardService.ReadFromClipboard();
                try
                {
                    Hex.ToBytes(_input.Content);
                    _input.ContentEncoding = FileInputEncoding.Hex;
                    return;
                }
                 catch
                {
                    // noop
                }
                try
                {
                    Convert.FromBase64String(_input.Content);
                    _input.ContentEncoding = FileInputEncoding.Base64;
                    return;
                }
                catch
                {
                    // noop
                }
                _input.ContentEncoding = FileInputEncoding.Text;
            }
        }
    }
    public class CreateFileInput
    {
        [Required(ErrorMessage = "Please select a Network Gateway Node.")]
        public Gateway Gateway { get; set; }
        [Required(ErrorMessage = "Please enter the Account that will pay the Transaction Fees.")]
        public Address Payer { get; set; }
        [Required(ErrorMessage = "Please enter or upload contents for the File.")]
        public string Content { get; set; }
        public FileInputEncoding ContentEncoding { get; set; }
        public Endorsement[] Endorsements { get; set; }
        [MaxLength(100, ErrorMessage = "The memo field cannot exceed 100 characters.")]
        public string Memo { get; set; }
    }
    public enum FileInputEncoding
    {
        Text,
        Hex,
        Base64
    }
}
