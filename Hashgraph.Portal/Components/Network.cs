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
            OnStartTransaction();
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
                    ctx.OnSendingRequest = OnSendingRequest;
                    ctx.OnResponseReceived = OnResponseReceived;
                    ctx.Signatory = new Signatory(OnSignRequest);
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
                OnTransactionError(ex);
            }
            finally
            {
                OnTransactionFinished();
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
        private void OnStartTransaction()
        {
            _transactionIds.Clear();
            _logEntries.Clear();
            _errors.Clear();
            ShowTab = LOG_TAB;
        }
        private void OnSendingRequest(IMessage message)
        {
            _logEntries.Add(new NetworkActivityEvent
            {
                Type = NetworkActivityEventType.SendingRequest,
                Data = JsonSerializer.Serialize(JsonDocument.Parse(JsonFormatter.Default.Format(message)).RootElement, new JsonSerializerOptions { WriteIndented = true })
            });
        }
        private void OnResponseReceived(int tryNo, IMessage message)
        {
            _logEntries.Add(new NetworkActivityEvent
            {
                Type = NetworkActivityEventType.ResponseReceived,
                TryNo = tryNo,
                Data = JsonSerializer.Serialize(JsonDocument.Parse(JsonFormatter.Default.Format(message)).RootElement, new JsonSerializerOptions { WriteIndented = true })
            });
        }
        private async Task OnSignRequest(IInvoice invoice)
        {
            var transactionBody = Proto.TransactionBody.Parser.ParseFrom(invoice.TxBytes.ToArray());
            _logEntries.Add(new NetworkActivityEvent { 
                Type = NetworkActivityEventType.WaitingForSignature,
                Data = JsonSerializer.Serialize(JsonDocument.Parse(JsonFormatter.Default.Format(transactionBody)).RootElement, new JsonSerializerOptions { WriteIndented = true })
            });
            await _signTransactionDialog.PromptForSignaturesAsync(invoice);
            _transactionIds.Add(invoice.TxId);
        }
        private void OnTransactionError(Exception ex)
        {
            _errors.Add(ex);
        }
        private void OnTransactionFinished()
        {
            ShowTab = _errors.Count == 0 ? RESULTS_TAB : ERRORS_TAB;
            StateHasChanged();
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
