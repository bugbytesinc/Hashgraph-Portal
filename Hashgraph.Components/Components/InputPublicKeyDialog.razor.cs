using Hashgraph.Components.Services;
using Microsoft.AspNetCore.Components;

namespace Hashgraph.Components
{
    public partial class InputPublicKeyDialog : ComponentBase
    {
        [Inject] public ClipboardService ClipboardService { get; set; } = default!;
        private PublicKeyInput _input = default!;
        private TaskCompletionSource<Endorsement?> _taskCompletionSource = default!;
        public Task<Endorsement?> PromptForPublicKey()
        {
            _input = new PublicKeyInput()
            {
                Type = KeyType.Ed25519,
                KeyInHex = string.Empty
            };
            _taskCompletionSource = new TaskCompletionSource<Endorsement?>();
            StateHasChanged();
            return _taskCompletionSource.Task;
        }
        private async Task PastePublicKeyFromClipboard()
        {
            _input.KeyInHex = await ClipboardService.ReadFromClipboardAsync();
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
            if (!string.IsNullOrWhiteSpace(_input.KeyInHex))
            {
                if (TryParseAddress(_input.KeyInHex, out Address contract))
                {
                    _input.Endorsement = new Endorsement(contract);
                    _input.Type = _input.Endorsement.Type;
                    _input.StatusMessage = "Contract Public Key (Address) Recognized.";
                    return;
                }
                else
                {
                    ReadOnlyMemory<byte> bytes;
                    try
                    {
                        bytes = Hex.ToBytes(_input.KeyInHex);
                    }
                    catch
                    {
                        _input.StatusMessage = "Text does not appear to be formatted in Hex, nor is it a contract address.";
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
                        try
                        {
                            // Try DER Parse First
                            _input.Endorsement = new Endorsement(bytes);
                            // If that worked, update the type
                            _input.Type = _input.Endorsement.Type;
                        }
                        catch
                        {
                            // Else, it did not work, try to parse as 
                            // the type from the dropdown.
                            _input.Endorsement = new Endorsement(_input.Type, bytes);
                        }
                        _input.StatusMessage = _input.Type switch
                        {
                            KeyType.Ed25519 => "Ed25519 Public Key Recognized.",
                            KeyType.ECDSASecp256K1 => "ECDSA (Secp256K1) Public Key Recognized.",
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
            }
            _input.StatusMessage = "Please enter or paste a public key...";
            _input.Endorsement = null;
            return;
        }

        private bool TryParseAddress(string value, out Address address)
        {
            var parts = value.Split('.');
            if (parts.Length == 3)
            {
                if (uint.TryParse(parts[0], out uint shard) &&
                    uint.TryParse(parts[1], out uint realm) &&
                    uint.TryParse(parts[2], out uint number))
                {
                    address = new Address(shard, realm, number);
                    return true;
                }
            }
            address = Address.None;
            return false;
        }

        private void Submit()
        {
            TryParseKey();
            if (_input.Endorsement is not null)
            {
                _taskCompletionSource.SetResult(_input.Endorsement);
                _taskCompletionSource = default!;
                _input = default!;
                StateHasChanged();
            }
        }
        private void Close()
        {
            _input = default!;
            _taskCompletionSource.SetResult(null);
            _taskCompletionSource = default!;
            StateHasChanged();
        }
    }
    public class PublicKeyInput
    {
        public KeyType Type { get; set; }
        public string? KeyInHex { get; set; }
        public string? StatusMessage { get; set; }
        public Endorsement? Endorsement { get; set; }
    }
}
