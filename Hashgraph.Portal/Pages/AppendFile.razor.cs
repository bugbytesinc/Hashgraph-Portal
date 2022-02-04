using Hashgraph.Portal.Components;
using Hashgraph.Portal.Services;
using Microsoft.AspNetCore.Components;
using System.ComponentModel.DataAnnotations;

namespace Hashgraph.Portal.Pages;

public partial class AppendFile : ComponentBase
{
    [Inject] public DefaultsService DefaultsService { get; set; } = default!;

    private Network _network = default!;
    private AppendFileInput _input = new AppendFileInput();
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
            var createParams = new AppendFileParams
            {
                File = _input.File!,
                Contents = _input.Content
            };
            _output = await client.AppendFileAsync(createParams, ctx => ctx.Memo = _input.Memo?.Trim());
        });
    }
    private async Task GetRecord()
    {
        _record = await _network.GetTransactionRecordAsync(_output!.Id);
    }
}
public class AppendFileInput
{
    [Required(ErrorMessage = "Please select a Network Gateway Node.")]
    public Gateway? Gateway { get; set; }
    [Required(ErrorMessage = "Please enter the Account that will pay the Transaction Fees.")]
    public Address? Payer { get; set; }
    [Required(ErrorMessage = "Please enter the File you wish to append content.")]
    public Address? File { get; set; }
    [Required(ErrorMessage = "Please enter or upload the additional contents for the File.")]
    public ReadOnlyMemory<byte> Content { get; set; }
    [MaxLength(100, ErrorMessage = "The memo field cannot exceed 100 characters.")]
    public string? Memo { get; set; }
}