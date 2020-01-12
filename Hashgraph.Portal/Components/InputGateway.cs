using Hashgraph.Portal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;

namespace Hashgraph.Portal.Components
{
    public class InputGateway : InputBase<Gateway>
    {
        [Inject] public GatewayListService GatewayListService { get; set; }
        private SelectGatewayDialog SelectGatewayDialog { get; set; }
        private void OnShowDialog()
        {
            SelectGatewayDialog.Show(Value);
        }
        private void OnGatewaySelected(Gateway gateway)
        {
            CurrentValueAsString = FormatValueAsString(gateway);
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
                builder.OpenElement(3, "input");
                builder.AddAttribute(4, "type", "hidden");
                builder.AddAttribute(5, "value", BindConverter.FormatValue(CurrentValueAsString));
                builder.AddAttribute(6, "onchange", EventCallback.Factory.CreateBinder<string>(this, __value => CurrentValueAsString = __value, CurrentValueAsString));
                builder.CloseElement();
                if (Value == null)
                {
                    builder.AddContent(7, "Click to select a Gateway Node");
                }
                else
                {
                    builder.OpenElement(8, "span");
                    builder.AddAttribute(9, "class", "account-id");
                    builder.AddContent(10, $"{Value.ShardNum}.{Value.RealmNum}.{Value.AccountNum}");
                    builder.CloseElement();
                    builder.OpenElement(11, "span");
                    builder.AddAttribute(12, "class", "at");
                    builder.AddContent(13, " at ");
                    builder.CloseElement();
                    builder.OpenElement(14, "span");
                    builder.AddAttribute(15, "class", "node-url");
                    builder.AddContent(16, Value.Url);
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
            if (!string.IsNullOrWhiteSpace(value))
            {
                var nodeAndUrl = value.Split(' ');
                if (nodeAndUrl.Length == 2)
                {
                    var parts = nodeAndUrl[0].Split('.');
                    if (parts.Length == 3)
                    {
                        if (uint.TryParse(parts[0], out uint shard) &&
                            uint.TryParse(parts[1], out uint realm) &&
                            uint.TryParse(parts[2], out uint number))
                        {
                            var candidate = new Gateway(nodeAndUrl[1], shard, realm, number);
                            if (GatewayListService.MainNet.Contains(candidate) || GatewayListService.TestNet.Contains(candidate))
                            {
                                result = candidate;
                                validationErrorMessage = null;
                                return true;
                            }
                            result = null;
                            validationErrorMessage = "Gateway not found in MainNet nor TestNet Lists.";
                            return false;
                        }
                    }
                }
                result = null;
                validationErrorMessage = "Expected a Node Address (S.R.N) followed by a URL (IP:Port)";
                return false;
            }
            result = null;
            validationErrorMessage = null;
            return true;
        }
        protected override string FormatValueAsString(Gateway value)
        {
            return value is null ? string.Empty : $"{value.ShardNum}.{value.RealmNum}.{value.AccountNum} {value.Url}";
        }
    }
}