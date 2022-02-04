using Hashgraph.Components.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.Globalization;
using System.Linq.Expressions;
using System.Text;

namespace Hashgraph.Components
{
    public partial class InputBinary : ComponentBase
    {
        private InputBinaryEncoding _encoding = InputBinaryEncoding.Text;
        private ReadOnlyMemory<byte> _data;
        private string? _text;
        private string? _textParsingError;
        private FieldIdentifier _fieldIdentifier;
        private string _fieldCssClasses => _editContext.FieldCssClass(_fieldIdentifier);
        private string _otherCssClasses = string.Empty;
        private ValidationMessageStore _validationMessages = default!;
        private UploadFileDialog _uploadFileDialog = default!;
        [Inject] ClipboardService ClipboardService { get; set; } = default!;
        [CascadingParameter] private EditContext _editContext { get; set; } = default!;
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
                    _ = NotifyValueChanged();
                }
            }
        }
        [Parameter] public EventCallback<ReadOnlyMemory<byte>> ValueChanged { get; set; }
        [Parameter] public Expression<Func<ReadOnlyMemory<byte>>>? ValueExpression { get; set; }
        [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

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
            if (AdditionalAttributes != null && AdditionalAttributes.TryGetValue("class", out var clsAttributeObj))
            {
                _otherCssClasses = Convert.ToString(clsAttributeObj, CultureInfo.InvariantCulture) ?? string.Empty;
            }
            _validationMessages = new ValidationMessageStore(_editContext);
            _editContext.OnValidationRequested += (o, e) => UpdateValidationMessages();
        }

        private async Task OnTextChanged(string text)
        {
            _text = text;
            (_data, _textParsingError) = ConvertTextToBinary(_text, _encoding);
            await NotifyValueChanged();
        }

        private async Task OnEncodingChanged(InputBinaryEncoding encoding)
        {
            _encoding = encoding;
            (_data, _textParsingError) = ConvertTextToBinary(_text, _encoding);
            await NotifyValueChanged();
        }
        private async Task OnPasteContentFromClipboard()
        {
            if (ClipboardService.Enabled == true)
            {
                _text = await ClipboardService.ReadFromClipboardAsync();
                (_data, _encoding) = GuessAtBestEncoding(_text);
                _textParsingError = null;
            }
            await NotifyValueChanged();
        }

        private async Task OnUploadFile()
        {
            var data = await _uploadFileDialog!.PromptToUploadFile();
            if (!data.IsEmpty)
            {
                _data = data;
                _encoding = GuessAtBestEncoding(_data);
                _text = ConvertBinaryToText(_data, _encoding);
                _textParsingError = null;
                await NotifyValueChanged();
            }
        }

        private async Task NotifyValueChanged()
        {
            UpdateValidationMessages();
            await ValueChanged.InvokeAsync(Value);
            // note the ? because we can have a race condition
            // where Value is assigned before Initialize() is called
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
        private static string ConvertBinaryToText(ReadOnlyMemory<byte> data, InputBinaryEncoding encoding)
        {
            return encoding switch
            {
                InputBinaryEncoding.Hex => Hex.FromBytes(data),
                InputBinaryEncoding.Base64 => Convert.ToBase64String(data.Span),
                _ => Encoding.Default.GetString(data.Span),
            };
        }
        private static (ReadOnlyMemory<byte> data, string? error) ConvertTextToBinary(string? text, InputBinaryEncoding encoding)
        {
            if (string.IsNullOrEmpty(text))
            {
                return (ReadOnlyMemory<byte>.Empty, null);
            }
            switch (encoding)
            {
                case InputBinaryEncoding.Base64:
                    try
                    {
                        return (Convert.FromBase64String(text), null);
                    }
                    catch (FormatException ex)
                    {
                        return (ReadOnlyMemory<byte>.Empty, $"Unable to recognize content as Base 64: {ex.Message}");
                    }
                case InputBinaryEncoding.Hex:
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
        private static InputBinaryEncoding GuessAtBestEncoding(ReadOnlyMemory<byte> data)
        {
            return data.ToArray().Any(b => b < 9 || b > 126 || (b > 13 && b < 32)) ? InputBinaryEncoding.Hex : InputBinaryEncoding.Text;
        }
        private static (ReadOnlyMemory<byte>, InputBinaryEncoding) GuessAtBestEncoding(string text)
        {
            var (data, error) = ConvertTextToBinary(text, InputBinaryEncoding.Hex);
            if (error == null)
            {
                return (data, InputBinaryEncoding.Hex);
            }
            (data, error) = ConvertTextToBinary(text, InputBinaryEncoding.Base64);
            if (error == null)
            {
                return (data, InputBinaryEncoding.Base64);
            }
            (data, _) = ConvertTextToBinary(text, InputBinaryEncoding.Text);
            return (data, InputBinaryEncoding.Text);
        }
        private enum InputBinaryEncoding
        {
            Text,
            Hex,
            Base64
        }
    }
}