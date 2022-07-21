using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Hashgraph.Components;

public class ContractCallResultDisplay : ComponentBase
{
    [Parameter][EditorRequired] public ContractCallResult? Value { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var value = Value;
        if (value == null)
        {
            builder.AddMarkupContent("<div hg-contract-call-result-display><div>Call Results</div><div hg-none>None</div></div>");
        }
        else
        {
            builder.OpenElement("div");
            builder.AddAttribute("hg-contract-call-result-display");
            builder.AddMultipleAttributes(AdditionalAttributes);
            if (!string.IsNullOrWhiteSpace(value.Error))
            {
                builder.AddMarkupContent("<div>Error</div>");
                builder.OpenElement("div");
                builder.AddContent(value.Error);
                builder.CloseElement();
            }
            builder.AddMarkupContent("<div>Call Result</div>");
            builder.OpenComponent<BinaryDisplay>();
            builder.AddAttribute("Value", value.Result.Data);
            builder.CloseComponent();

            builder.AddMarkupContent("<div>Bloom</div>");
            builder.OpenComponent<BinaryDisplay>();
            builder.AddAttribute("Value", value.Bloom);
            builder.CloseComponent();

            builder.AddMarkupContent("<div>Gas Used</div>");
            builder.OpenComponent<HbarDisplay>();
            builder.AddAttribute("Value", value.GasUsed);
            builder.CloseComponent();

            builder.AddMarkupContent("<div>Gas Limit</div>");
            builder.OpenComponent<HbarDisplay>();
            builder.AddAttribute("Value", value.GasLimit);
            builder.CloseComponent();

            builder.AddMarkupContent("<div>Payable Amount</div>");
            builder.OpenComponent<HbarDisplay>();
            builder.AddAttribute("Value", value.PayableAmount);
            builder.CloseComponent();

            builder.AddMarkupContent("<div>Message Sender</div>");
            builder.OpenComponent<AddressDisplay>();
            builder.AddAttribute("Value", value.MessageSender);
            builder.CloseComponent();

            builder.AddMarkupContent("<div>EVM Address</div>");
            builder.OpenComponent<MonikerDisplay>();
            builder.AddAttribute("Value", value.EncodedAddress);
            builder.CloseComponent();

            builder.AddMarkupContent("<div>Arguments</div>");
            builder.OpenComponent<BinaryDisplay>();
            builder.AddAttribute("Value", value.FunctionArgs.Data);
            builder.CloseComponent();

            if (value.Events.Count > 0)
            {
                builder.AddMarkupContent("<h4>Events</h4>");
                builder.OpenRegion();
                var count = 0;
                foreach (var evt in value.Events)
                {
                    builder.OpenRegion(count++);

                    builder.AddMarkupContent("<div>Emitting Contract</div>");
                    builder.OpenComponent<AddressDisplay>();
                    builder.AddAttribute("Value", evt.Contract);
                    builder.CloseComponent();

                    builder.AddMarkupContent("<div>Bloom</div>");
                    builder.OpenComponent<HashDisplay>();
                    builder.AddAttribute("Value", evt.Bloom);
                    builder.CloseComponent();
                    if (evt.Topic.Length > 0)
                    {
                        builder.AddMarkupContent("<div>Topics</div>");
                        builder.OpenElement("div");
                        builder.OpenRegion();
                        var count2 = 0;
                        foreach (var topic in evt.Topic)
                        {
                            builder.OpenRegion(count2++);
                            builder.OpenElement("div");
                            builder.OpenComponent<HashDisplay>();
                            builder.AddAttribute("Value", topic);
                            builder.CloseComponent();
                            builder.CloseElement();
                            builder.CloseRegion();
                        }
                        builder.CloseRegion();
                        builder.CloseElement();
                    }
                    builder.CloseRegion();
                    builder.AddMarkupContent("<div>Result</div>");
                    builder.OpenComponent<ContractCallResultDisplay>();
                    builder.AddAttribute("Value", evt.Data);
                    builder.CloseComponent();

                    builder.CloseRegion();
                }
                builder.CloseRegion();
            }
            builder.CloseElement();
        }
    }
}