using Hashgraph.Portal.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;

namespace Hashgraph.Portal.Components
{
    public partial class InputTransfers : ComponentBase
    {
        private bool _simpleMode = true;
        private FieldIdentifier _fieldIdentifier;
        private string _fieldCssClasses => _editContext?.FieldCssClass(_fieldIdentifier) ?? string.Empty;
        private string _otherCssClasses = string.Empty;
        private ValidationMessageStore _validationMessages;
        [CascadingParameter] private EditContext _editContext { get; set; }

        [Parameter] public CryptoTransferList Value { get; set; }
        [Parameter] public EventCallback<CryptoTransferList> ValueChanged { get; set; }
        [Parameter] public Expression<Func<CryptoTransferList>> ValueExpression { get; set; }
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
            _simpleMode = Value != null && Value.From.Count == 1 && Value.To.Count == 1 && Value.From[0] == Value.To[0];
            _validationMessages = new ValidationMessageStore(_editContext);
            _editContext.OnValidationRequested += (o, e) => UpdateValidationMessages();
            _editContext.OnFieldChanged += CheckForChildFieldChanges;
        }

        private void CheckForChildFieldChanges(object sender, FieldChangedEventArgs evt)
        {
            // Quick filter out to those likely contained inside this control
            if (evt.FieldIdentifier.Model is CryptoTransfer)
            {
                ProcessModelChange();
            }
        }

        private void ToggleSimpleMode()
        {
            if (_simpleMode)
            {
                _simpleMode = false;
                Value.To[0].Amount = Value.From[0].Amount;
            }
            else
            {
                if (Value.From.Count > 1)
                {
                    Value.From.RemoveRange(1, Value.From.Count - 1);
                }
                else if (Value.From.Count == 0)
                {
                    Value.From.Add(new CryptoTransfer());
                }
                if (Value.To.Count > 1)
                {
                    Value.To.RemoveRange(1, Value.To.Count - 1);
                }
                else if (Value.To.Count == 0)
                {
                    Value.To.Add(new CryptoTransfer());
                }
                Value.To[0].Amount = Value.From[0].Amount;
                _simpleMode = true;
            }
            ProcessModelChange();
        }
        private void RemoveToRow(CryptoTransfer item)
        {
            Value.To.Remove(item);
            ProcessModelChange();
        }
        private void RemoveFromRow(CryptoTransfer item)
        {
            Value.From.Remove(item);
            ProcessModelChange();
        }
        private void AddFromRow()
        {
            Value.From.Add(new CryptoTransfer());
            ProcessModelChange();
        }
        private void AddToRow()
        {
            Value.To.Add(new CryptoTransfer());
            ProcessModelChange();
        }
        private void ProcessModelChange()
        {
            if (_simpleMode && Value != null)
            {
                Value.To[0].Amount = Value.From[0].Amount;
            }
            UpdateValidationMessages();
            _ = ValueChanged.InvokeAsync(Value);
            _editContext?.NotifyFieldChanged(_fieldIdentifier);
        }
        private void UpdateValidationMessages()
        {
            if (_validationMessages != null)
            {
                _validationMessages.Clear();
                if (Value != null)
                {
                    var (invalidToAddress, invalidToAmount, sumTo) = ValidateList(Value.To);
                    var (invalidFromAddress, invalidFromAmount, sumFrom) = ValidateList(Value.From);
                    if (invalidToAddress || invalidFromAddress)
                    {
                        _validationMessages.Add(_fieldIdentifier, "Not all Transfer Addresses are Valid.");
                    }
                    else if (invalidToAmount || invalidFromAmount)
                    {
                        _validationMessages.Add(_fieldIdentifier, "Not all Transfer Amounts are Valid.");
                    }
                    else if (sumTo != sumFrom)
                    {
                        _validationMessages.Add(_fieldIdentifier, "The sum of Transfers From and To do not match.");
                    }
                    else if (Value.ToTransferDictionary().Any(pair => pair.Value == 0))
                    {
                        _validationMessages.Add(_fieldIdentifier, "The net sum of transfers is zero for one or more accounts.");
                    }
                }
                _editContext.NotifyValidationStateChanged();
            }
        }
        private static (bool invalidAddress, bool invalidAmount, long sum) ValidateList(List<CryptoTransfer> list)
        {
            bool invalidAddress = false;
            bool invalidAmount = false;
            long sum = 0;
            foreach (var xfer in list)
            {
                invalidAmount = invalidAmount || xfer.Amount <= 0;
                invalidAddress = invalidAddress || xfer.Address == null;
                sum = sum + xfer.Amount;
            }
            return (invalidAddress, invalidAmount, sum);
        }
    }
}