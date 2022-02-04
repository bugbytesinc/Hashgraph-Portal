using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Hashgraph.Components;
public class TimespanDisplay : ComponentBase
{
    [Parameter] [EditorRequired] public TimeSpan Value { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (Value == TimeSpan.Zero || Value.Ticks == 0)
        {
            builder.OpenElement("span");
            builder.AddAttribute("hg-timespan-display");
            builder.AddAttribute("hg-none");
            builder.AddMultipleAttributes(AdditionalAttributes);
            builder.AddContent("None");
            builder.CloseElement();
        }
        else
        {
            builder.OpenElement("span");
            builder.AddAttribute("hg-timespan-display");
            builder.AddMultipleAttributes(AdditionalAttributes);
            if (Value.Days > 0)
            {
                builder.OpenElement("span");
                builder.AddAttribute("hg-days");
                if (Value.Days == 1)
                {
                    builder.AddContent($"1 Day ");
                }
                else
                {
                    builder.AddContent($"{Value.Days} Days ");
                }
                builder.CloseElement();
            }
            if (Value.Hours > 0)
            {
                builder.OpenElement("span");
                builder.AddAttribute("hg-hours");
                if (Value.Hours == 1)
                {
                    builder.AddContent($"1 Hour ");
                }
                else
                {
                    builder.AddContent($"{Value.Hours} Hours ");
                }
                builder.CloseElement();
            }
            if (Value.Minutes > 0)
            {
                builder.OpenElement("span");
                builder.AddAttribute("hg-minutes");
                if (Value.Minutes == 1)
                {
                    builder.AddContent($"1 Minute ");
                }
                else
                {
                    builder.AddContent($"{Value.Minutes} Minutes ");
                }
                builder.CloseElement();
            }
            if (Value.Milliseconds > 0)
            {
                builder.OpenElement("span");
                builder.AddAttribute("hg-seconds");
                builder.AddContent($"{Value.Seconds}.{Value.Milliseconds:D4} Seconds ");
                builder.CloseElement();
            }
            else if (Value.Seconds > 0)
            {
                builder.OpenElement("span");
                builder.AddAttribute("hg-seconds");
                if (Value.Seconds == 1)
                {
                    builder.AddContent($"1 Second ");
                }
                else
                {
                    builder.AddContent($"{Value.Seconds} Seconds ");
                }
                builder.CloseElement();
            }
            builder.CloseElement();
        }
    }
}