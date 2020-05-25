#pragma warning disable CA1819
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.CompilerServices;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Hashgraph.Portal.Components
{
    public class InputEndorsementList : ComponentBase
    {
        [Parameter] public Endorsement[] Value { get; set; }
        [Parameter] public string Placeholder { get; set; }
        [Parameter] public EventCallback<Endorsement[]> ValueChanged { get; set; }
        [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object> AdditionalAttributes { get; set; }
        private InputPublicKeyDialog InputPublicKeyDialog { get; set; }
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (builder != null)
            {
                var seq = 0;
                builder.OpenElement(seq++, "div");
                builder.AddMultipleAttributes(seq++, AdditionalAttributes);
                builder.AddAttribute(seq++, "class", GetBaseClassAttributes());
                if(Value != null && Value.Length > 0)
                {
                    foreach(var endorsement in Value)
                    {
                        seq = BuildRenderTreeForEndorsement(builder, seq, endorsement);
                    }
                } 
                builder.OpenElement(seq++, "button");
                builder.AddAttribute(seq++, "class", "add-key");
                builder.AddAttribute(seq++, "title", "Add First Key");
                builder.AddAttribute(seq++, "type", "button");
                builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => AddNewKeyToRootList()));
                builder.CloseElement();
                if (!string.IsNullOrWhiteSpace(Placeholder) && (Value == null || Value.Length == 0))
                {
                    builder.OpenElement(seq++, "span");
                    builder.AddAttribute(seq++, "class", "placeholder");
                    builder.AddContent(seq++, Placeholder.Trim());
                    builder.CloseElement();
                }
                builder.CloseElement();
                builder.OpenComponent<InputPublicKeyDialog>(seq++);
                builder.AddComponentReferenceCapture(seq++, (__value) => { InputPublicKeyDialog = (InputPublicKeyDialog)__value; });
                builder.CloseComponent();
            }
        }
        private int BuildRenderTreeForEndorsement(RenderTreeBuilder builder, int seq, Endorsement root)
        {
            builder.OpenComponent<InputEndorsement>(seq++);
            builder.AddAttribute(seq++, "Value", RuntimeHelpers.TypeCheck(root));
            builder.AddAttribute(seq++, "ValueChanged", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create(this, RuntimeHelpers.CreateInferredEventCallback(this, __value => SwapRootEndorsment(root, __value), root))));
            builder.CloseComponent();
            return seq;
        }
        private async Task SwapRootEndorsment(Endorsement oldValue, Endorsement newValue)
        {            
            if(Value != null && Value.Length > 0)
            {
                var list = new List<Endorsement>(Value);                
                var index = list.IndexOf(oldValue);
                if (index > -1)
                {
                    if(newValue == null || Endorsement.None.Equals(newValue))
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
            var newKey = await InputPublicKeyDialog.PromptForPublicKey();
            if (newKey != null)
            {
                var newList = Value == null ? new Endorsement[] { newKey } : Value.Append(newKey).ToArray();
                Value = newList;
                await ValueChanged.InvokeAsync(newList);
            }
        }
        private string GetBaseClassAttributes()
        {
            if (AdditionalAttributes != null && AdditionalAttributes.TryGetValue("class", out var clsAttributeObj))
            {
                var classAttributes = Convert.ToString(clsAttributeObj, CultureInfo.InvariantCulture);
                if (!string.IsNullOrWhiteSpace(classAttributes))
                {
                    return $"input-endorsement-list {classAttributes}";
                }
            }
            return "input-endorsement-list";
        }
    }
}
