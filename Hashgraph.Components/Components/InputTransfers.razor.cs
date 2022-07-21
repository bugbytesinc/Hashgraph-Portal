using Hashgraph.Components.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.Globalization;
using System.Linq.Expressions;

namespace Hashgraph.Components
{
    public partial class InputTransfers : ComponentBase, IDisposable
    {
        private bool _simpleMode = true;
        private FieldIdentifier _fieldIdentifier;
        private string _fieldCssClasses => _editContext.FieldCssClass(_fieldIdentifier);
        private string _otherCssClasses = string.Empty;
        private ValidationMessageStore _validationMessages = default!;
        [CascadingParameter] private EditContext _editContext { get; set; } = default!;

        [Parameter] public string Unit { get; set; } = string.Empty;
        [Parameter] public string TransferLabel { get; set; } = string.Empty;
        [Parameter] public CryptoTransferList? Value { get; set; }
        [Parameter] public EventCallback<CryptoTransferList> ValueChanged { get; set; }
        [Parameter] public Expression<Func<CryptoTransferList>>? ValueExpression { get; set; }
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
            if (AdditionalAttributes is not null && AdditionalAttributes.TryGetValue("class", out var clsAttributeObj))
            {
                _otherCssClasses = Convert.ToString(clsAttributeObj, CultureInfo.InvariantCulture) ?? string.Empty;
            }
            _simpleMode = Value is not null && Value.From.Count == 1 && Value.To.Count == 1 && Value.From[0] == Value.To[0];
            _validationMessages = new ValidationMessageStore(_editContext);
            _editContext.OnValidationRequested += OnValidationRequested;
            _editContext.OnFieldChanged += CheckForChildFieldChanges;
        }

        private void CheckForChildFieldChanges(object? sender, FieldChangedEventArgs evt)
        {
            // Quick filter out to those likely contained inside this control
            if (evt.FieldIdentifier.Model is CryptoTransferModel)
            {
                ProcessModelChange();
            }
        }

        private void ToggleSimpleMode()
        {
            if (_simpleMode)
            {
                _simpleMode = false;
                if (Value is not null)
                {
                    Value.To[0].Amount = Value.From[0].Amount;
                }
            }
            else
            {
                if (Value is not null)
                {
                    if (Value.From.Count > 1)
                    {
                        Value.From.RemoveRange(1, Value.From.Count - 1);
                    }
                    else if (Value.From.Count == 0)
                    {
                        Value.From.Add(new CryptoTransferModel());
                    }
                    if (Value.To.Count > 1)
                    {
                        Value.To.RemoveRange(1, Value.To.Count - 1);
                    }
                    else if (Value.To.Count == 0)
                    {
                        Value.To.Add(new CryptoTransferModel());
                    }
                    Value.To[0].Amount = Value.From[0].Amount;
                }
                _simpleMode = true;
            }
            ProcessModelChange();
        }
        private void RemoveToRow(CryptoTransferModel item)
        {
            Value?.To.Remove(item);
            ProcessModelChange();
        }
        private void RemoveFromRow(CryptoTransferModel item)
        {
            Value?.From.Remove(item);
            ProcessModelChange();
        }
        private void AddFromRow()
        {
            Value?.From.Add(new CryptoTransferModel());
            ProcessModelChange();
        }
        private void AddToRow()
        {
            Value?.To.Add(new CryptoTransferModel());
            ProcessModelChange();
        }
        private void ProcessModelChange()
        {
            if (_simpleMode && Value is not null)
            {
                Value.To[0].Amount = Value.From[0].Amount;
            }
            UpdateValidationMessages();
            _ = ValueChanged.InvokeAsync(Value);
            _editContext?.NotifyFieldChanged(_fieldIdentifier);
        }
        private void OnValidationRequested(object? sender, ValidationRequestedEventArgs e)
        {
            UpdateValidationMessages();
        }
        private void UpdateValidationMessages()
        {
            _validationMessages.Clear();
            if (Value is not null)
            {
                var (invalidToAddress, invalidToAmount, sumTo) = ValidateList(Value.To);
                var (invalidFromAddress, invalidFromAmount, sumFrom) = ValidateList(Value.From);
                if (invalidToAddress || invalidFromAddress)
                {
                    _validationMessages.Add(_fieldIdentifier, $"Not all {TransferLabel} Transfer Addresses are Valid.");
                }
                else if (invalidToAmount || invalidFromAmount)
                {
                    _validationMessages.Add(_fieldIdentifier, $"Not all {TransferLabel} Transfer Amounts are Valid.");
                }
                else if (sumTo != sumFrom)
                {
                    _validationMessages.Add(_fieldIdentifier, $"The sum of {TransferLabel} Transfers From and To do not match.");
                }
                else if (Value.ToCryptoTransferList().Any(pair => pair.Amount == 0))
                {
                    _validationMessages.Add(_fieldIdentifier, $"The net sum of {TransferLabel} Transfers is zero for one or more accounts.");
                }
            }
            _editContext.NotifyValidationStateChanged();
        }
        private static (bool invalidAddress, bool invalidAmount, long sum) ValidateList(List<CryptoTransferModel> list)
        {
            bool invalidAddress = false;
            bool invalidAmount = false;
            long sum = 0;
            foreach (var xfer in list)
            {
                invalidAmount = invalidAmount || xfer.Amount.GetValueOrDefault() <= 0;
                invalidAddress = invalidAddress || xfer.Address == null;
                sum = sum + xfer.Amount.GetValueOrDefault();
            }
            return (invalidAddress, invalidAmount, sum);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _editContext.OnValidationRequested -= OnValidationRequested;
                _editContext.OnFieldChanged -= CheckForChildFieldChanges;
                if (_validationMessages != null)
                {
                    _validationMessages.Clear();
                }
                InvokeAsync(() => _editContext.NotifyValidationStateChanged());
            }
        }
    }
}