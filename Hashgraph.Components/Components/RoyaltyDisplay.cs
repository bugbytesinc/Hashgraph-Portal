using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Hashgraph.Components;

public class RoyaltyDisplay : ComponentBase
{
    [Parameter] [EditorRequired] public IRoyalty? Value { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement("span");
        builder.AddAttribute("hg-royalty-display");
        if(Value is null)
        {
            builder.AddAttribute("hg-none");
            builder.AddMultipleAttributes(AdditionalAttributes);
            builder.AddContent("None");
        }
        else
        {
            switch(Value)
            {
                case FixedRoyalty fixedRoyalty:
                    builder.AddAttribute("hg-fixed-royalty-display");
                    builder.AddMultipleAttributes(AdditionalAttributes);
                    if (fixedRoyalty.Token.IsNullOrNone())
                    {
                        builder.AddContent($"Pay Fixed ");
                        builder.OpenComponent<HbarDisplay>();
                        builder.AddAttribute("Value", (ulong)fixedRoyalty.Amount);
                        builder.CloseComponent();
                    }
                    else
                    {
                        builder.AddContent($"Pay Fixed {fixedRoyalty.Amount.ToString("N0")} of ");
                        builder.OpenComponent<AddressDisplay>();
                        builder.AddAttribute("Value", fixedRoyalty.Token);
                        builder.CloseComponent();
                    }
                    builder.AddContent(" to ");
                    builder.OpenComponent<AddressDisplay>();
                    builder.AddAttribute("Value", fixedRoyalty.Account);
                    builder.CloseComponent();
                    break;
                case TokenRoyalty tokenRoyalty:
                    builder.AddAttribute("hg-token-royalty-display");
                    builder.AddMultipleAttributes(AdditionalAttributes);
                    if (tokenRoyalty.AssessAsSurcharge)
                    {
                        builder.AddContent($"Pay an additional {tokenRoyalty.Numerator}/{tokenRoyalty.Denominator} fraction of token payment to ");
                    }
                    else
                    {
                        builder.AddContent($"Pay {tokenRoyalty.Numerator}/{tokenRoyalty.Denominator} fraction taken from token payment to ");
                    }
                    builder.OpenComponent<AddressDisplay>();
                    builder.AddAttribute("Value", tokenRoyalty.Account);
                    builder.CloseComponent();
                    if(tokenRoyalty.Minimum > 0)
                    {
                        builder.AddContent($", with a minimum payment of {tokenRoyalty.Minimum}");
                    }
                    if (tokenRoyalty.Maximum > 0)
                    {
                        builder.AddContent($", not to exceed {tokenRoyalty.Maximum}");
                    }
                    break;
                case AssetRoyalty assetRoyalty:
                    builder.AddAttribute("hg-asset-royalty-display");
                    builder.AddMultipleAttributes(AdditionalAttributes);
                    builder.AddContent($"Pay {assetRoyalty.Numerator}/{assetRoyalty.Denominator} fraction of payment for asset to ");
                    builder.OpenComponent<AddressDisplay>();
                    builder.AddAttribute("Value", assetRoyalty.Account);
                    builder.CloseComponent();
                    if(assetRoyalty.FallbackAmount > 0)
                    {
                        if(assetRoyalty.FallbackToken.IsNullOrNone())
                        {
                            builder.AddContent($", or ");
                            builder.OpenComponent<HbarDisplay>();
                            builder.AddAttribute("Value", (ulong)assetRoyalty.FallbackAmount);
                            builder.CloseComponent();
                        }
                        else
                        {
                            builder.AddContent($", or {assetRoyalty.FallbackAmount:N0} of ");
                            builder.OpenComponent<AddressDisplay>();
                            builder.AddAttribute("Value", assetRoyalty.FallbackToken);
                            builder.CloseComponent();
                        }
                        builder.AddContent(" if no payment for transfer of Asset");
                    }
                    break;
            }
        }
        builder.CloseElement();
    }
}