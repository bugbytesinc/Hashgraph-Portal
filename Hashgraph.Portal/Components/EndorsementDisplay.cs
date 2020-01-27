using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Hashgraph.Portal.Components
{
    public class EndorsementDisplay : ComponentBase
    {
        [Parameter] public Endorsement Endorsement { get; set; }
        [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object> AdditionalAttributes { get; set; }
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (builder != null)
            {
                if (Endorsement == null)
                {
                    builder.OpenElement(0, "span");
                    builder.AddMultipleAttributes(1, AdditionalAttributes);
                    builder.AddAttribute(2, "class", $"{GetBaseClassAttributes()} empty");
                    builder.AddContent(3, "None");
                    builder.CloseElement();
                }
                else
                {
                    switch (Endorsement.Type)
                    {
                        case KeyType.Ed25519:
                        case KeyType.RSA3072:
                        case KeyType.ECDSA384:
                            builder.OpenComponent<PublicKeyDisplay>(0);
                            builder.AddMultipleAttributes(1, AdditionalAttributes);
                            builder.AddAttribute(2, "class", GetBaseClassAttributes());
                            builder.AddAttribute(3, "Key", Endorsement);
                            builder.CloseComponent();
                            break;
                        case KeyType.List:
                            builder.OpenElement(0, "div");
                            builder.AddMultipleAttributes(1, AdditionalAttributes);
                            builder.AddAttribute(2, "class", GetBaseClassAttributes());
                            builder.OpenElement(3, "div");
                            builder.AddAttribute(4, "class", "list-header");
                            var list = Endorsement.List;
                            var required = Endorsement.RequiredCount;
                            var count = 7;
                            if (list.Length == required)
                            {
                                builder.AddMarkupContent(5, "Requires <b>All</b> of the following:");
                            }
                            else if (required == 1)
                            {
                                builder.AddMarkupContent(5, "Requires <b>One</b> of the following:");
                            }
                            else
                            {
                                builder.AddMarkupContent(5, $"Requires <b>{required}</b> of the following:");
                            }
                            builder.CloseElement();
                            builder.OpenElement(6, "ul");
                            foreach (var key in list)
                            {
                                builder.OpenElement(count++, "li");
                                builder.OpenComponent<EndorsementDisplay>(0);
                                builder.AddAttribute(count++, "Endorsement", key);
                                builder.CloseComponent();
                                builder.CloseElement();
                            }
                            builder.CloseElement();
                            builder.CloseElement();
                            break;
                        default:
                            builder.OpenElement(0, "span");
                            builder.AddMultipleAttributes(1, AdditionalAttributes);
                            builder.AddAttribute(2, "class", $"{GetBaseClassAttributes()} empty");
                            builder.AddContent(3, "Unknown Endorsment Type");
                            builder.CloseElement();
                            break;
                    }
                }
            }
        }
        private string GetBaseClassAttributes()
        {
            if (AdditionalAttributes != null && AdditionalAttributes.TryGetValue("class", out var clsAttributeObj))
            {
                var classAttributes = Convert.ToString(clsAttributeObj, CultureInfo.InvariantCulture);
                if (!string.IsNullOrWhiteSpace(classAttributes))
                {
                    return $"endorsment-display {classAttributes}";
                }
            }
            return "endorsment-display";
        }
        private (string prefix, string value) GetKeyAsHexParts()
        {
            var bytes = Endorsement.PublicKey.ToArray();
            return bytes.Length > 34 ?
                (Hex.FromBytes(bytes[..^32]), Hex.FromBytes(bytes[^32..])) :
                (string.Empty, Hex.FromBytes(bytes));
        }
    }
}
