using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Globalization;

namespace Hashgraph.Portal.Components
{
    public class InputUInt64 : InputBase<Nullable<UInt64>>
    {
        [Parameter] public string ParsingErrorMessage { get; set; } = "The {0} field must be a positive whole number.";
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (builder != null)
            {
                builder.OpenElement(0, "input");
                builder.AddAttribute(1, "step", "any");
                builder.AddMultipleAttributes(2, AdditionalAttributes);
                builder.AddAttribute(3, "type", "number");
                builder.AddAttribute(4, "class", CssClass);
                builder.AddAttribute(5, "value", BindConverter.FormatValue(CurrentValueAsString));
                builder.AddAttribute(6, "onchange", EventCallback.Factory.CreateBinder<string>(this, __value => CurrentValueAsString = __value, CurrentValueAsString));
                builder.CloseElement();
            }
        }

        protected override bool TryParseValueFromString(string value, out Nullable<UInt64> result, out string validationErrorMessage)
        {
            if(string.IsNullOrWhiteSpace(value))
            {
                result = null;
                validationErrorMessage = null;
                return true;
            }
            else if (UInt64.TryParse(value, out UInt64 resultAsUint64))
            {
                result = resultAsUint64;
                validationErrorMessage = null;
                return true;

            }
            else
            {
                result = null;
                validationErrorMessage = string.Format(CultureInfo.InvariantCulture, ParsingErrorMessage, FieldIdentifier.FieldName);
                return false;
            }
        }
        protected override string FormatValueAsString(Nullable<UInt64> value)
        {
            return value?.ToString(CultureInfo.InvariantCulture);
        }

























    }
}