using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using System;

namespace Hashgraph.Portal.Components
{
    public class InputPublicKeyType : InputBase<KeyType>
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (builder != null)
            {
                builder.OpenElement(0, "select");
                builder.AddMultipleAttributes(1, AdditionalAttributes);
                builder.AddAttribute(2, "class", $"input-public-key-type {CssClass}");
                builder.AddAttribute(3, "value", BindConverter.FormatValue(CurrentValueAsString));
                builder.AddAttribute(4, "onchange", EventCallback.Factory.CreateBinder<string>(this, __value => CurrentValueAsString = __value, CurrentValueAsString));
                builder.OpenElement(5, "option");
                builder.AddAttribute(6, "value", FormatValueAsString(KeyType.Ed25519));
                builder.AddContent(7, "Ed25519");
                builder.CloseElement();
                builder.OpenElement(5, "option");
                builder.AddAttribute(6, "value", FormatValueAsString(KeyType.ECDSA384));
                builder.AddContent(7, "ECDSA 384");
                builder.CloseElement();
                builder.OpenElement(5, "option");
                builder.AddAttribute(6, "value", FormatValueAsString(KeyType.RSA3072));
                builder.AddContent(7, "RSA 3072");
                builder.CloseElement();
                builder.CloseElement();
            }
        }

        protected override bool TryParseValueFromString(string value, out KeyType result, out string validationErrorMessage)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                if(Enum.TryParse(value, out KeyType keyType))
                {
                    switch(keyType)
                    {
                        case KeyType.Ed25519:
                        case KeyType.ECDSA384:
                        case KeyType.RSA3072:
                            result = keyType;
                            validationErrorMessage = null;
                            return true;
                    }
                    result = default;
                    validationErrorMessage = "Not a supported key type for non-list keys.";
                    return true;
                }
                result = default;
                validationErrorMessage = "Not a valid key type.";
                return false;
            }
            result = default;
            validationErrorMessage = null;
            return true;
        }
        protected override string FormatValueAsString(KeyType value)
        {
            return value.ToString();
        }
    }
}