using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using System.Globalization;
using System.Linq.Expressions;

namespace Hashgraph.Components
{
    public class InputAbi : ComponentBase
    {
        private const string ARG_BOOL = "bool";
        private const string ARG_INT = "int32";
        private const string ARG_LONG = "int64";
        private const string ARG_UINT = "uint32";
        private const string ARG_ULONG = "uint64";
        private const string ARG_STRING = "string";
        private const string ARG_BYTES = "bytes";
        private const string ARG_ADDRESS = "address";
        private static readonly string[] _types =
        {
            ARG_BOOL,
            ARG_INT,
            ARG_LONG,
            ARG_UINT,
            ARG_ULONG,
            ARG_STRING,
            ARG_BYTES,
            ARG_ADDRESS
        };
        private readonly List<Argument> _arguments = new List<Argument>();
        private FieldIdentifier _fieldIdentifier;
        private string _fieldCssClasses => _editContext.FieldCssClass(_fieldIdentifier);
        private string _otherCssClasses = string.Empty;
        private ValidationMessageStore _validationMessages = default!;
        private bool _renderAddArgumentSelect = true;
        [CascadingParameter] private EditContext _editContext { get; set; } = default!;
        [Parameter]
        public ReadOnlyMemory<object> Value
        {
            get
            {
                return _arguments.Select(a => a.Value).ToArray();
            }
            set
            {
                var values = value.ToArray();
                if (!_arguments.Select(a => a.Value).SequenceEqual(values))
                {
                    if (Array.IndexOf(values, null) > -1)
                    {
                        throw new ArgumentNullException(nameof(Value), "Value of argument cannot be null");
                    }
                    _arguments.Clear();
                    _arguments.AddRange(values.Select(v => new Argument(v)));
                    NotifyValueChanged();
                }
            }
        }
        [Parameter] public EventCallback<ReadOnlyMemory<object>> ValueChanged { get; set; }
        [Parameter] public Expression<Func<ReadOnlyMemory<object>>>? ValueExpression { get; set; }
        [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement("div");
            builder.AddAttribute("hg-abi-input");
            builder.AddMultipleAttributes(AdditionalAttributes);
            builder.AddAttribute("class", $"{_otherCssClasses} {_fieldCssClasses}");
            builder.OpenRegion();
            for (int i = 0; i < _arguments.Count; i++)
            {
                var index = i; // Capture for onchange callback below
                builder.OpenRegion(index);
                var arg = _arguments[index];
                if (arg.Type == ARG_BOOL)
                {
                    builder.OpenElement("label");
                    builder.OpenElement("input");
                    builder.AddAttribute("type", "checkbox");
                    if (!arg.Valid)
                    {
                        builder.AddAttribute("class", "invalid");
                    }
                    builder.AddAttribute("value", arg.Text);
                    builder.AddAttribute("onchange", EventCallback.Factory.CreateBinder<string>(this, __value => { arg.Text = __value; NotifyValueChanged(); }, arg.Text));
                    builder.CloseElement();
                    builder.AddContent(object.Equals(true, arg.Value) ? "True" : "False");
                    builder.CloseElement();
                }
                else
                {
                    builder.OpenElement("input");
                    builder.AddAttribute("type", "text");
                    if (!arg.Valid)
                    {
                        builder.AddAttribute("class", "invalid");
                    }
                    builder.AddAttribute("value", arg.Text);
                    builder.AddAttribute("onchange", EventCallback.Factory.CreateBinder<string>(this, __value => { arg.Text = __value; NotifyValueChanged(); }, arg.Text));
                    builder.CloseElement();
                }
                builder.OpenElement("select");
                builder.AddAttribute("onchange", EventCallback.Factory.CreateBinder<string>(this, newType => ChangArgumentType(index, arg, newType), arg.Type));
                builder.OpenRegion();
                RenderArgumentOptions(builder, arg.Type);
                builder.CloseRegion();
                builder.OpenElement("option");
                builder.AddAttribute("value", "delete");
                builder.AddContent("Delete...");
                builder.CloseElement();
                builder.CloseElement();
                builder.CloseRegion();
            }
            builder.CloseRegion();
            builder.OpenElement("div");
            builder.CloseElement();
            if(_renderAddArgumentSelect)
            {
                builder.OpenElement("select");
                builder.AddAttribute("class", "add-argument");
                builder.AddAttribute("onchange", EventCallback.Factory.CreateBinder<string>(this, OnAddArgument, ""));
                builder.OpenElement("option");
                builder.AddContent("Add...");
                builder.CloseElement();
                builder.OpenRegion();
                RenderArgumentOptions(builder, null);
                builder.CloseRegion();
                builder.CloseElement();
            }
            builder.CloseElement();
        }
        private void ChangArgumentType(int index, Argument arg, string newType)
        {
            if (newType == "delete")
            {
                _arguments.RemoveAt(index);
            }
            else
            {
                arg.Type = newType;
            }
            NotifyValueChanged();
        }
        protected override void OnInitialized()
        {
            if (_editContext == null)
            {
                throw new InvalidOperationException($"{GetType()} requires a cascading parameter " +
                    $"of type {nameof(EditContext)}. For example, you can use {GetType().FullName} inside " +
                    $"an {nameof(EditForm)}.");
            }

            if (ValueExpression == null)
            {
                throw new InvalidOperationException($"{GetType()} requires a value for the 'ValueExpression' " +
                    $"parameter. Normally this is provided automatically when using 'bind-Value'.");
            }
            _fieldIdentifier = FieldIdentifier.Create(ValueExpression);
            if (AdditionalAttributes is not null && AdditionalAttributes.TryGetValue("class", out var clsAttributeObj))
            {
                _otherCssClasses = Convert.ToString(clsAttributeObj, CultureInfo.InvariantCulture) ?? string.Empty;
            }
            _validationMessages = new ValidationMessageStore(_editContext);
            _editContext.OnValidationRequested += (o, e) => UpdateValidationMessages();
        }
        private void OnAddArgument(string type)
        {
            if (!string.IsNullOrWhiteSpace(type))
            {
                _renderAddArgumentSelect = false;
                _arguments.Add(new Argument(string.Empty) { Type = type });
                NotifyValueChanged();
                // Workaround to get the add select 
                // to re-render with "Add..." otherwise
                // we'd have to write some javascript
                // to reset the value manually since it
                // will not re-render on it own.
                _ = Task.Run(async () => {
                    await Task.Delay(1);
                    _renderAddArgumentSelect = true;
                    await InvokeAsync(StateHasChanged);
                });
            }
        }
        private void NotifyValueChanged()
        {
            _ = ValueChanged.InvokeAsync(Value);
            _editContext.NotifyFieldChanged(_fieldIdentifier);
        }
        private void UpdateValidationMessages()
        {
            if (_validationMessages != null)
            {
                _validationMessages.Clear();
                for (int i = 0; i < _arguments.Count; i++)
                {
                    _arguments[i].CheckValidation(_validationMessages, _fieldIdentifier, i);
                }
                _editContext.NotifyValidationStateChanged();
            }
        }
        private static void RenderArgumentOptions(RenderTreeBuilder builder, string? selected)
        {
            var seq = 0;
            foreach (var name in _types)
            {
                builder.OpenRegion(seq++);
                builder.OpenElement("option");
                builder.AddAttribute("value", name);
                if (name == selected)
                {
                    builder.AddAttribute("selected", "selected");
                }
                builder.AddContent(name);
                builder.CloseElement();
                builder.CloseRegion();
            }
        }
        protected class Argument
        {
            internal Argument(object value)
            {
                switch (value)
                {
                    case bool b:
                        Text = b.ToString(CultureInfo.InvariantCulture);
                        Type = ARG_BOOL;
                        break;
                    case int i:
                        Text = i.ToString(CultureInfo.InvariantCulture);
                        Type = ARG_INT;
                        break;
                    case long l:
                        Text = l.ToString(CultureInfo.InvariantCulture);
                        Type = ARG_LONG;
                        break;
                    case uint ui:
                        Text = ui.ToString(CultureInfo.InvariantCulture);
                        Type = ARG_UINT;
                        break;
                    case ulong ul:
                        Text = ul.ToString(CultureInfo.InvariantCulture);
                        Type = ARG_ULONG;
                        break;
                    case string s:
                        Text = s;
                        Type = ARG_STRING;
                        break;
                    case byte[] b:
                        Text = Hex.FromBytes(b);
                        Type = ARG_BYTES;
                        break;
                    case Address a:
                        Text = $"{a.ShardNum}.{a.RealmNum}.{a.RealmNum}";
                        Type = ARG_ADDRESS;
                        break;
                    default:
                        throw new ArgumentException("Value", $"type {value.GetType()} is not presently supported.");
                }
                Valid = true;
            }

