using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using System.Globalization;

namespace Hashgraph.Components
{
    public class InputEndorsement : ComponentBase
    {
        [Parameter] public Endorsement? Value { get; set; }
        [Parameter] public string Placeholder { get; set; } = string.Empty;
        [Parameter] public EventCallback<Endorsement> ValueChanged { get; set; }
        [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }
        private InputPublicKeyDialog? InputPublicKeyDialog { get; set; }
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (Value.IsNullOrNone())
            {
                builder.OpenElement("div");
                builder.AddAttribute("hg-input-endorsement");
                builder.AddAttribute("hg-none");
                builder.AddMultipleAttributes(AdditionalAttributes);
                builder.OpenElement("button");
                builder.AddAttribute("hg-add-key");
                builder.AddAttribute("title", "Add First Key");
                builder.AddAttribute("type", "button");
                builder.AddAttribute("onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => AddNewKeyToTree(Value)));
                builder.CloseElement();
                if (!string.IsNullOrWhiteSpace(Placeholder))
                {
                    builder.OpenElement("span");
                    builder.AddAttribute("hg-placeholder");
                    builder.AddContent(Placeholder.Trim());
                    builder.CloseElement();
                }
                builder.CloseElement();
            }
            else
            {
                builder.OpenElement("div");
                builder.AddAttribute("hg-input-endorsement");
                builder.AddMultipleAttributes(AdditionalAttributes);
                if (Value.Type == KeyType.List)
                {
                    builder.OpenRegion();
                    BuildRenderTreeForList(builder, Value);
                    builder.CloseRegion();
                }
                else
                {
                    builder.OpenElement("div");
                    builder.AddAttribute("hg-input-endorsement-public-key");
                    builder.OpenRegion();
                    BuildRenderTreeForKey(builder, Value);
                    builder.CloseRegion();
                    builder.CloseElement();
                }
                builder.CloseElement();
            }
            builder.OpenComponent<InputPublicKeyDialog>();
            builder.AddComponentReferenceCapture((__value) => { InputPublicKeyDialog = (InputPublicKeyDialog)__value; });
            builder.CloseComponent();
        }
        private void BuildRenderTreeForList(RenderTreeBuilder builder, Endorsement parent)
        {
            builder.OpenElement("div");
            builder.AddAttribute("hg-endorsement-list-header");
            var list = parent.List;
            var required = parent.RequiredCount;
            builder.OpenElement("span");
            builder.AddContent("Requires ");
            builder.OpenElement("select");
            builder.AddAttribute("value", BindConverter.FormatValue(required));
            builder.AddAttribute("onchange", EventCallback.Factory.CreateBinder<string>(this, async __value => { await ChangeRequiredCount(parent, __value); }, required.ToString(CultureInfo.InvariantCulture)));
            builder.OpenRegion();
            for (int i = 1; i < list.Length; i++)
            {
                builder.OpenRegion(i);
                builder.OpenElement("option");
                builder.AddAttribute("value", BindConverter.FormatValue(i));
                builder.AddContent(i.ToString());
                builder.CloseElement();
                builder.CloseRegion();
            }
            builder.CloseRegion();
            builder.OpenElement("option");
            builder.AddAttribute("value", BindConverter.FormatValue(list.Length));
            builder.AddContent("All");
            builder.CloseElement();
            builder.CloseElement();
            builder.AddContent(" of the following:");
            builder.CloseElement();
            if (list.Length == 1)
            {
                builder.OpenElement("button");
                builder.AddAttribute("hg-flatten-list");
                builder.AddAttribute("title", "Flatten List");
                builder.AddAttribute("onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => FlattenList(parent)));
                builder.CloseElement();
            }
            builder.OpenElement("button");
            builder.AddAttribute("hg-add-key");
            builder.AddAttribute("title", "Add Key");
            builder.AddAttribute("type", "button");
            builder.AddAttribute("onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => AddNewKeyToTree(parent)));
            builder.CloseElement();
            builder.OpenElement("button");
            builder.AddAttribute("hg-remove-key");
            builder.AddAttribute("title", "Remove Key");
            builder.AddAttribute("type", "button");
            builder.AddAttribute("onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => RemoveFromTree(parent)));
            builder.CloseElement();
            builder.CloseElement();
            builder.OpenElement("ul");
            builder.OpenRegion();
            int count = 0;
            foreach (var child in list)
            {
                builder.OpenRegion(count++);
                if (child.Type == KeyType.List)
                {
                    builder.OpenElement("li");
                    builder.AddAttribute("hg-nested-key-list");
                    builder.OpenRegion();
                    BuildRenderTreeForList(builder, child);
                    builder.CloseRegion();
                    builder.CloseElement();
                }
                else
                {
                    builder.OpenElement("li");
                    builder.AddAttribute("hg-nested-public-key");
                    builder.OpenRegion();
                    BuildRenderTreeForKey(builder, child);
                    builder.CloseRegion();
                    builder.CloseElement();
                }
                builder.CloseRegion();
            }
            builder.CloseRegion();
            builder.CloseElement();
        }

        private void BuildRenderTreeForKey(RenderTreeBuilder builder, Endorsement endorsement)
        {
            builder.OpenComponent<PublicKeyDisplay>();
            builder.AddAttribute("Value", endorsement);
            builder.CloseComponent();
            builder.OpenElement("button");
            builder.AddAttribute("hg-convert-to-list");
            builder.AddAttribute("title", "Convert Key to List");
            builder.AddAttribute("type", "button");
            builder.AddAttribute("onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => ConvertToList(endorsement)));
            builder.CloseElement();
            builder.OpenElement("button");
            builder.AddAttribute("hg-remove-key");
            builder.AddAttribute("title", "Remove Key");
            builder.AddAttribute("type", "button");
            builder.AddAttribute("onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => RemoveFromTree(endorsement)));
            builder.CloseElement();
        }

        private async Task ChangeRequiredCount(Endorsement endorsement, string value)
        {
            if (uint.TryParse(value, out uint requiredCount) && endorsement.RequiredCount != requiredCount)
            {
                var newEndorsement = new Endorsement(requiredCount, endorsement.List);
                if (SwapEndorsementsInTree(Value, endorsement, newEndorsement, out Endorsement? revisedValue))
                {
                    Value = revisedValue;
                    await ValueChanged.InvokeAsync(revisedValue);
                }
            }
        }

        private async Task RemoveFromTree(Endorsement omit)
        {
            if (SwapEndorsementsInTree(Value, omit, null, out Endorsement? revisedValue))
            {
                await UpdateKeyValue(revisedValue);
            }
        }

        private async Task ConvertToList(Endorsement key)
        {
            if (SwapEndorsementsInTree(Value, key, new Endorsement(key), out Endorsement? revisedValue))
            {
                await UpdateKeyValue(revisedValue);
            }
        }

        private async Task FlattenList(Endorsement endorsement)
        {
            var list = endorsement.List;
            if (list.Length == 1)
            {
                if (SwapEndorsementsInTree(Value, endorsement, list[0], out Endorsement? revisedValue))
                {
                    await UpdateKeyValue(revisedValue);
                }
            }
        }

        private async Task AddNewKeyToTree(Endorsement? parent)
        {
            var newKey = await InputPublicKeyDialog!.PromptForPublicKey();
            if (newKey is not null)
            {
                if (parent is null)
                {
                    await UpdateKeyValue(newKey);
                }
                else if (parent.Type == KeyType.List)
                {
                    var newlist = parent.List.Append(newKey).ToArray();
                    var requiredCount = parent.List.Length == parent.RequiredCount ? (uint)newlist.Length : parent.RequiredCount;
                    if (SwapEndorsementsInTree(Value, parent, new Endorsement(requiredCount, newlist), out Endorsement? revisedValue))
                    {
                        await UpdateKeyValue(revisedValue);
                    }
                }
                else
                {
                    // This shouldn't happen, but less bad than crashing or doing nothing.
                    if (SwapEndorsementsInTree(Value, parent, new Endorsement(parent, newKey), out Endorsement? revisedValue))
                    {
                        await UpdateKeyValue(revisedValue);
                    }
                }
            }
        }

        private static bool SwapEndorsementsInTree(Endorsement? original, Endorsement toRemove, Endorsement? toAdd, out Endorsement? revisedValue)
        {
            if (original is not null)
            {
                if (original.Equals(toRemove))
                {
                    revisedValue = toAdd;
                    return true;
                }
                else if (original.Type == KeyType.List)
                {
                    Endorsement?[] copy = original.List.ToArray();
                    for (int i = 0; i < copy.Length; i++)
                    {
                        if (SwapEndorsementsInTree(copy[i], toRemove, toAdd, out Endorsement? revisedChild))
                        {
                            copy[i] = revisedChild;
                            copy = copy.Where(e => e is not null).ToArray();
                            var required = (uint)Math.Min(copy.Length, original.RequiredCount);
                            revisedValue = copy.Length > 0 ? new Endorsement(required, copy!) : null;
                            return true;
                        }
                    }
                }
            }
            else if (toAdd is not null)
            {
                revisedValue = toAdd;
                return true;
            }
            revisedValue = original;
            return false;
        }
        private async Task UpdateKeyValue(Endorsement? newKey)
        {
            Value = newKey;
            await ValueChanged.InvokeAsync(newKey);
        }
    }
}
