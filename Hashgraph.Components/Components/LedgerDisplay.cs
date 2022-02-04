using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Text;

namespace Hashgraph.Components;

public class LedgerDisplay : ComponentBase
{
    [Parameter] [EditorRequired] public ReadOnlyMemory<byte> Value { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement("span");
        builder.AddAttribute("hg-ledger-display");
        if (Value.IsEmpty)
        {
            builder.AddAttribute("hg-none");
            builder.AddContent("Unknown");
        }
        else
        {
            builder.AddContent(GetLedgerText());
        }
        builder.CloseElement();
    }

    private string GetLedgerText()
    {
        try
        {
            var ledgerAsText = Encoding.Default.GetString(Value.Span);
            return ledgerAsText switch
            {
                "0x00" => "Main (0x00)",
                "0x01" => "Test (0x01)",
                "0x02" => "Preview (0x02)",
                _ => $"Other ({ledgerAsText})",
            };
        }
        catch
        {
            return Hex.FromBytes(Value);
        }
    }
}