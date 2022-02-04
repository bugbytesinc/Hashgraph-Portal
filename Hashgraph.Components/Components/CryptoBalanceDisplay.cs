using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Hashgraph.Components;

public class CryptoBalanceDisplay : ComponentBase
{
    [Parameter] [EditorRequired] public CryptoBalance? Value { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement("span");
        builder.AddAttribute("hg-crypto-balance-display");
        var amount = Value;
        if (amount is null)
        {
            builder.AddAttribute("hg-none");
            builder.AddMultipleAttributes(AdditionalAttributes);
        }
        else
        {
            builder.AddMultipleAttributes(AdditionalAttributes);
            if (amount.Decimals > 0)
            {
                var places = (ulong)Math.Pow(10, amount.Decimals);
                var whole = amount.Balance / places;
                var fraction = amount.Balance % places;
                if (fraction == 0)
                {
                    builder.AddContent(whole.ToString("N0"));
                }
                else
                {
                    var dec = fraction.ToString($"D{amount.Decimals}").TrimEnd('0');
                    builder.AddContent($"{whole:N0}.{dec}");
                }
            }
            else
            {
                builder.AddContent(amount.Balance.ToString("N0"));
            }
        }
        builder.CloseElement();
    }
}