            public string Text { get; set; }
            public string Type { get; set; }
            public bool Valid { get; private set; }

            public object Value
            {
                get
                {
                    try
                    {
                        switch (Type)
                        {
                            case ARG_BOOL: return Convert.ToBoolean(Text, CultureInfo.InvariantCulture);
                            case ARG_INT: return Convert.ToInt32(Text, CultureInfo.InvariantCulture);
                            case ARG_LONG: return Convert.ToInt64(Text, CultureInfo.InvariantCulture);
                            case ARG_UINT: return Convert.ToUInt32(Text, CultureInfo.InvariantCulture);
                            case ARG_ULONG: return Convert.ToUInt64(Text, CultureInfo.InvariantCulture);
                            case ARG_STRING: return Text;
                            case ARG_BYTES: return Hex.ToBytes(Text);
                            case ARG_ADDRESS:
                                if (!string.IsNullOrWhiteSpace(Text))
                                {
                                    var parts = Text.Split('.');
                                    if (parts.Length == 3)
                                    {
                                        if (uint.TryParse(parts[0], out uint shard) &&
                                            uint.TryParse(parts[1], out uint realm) &&
                                            uint.TryParse(parts[2], out uint number))
                                        {
                                            return new Address(shard, realm, number);
                                        }
                                    }
                                }
                                return Address.None;
                        }
                    }
                    catch
                    {
                        switch (Type)
                        {
                            case ARG_BOOL: return false;
                            case ARG_INT: return 0;
                            case ARG_LONG: return 0L;
                            case ARG_UINT: return 0U;
                            case ARG_ULONG: return 0UL;
                            case ARG_STRING: return string.Empty;
                            case ARG_BYTES: return Array.Empty<byte>();
                            case ARG_ADDRESS: return Address.None;
                        }
                    }
                    return string.Empty;
                }
            }

