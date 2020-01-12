using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Collections.Generic;

namespace Hashgraph.Portal.Components
{
    public class AddressDisplay : ComponentBase
    {
        [Parameter] public Address Address { get; set; }
        [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object> AdditionalAttributes { get; set; }
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (builder != null)
            {
                builder.OpenElement(0, "span");
                builder.AddMultipleAttributes(1, AdditionalAttributes);
                if (Address.ShardNum > 0 || Address.RealmNum > 0 || Address.AccountNum > 0)
                {
                    builder.AddContent(2, Address.ShardNum);
                    builder.AddContent(3, ".");
                    builder.AddContent(4, Address.RealmNum);
                    builder.AddContent(5, ".");
                    builder.AddContent(6, Address.AccountNum);
                }
                else
                {
                    builder.AddContent(2, "None");
                }
                builder.CloseElement();
            }
        }
    }
}