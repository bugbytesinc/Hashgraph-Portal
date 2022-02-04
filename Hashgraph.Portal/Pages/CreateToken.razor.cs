using Hashgraph.Components.Models;
using Hashgraph.Portal.Components;
using Hashgraph.Portal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.ComponentModel.DataAnnotations;

namespace Hashgraph.Portal.Pages;

public partial class CreateToken : ComponentBase
{
    [Inject] public DefaultsService DefaultsService { get; set; } = default!;

    private Network _network = null!;
    private EditContext _editContext = default!;
    private ValidationMessageStore _validationMessages = default!;
    private CreateTokenInput _input = new CreateTokenInput();
    private CreateTokenReceipt? _output = null;
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
        if (_input.TokenType == TokenType.Fungible)
        {
            //[Required(ErrorMessage = "Please enter the initial circulation of tokens (in the smallest denomination).")]
            if (_input.Circulation is null)
            {
                AddIfNoOtherErrors(nameof(_input.Circulation), "Please enter the initial circulation of tokens (in the smallest denomination).");
            }
            //[Range(1, long.MaxValue, ErrorMessage = "The initial circulation must be greater than or equal to zero.")]
            else if (_input.Circulation < 0)
            {
                AddIfNoOtherErrors(nameof(_input.Circulation), "The initial circulation must be greater than or equal to zero.");
            }
            //[Range(0, long.MaxValue, ErrorMessage = "The number of decimal places must be zero or larger.")]
            if (_input.Decimals is null || _input.Decimals < 0)
            {
                AddIfNoOtherErrors(nameof(_input.Decimals), "The number of decimal places must be zero or larger.");
            }
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
            var hasRenewalAccount = _input.RenewAccount != null && _input.RenewAccount != Address.None;
            if (_input.TokenType == TokenType.Fungible)
            {
                var createParams = new CreateTokenParams
                {
                    Name = _input.Name!,
                    Symbol = _input.Symbol!,
                    Memo = _input.TokenMemo ?? string.Empty,
                    Circulation = (ulong)_input.Circulation.GetValueOrDefault(),
                    Decimals = (uint)_input.Decimals.GetValueOrDefault(),
                    Treasury = _input.Treasury!,
                    Administrator = Endorsement.None.Equals(_input.Administrator) ? null : _input.Administrator,
                    GrantKycEndorsement = Endorsement.None.Equals(_input.GrantKycEndorsement) ? null : _input.GrantKycEndorsement,
                    SuspendEndorsement = Endorsement.None.Equals(_input.SuspendEndorsement) ? null : _input.SuspendEndorsement,
                    PauseEndorsement = Endorsement.None.Equals(_input.PauseEndorsement) ? null : _input.PauseEndorsement,
                    ConfiscateEndorsement = Endorsement.None.Equals(_input.ConfiscateEndorsement) ? null : _input.ConfiscateEndorsement,
                    SupplyEndorsement = Endorsement.None.Equals(_input.SupplyEndorsement) ? null : _input.SupplyEndorsement,
                    RoyaltiesEndorsement = Endorsement.None.Equals(_input.RoyaltyEndorsement) ? null : _input.RoyaltyEndorsement,
                    Royalties = _input.Royalties.Count > 0 ? _input.Royalties.ToRoyaltyList() : null,
                    InitializeSuspended = _input.InitializeSuspended,
                    Expiration = DateTime.UtcNow.AddDays(90),
                    RenewPeriod = hasRenewalAccount ? TimeSpan.FromDays(90) : null,
                    RenewAccount = hasRenewalAccount ? new AddressOrAlias(_input.RenewAccount!) : null
                };
                _output = await client.CreateTokenAsync(createParams, ctx => ctx.Memo = _input.TransactionMemo?.Trim());
            }
            else
            {
                var createParams = new CreateAssetParams
                {
                    Name = _input.Name!,
                    Symbol = _input.Symbol!,
                    Memo = _input.TokenMemo ?? string.Empty,
                    Treasury = _input.Treasury!,
                    Administrator = Endorsement.None.Equals(_input.Administrator) ? null : _input.Administrator,
                    GrantKycEndorsement = Endorsement.None.Equals(_input.GrantKycEndorsement) ? null : _input.GrantKycEndorsement,
                    SuspendEndorsement = Endorsement.None.Equals(_input.SuspendEndorsement) ? null : _input.SuspendEndorsement,
                    PauseEndorsement = Endorsement.None.Equals(_input.PauseEndorsement) ? null : _input.PauseEndorsement,
                    ConfiscateEndorsement = Endorsement.None.Equals(_input.ConfiscateEndorsement) ? null : _input.ConfiscateEndorsement,
                    SupplyEndorsement = Endorsement.None.Equals(_input.SupplyEndorsement) ? null : _input.SupplyEndorsement,
                    RoyaltiesEndorsement = Endorsement.None.Equals(_input.RoyaltyEndorsement) ? null : _input.RoyaltyEndorsement,
                    Royalties = _input.Royalties.Count > 0 ? _input.Royalties.ToRoyaltyList() : null,
                    InitializeSuspended = _input.InitializeSuspended,
                    Expiration = DateTime.UtcNow.AddDays(90),
                    RenewPeriod = hasRenewalAccount ? TimeSpan.FromDays(90) : null,
                    RenewAccount = hasRenewalAccount ? new AddressOrAlias(_input.RenewAccount!) : null
                };
                _output = await client.CreateTokenAsync(createParams, ctx => ctx.Memo = _input.TransactionMemo?.Trim());
            }
        });
    }

    protected void RemoveRoyalty(RoyaltyDefinition definition)
    {
        _input.Royalties.Remove(definition);
    }
    protected void AddRoyalty()
    {
        _input.Royalties.Add(new RoyaltyDefinition());
    }
    private async Task GetRecord()
    {
        _record = await _network.GetTransactionRecordAsync(_output!.Id);
    }
}
public class CreateTokenInput
{
    [Required(ErrorMessage = "Please select a Network Gateway Node.")]
    public Gateway? Gateway { get; set; }
    [Required(ErrorMessage = "Please enter the Account that will pay the Transaction Fees.")]
    public Address? Payer { get; set; }
    public TokenType TokenType { get; set; } = TokenType.Fungible;
    [Required(ErrorMessage = "Please the name of the token.")]
    [MaxLength(100, ErrorMessage = "The token name field cannot exceed 100 characters.")]
    public string? Name { get; set; }
    [Required(ErrorMessage = "Please enter the symbol for this token.")]
    [MaxLength(100, ErrorMessage = "The token symbol field cannot exceed 100 characters.")]
    public string? Symbol { get; set; }
    [MaxLength(100, ErrorMessage = "The token memo field cannot exceed 100 characters.")]
    public string? TokenMemo { get; set; }
    public long? Circulation { get; set; }
    public long? Decimals { get; set; }
    [Range(1, long.MaxValue, ErrorMessage = "The maximum allowed circulation must be greater than zero if specified.")]
    public long? Ceiling { get; set; }
    [Required(ErrorMessage = "Please enter the treasury account that will receive the initial tokens.")]
    public Address? Treasury { get; set; }
    public Endorsement? Administrator { get; set; }
    public Endorsement? GrantKycEndorsement { get; set; }
    public Endorsement? SuspendEndorsement { get; set; }
    public Endorsement? PauseEndorsement { get; set; }
    public Endorsement? ConfiscateEndorsement { get; set; }
    public Endorsement? SupplyEndorsement { get; set; }
    public Endorsement? RoyaltyEndorsement { get; set; }
    public bool InitializeSuspended { get; set; }
    // Skipping Expiration and Renew Period for now
    public Address? RenewAccount { get; set; }
    public RoyaltyList Royalties { get; } = new RoyaltyList();
    [MaxLength(100, ErrorMessage = "The transaction memo field cannot exceed 100 characters.")]
    public string? TransactionMemo { get; set; }
}