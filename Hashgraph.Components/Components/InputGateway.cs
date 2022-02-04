using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using System.Diagnostics.CodeAnalysis;

namespace Hashgraph.Components;

public class InputGateway : InputBase<Gateway>
{
    private SelectGatewayDialog? _selectGatewayDialog;

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement("div");
        builder.AddAttribute("hg-input-gateway");
        builder.AddMultipleAttributes(AdditionalAttributes);
        builder.AddAttribute("class", CssClass);
        builder.AddAttribute("onclick", EventCallback.Factory.Create<Microsoft.AspNetCore.Components.Web.MouseEventArgs>(this, OnShowDialogAsync));
        if (Value == null)
        {
            builder.OpenElement("span");
            builder.AddAttribute("hg-empty");
            builder.AddContent("Select Gateway");
            builder.CloseElement();
        }
        else
        {
            builder.OpenElement("span");
            builder.OpenElement("span");
            builder.AddAttribute("hg-account-id");
            builder.AddContent($"{Value.ShardNum}.{Value.RealmNum}.{Value.AccountNum}");
            builder.CloseElement();
            builder.OpenElement("span");
            builder.AddAttribute("hg-at");
            builder.AddContent(" at ");
            builder.CloseElement();
            builder.OpenElement("span");
            builder.AddAttribute("hg-node-url");
            builder.AddContent(Value.Url);
            builder.CloseElement();
            builder.CloseElement();
        }
        builder.CloseElement();
        builder.OpenComponent<SelectGatewayDialog>();
        builder.AddAttribute("SelectedEventCallback", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(EventCallback.Factory.Create<Gateway>(this, OnGatewaySelected)));
        builder.AddComponentReferenceCapture((__value) => { _selectGatewayDialog = (SelectGatewayDialog)__value; });
        builder.CloseComponent();
    }
    private async Task OnShowDialogAsync()
    {
        await _selectGatewayDialog!.ShowAsync(Value);
    }
    private async Task OnGatewaySelected(Gateway? gateway)
    {
        Value = gateway;
        await ValueChanged.InvokeAsync(gateway);
        EditContext.NotifyFieldChanged(FieldIdentifier);
        StateHasChanged();
    }
    protected override bool TryParseValueFromString(string? value, [MaybeNullWhen(false)] out Gateway result, [NotNullWhen(false)] out string? validationErrorMessage)
    {
        throw new NotImplementedException($"This component does not parse string inputs.");
    }
}