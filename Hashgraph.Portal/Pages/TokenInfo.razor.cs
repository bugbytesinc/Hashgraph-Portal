using Hashgraph.Portal.Components;
using Hashgraph.Portal.Services;
using Microsoft.AspNetCore.Components;
using System.ComponentModel.DataAnnotations;

namespace Hashgraph.Portal.Pages;

public partial class TokenInfo : ComponentBase
{
    [Inject] public DefaultsService DefaultsService { get; set; } = default!;

    private Network _network = default!;
    private TokenInfoInput _input = new TokenInfoInput();
    private Hashgraph.TokenInfo? _output = null;

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
            _output = await client.GetTokenInfoAsync(_input.Token!, ctx => ctx.Memo = _input.Memo?.Trim());
        });
    }

    protected static string FormatWithDecimals(ulong value, long decimals)
    {
        if(decimals > 0)
        {
            var places = (ulong)Math.Pow(10, decimals);
            var whole = value / places;
            var fraction = value % places;
            var dec = fraction.ToString($"D{decimals}");
            return $"{whole:N0}.{dec}";
        }
        return value.ToString("N0");
    }
}
public class TokenInfoInput
{
    [Required(ErrorMessage = "Please select a Network Gateway Node.")]
    public Gateway? Gateway { get; set; }
    [Required(ErrorMessage = "Please enter the account that will pay the Query Transaction Fees.")]
    public Address? Payer { get; set; }
    [Required(ErrorMessage = "Please enter the token address you wish to Query.")]
    public Address? Token { get; set; }
    [MaxLength(100, ErrorMessage = "The memo field cannot exceed 100 characters.")]
    public string? Memo { get; set; }
}
