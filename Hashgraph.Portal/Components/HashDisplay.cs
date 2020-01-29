using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;

namespace Hashgraph.Portal.Components
{
    public class HashDisplay : ComponentBase
    {
        [Parameter] public ReadOnlyMemory<byte> Hash { get; set; }
        [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object> AdditionalAttributes { get; set; }
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (builder != null)
            {
                builder.OpenElement(0, "span");
                builder.AddMultipleAttributes(1, AdditionalAttributes);
                if (Hash.IsEmpty)
                {
                    builder.AddContent(2, "Empty");
                }
                else
                {
                    builder.AddContent(2, Hex.FromBytes(Hash));

                }
                builder.CloseElement();
            }
        }
    }
}