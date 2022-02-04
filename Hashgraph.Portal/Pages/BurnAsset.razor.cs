using Hashgraph.Portal.Components;
using Hashgraph.Portal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.ComponentModel.DataAnnotations;

namespace Hashgraph.Portal.Pages;

public partial class BurnAsset : ComponentBase
{
    [Inject] public DefaultsService DefaultsService { get; set; } = default!;

    private Network _network = default!;
    private EditContext _editContext = default!;
    private ValidationMessageStore _validationMessages = default!;
    private BurnAssetInput _input = new BurnAssetInput();
    private TokenReceipt? _output = null;
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
        var parts = _input.SerialNumbers?.Split(',') ?? Array.Empty<string>();
        if (parts.Length == 0)
        {
            AddIfNoOtherErrors(nameof(_input.SerialNumbers), "Please enter the a comma seperated list of serial numbers to remove from the treasury.");
        }
        var serialNumbers = new long[parts.Length];
        for (int i = 0; i < parts.Length; i++)
        {
            if (long.TryParse(parts[i].Trim(), out long serialNumber))
            {
                serialNumbers[i] = serialNumber;
                if (serialNumber < 0)
                {
                    AddIfNoOtherErrors(nameof(_input.SerialNumbers), $"Serial Number at location {i + 1} must be a positive value.");
                }
            }
            else
            {
                AddIfNoOtherErrors(nameof(_input.SerialNumbers), $"Serial Number {parts[i].Trim()} at location {i + 1} is invalid.");
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
        var serialNumbers = _input.SerialNumbers!.Split(',').Select(s => long.Parse(s.Trim())).ToArray();
        await _network.ExecuteAsync(_input.Gateway!, _input.Payer, async client =>
        {
            _output = await client.BurnAssetsAsync(_input.Token!, serialNumbers, ctx => ctx.Memo = _input.Memo?.Trim());
        });
    }
    private async Task GetRecord()
    {
        _record = await _network.GetTransactionRecordAsync(_output!.Id);
    }
}
public class BurnAssetInput
{
    [Required(ErrorMessage = "Please select a Network Gateway Node.")]
    public Gateway? Gateway { get; set; }
    [Required(ErrorMessage = "Please enter the account that will pay the Transaction Fees.")]
    public Address? Payer { get; set; }
    [Required(ErrorMessage = "Please enter the token address.")]
    public Address? Token { get; set; }
    [Required(ErrorMessage = "Please enter the Serial Numbers of the Assets to remove from the Treasury (comma seperated).")]
    public string? SerialNumbers { get; set; }
    [MaxLength(100, ErrorMessage = "The memo field cannot exceed 100 characters.")]
    public string? Memo { get; set; }
}