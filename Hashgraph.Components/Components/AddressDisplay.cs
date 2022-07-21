using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Hashgraph.Components;

public class AddressDisplay : ComponentBase
{
    [Parameter][EditorRequired] public Address? Value { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var address = Value;
        builder.OpenElement("span");
        builder.AddMultipleAttributes(AdditionalAttributes);
        if (address is null || Address.None.Equals(address))
        {
            builder.AddAttribute("hg-address-display");
            builder.AddAttribute("hg-none");
            builder.AddContent("None");
        }
        else if (address.TryGetAlias(out var alias))
        {
            builder.AddAttribute("hg-alias-display");
            builder.AddAttribute($"hg-{alias.Endorsement.Type}");
            builder.AddMultipleAttributes(AdditionalAttributes);
            switch (alias.Endorsement.Type)
            {
                case KeyType.Ed25519:
                case KeyType.ECDSASecp256K1:
                    builder.OpenElement("span");
                    builder.AddContent($"{alias.ShardNum}.{alias.RealmNum}.");
                    builder.CloseElement();
                    builder.OpenElement("span");
                    builder.AddContent(GetRawKeyPart(alias.Endorsement));
                    builder.CloseElement();
                    break;
                case KeyType.Contract:
                    builder.AddContent(Hex.FromBytes(alias.Endorsement.PublicKey));
                    break;
                default:
                    builder.AddContent("Invalid");
                    break;
            }
        }
        else if (address.TryGetMoniker(out var moniker))
        {
            builder.AddAttribute("hg-moniker-display");
            builder.AddMultipleAttributes(AdditionalAttributes);
            builder.AddContent($"{moniker.ShardNum}.{moniker.RealmNum}.");
            builder.AddContent(Hex.FromBytes(moniker.Bytes));
        }
        else
        {
            builder.AddAttribute("hg-address-display");
            builder.AddContent(address.ShardNum);
            builder.AddContent(".");
            builder.AddContent(address.RealmNum);
            builder.AddContent(".");
            builder.AddContent(address.AccountNum);
        }
        builder.CloseElement();
    }
    private static string GetRawKeyPart(Endorsement endorsement)
    {
        var bytes = endorsement.PublicKey.ToArray();
        return bytes.Length > 34 ?
            Hex.FromBytes(bytes[^32..]) :
            Hex.FromBytes(bytes);
    }
}