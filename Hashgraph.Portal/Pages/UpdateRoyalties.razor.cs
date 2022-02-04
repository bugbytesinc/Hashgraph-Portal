using Hashgraph.Components.Models;
using Hashgraph.Portal.Components;
using Hashgraph.Portal.Services;
using Microsoft.AspNetCore.Components;
using System.ComponentModel.DataAnnotations;

namespace Hashgraph.Portal.Pages;

public partial class UpdateRoyalties : ComponentBase
{
    [Inject] public DefaultsService DefaultsService { get; set; } = default!;

    private Network _network = null!;
    private UpdateRoyaltiesInput _input = new UpdateRoyaltiesInput();
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
        var royalties = _input.Royalties.Count > 0 ? _input.Royalties.ToRoyaltyList() : null;
        await _network.ExecuteAsync(_input.Gateway!, _input.Payer, async client =>
        {
            _output = await client.UpdateRoyaltiesAsync(_input.Token!, royalties, ctx => ctx.Memo = _input.TransactionMemo?.Trim());
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
public class UpdateRoyaltiesInput
{
    [Required(ErrorMessage = "Please select a Network Gateway Node.")]
    public Gateway? Gateway { get; set; }
    [Required(ErrorMessage = "Please enter the Account that will pay the Transaction Fees.")]
    public Address? Payer { get; set; }
    [Required(ErrorMessage = "Please enter the token address.")]
    public Address? Token { get; set; }
    public RoyaltyList Royalties { get; } = new RoyaltyList();
    public string? TransactionMemo { get; set; }
}