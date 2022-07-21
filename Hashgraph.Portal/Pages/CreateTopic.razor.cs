using Hashgraph.Portal.Components;
using Hashgraph.Portal.Services;
using Microsoft.AspNetCore.Components;
using System.ComponentModel.DataAnnotations;

namespace Hashgraph.Portal.Pages;

public partial class CreateTopic : ComponentBase
{
    [Inject] public DefaultsService DefaultsService { get; set; } = default!;

    private Network _network = default!;
    private CreateTopicInput _input = new CreateTopicInput();
    private CreateTopicReceipt? _output = null;
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
            var createParams = new CreateTopicParams
            {
                Memo = _input.Desciption?.Trim(),
                Administrator = Endorsement.None.Equals(_input.Administrator) ? null : _input.Administrator,
                Participant = Endorsement.None.Equals(_input.Participant) ? null : _input.Participant,
                RenewAccount = Address.None.Equals(_input.RenewAccount) ? null : _input.RenewAccount,
            };
            _output = await client.CreateTopicAsync(createParams, ctx => ctx.Memo = _input.Memo?.Trim());
        });
    }
    private async Task GetRecord()
    {
        _record = await _network.GetTransactionRecordAsync(_output!.Id);
    }
}
public class CreateTopicInput
{
    [Required(ErrorMessage = "Please select a Network Gateway Node.")]
    public Gateway? Gateway { get; set; }
    [Required(ErrorMessage = "Please enter the Account that will pay the Transaction Fees.")]
    public Address? Payer { get; set; }
    [Required(ErrorMessage = "Please a short description or memo for the topic.")]
    [MaxLength(100, ErrorMessage = "The topic memo field cannot exceed 100 characters.")]
    public string? Desciption { get; set; }
    public Endorsement? Administrator { get; set; }
    public Endorsement? Participant { get; set; }
    public Address? RenewAccount { get; set; }
    [MaxLength(100, ErrorMessage = "The memo field cannot exceed 100 characters.")]
    public string? Memo { get; set; }
}