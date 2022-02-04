using Hashgraph.Components.Services;
using Hashgraph.Portal.Services;
using Microsoft.AspNetCore.Components;
using Proto;

namespace Hashgraph.Portal.Components;

public partial class SignTransactionDialog : ComponentBase
{
    [Inject] public ClipboardService ClipboardService { get; set; } = default!;
    private SigningInput _input = default!;
    private TaskCompletionSource<bool> _taskCompletionSource = default!;
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
    private async Task CopyTransactionToClipboard()
    {
        await ClipboardService.WriteToClipboardAsync(_input.TransactionInHex);
    }
    private async Task PasteSignaturesFromClipboard()
    {
        _input.SignatureInHex = await ClipboardService.ReadFromClipboardAsync();
        TryParseSignature();
    }
    private async Task SignatureInHexChanged(ChangeEventArgs evt)
    {
        var captured = _input.SignatureInHex = evt.Value?.ToString() ?? string.Empty;
        await Task.Delay(600);
        if (captured == _input.SignatureInHex)
        {
            TryParseSignature();
        }
    }
    private void TryParseSignature()
    {
        if (!string.IsNullOrWhiteSpace(_input.SignatureInHex))
        {
            ReadOnlyMemory<byte> bytes;
            try
            {
                bytes = Hex.ToBytes(_input.SignatureInHex);
            }
            catch
            {
                _input.StatusMessage = "Text does not appear to be formatted in Hex.";
                _input.PendingSignatureMap = null;
                return;
            }
            if (bytes.IsEmpty)
            {
                _input.StatusMessage = "There appears to be no transaction data on in the text.";
                _input.PendingSignatureMap = null;
                return;
            }
            try
            {
                _input.PendingSignatureMap = Proto.SignatureMap.Parser.ParseFrom(bytes.ToArray());
                foreach (var signature in _input.PendingSignatureMap.SigPair)
                {
                    switch (signature.SignatureCase)
                    {
                        case SignaturePair.SignatureOneofCase.Ed25519:
                        case SignaturePair.SignatureOneofCase.ECDSASecp256K1:
                        case SignaturePair.SignatureOneofCase.Contract:
                            continue;
                        default:
                            _input.PendingSignatureMap = null;
                            _input.StatusMessage = $"One or more signatures are of an unrecognized type.";
                            return;
                    }
                }
                // Check for inconsistent duplicates of previously
                // confirmed signatures.
                if (_input.ConfirmedSignatureMap is not null)
                {
                    foreach (var signature in _input.PendingSignatureMap.SigPair)
                    {
                        var existing = _input.ConfirmedSignatureMap.SigPair.FirstOrDefault(other => other.PubKeyPrefix.Equals(signature.PubKeyPrefix));
                        if (existing != null)
                        {
                            if (!existing.Equals(signature))
                            {
                                _input.PendingSignatureMap = null;
                                _input.StatusMessage = $"One or more signatures have conflicting public key prefix values (are duplicates, at least one is bad).";
                                return;
                            }
                        }
                    }
                }
                // Examine the current proposed list of
                // signatures for inconsistent duplicates
                var groupings = _input.PendingSignatureMap.SigPair.GroupBy(sig => sig.PubKeyPrefix);
                foreach (var grouping in groupings.Where(g => g.Count() > 1))
                {
                    var asArray = grouping.ToArray();
                    for (int i = 1; i < asArray.Length; i++)
                    {
                        if (!asArray[0].Equals(asArray[i]))
                        {
                            _input.PendingSignatureMap = null;
                            _input.StatusMessage = $"One or more signatures have conflicting public key prefix values (are duplicates, at least one is bad).";
                            return;
                        }
                    }
                }
                // Everything checks out if
                // we get this far.
                _input.StatusMessage = _input.PendingSignatureMap.SigPair.Count switch
                {
                    0 => "Are you sure you wish to submit without a signature?",
                    1 => "Please Confirm the Signature before continuing...",
                    _ => "Please Confirm the Signatures before continuing..."
                };
                return;
            }
            catch (Exception ex)
            {
                _input.PendingSignatureMap = null;
                _input.StatusMessage = $"Unable to Validate Transaction Format: {ex.Message}";
                return;
            }
        }
        _input.StatusMessage = "Waiting for signature(s)...";
        _input.PendingSignatureMap = null;
        return;
    }
    private void AddMoreSignatures()
    {
        if (_input.PendingSignatureMap is not null)
        {
            if (_input.ConfirmedSignatureMap is null)
            {
                _input.ConfirmedSignatureMap = _input.PendingSignatureMap;
            }
            else
            {
                _input.ConfirmedSignatureMap.SigPair.AddRange(_input.PendingSignatureMap.SigPair);
            }
            _input.StatusMessage = "Waiting for additional signature(s)...";
            _input.SignatureInHex = string.Empty;
            _input.PendingSignatureMap = null;
        }
    }
    private void SubmitToNetwork()
    {
        TryParseSignature();
        try
        {
            if (_input.PendingSignatureMap is not null || (string.IsNullOrWhiteSpace(_input.SignatureInHex) && _input.ConfirmedSignatureMap != null))
            {
                if (_input.ConfirmedSignatureMap is not null)
                {
                    foreach (var signature in _input.ConfirmedSignatureMap.SigPair)
                    {
                        AddSignatureToInvoice(signature);
                    }
                }
                if (_input.PendingSignatureMap is not null)
                {
                    foreach (var signature in _input.PendingSignatureMap.SigPair)
                    {
                        AddSignatureToInvoice(signature);
                    }
                }
                _input.CountDownTimer?.Dispose();
                _input.CountDownTimer = null;
                _input = default!;
                _taskCompletionSource.SetResult(true);
                _taskCompletionSource = default!;
                StateHasChanged();
            }
        }
        catch (Exception ex)
        {
            // Reset the whole dialog
            _input.SignatureInHex = string.Empty;
            _input.StatusMessage = "An Error Occurred and signatures have been cleared: " + ex.Message;
            _input.ConfirmedSignatureMap = null;
            _input.PendingSignatureMap = null;
        }
    }
    private void AddSignatureToInvoice(SignaturePair signature)
    {
        switch (signature.SignatureCase)
        {
            case SignaturePair.SignatureOneofCase.Ed25519:
                _input.Invoice.AddSignature(KeyType.Ed25519, signature.PubKeyPrefix.ToByteArray(), signature.Ed25519.ToByteArray());
                break;
            case SignaturePair.SignatureOneofCase.ECDSASecp256K1:
                _input.Invoice.AddSignature(KeyType.ECDSASecp256K1, signature.PubKeyPrefix.ToByteArray(), signature.ECDSASecp256K1.ToByteArray());
                break;
            case SignaturePair.SignatureOneofCase.Contract:
                _input.Invoice.AddSignature(KeyType.Contract, signature.PubKeyPrefix.ToByteArray(), signature.Contract.ToByteArray());
                break;
        }
    }

