using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Hashgraph.Components;

public class AddressDisplay : ComponentBase
{
    [Parameter] [EditorRequired] public Address? Value { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var address = Value;
        builder.OpenElement("span");
        builder.AddAttribute("hg-address-display");
        builder.AddMultipleAttributes(AdditionalAttributes);
        if (address is null || Address.None.Equals(address))
        {
            builder.AddAttribute("hg-none");
            builder.AddContent("None");
        }
        else
        {
            builder.AddContent(address.ShardNum);
            builder.AddContent(".");
            builder.AddContent(address.RealmNum);
            builder.AddContent(".");
            builder.AddContent(address.AccountNum);
        }
        builder.CloseElement();
    }
}