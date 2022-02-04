using Hashgraph.Portal.Components;
using Hashgraph.Portal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.ComponentModel.DataAnnotations;

namespace Hashgraph.Portal.Pages;

public partial class UpdateToken : ComponentBase
{
    [Inject] public DefaultsService DefaultsService { get; set; } = default!;

    private Network _network = default!;
    private EditContext _editContext = default!;
    private ValidationMessageStore _validationMessages = default!;
    private UpdateTokenInput _input = new UpdateTokenInput();
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
        bool somethingIsSelected =
            _input.UpdateAdministrator ||
            _input.UpdateGrantKycEndorsement ||
            _input.UpdateSuspendEndorsement ||
            _input.UpdatePauseEndorsement ||
            _input.UpdateConfiscateEndorsement ||
            _input.UpdateSupplyEndorsement ||
            _input.UpdateRoyaltiesEndorsement ||
            _input.UpdateRenewAccount ||
            _input.UpdateMemo;
        if (_input.UpdateSymbol)
        {
            somethingIsSelected = true;
            if (string.IsNullOrWhiteSpace(_input.Symbol))
            {
                AddIfNoOtherErrors(nameof(_input.Symbol), "Please enter a new symbol value.");
            }
            else if (!_input.Symbol.All(c => c >= 'A' && c <= 'Z'))
            {
                AddIfNoOtherErrors(nameof(_input.Symbol), "The symbol can only be upper case characters A-Z.");
            }
        }
        if (_input.UpdateName)
        {
            somethingIsSelected = true;
            if (string.IsNullOrWhiteSpace(_input.Name))
            {
                AddIfNoOtherErrors(nameof(_input.Name), "Please enter a new name.");
            }
        }
        if (_input.UpdateTreasury)
        {
            somethingIsSelected = true;
            if (_input.Treasury == null || _input.Treasury == Address.None)
            {
                AddIfNoOtherErrors(nameof(_input.Treasury), "Please enter a valid treasury account.");
            }
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
            var updateParams = new UpdateTokenParams
            {
                Token = _input.Token!
            };
            if (_input.UpdateTreasury)
            {
                updateParams.Treasury = _input.Treasury ?? Address.None;
            }
            if (_input.UpdateAdministrator)
            {
                updateParams.Administrator = _input.Administrator ?? Endorsement.None;
            }
            if (_input.UpdateGrantKycEndorsement)
            {
                updateParams.GrantKycEndorsement = _input.GrantKycEndorsement ?? Endorsement.None;
            }
            if (_input.UpdateSuspendEndorsement)
            {
                updateParams.SuspendEndorsement = _input.SuspendEndorsement ?? Endorsement.None;
            }
            if (_input.UpdatePauseEndorsement)
            {
                updateParams.PauseEndorsement = _input.PauseEndorsement ?? Endorsement.None;
            }
            if (_input.UpdateConfiscateEndorsement)
            {
                updateParams.ConfiscateEndorsement = _input.ConfiscateEndorsement ?? Endorsement.None;
            }
            if (_input.UpdateSupplyEndorsement)
            {
                updateParams.SupplyEndorsement = _input.SupplyEndorsement ?? Endorsement.None;
            }
            if (_input.UpdateRoyaltiesEndorsement)
            {
                updateParams.RoyaltiesEndorsement = _input.RoyaltiesEndorsement ?? Endorsement.None;
            }
            if (_input.UpdateSymbol)
            {
                updateParams.Symbol = _input.Symbol?.Trim()!;
            }
            if (_input.UpdateName)
            {
                updateParams.Name = _input.Name?.Trim()!;
            }
            if (_input.UpdateMemo)
            {
                updateParams.Memo = _input.TokenMemo?.Trim() ?? string.Empty;
            }
            if (_input.UpdateRenewAccount)
            {
                var isAdding = _input.RenewAccount == null;
                updateParams.RenewAccount = isAdding ? Address.None : _input.RenewAccount!;
                updateParams.RenewPeriod = TimeSpan.FromDays(90);
            }
            _output = await client.UpdateTokenAsync(updateParams, ctx => ctx.Memo = _input.TransactionMemo?.Trim());
        });
    }
    private async Task GetRecord()
    {
        _record = await _network.GetTransactionRecordAsync(_output!.Id);
    }
}
public class UpdateTokenInput
{
    [Required(ErrorMessage = "Please select a Network Gateway Node.")]
    public Gateway? Gateway { get; set; }
    [Required(ErrorMessage = "Please enter the Token that will pay the Update Transaction Fees.")]
    public Address? Payer { get; set; }
    [Required(ErrorMessage = "Please enter the Token you wish to update.")]
    public Address? Token { get; set; }
    public bool UpdateSymbol { get; set; }
    [MaxLength(100, ErrorMessage = "The token symbol field cannot exceed 100 characters.")]
    public string? Symbol { get; set; }
    public bool UpdateName { get; set; }
    [MaxLength(100, ErrorMessage = "The token name field cannot exceed 100 characters.")]
    public string? Name { get; set; }
    public bool UpdateMemo { get; set; }
    [MaxLength(100, ErrorMessage = "The memo field cannot exceed 100 characters.")]
    public string? TokenMemo { get; set; }
    public bool UpdateTreasury { get; set; }
    public Address? Treasury { get; set; }
    public bool UpdateAdministrator { get; set; }
    public Endorsement? Administrator { get; set; }
    public bool UpdateGrantKycEndorsement { get; set; }
    public Endorsement? GrantKycEndorsement { get; set; }
    public bool UpdateSuspendEndorsement { get; set; }
    public Endorsement? SuspendEndorsement { get; set; }
    public bool UpdatePauseEndorsement { get; set; }
    public Endorsement? PauseEndorsement { get; set; }
    public bool UpdateConfiscateEndorsement { get; set; }
    public Endorsement? ConfiscateEndorsement { get; set; }
    public bool UpdateSupplyEndorsement { get; set; }
    public Endorsement? SupplyEndorsement { get; set; }
    public bool UpdateRoyaltiesEndorsement { get; set; }
    public Endorsement? RoyaltiesEndorsement { get; set; }
    public bool UpdateRenewAccount { get; set; }
    public Address? RenewAccount { get; set; }
    [MaxLength(100, ErrorMessage = "The memo field cannot exceed 100 characters.")]
    public string? TransactionMemo { get; set; }
}
