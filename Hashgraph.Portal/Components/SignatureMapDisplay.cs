using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Proto;
using System.Collections.Generic;

namespace Hashgraph.Portal.Components
{
    public class SignatureMapDisplay : ComponentBase
    {
        [Parameter] public SignatureMap SignatureMap { get; set; }
        [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object> AdditionalAttributes { get; set; }
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (builder != null)
            {
                builder.OpenElement(0, "div");
                builder.AddMultipleAttributes(1, AdditionalAttributes);
                int count = 2;
                foreach (var signaturePair in SignatureMap.SigPair)
                {
                    count = BuildSignatureMapRow(builder, count, signaturePair);
                }
                builder.CloseElement();
            }
        }
        private int BuildSignatureMapRow(RenderTreeBuilder builder, int count, SignaturePair signaturePair)
        {
            builder.OpenElement(count++, "span");
            builder.AddAttribute(count++, "class", "sig-type");
            builder.AddContent(count++, signaturePair.SignatureCase);
            builder.CloseElement();
            builder.OpenElement(count++, "span");
            builder.AddAttribute(count++, "class", "sig-pub");
            builder.AddContent(count++, GetThumbprintHex(signaturePair));
            builder.CloseElement();
            builder.OpenElement(count++, "span");
            builder.AddAttribute(count++, "class", "sig-hex");
            builder.AddContent(count++, GetSignatureHex(signaturePair));
            builder.CloseElement();
            return count;
        }

        private string GetThumbprintHex(SignaturePair signaturePair)
        {
            return Hex.FromBytes(signaturePair.PubKeyPrefix.ToByteArray());
        }
        private string GetSignatureHex(SignaturePair signaturePair)
        {
            switch (signaturePair.SignatureCase)
            {
                case SignaturePair.SignatureOneofCase.Ed25519: return Hex.FromBytes(signaturePair.Ed25519.ToByteArray());
                case SignaturePair.SignatureOneofCase.RSA3072: return Hex.FromBytes(signaturePair.RSA3072.ToByteArray());
                case SignaturePair.SignatureOneofCase.ECDSA384: return Hex.FromBytes(signaturePair.ECDSA384.ToByteArray());
                case SignaturePair.SignatureOneofCase.Contract: return Hex.FromBytes(signaturePair.Contract.ToByteArray());
            }
            return string.Empty;
        }
    }
}