using Hashgraph.Components.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using System.Globalization;
using System.Text;

namespace Hashgraph.Components
{
    public class BinaryDisplay : ComponentBase
    {
        private BinaryDisplayEncoding _encoding = BinaryDisplayEncoding.Text;
        private ReadOnlyMemory<byte> _data;

        [Inject] public ClipboardService ClipboardService { get; set; } = default!;

        [Parameter] public string? Filename { get; set; }

        [Parameter]
        [EditorRequired]
        public ReadOnlyMemory<byte> Value
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
        [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement("div");
            builder.AddAttribute("hg-binary-display");
            if (_data.IsEmpty)
            {
                builder.AddAttribute("hg-none");
                builder.AddMultipleAttributes(AdditionalAttributes);
                builder.AddContent("Empty");
                builder.CloseElement();
            }
            else
            {
                builder.AddMultipleAttributes(AdditionalAttributes);
                builder.OpenElement("div");
                switch (_encoding)
                {
                    case BinaryDisplayEncoding.Text:
                        builder.AddAttribute($"hg-binary-type-text");
                        builder.AddContent(Encoding.Default.GetString(_data.Span));
                        break;
                    case BinaryDisplayEncoding.Hex:
                        builder.AddAttribute($"hg-binary-type-hex");
                        builder.AddContent(Hex.FromBytes(_data));
                        break;
                    case BinaryDisplayEncoding.Base64:
                        builder.AddAttribute($"hg-binary-type-base64");
                        builder.AddContent(Convert.ToBase64String(_data.Span));
                        break;
                    case BinaryDisplayEncoding.Bytes:
                        builder.AddAttribute($"hg-binary-type-bytes");
                        builder.AddMarkupContent($"<div>{string.Join("</div> <div>", GetWords(_data.ToArray()))}</div>");
                        break;
                }
                builder.CloseElement();
                builder.OpenElement("div");
                builder.AddAttribute("class", "show-as");
                builder.OpenElement("div");
                builder.AddMarkupContent("<span>Show As</span>");
                builder.OpenElement("label");
                builder.OpenElement("input");
                builder.AddAttribute("type", "radio");
                builder.AddAttribute("value", BinaryDisplayEncoding.Text);
                builder.AddAttribute("checked", _encoding == BinaryDisplayEncoding.Text);
                builder.AddAttribute("onchange", EventCallback.Factory.Create<ChangeEventArgs>(this, () => _encoding = BinaryDisplayEncoding.Text));
                builder.CloseElement();
                builder.AddContent("Text");
                builder.CloseElement();

                builder.OpenElement("label");
                builder.OpenElement("input");
                builder.AddAttribute("type", "radio");
                builder.AddAttribute("value", BinaryDisplayEncoding.Hex);
                builder.AddAttribute("checked", _encoding == BinaryDisplayEncoding.Hex);
                builder.AddAttribute("onchange", EventCallback.Factory.Create<ChangeEventArgs>(this, () => _encoding = BinaryDisplayEncoding.Hex));
                builder.CloseElement();
                builder.AddContent("Hex");
                builder.CloseElement();

                builder.OpenElement("label");
                builder.OpenElement("input");
                builder.AddAttribute("type", "radio");
                builder.AddAttribute("value", BinaryDisplayEncoding.Base64);
                builder.AddAttribute("checked", _encoding == BinaryDisplayEncoding.Base64);
                builder.AddAttribute("onchange", EventCallback.Factory.Create<ChangeEventArgs>(this, () => _encoding = BinaryDisplayEncoding.Base64));
                builder.CloseElement();
                builder.AddContent("Base 64");
                builder.CloseElement();

                builder.OpenElement("label");
                builder.OpenElement("input");
                builder.AddAttribute("type", "radio");
                builder.AddAttribute("value", BinaryDisplayEncoding.Bytes);
                builder.AddAttribute("checked", _encoding == BinaryDisplayEncoding.Bytes);
                builder.AddAttribute("onchange", EventCallback.Factory.Create<ChangeEventArgs>(this, () => _encoding = BinaryDisplayEncoding.Bytes));
                builder.CloseElement();
                builder.AddContent("Bytes");
                builder.CloseElement();
                builder.CloseElement();

                if (!string.IsNullOrWhiteSpace(Filename))
                {
                    builder.OpenElement("a");
                    builder.AddAttribute("download", Filename);
                    builder.AddAttribute("href", $"data:application/octet-stream;base64,{Convert.ToBase64String(_data.Span)}");
                    builder.AddContent("download");
                    builder.CloseElement();
                }

                if (ClipboardService?.Enabled == true)
                {
                    builder.OpenElement("button");
                    builder.AddAttribute("type", "button");
                    builder.AddAttribute("onclick", EventCallback.Factory.Create<MouseEventArgs>(this, CopyDataToClipboard));
                    builder.AddContent("Copy");
                    builder.CloseElement();
                }
                builder.CloseElement();
                builder.CloseElement();
            }
        }

        private async Task CopyDataToClipboard()
        {
            if (ClipboardService is not null)
            {
                if (_data.ToArray().Any(b => b < 9 || b > 126 || (b > 13 && b < 32)))
                {
                    await ClipboardService.WriteToClipboardAsync(Hex.FromBytes(_data));
                }
                else
                {
                    await ClipboardService.WriteToClipboardAsync(Encoding.Default.GetString(_data.Span));
                }
            }
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