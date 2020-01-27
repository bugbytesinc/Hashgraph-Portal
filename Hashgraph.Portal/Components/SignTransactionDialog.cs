#pragma warning disable CA1031
using Hashgraph.Portal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Hashgraph.Portal.Components
{
    public partial class SignTransactionDialog : ComponentBase
    {
        [Inject] public IJSRuntime Runtime { get; set; }
        private SigningInput _input = null;
        private TaskCompletionSource<bool> _taskCompletionSource = null;
        private bool _supportsClipboard = true;
        public Task<bool> PromptForSignaturesAsync(IInvoice invoice)
        {
            if (invoice is null)
            {
                throw new ArgumentNullException(nameof(invoice));
            }
            _input = new SigningInput()
            {
                Invoice = invoice,
                TransactionInHex = Hex.FromBytes(invoice.TxBytes),
                StatusMessage = "Waiting for signature(s)...",
            };
            _taskCompletionSource = new TaskCompletionSource<bool>();
            StartCountDown();
            StateHasChanged();
            return _taskCompletionSource.Task;
        }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            _supportsClipboard = await Runtime.InvokeAsync<bool>("window.hashgraph.supportsClipboard");
            await base.OnAfterRenderAsync(firstRender);
        }
        private async Task CopyTransactionToClipboard()
        {            
            await Runtime.InvokeVoidAsync("navigator.clipboard.writeText", _input.TransactionInHex);
        }
        private async Task PasteSignaturesFromClipboard()
        {
            _input.SignatureInHex = await Runtime.InvokeAsync<string>("navigator.clipboard.readText");
            TryParseSignature();
        }
        private async Task SignatureInHexChanged(ChangeEventArgs evt)
        {
            var captured = _input.SignatureInHex = evt.Value?.ToString();
            await Task.Delay(600);
            if (captured == _input.SignatureInHex)
            {
                TryParseSignature();
            }
        }
        private void TryParseSignature()
        {
            if (!String.IsNullOrWhiteSpace(_input.SignatureInHex))
            {
                ReadOnlyMemory<byte> bytes;
                try
                {
                    bytes = Hex.ToBytes(_input.SignatureInHex);
                }
                catch
                {
                    _input.StatusMessage = "Text does not appear to be formatted in Hex.";
                    _input.SignatureMap = null;
                    return;
                }
                if (bytes.IsEmpty)
                {
                    _input.StatusMessage = "There appears to be no transaction data on in the text.";
                    _input.SignatureMap = null;
                    return;
                }
                try
                {
                    _input.SignatureMap = Proto.SignatureMap.Parser.ParseFrom(bytes.ToArray());
                    _input.StatusMessage = _input.SignatureMap.SigPair.Count switch
                    {
                        0 => "Are you sure you wish to submit without a signature?",
                        1 => "Please Confirm the Signature before continuing...",
                        _ => "Please Confirm the Signatures before continuing..."
                    };
                    return;
                }
                catch (Exception ex)
                {
                    _input.SignatureMap = null;
                    _input.StatusMessage = $"Unable to Validate Transaction Format: {ex.Message}";
                    return;
                }
            }
            _input.StatusMessage = "Waiting for signature(s)...";
            _input.SignatureMap = null;
            return;
        }
        private void SubmitToNetwork()
        {
            TryParseSignature();
            if (_input.SignatureMap != null)
            {
                foreach (var signature in _input.SignatureMap.SigPair)
                {
                    switch (signature.SignatureCase)
                    {
                        case Proto.SignaturePair.SignatureOneofCase.Ed25519:
                            _input.Invoice.AddSignature(KeyType.Ed25519, signature.PubKeyPrefix.ToByteArray(), signature.Ed25519.ToByteArray());
                            break;
                        case Proto.SignaturePair.SignatureOneofCase.RSA3072:
                            _input.Invoice.AddSignature(KeyType.RSA3072, signature.PubKeyPrefix.ToByteArray(), signature.RSA3072.ToByteArray());
                            break;
                        case Proto.SignaturePair.SignatureOneofCase.ECDSA384:
                            _input.Invoice.AddSignature(KeyType.ECDSA384, signature.PubKeyPrefix.ToByteArray(), signature.ECDSA384.ToByteArray());
                            break;
                    }
                }
                _input.CountDownTimer?.Dispose();
                _input.CountDownTimer = null;
                _input = null;
                _taskCompletionSource.SetResult(true);
                _taskCompletionSource = null;
                StateHasChanged();
            }
        }
        private void Close()
        {
            _input.CountDownTimer?.Dispose();
            _input.CountDownTimer = null;
            _input = null;
            _taskCompletionSource.SetException(new OperationCanceledException("Transaction Canceled by User."));
            _taskCompletionSource = null;
            StateHasChanged();
        }
        private void StartCountDown()
        {
            var transactionBody = Proto.TransactionBody.Parser.ParseFrom(_input.Invoice.TxBytes.ToArray());
            var timestamp = Util.ToDate(transactionBody.TransactionID);
            var expiration = timestamp.AddSeconds(transactionBody.TransactionValidDuration.Seconds);            
            _input.RemainingSeconds = Math.Max((int)(expiration - DateTime.UtcNow).TotalSeconds, 0);
            _input.CountDownTimer = new Timer((_) => {
                InvokeAsync(() =>
                {
                    var input = _input;
                    if (input != null)
                    {
                        if (input.RemainingSeconds > 0)
                        {
                            input.RemainingSeconds = Math.Max((int)(expiration - DateTime.UtcNow).TotalSeconds, 0);
                            StateHasChanged();
                        }
                        else
                        {
                            input.CountDownTimer?.Dispose();
                            input.CountDownTimer = null;
                        }
                    }
                });
            }, null, 0, 1000);
        }
    }
    public class SigningInput
    {
        public IInvoice Invoice { get; set; }
        public Timer CountDownTimer { get; set; }
        public string TransactionInHex { get; set; }
        public string SignatureInHex { get; set; }
        public string StatusMessage { get; set; }
        public Proto.SignatureMap SignatureMap { get; set; }
        public int RemainingSeconds { get; set; }
    }
}
