using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using System.Diagnostics.CodeAnalysis;

namespace Hashgraph.Components
{
    public class InputKeyType : InputBase<KeyType>
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement("select");
            builder.AddAttribute("hg-input-key-type");
            builder.AddMultipleAttributes(AdditionalAttributes);
            builder.AddAttribute("value", BindConverter.FormatValue(CurrentValueAsString));
            builder.AddAttribute("onchange", EventCallback.Factory.CreateBinder<string?>(this, __value => CurrentValueAsString = __value, CurrentValueAsString));

            builder.OpenElement("option");
            builder.AddAttribute("value", FormatValueAsString(KeyType.Ed25519));
            builder.AddContent("Ed25519");
            builder.CloseElement();

            builder.OpenElement("option");
            builder.AddAttribute("value", FormatValueAsString(KeyType.ECDSASecp256K1));
            builder.AddContent("ECDSA Secp256K1");
            builder.CloseElement();

            builder.OpenElement("option");
            builder.AddAttribute("value", FormatValueAsString(KeyType.Contract));
            builder.AddContent("CONTRACT");
            builder.CloseElement();
            builder.CloseElement();
        }
        protected override bool TryParseValueFromString(string? value, [MaybeNullWhen(false)] out KeyType result, [NotNullWhen(false)] out string? validationErrorMessage)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                if (Enum.TryParse(value, out KeyType keyType))
                {
                    switch (keyType)
                    {
                        case KeyType.Ed25519:
                        case KeyType.ECDSASecp256K1:
                        case KeyType.Contract:
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