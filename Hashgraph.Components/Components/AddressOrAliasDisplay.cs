using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Hashgraph.Components;

public class AddressOrAliasDisplay : ComponentBase
{
    [Parameter] [EditorRequired] public AddressOrAlias? Value { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var value = Value;
        if (value == null || AddressOrAlias.None.Equals(value))
        {
            builder.AddMarkupContent("<span hg-address-display hg-none>None</span>");
        }
        else if (value.Endorsement is null)
        {
            builder.OpenElement("span");
            builder.AddAttribute("hg-address-display");
            builder.AddContent(value.ShardNum);
            builder.AddContent(".");
            builder.AddContent(value.RealmNum);
            builder.AddContent(".");
            builder.AddContent(value.AccountNum);
            builder.CloseElement();
        }
        else
        {
            builder.OpenElement("span");
            builder.AddAttribute("hg-alias-display");
            builder.AddAttribute($"hg-{value.Endorsement.Type}");
            builder.AddMultipleAttributes(AdditionalAttributes);
            switch (value.Endorsement.Type)
            {
                case KeyType.Ed25519:
                case KeyType.ECDSASecp256K1:
                    builder.OpenElement("span");
                    builder.AddContent($"{value.ShardNum}.{value.RealmNum}.");
                    builder.CloseElement();
                    builder.OpenElement("span");
                    builder.AddContent(GetRawKeyPart(value.Endorsement));
                    builder.CloseElement();
                    break;
                case KeyType.Contract:
                    builder.AddContent(Hex.FromBytes(value.Endorsement.PublicKey));
                    break;
                default:
                    builder.AddContent("Invalid");
                    break;
            }
            builder.CloseElement();
        }

    }
    private static string GetRawKeyPart(Endorsement endorsement)
    {
        var bytes = endorsement.PublicKey.ToArray();
        return bytes.Length > 34 ?
            Hex.FromBytes(bytes[^32..]) :
            Hex.FromBytes(bytes);
    }
}