using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Hashgraph.Components;

public class RecordDisplay : ComponentBase
{
    [Parameter] [EditorRequired] public TransactionRecord? Value { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement("div");
        builder.AddAttribute("hg-record-display");
        if (Value is null)
        {
            builder.AddAttribute("hg-none");
            builder.AddMultipleAttributes(AdditionalAttributes);
        }
        else
        {
            builder.AddMultipleAttributes(AdditionalAttributes);

            builder.AddMarkupContent("<div>Id</div>");
            builder.OpenComponent<TxIdDisplay>();
            builder.AddAttribute("Value", Value.Id);
            builder.CloseComponent();

            builder.AddMarkupContent("<div>Type</div>");
            builder.OpenElement("div");
            builder.AddContent(GetRecordType());
            builder.CloseElement();

            builder.AddMarkupContent("<div>Consensus Timestamp</div>");
            builder.OpenElement("div");
            builder.AddContent(Value.Concensus);
            builder.CloseElement();

            if (Value.ParentTransactionConcensus is not null)
            {
                builder.AddMarkupContent("<div>Parent Transaction Consensus</div>");
                builder.OpenElement("div");
                builder.AddContent(Value.ParentTransactionConcensus);
                builder.CloseElement();
            }

            builder.AddMarkupContent("<div>Status</div>");
            builder.OpenElement("div");
            builder.AddContent(Value.Status);
            builder.CloseElement();

            switch (Value)
            {
                case CreateAccountRecord createRecord:
                    builder.AddMarkupContent("<div>Created Account</div>");
                    builder.OpenComponent<AddressDisplay>();
                    builder.AddAttribute("Value", createRecord.Address);
                    builder.CloseComponent();
                    break;
                case FileRecord fileRecord:
                    builder.AddMarkupContent("<div>Created File</div>");
                    builder.OpenComponent<AddressDisplay>();
                    builder.AddAttribute("Value", fileRecord.File);
                    builder.CloseComponent();
                    break;
                case CreateTopicRecord createTopicRecord:
                    builder.AddMarkupContent("<div>Created Topic</div>");
                    builder.OpenComponent<AddressDisplay>();
                    builder.AddAttribute("Value", createTopicRecord.Topic);
                    builder.CloseComponent();
                    break;
                case CreateContractRecord createContractRecord:
                    builder.AddMarkupContent("<div>Created Contract</div>");
                    builder.OpenComponent<AddressDisplay>();
                    builder.AddAttribute("Value", createContractRecord.Contract);
                    builder.CloseComponent();
                    builder.OpenComponent<ContractCallResultDisplay>();
                    builder.AddAttribute("Value", createContractRecord.CallResult);
                    builder.CloseComponent();
                    break;
                case CreateTokenRecord createTokenRecord:
                    builder.AddMarkupContent("<div>Created Token</div>");
                    builder.OpenComponent<AddressDisplay>();
                    builder.AddAttribute("Value", createTokenRecord.Token);
                    builder.CloseComponent();
                    break;
                case SubmitMessageRecord submitMessageRecord:
                    builder.AddMarkupContent("<div>Sequence Number</div>");
                    builder.OpenElement("div");
                    builder.AddContent(submitMessageRecord.SequenceNumber);
                    builder.CloseElement();
                    builder.AddMarkupContent("<div>Running Hash</div>");
                    builder.OpenComponent<HashDisplay>();
                    builder.AddAttribute("Value", submitMessageRecord.RunningHash);
                    builder.CloseComponent();
                    builder.AddMarkupContent("<div>Hash Version</div>");
                    builder.OpenElement("div");
                    builder.AddContent(submitMessageRecord.RunningHashVersion);
                    builder.CloseElement();
                    break;
                case AssetMintRecord assetMintRecord:
                    builder.AddMarkupContent("<div>Serial Numbers</div>");
                    builder.OpenElement("div");
                    builder.AddContent(string.Join(", ", assetMintRecord.SerialNumbers.ToArray()));
                    builder.CloseElement();
                    builder.AddMarkupContent("<div>New Circulation</div>");
                    builder.OpenElement("div");
                    builder.AddContent(assetMintRecord.Circulation);
                    builder.CloseElement();
                    break;
                case TokenRecord tokenMintRecord:
                    builder.AddMarkupContent("<div>New Circulation</div>");
                    builder.OpenElement("div");
                    builder.AddContent(tokenMintRecord.Circulation);
                    builder.CloseElement();
                    break;
                case CallContractRecord callContractRecord:
                    builder.OpenComponent<ContractCallResultDisplay>();
                    builder.AddAttribute("Value", callContractRecord.CallResult);
                    builder.CloseComponent();
                    break;
            }

            builder.AddMarkupContent("<div>Hash</div>");
            builder.OpenComponent<HashDisplay>();
            builder.AddAttribute("Value", Value.Hash);
            builder.CloseComponent();

            builder.AddMarkupContent("<div>Payer</div>");
            builder.OpenComponent<AddressDisplay>();
            builder.AddAttribute("Value", Value.Id.Address);
            builder.CloseComponent();

            builder.AddMarkupContent("<div>Transaction Fee</div>");
            builder.OpenComponent<HbarDisplay>();
            builder.AddAttribute("Value", Value.Fee);
            builder.CloseComponent();

            builder.AddMarkupContent("<div>Memo</div>");
            builder.OpenComponent<MemoDisplay>();
            builder.AddAttribute("Value", Value.Memo);
            builder.CloseComponent();

            if (Value.Transfers.Count > 0)
            {
                builder.AddMarkupContent("<div>Crypto Transfers</div>");
                builder.OpenElement("div");
                builder.AddAttribute("hg-transfers");
                builder.AddMarkupContent("<div>Account</div><div>Amount</div>");
                builder.OpenRegion();
                int count = 0;
                foreach (var transfer in Value.Transfers)
                {
                    builder.OpenRegion(count++);
                    builder.OpenComponent<AddressDisplay>();
                    builder.AddAttribute("Value", transfer.Key);
                    builder.CloseComponent();
                    if (transfer.Value > 0)
                    {
                        builder.OpenComponent<HbarDisplay>();
                        builder.AddAttribute("Value", (ulong)transfer.Value);
                        builder.CloseComponent();
                    }
                    else
                    {
                        builder.OpenElement("div");
                        builder.AddContent("-");
                        builder.OpenComponent<HbarDisplay>();
                        builder.AddAttribute("Value", (ulong)-transfer.Value);
                        builder.CloseComponent();
                        builder.CloseElement();
                    }
                    builder.CloseRegion();
                }
                builder.CloseRegion();
                builder.CloseElement();
            }

            if (Value.TokenTransfers.Count > 0)
            {
                builder.AddMarkupContent("<div>Token Transfers</div>");
                builder.OpenElement("div");
                builder.AddAttribute("hg-token-transfers");
                builder.AddMarkupContent("<div>Token</div><div>Account</div><div>Amount</div>");
                builder.OpenRegion();
                int count = 0;
                var previous = Address.None;
                foreach (var transfer in Value.TokenTransfers)
                {
                    builder.OpenRegion(count++);
                    if (previous != transfer.Token)
                    {
                        builder.OpenComponent<AddressDisplay>();
                        builder.AddAttribute("Value", transfer.Token);
                        builder.CloseComponent();
                        previous = transfer.Token;
                    }
                    else
                    {
                        builder.OpenElement("div");
                        builder.CloseElement();
                    }
                    builder.OpenComponent<AddressDisplay>();
                    builder.AddAttribute("Value", transfer.Address);
                    builder.CloseComponent();
                    builder.OpenElement("div");
                    builder.AddContent($"{transfer.Amount:N}");
                    builder.CloseElement();
                    builder.CloseRegion();
                }
                builder.CloseRegion();
                builder.CloseElement();
            }
            if (Value.AssetTransfers.Count > 0)
            {
                builder.AddMarkupContent("<div>Asset Transfers</div>");
                builder.OpenElement("div");
                builder.AddAttribute("hg-asset-transfers");
                builder.AddMarkupContent("<div>Asset</div><div>From</div><div>To</div>");
                builder.OpenRegion();
                int count = 0;
                foreach (var transfer in Value.AssetTransfers)
                {
                    builder.OpenRegion(count++);
                    builder.OpenComponent<AssetDisplay>();
                    builder.AddAttribute("Value", transfer.Asset);
                    builder.CloseComponent();
                    builder.OpenComponent<AddressDisplay>();
                    builder.AddAttribute("Value", transfer.From);
                    builder.CloseComponent();
                    builder.OpenComponent<AddressDisplay>();
                    builder.AddAttribute("Value", transfer.To);
                    builder.CloseComponent();
                    builder.CloseRegion();
                }
                builder.CloseRegion();
                builder.CloseElement();
            }
            if (Value.Royalties.Count > 0)
            {
                builder.AddMarkupContent("<div>Royalty Payments</div>");
                builder.OpenElement("div");
                builder.AddAttribute("hg-royalty-payments");
                builder.AddMarkupContent("<div>Token</div><div>Payer</div><div>Amount</div><div>Receiver</div>");
                builder.OpenRegion();
                int count = 0;
                var previous = Address.None;
                foreach (var transfer in Value.Royalties)
                {
                    builder.OpenRegion(count++);
                    if (previous != transfer.Token)
                    {
                        builder.OpenComponent<AddressDisplay>();
                        builder.AddAttribute("Value", transfer.Token);
                        builder.CloseComponent();
                        previous = transfer.Token;
                    }
                    else
                    {
                        builder.OpenElement("div");
                        builder.CloseElement();
                    }
                    builder.OpenElement("div");
                    builder.OpenRegion();
                    var count2 = 0;
                    foreach (var payer in transfer.Payers)
                    {
                        builder.OpenRegion(count2++);
                        builder.OpenComponent<AddressDisplay>();
                        builder.AddAttribute("Value", payer);
                        builder.CloseComponent();
                        builder.CloseRegion();
                    }
                    builder.CloseRegion();
                    builder.CloseElement();
                    builder.OpenElement("div");
                    builder.AddContent($"{transfer.Amount:N}");
                    builder.CloseElement();
                    builder.OpenComponent<AddressDisplay>();
                    builder.AddAttribute("Value", transfer.Receiver);
                    builder.CloseComponent();
                    builder.CloseRegion();
                }
                builder.CloseRegion();
                builder.CloseElement();
            }
            if (Value.Associations.Count > 0)
            {
                builder.AddMarkupContent("<div>Token Associations</div>");
                builder.OpenElement("div");
                builder.AddAttribute("hg-associations");
                builder.AddMarkupContent("<div>Token</div><div>Account</div>");
                builder.OpenRegion();
                int count = 0;
                var previous = Address.None;
                foreach (var transfer in Value.Associations)
                {
                    builder.OpenRegion(count++);
                    if (previous != transfer.Account)
                    {
                        builder.OpenComponent<AddressDisplay>();
                        builder.AddAttribute("Value", transfer.Token);
                        builder.CloseComponent();
                        previous = transfer.Token;
                    }
                    else
                    {
                        builder.OpenElement("div");
                        builder.CloseElement();
                    }
                    builder.OpenComponent<AddressDisplay>();
                    builder.AddAttribute("Value", transfer.Account);
                    builder.CloseComponent();
                    builder.CloseRegion();
                }
                builder.CloseRegion();
                builder.CloseElement();
            }
        }
        builder.CloseElement();
    }

    private string GetRecordType()
    {
        return Value switch
        {
            CreateAccountRecord => "Create Account",
            FileRecord => "Create File",
            CreateTopicRecord => "Create Topic",
            CreateContractRecord => "Create Contract",
            CreateTokenRecord => "Create Token",
            SubmitMessageRecord => "Submit Message",
            AssetMintRecord => "Mint Asset (NFT)",
            TokenRecord => "Mint, Burn or Confiscate Token",
            CallContractRecord => "Call Contract",
            _ => "Transaction",
        };
    }
}
