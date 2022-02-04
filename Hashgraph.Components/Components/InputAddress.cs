using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Hashgraph.Components;

public class InputAddress : InputBase<Address>
{
    [Parameter] public string ParsingErrorMessage { get; set; } = "The {0} field expected an address in the format <shard>.<realm>.<number>";

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement("input");
        builder.AddAttribute("hg-input-address");
        builder.AddMultipleAttributes(AdditionalAttributes);
        builder.AddAttribute("class", CssClass);
        builder.AddAttribute("type", "text");
        builder.AddAttribute("value", BindConverter.FormatValue(CurrentValueAsString));
        builder.AddAttribute("onchange", EventCallback.Factory.CreateBinder<string?>(this, __value => CurrentValueAsString = __value, CurrentValueAsString));
        builder.CloseElement();
    }

    protected override bool TryParseValueFromString(string? value, [MaybeNullWhen(false)] out Address result, [NotNullWhen(false)] out string? validationErrorMessage)
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
        result = Address.None;
        validationErrorMessage = null;
        return true;
    }
    protected override string? FormatValueAsString(Address? value)
    {
        return value is null ? null : $"{value.ShardNum}.{value.RealmNum}.{value.AccountNum}";
    }
}