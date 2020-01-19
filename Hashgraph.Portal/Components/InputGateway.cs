using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Threading.Tasks;

namespace Hashgraph.Portal.Components
{
    public class InputGateway : InputBase<Gateway>
    {
        private SelectGatewayDialog SelectGatewayDialog { get; set; }
        private void OnShowDialog()
        {
            SelectGatewayDialog.Show(Value);
        }
        private async Task OnGatewaySelected(Gateway gateway)
        {
            Value = gateway;
            await ValueChanged.InvokeAsync(gateway);
            EditContext.NotifyFieldChanged(FieldIdentifier);
            StateHasChanged();
        }
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (builder != null)
            {
                builder.OpenElement(0, "div");
                builder.AddMultipleAttributes(1, AdditionalAttributes);
                builder.AddAttribute(2, "class", $"gateway-node-selector {CssClass}");
                builder.AddAttribute(3, "onclick", EventCallback.Factory.Create<Microsoft.AspNetCore.Components.Web.MouseEventArgs>(this, OnShowDialog));
                if (Value == null)
                {
                    builder.OpenElement(4, "span");
                    builder.AddAttribute(5, "class", "empty");
                    builder.AddContent(6, "Select Gateway");
                    builder.CloseElement();
                }
                else
                {
                    builder.OpenElement(8, "span");
                    builder.OpenElement(9, "span");
                    builder.AddAttribute(10, "class", "account-id");
                    builder.AddContent(11, $"{Value.ShardNum}.{Value.RealmNum}.{Value.AccountNum}");
                    builder.CloseElement();
                    builder.OpenElement(12, "span");
                    builder.AddAttribute(13, "class", "at");
                    builder.AddContent(14, " at ");
                    builder.CloseElement();
                    builder.OpenElement(15, "span");
                    builder.AddAttribute(16, "class", "node-url");
                    builder.AddContent(17, Value.Url);
                    builder.CloseElement();
                    builder.CloseElement();
                }
                builder.CloseElement();
                builder.OpenComponent<SelectGatewayDialog>(17);
                builder.AddAttribute(18, "SelectedEventCallback", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(EventCallback.Factory.Create<Gateway>(this, OnGatewaySelected)));
                builder.AddComponentReferenceCapture(19, (__value) => { SelectGatewayDialog = (SelectGatewayDialog)__value; });
                builder.CloseComponent();
            }
        }

        protected override bool TryParseValueFromString(string value, out Gateway result, out string validationErrorMessage)
        {
            throw new NotImplementedException($"This component does not parse string inputs.");
        }
    }
}