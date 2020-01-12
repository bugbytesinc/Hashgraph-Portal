using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Collections.Generic;

namespace Hashgraph.Portal.Components
{
    public class BalanceDisplay : ComponentBase
    {
        [Parameter] public ulong Balance { get; set; }
        [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object> AdditionalAttributes { get; set; }
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (builder != null)
            {
                builder.OpenElement(0, "span");
                builder.AddMultipleAttributes(1, AdditionalAttributes);
                if (Balance < 1_000_000)
                {
                    builder.AddContent(2, $"{Balance:#,#} tℏ");
                }
                else
                {
                    var hbar = Balance / 100_000_000;
                    var tbar = Balance % 100_000_000;
                    if(tbar == 0 )
                    {
                        builder.AddContent(2, $"{hbar:N0} ℏ");
                    }
                    else
                    {
                        var dec = $"{tbar:D8}".TrimEnd('0');
                        builder.AddContent(2, $"{hbar:N0}.{dec} ℏ");
                    }
                }
                builder.CloseElement();
            }
        }
    }
}