using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Hashgraph.Portal.Components
{
    public class PublicKeyDisplay : ComponentBase
    {
        [Parameter] public Endorsement Key { get; set; }
        [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object> AdditionalAttributes { get; set; }
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (builder != null)
            {
                builder.OpenElement(0, "span");
                builder.AddMultipleAttributes(1, AdditionalAttributes);
                if (Key == null)
                {
                    builder.AddAttribute(2, "class", $"{GetBaseClassAttributes()} empty");
                    builder.AddContent(3, "None");
                }
                else
                {
                    builder.AddAttribute(2, "class", $"{GetBaseClassAttributes()} {GetKeyTypeClass()}");
                    switch (Key.Type)
                    {
                        case KeyType.Ed25519:
                            var (prefix, value) = GetKeyAsHexParts();
                            builder.OpenElement(3, "span");
                            builder.AddContent(4, prefix);
                            builder.CloseElement();
                            builder.OpenElement(5, "span");
                            builder.AddContent(6, value);
                            builder.CloseElement();
                            break;
                        case KeyType.RSA3072:
                        case KeyType.ECDSA384:
                        case KeyType.ContractID:
                            builder.AddContent(3, Hex.FromBytes(Key.PublicKey));
                            break;
                        case KeyType.List:
                            builder.AddContent(3, $"{Key.RequiredCount} of {Key.List.Length} List");
                            break;
                    }
                }
                builder.CloseElement();
            }
        }

        private string GetBaseClassAttributes()
        {
            if (AdditionalAttributes != null && AdditionalAttributes.TryGetValue("class", out var clsAttributeObj))
            {
                var classAttributes = Convert.ToString(clsAttributeObj, CultureInfo.InvariantCulture);
                if (!string.IsNullOrWhiteSpace(classAttributes))
                {
                    return $"public-key-display {classAttributes}";
                }
            }
            return "public-key-display";
        }

        private string GetKeyTypeClass()
        {
            return Key.Type switch
            {
                KeyType.Ed25519 => "key-type-ed25519",
                KeyType.RSA3072 => "key-type-rsa3072",
                KeyType.ECDSA384 => "key-type-ecdsa384",
                KeyType.ContractID => "key-type-contract",
                KeyType.List => "key-type-list",
                _ => string.Empty
            };
        }
        private (string prefix, string value) GetKeyAsHexParts()
        {
            var bytes = Key.PublicKey.ToArray();
            return bytes.Length > 34 ?
                (Hex.FromBytes(bytes[..^32]), Hex.FromBytes(bytes[^32..])) :
                (string.Empty, Hex.FromBytes(bytes));
        }
    }
}
