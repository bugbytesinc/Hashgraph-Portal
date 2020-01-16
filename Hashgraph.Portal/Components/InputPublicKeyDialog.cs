#pragma warning disable CA1031
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace Hashgraph.Portal.Components
{
    public partial class InputPublicKeyDialog : ComponentBase
    {
        [Inject] public IJSRuntime Runtime { get; set; }
        private PublicKeyInput _input = null;
        private TaskCompletionSource<Endorsement> _taskCompletionSource = null;
        private bool _supportsClipboard = true;
        public Task<Endorsement> PromptForPublicKey()
        {
            _input = new PublicKeyInput()
            {
                Type = KeyType.Ed25519,
                KeyInHex = string.Empty
            };
            _taskCompletionSource = new TaskCompletionSource<Endorsement>();
            StateHasChanged();
            return _taskCompletionSource.Task;
        }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            _supportsClipboard = await Runtime.InvokeAsync<bool>("window.hashgraph.supportsClipboard");
            await base.OnAfterRenderAsync(firstRender);
        }
        private async Task PastePublicKeyFromClipboard()
        {
            _input.KeyInHex = await Runtime.InvokeAsync<string>("navigator.clipboard.readText");
            TryParseKey();
        }
        private async Task KeyInHexChanged(ChangeEventArgs evt)
        {
            var captured = _input.KeyInHex = evt.Value?.ToString();
            await Task.Delay(600);
            if (captured == _input.KeyInHex)
            {
                TryParseKey();
            }
        }
        private void TryParseKey()
        {
            if (!String.IsNullOrWhiteSpace(_input.KeyInHex))
            {
                ReadOnlyMemory<byte> bytes;
                try
                {
                    bytes = Hex.ToBytes(_input.KeyInHex);
                }
                catch
                {
                    _input.StatusMessage = "Text does not appear to be formatted in Hex.";
                    _input.Endorsement = null;
                    return;
                }
                if (bytes.IsEmpty)
                {
                    _input.StatusMessage = "There appears to be no key data on in the text.";
                    _input.Endorsement = null;
                    return;
                }
                try
                {
                    _input.Endorsement = new Endorsement(_input.Type, bytes);
                    _input.StatusMessage = _input.Type switch
                    {
                        KeyType.Ed25519 => "Ed25519 Public Key Recognized.",
                        KeyType.RSA3072 => "RSA 3072 Key Accepted (note software does not know how to validate key).",
                        KeyType.ECDSA384 => "ECDSA 384 Key Accepted (note software does not know how to validate key).",
                        KeyType.ContractID => "Contract Key Accepted (note software does not know how to validate key).",
                        _ => "Generic Key Accepted"
                    };
                    return;
                }
                catch (Exception ex)
                {
                    _input.Endorsement = null;
                    _input.StatusMessage = $"Unable to Parse Key Data: {ex.Message}";
                    return;
                }
            }
            _input.StatusMessage = "Please enter or paste a public key...";
            _input.Endorsement = null;
            return;
        }
        private void Submit()
        {
            TryParseKey();
            if (_input.Endorsement != null)
            {
                _taskCompletionSource.SetResult(_input.Endorsement);
                _taskCompletionSource = null;
                _input = null;
                StateHasChanged();
            }
        }
        private void Close()
        {
            _input = null;
            _taskCompletionSource.SetResult(null);
            _taskCompletionSource = null;
            StateHasChanged();
        }
    }
    public class PublicKeyInput
    {
        public KeyType Type { get; set; }
        public string KeyInHex { get; set; }
        public string StatusMessage { get; set; }
        public Endorsement Endorsement { get; set; }
    }
}
