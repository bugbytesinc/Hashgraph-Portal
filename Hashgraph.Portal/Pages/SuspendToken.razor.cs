using Hashgraph.Portal.Components;
using Hashgraph.Portal.Services;
using Microsoft.AspNetCore.Components;
using System.ComponentModel.DataAnnotations;

namespace Hashgraph.Portal.Pages;

public partial class SuspendToken : ComponentBase
{
    [Inject] public DefaultsService DefaultsService { get; set; } = default!;

    private Network _network = default!;
    private SuspendTokenInput _input = new SuspendTokenInput();
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
            _output = await client.SuspendTokenAsync(_input.Token!, _input.Account!, ctx => ctx.Memo = _input.Memo?.Trim());
        });
    }
    private async Task GetRecord()
    {
        _record = await _network.GetTransactionRecordAsync(_output!.Id);
    }
}
public class SuspendTokenInput
{
    [Required(ErrorMessage = "Please select a Network Gateway Node.")]
    public Gateway? Gateway { get; set; }
    [Required(ErrorMessage = "Please enter the account that will pay the Transaction Fees.")]
    public Address? Payer { get; set; }
    [Required(ErrorMessage = "Please enter the token address.")]
    public Address? Token { get; set; }
    [Required(ErrorMessage = "Please enter the account address.")]
    public Address? Account { get; set; }
    [MaxLength(100, ErrorMessage = "The memo field cannot exceed 100 characters.")]
    public string? Memo { get; set; }
}