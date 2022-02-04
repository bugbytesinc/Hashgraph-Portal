using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Hashgraph.Components;

public class AliasDisplay : ComponentBase
{
    [Parameter] [EditorRequired] public Alias? Value { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement("span");
        builder.AddAttribute("hg-alias-display");
        if (Value is null || Alias.None.Equals(Value))
        {
            builder.AddAttribute("hg-none");
            builder.AddMultipleAttributes(AdditionalAttributes);
            builder.AddContent("None");
        }
        else
        {
            builder.AddAttribute($"hg-{Value.Endorsement.Type}");
            builder.AddMultipleAttributes(AdditionalAttributes);
            switch (Value.Endorsement.Type)
            {
                case KeyType.Ed25519:
                case KeyType.ECDSASecp256K1:
                    builder.OpenElement("span");
                    builder.AddContent($"{Value.ShardNum}.{Value.RealmNum}.");
                    builder.CloseElement();
                    builder.OpenElement("span");
                    builder.AddContent(GetRawKeyPart());
                    builder.CloseElement();
                    break;
                case KeyType.Contract:
                    builder.AddContent(Hex.FromBytes(Value.Endorsement.PublicKey));
                    break;
                default:
                    builder.AddContent("Invalid");
                    break;
            }
        }
        builder.CloseElement();
    }
    private string GetRawKeyPart()
    {
        var bytes = Value!.Endorsement.PublicKey.ToArray();
        return bytes.Length > 34 ?
            Hex.FromBytes(bytes[^32..]) :
            Hex.FromBytes(bytes);
    }
}
