using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Hashgraph.Components;

public class MonikerDisplay : ComponentBase
{
    [Parameter][EditorRequired] public Moniker? Value { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var moniker = Value;
        builder.OpenElement("span");
        builder.AddMultipleAttributes(AdditionalAttributes);
        if (moniker is null || Address.None.Equals(moniker))
        {
            builder.AddAttribute("hg-moniker-display");
            builder.AddAttribute("hg-none");
            builder.AddContent("None");
        }
        else
        {
            builder.AddAttribute("hg-moniker-display");
            builder.AddMultipleAttributes(AdditionalAttributes);
            builder.AddContent($"{moniker.ShardNum}.{moniker.RealmNum}.");
            builder.AddContent(Hex.FromBytes(moniker.Bytes));
        }
        builder.CloseElement();
    }
}