#pragma warning disable CA1819
using Hashgraph.Portal.Components;
using Hashgraph.Portal.Services;
using Microsoft.AspNetCore.Components;
using System.ComponentModel.DataAnnotations;

namespace Hashgraph.Portal.Pages;

public partial class QueryContract : ComponentBase
{
    [Inject] public DefaultsService DefaultsService { get; set; } = default!;

    private Network _network = default!;
    private QueryContractInput _input = new QueryContractInput();
    private ContractCallResult? _output = null;

    protected override void OnInitialized()
    {
        _input.Gateway = DefaultsService.Gateway;
        _input.Payer = DefaultsService.Payer;
        base.OnInitialized();
    }

    protected async Task HandleValidSubmit()
    {
        _output = null;
        await _network.ExecuteAsync(_input.Gateway!, _input.Payer, async client =>
        {
            var queryParams = new QueryContractParams
            {
                Contract = _input.Contract!,
                Gas = _input.Gas.GetValueOrDefault(),
                ReturnValueCharge = _input.ReturnValueCharge.GetValueOrDefault(),
                FunctionName = _input.FunctionName?.Trim() ?? string.Empty,
                FunctionArgs = _input.Arguments.ToArray(),
                ThrowOnFail = false,
            };
            _output = await client.QueryContractAsync(queryParams, ctx => ctx.Memo = _input.Memo?.Trim());
        });
    }
}
public class QueryContractInput
{
    [Required(ErrorMessage = "Please select a Network Gateway Node.")]
    public Gateway? Gateway { get; set; }
    [Required(ErrorMessage = "Please enter the Account that will pay the Transaction Fees.")]
    public Address? Payer { get; set; }
    [Required(ErrorMessage = "Please enter the file address of the contract.")]
    public Address? Contract { get; set; }
    [Required(ErrorMessage = "Please enter a maximum gas limit, this will be taken from the Payer account.")]
    [Range(1, long.MaxValue, ErrorMessage = "The maximum allowed gas must be greater than zero.")]
    public long? Gas { get; set; }
    [Required(ErrorMessage = "Please enter the gas to pay for bytes returned from query.")]
    [Range(0, long.MaxValue, ErrorMessage = "The charge to pay for bytes returned from query must be greater than or equal to zero.")]
    public long? ReturnValueCharge { get; set; }
    [MaxLength(100, ErrorMessage = "The function name cannot exceed 100 characters.")]
    public string? FunctionName { get; set; }
    public ReadOnlyMemory<object> Arguments { get; set; }
    [MaxLength(100, ErrorMessage = "The memo field cannot exceed 100 characters.")]
    public string? Memo { get; set; }
}