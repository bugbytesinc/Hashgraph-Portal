using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Hashgraph.Components;

public class MemoDisplay : ComponentBase
{
    [Parameter] [EditorRequired] public string? Value { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement("span");
        builder.AddAttribute("hg-memo-display");
        if (string.IsNullOrWhiteSpace(Value))
        {
            builder.AddAttribute("hg-none");
            builder.AddMultipleAttributes(AdditionalAttributes);
            builder.AddContent("None");
        }
        else
        {
            builder.AddMultipleAttributes(AdditionalAttributes);
            builder.AddContent(Value.Trim());
        }
        builder.CloseElement();
    }
}