    private void Close()
    {
        _input.CountDownTimer?.Dispose();
        _input.CountDownTimer = null;
        _input = default!;
        _taskCompletionSource.SetException(new OperationCanceledException("Transaction Canceled by User."));
        _taskCompletionSource = default!;
        StateHasChanged();
    }
    private void StartCountDown()
    {
        var transactionBody = TransactionBody.Parser.ParseFrom(_input.Invoice.TxBytes.ToArray());
        var timestamp = Util.ToDate(transactionBody.TransactionID);
        var expiration = timestamp.AddSeconds(transactionBody.TransactionValidDuration.Seconds);
        _input.RemainingSeconds = Math.Max((int)(expiration - DateTime.UtcNow).TotalSeconds, 0);
        _input.CountDownTimer = new Timer((_) =>
        {
            InvokeAsync(() =>
            {
                var input = _input;
                if (input is not null)
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
    public IInvoice Invoice { get; set; } = default!;
    public Timer? CountDownTimer { get; set; }
    public string TransactionInHex { get; set; } = string.Empty;
    public string SignatureInHex { get; set; } = string.Empty;
    public string StatusMessage { get; set; } = string.Empty;
    public SignatureMap? PendingSignatureMap { get; set; }
    public SignatureMap? ConfirmedSignatureMap { get; set; }
    public int RemainingSeconds { get; set; }
}