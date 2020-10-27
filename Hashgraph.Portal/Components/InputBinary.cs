using Hashgraph.Portal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Hashgraph.Portal.Components
{
    public partial class InputBinary : ComponentBase
    {
        private BinaryInputEncoding _encoding = BinaryInputEncoding.Text;
        private ReadOnlyMemory<byte> _data;
        private string _text;
        private string _textParsingError;
        private FieldIdentifier _fieldIdentifier;
        private string _fieldCssClasses => _editContext?.FieldCssClass(_fieldIdentifier) ?? string.Empty;
        private string _otherCssClasses = string.Empty;
        private ValidationMessageStore _validationMessages;
        private UploadFileDialog _uploadFileDialog;
        [Inject] ClipboardService ClipboardService { get; set; }
        [CascadingParameter] private EditContext _editContext { get; set; }
        [Parameter]
        public ReadOnlyMemory<byte> Value
        {
            get
            {
                return _data;
            }
            set
            {
                if (!_data.Equals(value))
                {
                    _data = value;
                    _encoding = GuessAtBestEncoding(_data);
                    _text = ConvertBinaryToText(_data, _encoding);
                    _textParsingError = null;
                    NotifyValueChanged();
                }
            }
        }
        [Parameter] public EventCallback<ReadOnlyMemory<byte>> ValueChanged { get; set; }
        [Parameter] public Expression<Func<ReadOnlyMemory<byte>>> ValueExpression { get; set; }
        [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object> AdditionalAttributes { get; set; }

        protected override void OnInitialized()
        {
            if (_editContext == null)
            {
                throw new InvalidOperationException($"{GetType()} requires a cascading parameter " +
                    $"of type {nameof(Microsoft.AspNetCore.Components.Forms.EditContext)}. For example, you can use {GetType().FullName} inside " +
                    $"an {nameof(EditForm)}.");
            }

            if (ValueExpression == null)
            {
                throw new InvalidOperationException($"{GetType()} requires a value for the 'ValueExpression' " +
                    $"parameter. Normally this is provided automatically when using 'bind-Value'.");
            }
            _fieldIdentifier = FieldIdentifier.Create(ValueExpression);
            _otherCssClasses = AdditionalAttributes != null && AdditionalAttributes.TryGetValue("class", out var clsAttributeObj) ?
                Convert.ToString(clsAttributeObj, CultureInfo.InvariantCulture) :
                string.Empty;
            _validationMessages = new ValidationMessageStore(_editContext);
            _editContext.OnValidationRequested += (o, e) => UpdateValidationMessages();
        }

        private void OnTextChanged(string text)
        {
            _text = text;
            (_data, _textParsingError) = ConvertTextToBinary(_text, _encoding);
            NotifyValueChanged();
        }

        private void OnEncodingChanged(BinaryInputEncoding encoding)
        {
            _encoding = encoding;
            (_data, _textParsingError) = ConvertTextToBinary(_text, _encoding);
            NotifyValueChanged();
        }
        private async Task OnPasteContentFromClipboard()
        {
            if (ClipboardService.Enabled)
            {
                _text = await ClipboardService.ReadFromClipboardAsync();
                (_data, _encoding) = GuessAtBestEncoding(_text);
                _textParsingError = null;
            }
            NotifyValueChanged();
        }

        private async Task OnUploadFile()
        {
            var data = await _uploadFileDialog.PromptToUploadFile();
            if(!data.IsEmpty)
            {
                _data = data;
                _encoding = GuessAtBestEncoding(_data);
                _text = ConvertBinaryToText(_data, _encoding);
                _textParsingError = null;
                NotifyValueChanged();
            }
        }

        // when file upload process model chane

        private void NotifyValueChanged()
        {
            UpdateValidationMessages();
            _ = ValueChanged.InvokeAsync(Value);
            _editContext?.NotifyFieldChanged(_fieldIdentifier);
        }

        private void UpdateValidationMessages()
        {
            if (_validationMessages != null)
            {
                _validationMessages.Clear();
                if (!string.IsNullOrWhiteSpace(_textParsingError))
                {
                    _validationMessages.Add(_fieldIdentifier, _textParsingError);
                }
                _editContext.NotifyValidationStateChanged();
            }
        }
        private static string ConvertBinaryToText(ReadOnlyMemory<byte> data, BinaryInputEncoding encoding)
        {
            switch (encoding)
            {
                case BinaryInputEncoding.Hex: return Hex.FromBytes(data);
                case BinaryInputEncoding.Base64: return Convert.ToBase64String(data.Span);
            }
            return Encoding.Default.GetString(data.Span);
        }
        private static (ReadOnlyMemory<byte> data, string error) ConvertTextToBinary(string text, BinaryInputEncoding encoding)
        {
            if (string.IsNullOrEmpty(text))
            {
                return (ReadOnlyMemory<byte>.Empty, null);
            }
            switch (encoding)
            {
                case BinaryInputEncoding.Base64:
                    try
                    {
                        return (Convert.FromBase64String(text), null);
                    }
                    catch (FormatException ex)
                    {
                        return (ReadOnlyMemory<byte>.Empty, $"Unable to recognize content as Base 64: {ex.Message}");
                    }
                case BinaryInputEncoding.Hex:
                    try
                    {
                        return (Hex.ToBytes(text), null);
                    }
                    catch (ArgumentException ex)
                    {
                        return (ReadOnlyMemory<byte>.Empty, $"Unable to recognize content as HEX: {ex.Message}");
                    }
                default:
                    try
                    {
                        return (Encoding.Default.GetBytes(text), null);
                    }
                    catch (ArgumentException ex)
                    {
                        return (ReadOnlyMemory<byte>.Empty, $"Unable to recognize content as text: {ex.Message}");
                    }
            }
        }
        private static BinaryInputEncoding GuessAtBestEncoding(ReadOnlyMemory<byte> data)
        {
            return data.ToArray().Any(b => b < 9 || b > 126 || (b > 13 && b < 32)) ? BinaryInputEncoding.Hex : BinaryInputEncoding.Text;
        }
        private static (ReadOnlyMemory<byte>, BinaryInputEncoding) GuessAtBestEncoding(string text)
        {
            var (data, error) = ConvertTextToBinary(text, BinaryInputEncoding.Hex);
            if (error == null)
            {
                return (data, BinaryInputEncoding.Hex);
            }
            (data, error) = ConvertTextToBinary(text, BinaryInputEncoding.Base64);
            if (error == null)
            {
                return (data, BinaryInputEncoding.Base64);
            }
            (data, error) = ConvertTextToBinary(text, BinaryInputEncoding.Text);
            return (data, BinaryInputEncoding.Text);
        }
        private enum BinaryInputEncoding
        {
            Text,
            Hex,
            Base64
        }
    }
}