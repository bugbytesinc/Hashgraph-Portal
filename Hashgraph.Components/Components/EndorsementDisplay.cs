using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Hashgraph.Components
{
    public class EndorsementDisplay : ComponentBase
    {
        [Parameter] [EditorRequired] public Endorsement? Value { get; set; }
        [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            var endorsement = Value;
            if (endorsement.IsNullOrNone())
            {
                builder.OpenElement("span");
                builder.AddAttribute("hg-endorsment-display");
                builder.AddAttribute("hg-none");
                builder.AddMultipleAttributes(AdditionalAttributes);
                builder.AddContent("None");
                builder.CloseElement();
            }
            else
            {
                switch (endorsement.Type)
                {
                    case KeyType.Ed25519:
                    case KeyType.ECDSASecp256K1:
                    case KeyType.Contract:
                        builder.OpenComponent<PublicKeyDisplay>();
                        builder.AddMultipleAttributes(AdditionalAttributes);
                        builder.AddAttribute("Value", endorsement);
                        builder.CloseComponent();
                        break;
                    case KeyType.List:
                        builder.OpenElement("div");
                        builder.AddAttribute("hg-endorsment-display");
                        builder.AddMultipleAttributes(AdditionalAttributes);
                        builder.OpenElement("div");
                        var list = endorsement.List;
                        var required = endorsement.RequiredCount;
                        if (list.Length == required)
                        {
                            builder.AddMarkupContent("Requires <b>All</b> of the following:");
                        }
                        else if (required == 1)
                        {
                            builder.AddMarkupContent("Requires <b>One</b> of the following:");
                        }
                        else
                        {
                            builder.AddMarkupContent($"Requires <b>{required}</b> of the following:");
                        }
                        builder.CloseElement();
                        builder.OpenElement("ul");
                        builder.OpenRegion();
                        for (int i = 0; i < list.Length; i++)
                        {
                            builder.OpenRegion(i);
                            builder.OpenElement("li");
                            builder.OpenComponent<EndorsementDisplay>();
                            builder.AddAttribute("Value", list[i]);
                            builder.CloseComponent();
                            builder.CloseElement();
                            builder.CloseRegion();
                        }
                        builder.CloseRegion();
                        builder.CloseElement();
                        builder.CloseElement();
                        break;
                    default:
                        builder.OpenElement("span");
                        builder.AddAttribute("hg-endorsment-display");
                        builder.AddAttribute("hg-none");
                        builder.AddMultipleAttributes(AdditionalAttributes);
                        builder.AddContent("Unknown Endorsment Type");
                        builder.CloseElement();
                        break;
                }
            }
        }
    }
}