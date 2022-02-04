using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Hashgraph.Components;

public class TxIdDisplay : ComponentBase
{
    [Parameter] [EditorRequired] public TxId Value { get; set; } = TxId.None;
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement("span");
        if (Value == null)
        {
            builder.AddMultipleAttributes(AdditionalAttributes);
            builder.AddAttribute("hg-none");
            builder.AddContent("None");
        }
        else
        {
            builder.AddMultipleAttributes(AdditionalAttributes);
            builder.AddContent($"{Value.Address.ShardNum}.{Value.Address.RealmNum}.{Value.Address.AccountNum}@{Value.ValidStartSeconds}.{Value.ValidStartNanos}");
            if(Value.Nonce > 0)
            {
                builder.OpenElement("span");
                builder.AddAttribute("hg-nonce");
                builder.AddContent($"#{Value.Nonce}");
                builder.CloseElement();
            }
            if(Value.Pending)
            {
                builder.OpenElement("span");
                builder.AddAttribute("hg-pending");
                builder.AddContent($"(Pending)");
                builder.CloseElement();
            }
        }
        builder.CloseElement();
    }
}