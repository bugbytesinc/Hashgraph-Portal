using Hashgraph.Components.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.Globalization;
using System.Linq.Expressions;

namespace Hashgraph.Components
{
    public partial class InputAssetTransfers : ComponentBase, IDisposable
    {
        private FieldIdentifier _fieldIdentifier;
        private string _fieldCssClasses => _editContext.FieldCssClass(_fieldIdentifier);
        private string _otherCssClasses = string.Empty;
        private ValidationMessageStore _validationMessages = default!;
        [CascadingParameter] private EditContext _editContext { get; set; } = default!;

        [Parameter] public AssetTransferGroup? Value { get; set; }
        [Parameter] public EventCallback<AssetTransferGroup> ValueChanged { get; set; }
        [Parameter] public Expression<Func<AssetTransferGroup>>? ValueExpression { get; set; }
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
            _validationMessages = new ValidationMessageStore(_editContext);
            _editContext.OnValidationRequested += OnValidationRequested;
            _editContext.OnFieldChanged += CheckForChildFieldChanges;
        }

        private void CheckForChildFieldChanges(object? sender, FieldChangedEventArgs evt)
        {
            // Quick filter out to those likely contained inside this control
            if (evt.FieldIdentifier.Model is AssetTransferGroup)
            {
                ProcessModelChange();
            }
        }

        private void ProcessModelChange()
        {
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
                if (Value.Token.IsNullOrNone())
                {
                    _validationMessages.Add(_fieldIdentifier, $"Missing Asset Token Address.");
                }
                if (Value.From.IsNullOrNone())
                {
                    _validationMessages.Add(_fieldIdentifier, $"Missing Asset Source Account.");
                }
                if (Value.To.IsNullOrNone())
                {
                    _validationMessages.Add(_fieldIdentifier, $"Missing Asset Destination Account.");
                }
                var parts = Value.SerialNumbers?.Split(',') ?? Array.Empty<string>();
                if (parts.Length == 0)
                {
                    _validationMessages.Add(_fieldIdentifier, "Please enter the a comma seperated list of serial numbers to transfer between accounts.");
                }
                for (int i = 0; i < parts.Length; i++)
                {
                    if (long.TryParse(parts[i].Trim(), out long serialNumber))
                    {
                        if (serialNumber < 0)
                        {
                            _validationMessages.Add(_fieldIdentifier, $"Serial Number at location {i + 1} must be a positive value.");
                        }
                    }
                    else
                    {
                        _validationMessages.Add(_fieldIdentifier, $"Serial Number {parts[i].Trim()} at location {i + 1} is invalid.");
                    }
                }
            }
            _editContext.NotifyValidationStateChanged();
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