#pragma warning disable CA1308 // Normalize strings to uppercase
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hashgraph.Portal.Components
{
    public class BinaryDisplay : ComponentBase
    {
        private BinaryDisplayEncoding _encoding = BinaryDisplayEncoding.Text;
        private ReadOnlyMemory<byte> _data;
        private bool _supportsClipboard = true;

        [Inject] public IJSRuntime Runtime { get; set; }

        [Parameter] public string Filename { get; set; }

        [Parameter]
        public ReadOnlyMemory<byte> Data
        {
            get
            {
                return _data;
            }
            set
            {
                _data = value;
                _encoding = _data.ToArray().Any(b => b < 9 || b > 126 || (b > 13 && b < 32)) ? BinaryDisplayEncoding.Hex : BinaryDisplayEncoding.Text;
            }
        }
        [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object> AdditionalAttributes { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (builder != null)
            {
                int seq = 0;

                if (Data.IsEmpty)
                {
                    builder.OpenElement(seq++, "div");
                    builder.AddMultipleAttributes(seq++, AdditionalAttributes);
                    builder.AddAttribute(seq++, "class", $"{GetBaseClassAttributes()} empty");
                    builder.AddContent(seq++, "Empty");
                    builder.CloseElement();
                }
                else
                {
                    builder.OpenElement(seq++, "div");
                    builder.AddMultipleAttributes(seq++, AdditionalAttributes);
                    builder.AddAttribute(seq++, "class", GetBaseClassAttributes());
                    builder.OpenElement(seq++, "div");
                    builder.AddAttribute(seq++, "class", $"content {_encoding.ToString().ToLowerInvariant()}");
                    switch (_encoding)
                    {
                        case BinaryDisplayEncoding.Text:
                            builder.AddContent(seq++, Encoding.Default.GetString(_data.Span));
                            break;
                        case BinaryDisplayEncoding.Hex:
                            builder.AddContent(seq++, Hex.FromBytes(_data));
                            break;
                        case BinaryDisplayEncoding.Base64:
                            builder.AddContent(seq++, Convert.ToBase64String(_data.Span));
                            break;
                        case BinaryDisplayEncoding.Bytes:
                            builder.AddMarkupContent(seq++, $"<div>{string.Join("</div> <div>", GetWords(_data.ToArray()))}</div>");
                            break;
                    }
                    builder.CloseElement();
                    builder.OpenElement(seq++, "div");
                    builder.AddAttribute(seq++, "class", "show-as");
                    builder.OpenElement(seq++, "div");
                    builder.AddMarkupContent(seq++, "<span>Show As </span>");

                    builder.OpenElement(seq++, "input");
                    builder.AddAttribute(seq++, "type", "radio");
                    builder.AddAttribute(seq++, "id", "encodeAsText");
                    builder.AddAttribute(seq++, "name", "displayEncoding");
                    builder.AddAttribute(seq++, "value", BinaryDisplayEncoding.Text);
                    builder.AddAttribute(seq++, "checked", _encoding == BinaryDisplayEncoding.Text);
                    builder.AddAttribute(seq++, "onchange", EventCallback.Factory.Create<ChangeEventArgs>(this, () => _encoding = BinaryDisplayEncoding.Text));
                    builder.CloseElement();
                    builder.AddMarkupContent(seq++, "<label for=\"encodeAsText\">Text</label>");

                    builder.OpenElement(seq++, "input");
                    builder.AddAttribute(seq++, "type", "radio");
                    builder.AddAttribute(seq++, "id", "encodeAsHex");
                    builder.AddAttribute(seq++, "name", "displayEncoding");
                    builder.AddAttribute(seq++, "value", BinaryDisplayEncoding.Hex);
                    builder.AddAttribute(seq++, "checked", _encoding == BinaryDisplayEncoding.Hex);
                    builder.AddAttribute(seq++, "onchange", EventCallback.Factory.Create<ChangeEventArgs>(this, () => _encoding = BinaryDisplayEncoding.Hex));
                    builder.CloseElement();
                    builder.AddMarkupContent(seq++, "<label for=\"encodeAsHex\">Hex</label>");

                    builder.OpenElement(seq++, "input");
                    builder.AddAttribute(seq++, "type", "radio");
                    builder.AddAttribute(seq++, "id", "encodeAsBase64");
                    builder.AddAttribute(seq++, "name", "displayEncoding");
                    builder.AddAttribute(seq++, "value", BinaryDisplayEncoding.Base64);
                    builder.AddAttribute(seq++, "checked", _encoding == BinaryDisplayEncoding.Base64);
                    builder.AddAttribute(seq++, "onchange", EventCallback.Factory.Create<ChangeEventArgs>(this, () => _encoding = BinaryDisplayEncoding.Base64));
                    builder.CloseElement();
                    builder.AddMarkupContent(seq++, "<label for=\"encodeAsBase64\">Base 64</label>");

                    builder.OpenElement(seq++, "input");
                    builder.AddAttribute(seq++, "type", "radio");
                    builder.AddAttribute(seq++, "id", "encodeAsBytes");
                    builder.AddAttribute(seq++, "name", "displayEncoding");
                    builder.AddAttribute(seq++, "value", BinaryDisplayEncoding.Bytes);
                    builder.AddAttribute(seq++, "checked", _encoding == BinaryDisplayEncoding.Bytes);
                    builder.AddAttribute(seq++, "onchange", EventCallback.Factory.Create<ChangeEventArgs>(this, () => _encoding = BinaryDisplayEncoding.Bytes));
                    builder.CloseElement();
                    builder.AddMarkupContent(seq++, "<label for=\"encodeAsBytes\">Bytes</label>");
                    builder.CloseElement();

                    if (!string.IsNullOrWhiteSpace(Filename))
                    {
                        builder.OpenElement(seq++, "a");
                        builder.AddAttribute(seq++, "download", Filename);
                        builder.AddAttribute(seq++, "href", $"data:application/octet-stream;base64,{Convert.ToBase64String(_data.Span)}");
                        builder.AddContent(seq++, "download");
                        builder.CloseElement();
                    }

                    if (_supportsClipboard)
                    {
                        builder.OpenElement(seq++, "button");
                        builder.AddAttribute(seq++, "type", "button");
                        builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, CopyDataToClipboard));
                        builder.AddContent(seq++, "Copy");
                        builder.CloseElement();
                    }
                    builder.CloseElement();
                    builder.CloseElement();
                }
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            _supportsClipboard = await Runtime.InvokeAsync<bool>("window.hashgraph.supportsClipboard");
            await base.OnAfterRenderAsync(firstRender);
        }

        private async Task CopyDataToClipboard()
        {
            await Runtime.InvokeVoidAsync("navigator.clipboard.writeText", Encoding.Default.GetString(_data.Span));
        }

        private string GetBaseClassAttributes()
        {
            if (AdditionalAttributes != null && AdditionalAttributes.TryGetValue("class", out var clsAttributeObj))
            {
                var classAttributes = Convert.ToString(clsAttributeObj, CultureInfo.InvariantCulture);
                if (!string.IsNullOrWhiteSpace(classAttributes))
                {
                    return $"binary-display {classAttributes}";
                }
            }
            return "binary-display";
        }

        private static IEnumerable<string> GetWords(IEnumerable<byte> bytes)
        {
            var count = 0;
            var word = new byte[8];
            foreach (byte b in bytes)
            {
                word[count++] = b;
                if (count == 8)
                {
                    yield return string.Join(" ", word.Select(b => b.ToString("X2", CultureInfo.InvariantCulture)));
                    count = 0;
                }
            }
            if (count > 0)
            {
                yield return string.Join(" ", word.Take(count).Select(b => b.ToString("X2", CultureInfo.InvariantCulture)));
            }
        }

        private enum BinaryDisplayEncoding
        {
            Text,
            Hex,
            Base64,
            Bytes
        }
    }
}