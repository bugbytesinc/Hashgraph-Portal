using System;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Collections.Generic;

namespace Hashgraph.Portal.Components
{
    public class TimespanDisplay : ComponentBase
    {
        [Parameter] public TimeSpan TimeSpan { get; set; }
        [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object> AdditionalAttributes { get; set; }
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (builder != null)
            {
                if(TimeSpan == TimeSpan.Zero || TimeSpan.Ticks == 0)
                {
                    builder.OpenElement(0, "span");
                    builder.AddMultipleAttributes(1, AdditionalAttributes);
                    builder.AddContent(1,"None");
                    builder.CloseElement();
                }
                else
                {
                    builder.OpenElement(0, "span");
                    builder.AddMultipleAttributes(1, AdditionalAttributes);
                    if(TimeSpan.Days > 0)
                    {
                        builder.OpenElement(0, "span");
                        builder.AddAttribute(1, "class", "days");
                        if(TimeSpan.Days == 1)
                        {
                            builder.AddContent(2, $"1 Day ");
                        }
                        else
                        {
                            builder.AddContent(2, $"{TimeSpan.Days} Days ");
                        }
                        builder.CloseElement();
                    }
                    if (TimeSpan.Hours > 0)
                    {
                        builder.OpenElement(0, "span");
                        builder.AddAttribute(1, "class", "hours");
                        if(TimeSpan.Hours == 1)
                        {
                            builder.AddContent(2, $"1 Hour ");
                        }
                        else
                        {
                            builder.AddContent(2, $"{TimeSpan.Hours} Hours ");
                        }
                        builder.CloseElement();
                    }
                    if (TimeSpan.Minutes > 0)
                    {
                        builder.OpenElement(0, "span");
                        builder.AddAttribute(1, "class", "minutes");
                        if(TimeSpan.Minutes == 1)
                        {
                            builder.AddContent(2, $"1 Minute ");
                        }
                        else
                        {
                            builder.AddContent(2, $"{TimeSpan.Minutes} Minutes ");
                        }
                        builder.CloseElement();
                    }
                    if (TimeSpan.Milliseconds > 0)
                    {
                        builder.OpenElement(0, "span");
                        builder.AddAttribute(1, "class", "seconds");
                        builder.AddContent(2, $"{TimeSpan.Seconds}.{TimeSpan.Milliseconds:D4} Seconds ");
                        builder.CloseElement();
                    }
                    else if (TimeSpan.Seconds > 0)
                    {
                        builder.OpenElement(0, "span");
                        builder.AddAttribute(1, "class", "seconds");
                        if(TimeSpan.Seconds == 1)
                        {
                            builder.AddContent(2, $"1 Second ");
                        }
                        else
                        {
                            builder.AddContent(2, $"{TimeSpan.Seconds} Seconds ");
                        }
                        builder.CloseElement();
                    }
                    builder.CloseElement();
                }
            }
        }
    }
}