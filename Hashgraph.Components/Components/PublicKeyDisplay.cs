using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Hashgraph.Components;

public class PublicKeyDisplay : ComponentBase
{
    [Parameter] [EditorRequired] public Endorsement? Value { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement("span");
        builder.AddAttribute("hg-public-key-display");
        if (Value.IsNullOrNone())
        {
            builder.AddAttribute("hg-none");
            builder.AddMultipleAttributes(AdditionalAttributes);
            builder.AddContent("None");
        }
        else
        {
            builder.AddAttribute($"hg-{Value.Type}");
            builder.AddMultipleAttributes(AdditionalAttributes);
            switch (Value.Type)
            {
                case KeyType.Ed25519:
                case KeyType.ECDSASecp256K1:
                    var (prefix, value) = GetKeyAsHexParts();
                    builder.OpenElement("span");
                    builder.AddContent(prefix);
                    builder.CloseElement();
                    builder.OpenElement("span");
                    builder.AddContent(value);
                    builder.CloseElement();
                    break;
                case KeyType.Contract:
                    builder.OpenComponent<AddressDisplay>();
                    builder.AddAttribute("Value", Value.Contract);
                    builder.CloseComponent();
                    break;
                case KeyType.List:
                    builder.AddContent($"{Value.RequiredCount} of {Value.List.Length} List");
                    break;
            }
        }
        builder.CloseElement();
    }
    private (string prefix, string value) GetKeyAsHexParts()
    {
        var bytes = Value!.PublicKey.ToArray();
        return bytes.Length > 34 ?
            (Hex.FromBytes(bytes[..^32]), Hex.FromBytes(bytes[^32..])) :
            (string.Empty, Hex.FromBytes(bytes));
    }
}
