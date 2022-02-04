using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Proto;

namespace Hashgraph.Components
{
    public class SignatureMapDisplay : ComponentBase
    {
        [Parameter] public SignatureMap? Value { get; set; }
        [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement("div");
            builder.AddAttribute("hg-signature-map-display");
            builder.AddMultipleAttributes(AdditionalAttributes);
            if (Value is not null)
            {
                builder.OpenRegion();
                int count = 0;
                foreach (var signaturePair in Value.SigPair)
                {
                    builder.OpenRegion(count++);
                    BuildSignatureMapRow(builder, signaturePair);
                    builder.CloseRegion();
                }
                builder.CloseRegion();
            }
            builder.CloseElement();
        }
        private void BuildSignatureMapRow(RenderTreeBuilder builder, SignaturePair signaturePair)
        {
            builder.OpenElement("span");
            builder.AddAttribute("hg-sig-type");
            builder.AddContent(signaturePair.SignatureCase);
            builder.CloseElement();
            builder.OpenElement("span");
            builder.AddAttribute("hg-sig-pub");
            builder.AddContent(GetThumbprintHex(signaturePair));
            builder.CloseElement();
            builder.OpenElement("span");
            builder.AddAttribute("hg-sig-hex");
            builder.AddContent(GetSignatureHex(signaturePair));
            builder.CloseElement();
        }

        private static string GetThumbprintHex(SignaturePair signaturePair)
        {
            return Hex.FromBytes(signaturePair.PubKeyPrefix.ToByteArray());
        }
        private static string GetSignatureHex(SignaturePair signaturePair)
        {
            switch (signaturePair.SignatureCase)
            {
                case SignaturePair.SignatureOneofCase.Ed25519: return Hex.FromBytes(signaturePair.Ed25519.ToByteArray());
                case SignaturePair.SignatureOneofCase.ECDSASecp256K1: return Hex.FromBytes(signaturePair.ECDSASecp256K1.ToByteArray());
                case SignaturePair.SignatureOneofCase.Contract: return Hex.FromBytes(signaturePair.Contract.ToByteArray());
            }
            return string.Empty;
        }
    }
}