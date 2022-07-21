using Hashgraph.Components.Models;
using Hashgraph.Portal.Components;
using Hashgraph.Portal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.ComponentModel.DataAnnotations;

namespace Hashgraph.Portal.Pages;

public partial class TransferCrypto : ComponentBase
{
    [Inject] public DefaultsService DefaultsService { get; set; } = default!;

    private Network _network = default!;
    private EditContext _editContext = default!;
    private ValidationMessageStore _validationMessages = default!;
    private TransferCryptoInput _input = new TransferCryptoInput();
    private TransactionReceipt? _output = null;
    private TransactionRecord? _record = null;

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

    private void OnValidationRequested(object? sender, ValidationRequestedEventArgs e)
    {
        _validationMessages.Clear();
        if (_input.CryptoTransfers is null && _input.TokenTransfers.Count == 0 && _input.AssetTransfers.Count == 0)
        {
            _validationMessages.Add(new FieldIdentifier(_input, string.Empty), "Please add a Crypto, Token or Asset Transfer.");
        }
    }

    protected async Task HandleValidSubmit()
    {
        _output = null;
        _record = null;
        await _network.ExecuteAsync(_input.Gateway!, _input.Payer!, async client =>
        {
            var tokenTransfers = _input.TokenTransfers.ToTransferList().ToArray();
            var assetTransfers = _input.AssetTransfers.ToAssetTransferList().ToArray();
            var cryptoTransfers = _input.CryptoTransfers?.ToCryptoTransferList();
            _output = await client.TransferAsync(new TransferParams
            {
                CryptoTransfers = cryptoTransfers?.Length > 0 ? cryptoTransfers : null,
                TokenTransfers = tokenTransfers.Length > 0 ? tokenTransfers : null,
                AssetTransfers = assetTransfers.Length > 0 ? assetTransfers : null,
            },
            ctx => ctx.Memo = _input.Memo?.Trim());
        });
    }

    private void AddCryptoTransfer()
    {
        if (_input.CryptoTransfers is null)
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

    private void AddAssetTransferGroup()
    {
        _input.AssetTransfers.Add(new AssetTransferGroup());
        _editContext.Validate();
    }

    private void RemoveTokenTransferGroup(TokenTransferGroup group)
    {
        _input.TokenTransfers.Remove(group);
        _editContext.NotifyValidationStateChanged();
    }
    private void RemoveAssetTransferGroup(AssetTransferGroup group)
    {
        _input.AssetTransfers.Remove(group);
        _editContext.NotifyValidationStateChanged();
    }

    private void RemoveCryptoTransfers()
    {
        _input.CryptoTransfers = null;
        _editContext.NotifyValidationStateChanged();
    }

    private async Task GetRecord()
    {
        _record = await _network.GetTransactionRecordAsync(_output!.Id);
    }
}
public class TransferCryptoInput
{
    [Required(ErrorMessage = "Please select a Network Gateway Node.")]
    public Gateway? Gateway { get; set; }
    [Required(ErrorMessage = "Please enter the account that will pay the Transaction Fees.")]
    public Address? Payer { get; set; }
    public CryptoTransferList? CryptoTransfers { get; set; }
    public TokenTransferList TokenTransfers { get; } = new TokenTransferList();
    public AssetTransferList AssetTransfers { get; } = new AssetTransferList();
    [MaxLength(100, ErrorMessage = "The memo field cannot exceed 100 characters.")]
    public string? Memo { get; set; }
}