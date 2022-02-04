using Hashgraph.Portal.Components;
using Hashgraph.Portal.Services;
using Microsoft.AspNetCore.Components;
using System.ComponentModel.DataAnnotations;

namespace Hashgraph.Portal.Pages;
public partial class CreateAccount : ComponentBase
{
    [Inject] public DefaultsService DefaultsService { get; set; } = default!;

    private Network _network = default!;
    private CreateAccountInput _input = new CreateAccountInput();
    private CreateAccountReceipt? _output = null;
    private TransactionRecord? _record = null;

    protected override void OnInitialized()
    {
        _input.Gateway = DefaultsService.Gateway;
        _input.Payer = DefaultsService.Payer;
        base.OnInitialized();
    }

    protected async Task HandleValidSubmit()
    {
        _output = null;
        _record = null;
        await _network.ExecuteAsync(_input.Gateway!, _input.Payer, async client =>
        {
            var createParams = new CreateAccountParams
            {
                Endorsement = _input.Endorsement,
                InitialBalance = (ulong)_input.InitialBalance.GetValueOrDefault(),
                RequireReceiveSignature = _input.RequireReceiveSignature,
                AutoAssociationLimit = _input.AutoAssociationLimit.GetValueOrDefault(),
                Proxy = _input.Proxy is null ? null : new AddressOrAlias(_input.Proxy),
                Memo = _input.AccountMemo?.Trim()
            };
            _output = await client.CreateAccountAsync(createParams, ctx => ctx.Memo = _input.TransactionMemo?.Trim());
        });
    }
    private async Task GetRecord()
    {
        _record = await _network.GetTransactionRecordAsync(_output!.Id);
    }
}
public class CreateAccountInput
{
    [Required(ErrorMessage = "Please select a Network Gateway Node.")]
    public Gateway? Gateway { get; set; }
    [Required(ErrorMessage = "Please enter the account that will pay the Transaction Fees.")]
    public Address? Payer { get; set; }
    [Required(ErrorMessage = "Please enter an initial balance, this will be taken from the Payer account, it can be zero.")]
    [Range(0, long.MaxValue, ErrorMessage = "The initial balance must be greater than or equal to zero.")]
    public long? InitialBalance { get; set; }
    [Range(0, long.MaxValue, ErrorMessage = "If specified, the token auto-association limit must be greater than zero.")]
    public int? AutoAssociationLimit { get; set; } = null;
    [Required]
    public Endorsement? Endorsement { get; set; } = default!;
    public bool RequireReceiveSignature { get; set; }
    public Address? Proxy { get; set; }
    [MaxLength(100, ErrorMessage = "The account's memo field cannot exceed 100 characters.")]
    public string? AccountMemo { get; set; }
    [MaxLength(100, ErrorMessage = "The paying transaction memo field cannot exceed 100 characters.")]
    public string? TransactionMemo { get; set; }
}
