using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;

namespace Hashgraph.Portal.Components
{
    public class TxIdDisplay : ComponentBase
    {
        [Parameter] public TxId TxId { get; set; }
        [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object> AdditionalAttributes { get; set; }
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (builder != null)
            {
                builder.OpenElement(0, "span");
                builder.AddMultipleAttributes(1, AdditionalAttributes);
                if (TxId == null)
                {
                    builder.AddContent(2, "None");
                }
                else
                {
                    builder.AddContent(2, $"{TxId.Address.ShardNum}.{TxId.Address.RealmNum}.{TxId.Address.AccountNum}@{TxId.ValidStartSeconds}.{TxId.ValidStartNanos}");

                }
                builder.CloseElement();
            }
        }
    }
}