using Hashgraph.Portal.Components;
using Hashgraph.Portal.Services;
using Microsoft.AspNetCore.Components;
using System.ComponentModel.DataAnnotations;

namespace Hashgraph.Portal.Pages;

public partial class AssetInfo : ComponentBase
{
    [Inject] public DefaultsService DefaultsService { get; set; } = default!;

    private Network _network = default!;
    private AssetInfoInput _input = new AssetInfoInput();
    private Hashgraph.AssetInfo? _output = null;

    protected override void OnInitialized()
    {
        _input.Payer = DefaultsService.Payer;
        _input.Gateway = DefaultsService.Gateway;
        base.OnInitialized();
    }
    protected async Task HandleValidSubmit()
    {
        _output = null;
        await _network.ExecuteAsync(_input.Gateway!, _input.Payer, async client =>
        {
            _output = await client.GetAssetInfoAsync(new Asset(_input.Token!, _input.SerialNumber.GetValueOrDefault()), ctx => ctx.Memo = _input.Memo?.Trim());
        });
    }
}
public class AssetInfoInput
{
    [Required(ErrorMessage = "Please select a Network Gateway Node.")]
    public Gateway? Gateway { get; set; }
    [Required(ErrorMessage = "Please enter the account that will pay the Query Transaction Fees.")]
    public Address? Payer { get; set; }
    [Required(ErrorMessage = "Please enter the assets token address you wish to Query.")]
    public Address? Token { get; set; }
    [Required(ErrorMessage = "Please enter the asset token's serial number you wish to Query.")]
    [Range(0, long.MaxValue, ErrorMessage = "The serial number must be greater than or equal to zero.")]
    public long? SerialNumber { get; set; }
    [MaxLength(100, ErrorMessage = "The memo field cannot exceed 100 characters.")]
    public string? Memo { get; set; }
}
