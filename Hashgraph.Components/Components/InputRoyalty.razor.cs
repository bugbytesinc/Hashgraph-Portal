using Hashgraph.Components.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.Globalization;
using System.Linq.Expressions;

namespace Hashgraph.Components;

public partial class InputRoyalty : ComponentBase, IDisposable
{
    private FieldIdentifier _fieldIdentifier;
    private string _fieldCssClasses => _editContext.FieldCssClass(_fieldIdentifier);
    private string _otherCssClasses = string.Empty;
    private ValidationMessageStore _validationMessages = default!;
    [CascadingParameter] private EditContext _editContext { get; set; } = default!;

    [Parameter] public RoyaltyDefinition? Value { get; set; }
    [Parameter] public EventCallback<RoyaltyDefinition> ValueChanged { get; set; }
    [Parameter] public Expression<Func<RoyaltyDefinition>>? ValueExpression { get; set; }
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
        if (evt.FieldIdentifier.Model is RoyaltyDefinition)
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
            if (Value.Account.IsNullOrNone())
            {
                _validationMessages.Add(_fieldIdentifier, $"The account receiving royalties is not valid.");
            }
            switch (Value.RoyaltyType)
            {
                case RoyaltyType.Fixed:
                    if (Value.FixedAmount.GetValueOrDefault() <= 0)
                    {
                        _validationMessages.Add(_fieldIdentifier, $"A fixed royalty requires an amount greater than zero.");
                    }
                    break;
                case RoyaltyType.Token:
                    if (Value.Numerator.GetValueOrDefault() <= 0)
                    {
                        _validationMessages.Add(_fieldIdentifier, $"A variable royalty requires an amount greater than zero for the assessment numerator.");
                    }
                    if (Value.Denominator.GetValueOrDefault() <= 0)
                    {
                        _validationMessages.Add(_fieldIdentifier, $"A variable royalty requires an amount greater than zero for the denominator numerator.");
                    }
                    if (Value.Minimum is not null && Value.Minimum.Value < 0)
                    {
                        _validationMessages.Add(_fieldIdentifier, $"The minimum royalty value must be be greater than or equal to zero.");
                    }
                    if (Value.Maximum is not null && Value.Maximum.Value < 0)
                    {
                        _validationMessages.Add(_fieldIdentifier, $"The maximum royalty value must be be greater than or equal to zero.");
                    }
                    if (Value.Minimum is not null && Value.Maximum is not null && Value.Minimum > Value.Maximum)
                    {
                        _validationMessages.Add(_fieldIdentifier, $"The maximum royalty cannot be greater than the minimum royalty value.");
                    }
                    break;
                case RoyaltyType.Asset:
                    if (Value.Numerator.GetValueOrDefault() <= 0)
                    {
                        _validationMessages.Add(_fieldIdentifier, $"An exchange value royalty requires an amount greater than zero for the assessment numerator.");
                    }
                    if (Value.Denominator.GetValueOrDefault() <= 0)
                    {
                        _validationMessages.Add(_fieldIdentifier, $"An exchange value royalty requires an amount greater than zero for the denominator numerator.");
                    }
                    if (Value.FallbackAmount is not null && Value.FallbackAmount.Value < 0)
                    {
                        _validationMessages.Add(_fieldIdentifier, $"The fall back payment amount must be greater than zero.");
                    }
                    if (Value.FallbackToken is not null && Value.FallbackAmount is null)
                    {
                        _validationMessages.Add(_fieldIdentifier, $"The fall back payment amount must specified if a fallback token is specified.");
                    }
                    break;
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