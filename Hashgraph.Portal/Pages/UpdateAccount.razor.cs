using Hashgraph.Portal.Components;
using Hashgraph.Portal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.ComponentModel.DataAnnotations;

namespace Hashgraph.Portal.Pages;

public partial class UpdateAccount : ComponentBase
{
    [Inject] public DefaultsService DefaultsService { get; set; } = default!;

    private Network _network = default!;
    private EditContext _editContext = default!;
    private ValidationMessageStore _validationMessages = default!;
    private UpdateAccountInput _input = new UpdateAccountInput();
    private TransactionReceipt? _output = null;
    private TransactionRecord? _record = null;

    protected override void OnInitialized()
    {
        _input.Gateway = DefaultsService.Gateway;
        _input.Payer = DefaultsService.Payer;
        _editContext = new EditContext(_input);
        _editContext.OnValidationRequested += OnValidationRequested;
        _validationMessages = new ValidationMessageStore(_editContext);
        base.OnInitialized();
    }
    private void OnValidationRequested(object? sender, ValidationRequestedEventArgs e)
    {
        _validationMessages.Clear();
        bool somethingIsSelected = false;
        if (_input.UpdateEndorsement)
        {
            somethingIsSelected = true;
            if (_input.Endorsement is null)
            {
                AddIfNoOtherErrors(nameof(_input.Endorsement), "Please enter a new Endorsement.");
            }
        }
        if (_input.UpdateReceiveSignatureRequired)
        {
            somethingIsSelected = true;
        }
        if (_input.UpdateAutoAssociationLimit)
        {
            somethingIsSelected = true;
            if (_input.AutoAssociationLimit is null)
            {
                AddIfNoOtherErrors(nameof(_input.Endorsement), "Please enter an auto association limit value.");
            }
        }
        if (!string.IsNullOrWhiteSpace(_input.AccountMemo))
        {
            somethingIsSelected = true;
        }
        if (!somethingIsSelected)
        {
            _validationMessages.Add(new FieldIdentifier(_input, string.Empty), "Nothing has been selected to change.");
        }
    }

    private void AddIfNoOtherErrors(string fieldName, string message)
    {
        var field = _editContext.Field(fieldName);
        if (!_editContext.GetValidationMessages(field).Any())
        {
            _validationMessages.Add(field, message);
        }
    }

    protected async Task HandleValidSubmit()
    {
        _output = null;
        _record = null;
        await _network.ExecuteAsync(_input.Gateway!, _input.Payer, async client =>
        {
            var updateParams = new UpdateAccountParams
            {
                Address = _input.Address!
            };
            if (_input.UpdateEndorsement)
            {
                updateParams.Endorsement = _input.Endorsement;
            }
            if (_input.UpdateReceiveSignatureRequired)
            {
                updateParams.RequireReceiveSignature = _input.ReceiveSignatureRequired;
            }
            if (_input.UpdateAutoAssociationLimit)
            {
                updateParams.AutoAssociationLimit = _input.AutoAssociationLimit;
            }
            if (!string.IsNullOrWhiteSpace(_input.AccountMemo))
            {
                updateParams.Memo = _input.AccountMemo;
            }
            _output = await client.UpdateAccountAsync(updateParams, ctx => ctx.Memo = _input.TransactionMemo?.Trim());
        });
    }
    private async Task GetRecord()
    {
        _record = await _network.GetTransactionRecordAsync(_output!.Id);
    }
}
public class UpdateAccountInput
{
    [Required(ErrorMessage = "Please select a Network Gateway Node.")]
    public Gateway? Gateway { get; set; }
    [Required(ErrorMessage = "Please enter the account that will pay the Update Transaction Fees.")]
    public Address? Payer { get; set; }
    [MaxLength(100, ErrorMessage = "The paying transaction memo field cannot exceed 100 characters.")]
    public string? TransactionMemo { get; set; }
    [Required(ErrorMessage = "Please enter the account you wish to update.")]
    public Address? Address { get; set; }
    public bool UpdateEndorsement { get; set; }
    public Endorsement? Endorsement { get; set; }
    public bool UpdateReceiveSignatureRequired { get; set; }
    public bool ReceiveSignatureRequired { get; set; }
    public bool UpdateAutoAssociationLimit { get; set; }
    [Range(0, int.MaxValue, ErrorMessage = "The auto association limit must be greater than or equal to zero.")]
    public int? AutoAssociationLimit { get; set; }
    [MaxLength(100, ErrorMessage = "The account's memo field cannot exceed 100 characters.")]
    public string? AccountMemo { get; set; }
}