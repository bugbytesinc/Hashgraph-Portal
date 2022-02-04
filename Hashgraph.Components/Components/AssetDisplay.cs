using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Hashgraph.Components;

public class AssetDisplay : ComponentBase
{
    [Parameter] [EditorRequired] public Asset? Value { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var asset = Value;
        builder.OpenElement("span");
        builder.AddAttribute("hg-asset-display");
        builder.AddMultipleAttributes(AdditionalAttributes);
        if (asset is not null && asset.ShardNum > 0 || asset!.RealmNum > 0 || asset.AccountNum > 0)
        {
            builder.AddContent(asset.ShardNum);
            builder.AddContent(".");
            builder.AddContent(asset.RealmNum);
            builder.AddContent(".");
            builder.AddContent(asset.AccountNum);
            builder.AddContent(".");
            builder.AddContent(asset.SerialNum);
        }
        else
        {
            builder.AddAttribute("hg-none");
            builder.AddContent("None");
        }
        builder.CloseElement();
    }
}