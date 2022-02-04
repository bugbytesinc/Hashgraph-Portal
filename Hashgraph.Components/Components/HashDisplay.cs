using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Hashgraph.Components
{
    public class HashDisplay : ComponentBase
    {
        [Parameter] [EditorRequired] public ReadOnlyMemory<byte> Value { get; set; }
        [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement("span");
            builder.AddAttribute("hg-hash-display");
            if (Value.IsEmpty)
            {
                builder.AddAttribute("hg-none");
                builder.AddMultipleAttributes(AdditionalAttributes);
                builder.AddContent(2, "Empty");
            }
            else
            {
                builder.AddMultipleAttributes(AdditionalAttributes);
                builder.AddContent(Hex.FromBytes(Value));
            }
            builder.CloseElement();
        }
    }
}