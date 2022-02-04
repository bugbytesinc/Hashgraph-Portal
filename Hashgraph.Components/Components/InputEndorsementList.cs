using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.CompilerServices;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace Hashgraph.Components
{
    public class InputEndorsementList : ComponentBase
    {
        [Parameter] public Endorsement[]? Value { get; set; }
        [Parameter] public string Placeholder { get; set; } = string.Empty;
        [Parameter] public EventCallback<Endorsement[]> ValueChanged { get; set; }
        [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }
        private InputPublicKeyDialog? InputPublicKeyDialog { get; set; }
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement("div");
            builder.AddAttribute("hg-input-endorsement-list");
            builder.AddMultipleAttributes(AdditionalAttributes);
            if (Value is not null && Value.Length > 0)
            {
                builder.OpenRegion();
                for (int i = 0; i < Value.Length; i++)
                {
                    builder.OpenRegion(i);
                    BuildRenderTreeForEndorsement(builder, Value[i]);
                    builder.CloseRegion();
                }
                builder.CloseRegion();
            }
            builder.OpenElement("button");
            builder.AddAttribute("hg-add-key");
            builder.AddAttribute("title", "Add First Key");
            builder.AddAttribute("type", "button");
            builder.AddAttribute("onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => AddNewKeyToRootList()));
            builder.CloseElement();
            if (!string.IsNullOrWhiteSpace(Placeholder) && (Value is null || Value.Length == 0))
            {
                builder.OpenElement("span");
                builder.AddAttribute("hg-placeholder");
                builder.AddContent(Placeholder.Trim());
                builder.CloseElement();
            }
            builder.CloseElement();
            builder.OpenComponent<InputPublicKeyDialog>();
            builder.AddComponentReferenceCapture((__value) => { InputPublicKeyDialog = (InputPublicKeyDialog)__value; });
            builder.CloseComponent();
        }
        private void BuildRenderTreeForEndorsement(RenderTreeBuilder builder, Endorsement root)
        {
            builder.OpenComponent<InputEndorsement>();
            builder.AddAttribute("Value", RuntimeHelpers.TypeCheck(root));
            builder.AddAttribute("ValueChanged", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create(this, RuntimeHelpers.CreateInferredEventCallback(this, __value => SwapRootEndorsment(root, __value), root))));
            builder.CloseComponent();
        }
        private async Task SwapRootEndorsment(Endorsement oldValue, Endorsement newValue)
        {
            if (Value is not null && Value.Length > 0)
            {
                var list = new List<Endorsement>(Value);
                var index = list.IndexOf(oldValue);
                if (index > -1)
                {
                    if (newValue is null || Endorsement.None.Equals(newValue))
                    {
                        list.RemoveAt(index);
                    }
                    else
                    {
                        list[index] = newValue;
                    }
                    var newList = list.ToArray();
                    Value = newList;
                    await ValueChanged.InvokeAsync(newList);
                }
            }
        }

        private async Task AddNewKeyToRootList()
        {
            var newKey = await InputPublicKeyDialog!.PromptForPublicKey();
            if (newKey is not null && !Endorsement.None.Equals(newKey))
            {
                var newList = Value is null ? new Endorsement[] { newKey } : Value.Append(newKey).ToArray();
                Value = newList;
                await ValueChanged.InvokeAsync(newList);
            }
        }
    }
}
