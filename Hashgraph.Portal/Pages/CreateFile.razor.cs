using Hashgraph.Portal.Components;
using Hashgraph.Portal.Services;
using Microsoft.AspNetCore.Components;
using System.ComponentModel.DataAnnotations;

namespace Hashgraph.Portal.Pages;

public partial class CreateFile : ComponentBase
{
    [Inject] public DefaultsService DefaultsService { get; set; } = default!;

    private Network _network = default!;
    private CreateFileInput _input = new CreateFileInput();
    private FileReceipt? _output = null;
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
            var createParams = new CreateFileParams
            {
                Expiration = DateTime.UtcNow.AddSeconds(7890000), // Default enforced by network at the moment
                Endorsements = _input.Endorsements != null ? _input.Endorsements : Array.Empty<Endorsement>(),
                Contents = _input.Content,
                Memo = _input.FileMemo?.Trim() ?? string.Empty
            };
            _output = await client.CreateFileAsync(createParams, ctx => ctx.Memo = _input.TransactionMemo?.Trim());
        });
    }
    private async Task GetRecord()
    {
        _record = await _network.GetTransactionRecordAsync(_output!.Id);
    }
}
public class CreateFileInput
{
    [Required(ErrorMessage = "Please select a Network Gateway Node.")]
    public Gateway? Gateway { get; set; }
    [Required(ErrorMessage = "Please enter the Account that will pay the Transaction Fees.")]
    public Address? Payer { get; set; }
    [Required(ErrorMessage = "Please enter or upload contents for the File.")]
    public ReadOnlyMemory<byte> Content { get; set; }
    public Endorsement[]? Endorsements { get; set; }
    [MaxLength(100, ErrorMessage = "The file memo field cannot exceed 100 characters.")]
    public string? FileMemo { get; set; }
    [MaxLength(100, ErrorMessage = "The transaction memo field cannot exceed 100 characters.")]
    public string? TransactionMemo { get; set; }
}
