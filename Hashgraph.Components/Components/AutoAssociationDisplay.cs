using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Hashgraph.Components;

public class AutoAssociationDisplay : ComponentBase
{
    [Parameter] [EditorRequired] public long? Value { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement("span");
        builder.AddAttribute("hg-auto-association-display");
        if (Value.GetValueOrDefault() > 0)
        {
            builder.AddContent(Value.ToString());
        }
        else
        {
            builder.AddAttribute("hg-none");
            builder.AddContent("Off");
        }
        builder.CloseElement();
    }
}