using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Hashgraph.Portal.Components
{
    public class InputEndorsement : ComponentBase
    {
        [Parameter] public Endorsement Value { get; set; }
        [Parameter] public string Placeholder { get; set; }
        [Parameter] public EventCallback<Endorsement> ValueChanged { get; set; }
        [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object> AdditionalAttributes { get; set; }
        private InputPublicKeyDialog InputPublicKeyDialog { get; set; }
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (builder != null)
            {
                var seq = 0;
                if (Value == null)
                {
                    builder.OpenElement(seq++, "div");
                    builder.AddMultipleAttributes(seq++, AdditionalAttributes);
                    builder.AddAttribute(seq++, "class", $"{GetBaseClassAttributes()} empty");
                    builder.OpenElement(seq++, "button");
                    builder.AddAttribute(seq++, "class", "add-key");
                    builder.AddAttribute(seq++, "title", "Add First Key");
                    builder.AddAttribute(seq++, "type", "button");
                    builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => AddNewKeyToTree(Value)));
                    builder.CloseElement();
                    if (!string.IsNullOrWhiteSpace(Placeholder))
                    {
                        builder.OpenElement(seq++, "span");
                        builder.AddAttribute(seq++, "class", "placeholder");
                        builder.AddContent(seq++, Placeholder.Trim());
                        builder.CloseElement();
                    }
                    builder.CloseElement();
                }
                else
                {
                    builder.OpenElement(seq++, "div");
                    builder.AddMultipleAttributes(seq++, AdditionalAttributes);
                    builder.AddAttribute(seq++, "class", GetBaseClassAttributes());
                    if (Value.Type == KeyType.List)
                    {
                        seq = BuildRenderTreeForList(builder, seq, Value);
                    }
                    else
                    {
                        builder.OpenElement(seq++, "div");
                        builder.AddAttribute(seq++, "class", "public-key");
                        seq = BuildRenderTreeForKey(builder, seq, Value);
                        builder.CloseElement();
                    }
                    builder.CloseElement();
                }
                builder.OpenComponent<InputPublicKeyDialog>(seq++);
                builder.AddComponentReferenceCapture(seq++, (__value) => { InputPublicKeyDialog = (InputPublicKeyDialog)__value; });
                builder.CloseComponent();
            }
        }
        private int BuildRenderTreeForList(RenderTreeBuilder builder, int seq, Endorsement parent)
        {
            builder.OpenElement(seq++, "div");
            builder.AddAttribute(seq++, "class", "list-header");
            var list = parent.List;
            var required = parent.RequiredCount;
            builder.OpenElement(seq++, "span");
            builder.AddContent(seq++, "Requires ");
            builder.OpenElement(seq++, "select");
            builder.AddAttribute(seq++, "value", BindConverter.FormatValue(required));
            builder.AddAttribute(seq++, "onchange", EventCallback.Factory.CreateBinder<string>(this, async __value => { await ChangeRequiredCount(parent, __value); }, required.ToString(CultureInfo.InvariantCulture)));
            for (int i = 1; i < list.Length; i++)
            {
                builder.OpenElement(seq++, "option");
                builder.AddAttribute(seq++, "value", i);
                builder.AddContent(seq++, i);
                builder.CloseElement();
            }
            builder.OpenElement(seq++, "option");
            builder.AddAttribute(seq++, "value", list.Length);
            builder.AddContent(seq++, "All");
            builder.CloseElement();
            builder.CloseElement();
            builder.AddContent(seq++, " of the following:");
            builder.CloseElement();
            if (list.Length == 1)
            {
                builder.OpenElement(seq++, "button");
                builder.AddAttribute(seq++, "class", "flatten-list");
                builder.AddAttribute(seq++, "title", "Flatten List");
                builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => FlattenList(parent)));
                builder.CloseElement();
            }
            builder.OpenElement(seq++, "button");
            builder.AddAttribute(seq++, "class", "add-key");
            builder.AddAttribute(seq++, "title", "Add Key");
            builder.AddAttribute(seq++, "type", "button");
            builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => AddNewKeyToTree(parent)));
            builder.CloseElement();
            builder.OpenElement(seq++, "button");
            builder.AddAttribute(seq++, "class", "remove-key");
            builder.AddAttribute(seq++, "title", "Remove Key");
            builder.AddAttribute(seq++, "type", "button");
            builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => RemoveFromTree(parent)));
            builder.CloseElement();
            builder.CloseElement();
            builder.OpenElement(seq++, "ul");
            foreach (var child in list)
            {
                if (child.Type == KeyType.List)
                {
                    builder.OpenElement(seq++, "li");
                    builder.AddAttribute(seq++, "class", "key-list");
                    seq = BuildRenderTreeForList(builder, seq, child);
                    builder.CloseElement();
                }
                else
                {
                    builder.OpenElement(seq++, "li");
                    builder.AddAttribute(seq++, "class", "public-key");
                    seq = BuildRenderTreeForKey(builder, seq, child);
                    builder.CloseElement();
                }
            }
            builder.CloseElement();
            return seq;
        }

        private async Task ChangeRequiredCount(Endorsement endorsement, string value)
        {
            if (uint.TryParse(value, out uint requiredCount) && endorsement.RequiredCount != requiredCount)
            {
                var newEndorsement = new Endorsement(requiredCount, endorsement.List);
                if (SwapEndorsementsInTree(Value, endorsement, newEndorsement, out Endorsement revisedValue))
                {
                    Value = revisedValue;
                    await ValueChanged.InvokeAsync(revisedValue);
                }
            }
        }

        private int BuildRenderTreeForKey(RenderTreeBuilder builder, int seq, Endorsement endorsement)
        {
            builder.OpenComponent<PublicKeyDisplay>(seq++);
            builder.AddAttribute(seq++, "Key", endorsement);
            builder.CloseComponent();
            builder.OpenElement(seq++, "button");
            builder.AddAttribute(seq++, "class", "convert-to-list");
            builder.AddAttribute(seq++, "title", "Convert Key to List");
            builder.AddAttribute(seq++, "type", "button");
            builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => ConvertToList(endorsement)));
            builder.CloseElement();
            builder.OpenElement(seq++, "button");
            builder.AddAttribute(seq++, "class", "remove-key");
            builder.AddAttribute(seq++, "title", "Remove Key");
            builder.AddAttribute(seq++, "type", "button");
            builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => RemoveFromTree(endorsement)));
            builder.CloseElement();
            return seq;
        }

        private async Task RemoveFromTree(Endorsement omit)
        {
            if (SwapEndorsementsInTree(Value, omit, null, out Endorsement revisedValue))
            {
                await UpdateKeyValue(revisedValue);
            }
        }
        private async Task ConvertToList(Endorsement key)
        {
            if (SwapEndorsementsInTree(Value, key, new Endorsement(key), out Endorsement revisedValue))
            {
                await UpdateKeyValue(revisedValue);
            }
        }
        private async Task FlattenList(Endorsement endorsement)
        {
            var list = endorsement.List;
            if (list.Length == 1)
            {
                if (SwapEndorsementsInTree(Value, endorsement, list[0], out Endorsement revisedValue))
                {
                    await UpdateKeyValue(revisedValue);
                }
            }
        }
        private async Task AddNewKeyToTree(Endorsement parent)
        {
            var newKey = await InputPublicKeyDialog.PromptForPublicKey();
            if (newKey != null)
            {
                if (parent == null)
                {
                    await UpdateKeyValue(newKey);
                }
                else if (parent.Type == KeyType.List)
                {
                    var newlist = parent.List.Append(newKey).ToArray();
                    var requiredCount = parent.List.Length == parent.RequiredCount ? (uint)newlist.Length : parent.RequiredCount;
                    if (SwapEndorsementsInTree(Value, parent, new Endorsement(requiredCount, newlist), out Endorsement revisedValue))
                    {
                        await UpdateKeyValue(revisedValue);
                    }
                }
                else
                {
                    // This shouldn't happen, but less bad than crashing or doing nothing.
                    if (SwapEndorsementsInTree(Value, parent, new Endorsement(parent, newKey), out Endorsement revisedValue))
                    {
                        await UpdateKeyValue(revisedValue);
                    }
                }
            }
        }
        private static bool SwapEndorsementsInTree(Endorsement original, Endorsement toRemove, Endorsement toAdd, out Endorsement revisedValue)
        {
            if (original != null)
            {
                if (original.Equals(toRemove))
                {
                    revisedValue = toAdd;
                    return true;
                }
                else if (original.Type == KeyType.List)
                {
                    var copy = original.List.ToArray();
                    for (int i = 0; i < copy.Length; i++)
                    {
                        if (SwapEndorsementsInTree(copy[i], toRemove, toAdd, out Endorsement revisedChild))
                        {
                            copy[i] = revisedChild;
                            copy = copy.Where(e => e != null).ToArray();
                            var required = (uint)Math.Min(copy.Length, original.RequiredCount);
                            revisedValue = copy.Length > 0 ? new Endorsement(required, copy) : null;
                            return true;
                        }
                    }
                }
            }
            else if (toAdd != null)
            {
                revisedValue = toAdd;
                return true;
            }
            revisedValue = original;
            return false;
        }
        private async Task UpdateKeyValue(Endorsement newKey)
        {
            Value = newKey;
            await ValueChanged.InvokeAsync(newKey);
        }
        private string GetBaseClassAttributes()
        {
            if (AdditionalAttributes != null && AdditionalAttributes.TryGetValue("class", out var clsAttributeObj))
            {
                var classAttributes = Convert.ToString(clsAttributeObj, CultureInfo.InvariantCulture);
                if (!string.IsNullOrWhiteSpace(classAttributes))
                {
                    return $"input-endorsement {classAttributes}";
                }
            }
            return "input-endorsement";
        }
    }
}
