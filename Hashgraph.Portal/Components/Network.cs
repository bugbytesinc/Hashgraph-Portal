#pragma warning disable CA1031
#pragma warning disable CA1707
using Google.Protobuf;
using Hashgraph.Portal.Models;
using Hashgraph.Portal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Hashgraph.Portal.Components
{
    public partial class Network : ComponentBase, IDisposable
    {
        [Parameter] public RenderFragment ChildContent { get; set; }
        [CascadingParameter] public EditContext CurrentEditContext { get; set; }
        [Inject] public DefaultsService DefaultsService { get; set; }
        [Inject] public GatewayListService GatewayListService { get; set; }

        protected const int RESULTS_TAB = 0;
        protected const int LOG_TAB = 1;
        protected const int TXID_TAB = 2;
        protected const int ERRORS_TAB = 3;
        protected int ShowTab { get; set; } = RESULTS_TAB;
        private List<NetworkActivityEvent> _logEntries { get; } = new List<NetworkActivityEvent>();
        private List<TxId> _transactionIds { get; } = new List<TxId>();
        private List<Exception> _errors { get; } = new List<Exception>();
        private bool _isMainNetwork = false;
        private int _txSequenceNo = 0;
        private bool _isProcessing = false;

        private EditContext _previousEditContext;
        private SignTransactionDialog _signTransactionDialog;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                DetachValidationStateChangedListener();
            }
        }
        public async Task ExecuteAsync(Gateway gateway, Address payer, Func<Client, Task> executeFunction)
        {
            int txSequenceNo = OnStartTransaction();
            try
            {
                if (executeFunction is null)
                {
                    throw new ArgumentNullException(nameof(executeFunction));
                }
                _isMainNetwork = GatewayListService.MainNet.Contains(gateway);
                await using var client = DefaultsService.RootClient.Clone(ctx =>
                {
                    ctx.Gateway = gateway;
                    ctx.Payer = payer;
                    ctx.FeeLimit = DefaultsService.FeeLimit;
                    ctx.TransactionDuration = DefaultsService.TransactionDuration;
                    ctx.RetryDelay = Util.ComputeRetryDelay(DefaultsService.ReceiptWaitDuration, DefaultsService.ReceiptRetryCount);
                    ctx.RetryCount = DefaultsService.ReceiptRetryCount;
                    ctx.OnSendingRequest = SetupOnSendingRequest(txSequenceNo);
                    ctx.OnResponseReceived = SetupOnResponseReceived(txSequenceNo);
                    ctx.Signatory = SetupOnSignRequest(txSequenceNo);
                });
                await executeFunction(client);
                if (gateway != null)
                {
                    DefaultsService.Gateway = gateway;
                }
                if (payer != null)
                {
                    DefaultsService.Payer = payer;
                }
            }
            catch (Exception ex)
            {
                OnTransactionError(txSequenceNo, ex);
            }
            finally
            {
                OnTransactionFinished(txSequenceNo);
            }
        }

        protected override void OnParametersSet()
        {
            if (CurrentEditContext != _previousEditContext)
            {
                DetachValidationStateChangedListener();
                if (CurrentEditContext != null)
                {
                    CurrentEditContext.OnValidationStateChanged += HandleStateValidationStateChange;
                }
                _previousEditContext = CurrentEditContext;
            }
        }
        private void HandleStateValidationStateChange(object sender, ValidationStateChangedEventArgs e)
        {
            if (ShowTab != 2 && sender is EditContext context && context.GetValidationMessages().Any())
            {
                ShowTab = ERRORS_TAB;
            }
        }
        private int OnStartTransaction()
        {
            // We need to track the individual requests, it is possible
            // to abandon a query on a node if it is mis-behaving and re-
            // submit it to a different node.  When the errors come back
            // from the previous request (eventually) we want to ignore them
            // since the user's focus has moved on.  Since the framework
            // re-uses this control for the new request, we need to track
            // the individual requests ourselves.
            var txSequenceNo = Interlocked.Increment(ref _txSequenceNo);
            _isProcessing = true;
            _transactionIds.Clear();
            _logEntries.Clear();
            _errors.Clear();
            ShowTab = LOG_TAB;
            return txSequenceNo;
        }
        private Action<IMessage> SetupOnSendingRequest(int txSequenceNo)
        {
            return OnSendingRequest;

            void OnSendingRequest(IMessage message)
            {
                if (txSequenceNo == _txSequenceNo)
                {
                    _logEntries.Add(new NetworkActivityEvent
                    {
                        Type = NetworkActivityEventType.SendingRequest,
                        Data = JsonSerializer.Serialize(JsonDocument.Parse(JsonFormatter.Default.Format(message)).RootElement, new JsonSerializerOptions { WriteIndented = true })
                    });
                    StateHasChanged();
                }
            }
        }
        private Action<int, IMessage> SetupOnResponseReceived(int txSequenceNo)
        {
            return OnResponseReceived;

            void OnResponseReceived(int tryNo, IMessage message)
            {
                if (txSequenceNo == _txSequenceNo)
                {
                    _logEntries.Add(new NetworkActivityEvent
                    {
                        Type = NetworkActivityEventType.ResponseReceived,
                        TryNo = tryNo,
                        Data = JsonSerializer.Serialize(JsonDocument.Parse(JsonFormatter.Default.Format(message)).RootElement, new JsonSerializerOptions { WriteIndented = true })
                    });
                    StateHasChanged();
                }
            }
        }
        private Signatory SetupOnSignRequest(int txSequenceNo)
        {
            return new Signatory(OnSignRequest);

            async Task OnSignRequest(IInvoice invoice)
            {
                if (txSequenceNo == _txSequenceNo)
                {
                    var transactionBody = Proto.TransactionBody.Parser.ParseFrom(invoice.TxBytes.ToArray());
                    _logEntries.Add(new NetworkActivityEvent
                    {
                        Type = NetworkActivityEventType.WaitingForSignature,
                        Data = JsonSerializer.Serialize(JsonDocument.Parse(JsonFormatter.Default.Format(transactionBody)).RootElement, new JsonSerializerOptions { WriteIndented = true })
                    });
                    StateHasChanged();
                    await _signTransactionDialog.PromptForSignaturesAsync(invoice);
                    _transactionIds.Add(invoice.TxId);
                }
            }
        }
        private void OnTransactionError(int txSequenceNo, Exception ex)
        {
            if (txSequenceNo == _txSequenceNo)
            {
                _errors.Add(ex);
            }
        }
        private void OnTransactionFinished(int txSequenceNo)
        {
            if (txSequenceNo == _txSequenceNo)
            {
                ShowTab = _errors.Count == 0 ? RESULTS_TAB : ERRORS_TAB;
                _isProcessing = false;
                StateHasChanged();
            }
        }
        private void DetachValidationStateChangedListener()
        {
            if (_previousEditContext != null)
            {
                _previousEditContext.OnValidationStateChanged -= HandleStateValidationStateChange;
            }
        }
        private string[] GetValidationErrors()
        {
            if (CurrentEditContext != null)
            {
                return CurrentEditContext.GetValidationMessages().ToArray();
            }
            return Array.Empty<string>();
        }
    }
}
