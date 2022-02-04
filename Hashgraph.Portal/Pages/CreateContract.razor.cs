#pragma warning disable CA1819
using Hashgraph.Portal.Components;
using Hashgraph.Portal.Services;
using Microsoft.AspNetCore.Components;
using System.ComponentModel.DataAnnotations;

namespace Hashgraph.Portal.Pages;

public partial class CreateContract : ComponentBase
{
    [Inject] public DefaultsService DefaultsService { get; set; } = default!;

    private Network _network = default!;
    private CreateContractInput _input = new CreateContractInput();
    private CreateContractReceipt? _output = null;
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
            var createParams = new CreateContractParams
            {
                File = _input.File!,
                Administrator = Endorsement.None.Equals(_input.Administrator) ? null : _input.Administrator,
                Memo = _input.ContractMemo?.Trim() ?? string.Empty,
                Gas = _input.Gas.GetValueOrDefault(),
                RenewPeriod = TimeSpan.FromSeconds(7890000),
                InitialBalance = _input.InitialBalance.GetValueOrDefault(),
                Arguments = _input.Arguments.ToArray()
            };
            _output = await client.CreateContractAsync(createParams, ctx => ctx.Memo = _input.TransactionMemo?.Trim());
        });
    }
    private async Task GetRecord()
    {
        _record = await _network.GetTransactionRecordAsync(_output!.Id);
    }
}
public class CreateContractInput
{
    [Required(ErrorMessage = "Please select a Network Gateway Node.")]
    public Gateway? Gateway { get; set; }
    [Required(ErrorMessage = "Please enter the Account that will pay the Transaction Fees.")]
    public Address? Payer { get; set; }
    [Required(ErrorMessage = "Please enter the file address having the contract bytecode.")]
    public Address? File { get; set; }
    [MaxLength(100, ErrorMessage = "The contract memo field cannot exceed 100 characters.")]
    public string? ContractMemo { get; set; }
    public Endorsement? Administrator { get; set; }
    [Required(ErrorMessage = "Please enter a maximum gas limit, this will be taken from the Payer account.")]
    [Range(1, long.MaxValue, ErrorMessage = "The maximum allowed gas must be greater than zero.")]
    public long? Gas { get; set; }
    [Required(ErrorMessage = "Please enter an initial balance, this will be taken from the Payer account, it can be zero. It must be zero if the contract constructor is not payable.")]
    [Range(0, long.MaxValue, ErrorMessage = "The initial balance must be greater than or equal to zero.")]
    public long? InitialBalance { get; set; }
    public ReadOnlyMemory<object> Arguments { get; set; }
    [MaxLength(100, ErrorMessage = "The transaction memo field cannot exceed 100 characters.")]
    public string? TransactionMemo { get; set; }
}