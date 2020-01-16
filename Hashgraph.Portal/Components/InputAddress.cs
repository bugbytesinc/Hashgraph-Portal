using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using System.Globalization;

namespace Hashgraph.Portal.Components
{
    public class InputAddress : InputBase<Address>
    {
        [Parameter] public string ParsingErrorMessage { get; set; } = "The {0} field expected an address in the format <shard>.<realm>.<number>";
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (builder != null)
            {
                builder.OpenElement(0, "input");
                builder.AddMultipleAttributes(2, AdditionalAttributes);
                builder.AddAttribute(3, "type", "text");
                builder.AddAttribute(4, "class", CssClass);
                builder.AddAttribute(5, "value", BindConverter.FormatValue(CurrentValueAsString));
                builder.AddAttribute(6, "onchange", EventCallback.Factory.CreateBinder<string>(this, __value => CurrentValueAsString = __value, CurrentValueAsString));
                builder.CloseElement();
            }
        }

        protected override bool TryParseValueFromString(string value, out Address result, out string validationErrorMessage)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                var parts = value.Split('.');
                if (parts.Length == 3)
                {
                    if (uint.TryParse(parts[0], out uint shard) &&
                        uint.TryParse(parts[1], out uint realm) &&
                        uint.TryParse(parts[2], out uint number))
                    {
                        result = new Address(shard, realm, number);
                        validationErrorMessage = null;
                        return true;
                    }
                }
                result = null;
                validationErrorMessage = string.Format(CultureInfo.InvariantCulture, ParsingErrorMessage, FieldIdentifier.FieldName);
                return false;
            }
            result = null;
            validationErrorMessage = null;
            return true;
        }
        protected override string FormatValueAsString(Address value)
        {
            return value is null ? string.Empty : $"{value.ShardNum}.{value.RealmNum}.{value.AccountNum}";
        }
    }
}