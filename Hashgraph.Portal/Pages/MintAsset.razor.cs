using Hashgraph.Portal.Components;
using Hashgraph.Portal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.ComponentModel.DataAnnotations;

namespace Hashgraph.Portal.Pages;

public partial class MintAsset : ComponentBase
{
    [Inject] public DefaultsService DefaultsService { get; set; } = default!;

    private Network _network = default!;
    private EditContext _editContext = default!;
    private MintAssetInput _input = new MintAssetInput();
    private AssetMintReceipt? _output = null;
    private TransactionRecord? _record = null;
    private int assetIdCount = 0;

    protected override void OnInitialized()
    {
        _input.Gateway = DefaultsService.Gateway;
        _input.Payer = DefaultsService.Payer;
        _input.Metadata = new[] { new MintAssetInput.AssetMetadata { id = assetIdCount++ } };
        _editContext = new EditContext(_input);
        base.OnInitialized();
    }

    protected async Task HandleValidSubmit()
    {
        _output = null;
        _record = null;
        var metadata = _input.Metadata.Select(m => m.Data);
        await _network.ExecuteAsync(_input.Gateway!, _input.Payer, async client =>
        {
            _output = await client.MintAssetAsync(_input.Token!, metadata, ctx => ctx.Memo = _input.Memo?.Trim());
        });
    }

    private void RemoveMetadata(int id)
    {
        var newItems = _input.Metadata.Where(item => item.id != id).ToArray();
        _input.Metadata = newItems;
        _editContext.NotifyValidationStateChanged();
    }

    private void AddMetadata()
    {
        var newIems = _input.Metadata.Append(new MintAssetInput.AssetMetadata { id = assetIdCount++ }).ToArray();
        _input.Metadata = newIems;
        _editContext.NotifyValidationStateChanged();
    }
    private async Task GetRecord()
    {
        _record = await _network.GetTransactionRecordAsync(_output!.Id);
    }
}
public class MintAssetInput
{
    [Required(ErrorMessage = "Please select a Network Gateway Node.")]
    public Gateway? Gateway { get; set; }
    [Required(ErrorMessage = "Please enter the account that will pay the Transaction Fees.")]
    public Address? Payer { get; set; }
    [Required(ErrorMessage = "Please enter the token address.")]
    public Address? Token { get; set; }
    [MinLength(1, ErrorMessage = "Metadata for at least one Asset must be specified (it can be blank).")]
    public AssetMetadata[] Metadata { get; set; } = default!;
    [MaxLength(100, ErrorMessage = "The memo field cannot exceed 100 characters.")]
    public string? Memo { get; set; }

    public class AssetMetadata
    {
        public int id { get; set; }
        public ReadOnlyMemory<byte> Data { get; set; }
    }

}
