using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Hashgraph.Components;

public class HbarDisplay : ComponentBase
{
    [Parameter] [EditorRequired] public ulong Value { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement("span");
        builder.AddAttribute("hg-balance-display");
        builder.AddMultipleAttributes(AdditionalAttributes);
        if (Value == 0)
        {
            builder.AddContent($"0 tℏ");
        }
        else if (Value < 1_000_000)
        {
            builder.AddContent($"{Value:#,#} tℏ");
        }
        else
        {
            var hbar = Value / 100_000_000;
            var tbar = Value % 100_000_000;
            if (tbar == 0)
            {
                builder.AddContent($"{hbar:N0} ℏ");
            }
            else
            {
                var dec = $"{tbar:D8}".TrimEnd('0');
                builder.AddContent($"{hbar:N0}.{dec} ℏ");
            }
        }
        builder.CloseElement();
    }
}