            internal void CheckValidation(ValidationMessageStore validationMessages, FieldIdentifier fieldIdentifier, int index)
            {
                switch (Type)
                {
                    case ARG_BOOL:
                        if (!(Valid = bool.TryParse(Text, out _)))
                        {
                            validationMessages.Add(fieldIdentifier, $"Argument {index} is not a valid boolean value.");
                        }
                        break;
                    case ARG_INT:
                        if (!(Valid = int.TryParse(Text, out _)))
                        {
                            validationMessages.Add(fieldIdentifier, $"Argument {index} is not a valid 32 bit integer value.");
                        }
                        break;
                    case ARG_LONG:
                        if (!(Valid = long.TryParse(Text, out _)))
                        {
                            validationMessages.Add(fieldIdentifier, $"Argument {index} is not a valid 64 bit integer value.");
                        }
                        break;
                    case ARG_UINT:
                        if (!(Valid = uint.TryParse(Text, out _)))
                        {
                            validationMessages.Add(fieldIdentifier, $"Argument {index} is not a valid unsigned 32 bit integer value.");
                        }
                        break;
                    case ARG_ULONG:
                        if (!(Valid = ulong.TryParse(Text, out _)))
                        {
                            validationMessages.Add(fieldIdentifier, $"Argument {index} is not a valid unsigned 64 bit integer value.");
                        }
                        break;
                    case ARG_STRING:
                        Valid = true;
                        break;
                    case ARG_BYTES:
                        try
                        {
                            Hex.ToBytes(Text);
                            Valid = true;
                        }
                        catch
                        {
                            Valid = false;
                            validationMessages.Add(fieldIdentifier, $"Argument {index} is not a valid hex value.");
                        }
                        break;
                    case ARG_ADDRESS:
                        if (!string.IsNullOrWhiteSpace(Text))
                        {
                            var parts = Text.Split('.');
                            if (parts.Length == 3)
                            {
                                if (uint.TryParse(parts[0], out uint shard) &&
                                    uint.TryParse(parts[1], out uint realm) &&
                                    uint.TryParse(parts[2], out uint number))
                                {
                                    Valid = true;
                                    break;
                                }
                            }
                        }
                        validationMessages.Add(fieldIdentifier, $"Argument {index} is not a valid address value.");
                        Valid = false;
                        break;
                }
            }
        }
    }
}