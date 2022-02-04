using Hashgraph.Portal.Components;
using Hashgraph.Portal.Services;
using Microsoft.AspNetCore.Components;
using System.ComponentModel.DataAnnotations;

namespace Hashgraph.Portal.Pages;

public partial class CallContract : ComponentBase
{
    [Inject] public DefaultsService DefaultsService { get; set; } = default!;

    private Network _network = default!;
    private CallContractInput _input = new CallContractInput();
    private TransactionReceipt? _output = null;
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
            var callParams = new CallContractParams
            {
                Contract = _input.Contract!,
                Gas = _input.Gas.GetValueOrDefault(),
                PayableAmount = _input.Amount.GetValueOrDefault(),
                FunctionName = _input.FunctionName!,
                FunctionArgs = _input.Arguments.ToArray()
            };
            _output = await client.CallContractAsync(callParams, ctx => ctx.Memo = _input.Memo?.Trim());
        });
    }
    private async Task GetRecord()
    {
        _record = await _network.GetTransactionRecordAsync(_output!.Id);
    }
}
public class CallContractInput
{
    [Required(ErrorMessage = "Please select a Network Gateway Node.")]
    public Gateway? Gateway { get; set; }
    [Required(ErrorMessage = "Please enter the Account that will pay the Transaction Fees.")]
    public Address? Payer { get; set; }
    [Required(ErrorMessage = "Please enter the file address of the contract.")]
    public Address? Contract { get; set; }
    [Required(ErrorMessage = "Please enter an amount to send, this will be taken from the Payer account, it can be zero. It must be zero if the contract method is not payable.")]
    [Range(0, long.MaxValue, ErrorMessage = "The payable amount must be greater than or equal to zero.")]
    public long? Amount { get; set; }
    [Required(ErrorMessage = "Please enter a maximum gas limit, this will be taken from the Payer account.")]
    [Range(1, long.MaxValue, ErrorMessage = "The maximum allowed gas must be greater than zero.")]
    public long? Gas { get; set; }
    [MaxLength(100, ErrorMessage = "The function name cannot exceed 100 characters.")]
    public string? FunctionName { get; set; }
    public ReadOnlyMemory<object> Arguments { get; set; }
    [MaxLength(100, ErrorMessage = "The memo field cannot exceed 100 characters.")]
    public string? Memo { get; set; }
}
