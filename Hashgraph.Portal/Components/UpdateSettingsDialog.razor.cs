using Hashgraph.Portal.Services;
using Microsoft.AspNetCore.Components;
using System.ComponentModel.DataAnnotations;

namespace Hashgraph.Portal.Components
{
    public partial class UpdateSettingsDialog : ComponentBase
    {
        [Inject]
        public DefaultsService DefaultsService { get; set; } = default!;

        private UpdateSettingsDialogInput _input = default!;
        private TaskCompletionSource<bool> _taskCompletionSource = default!;
        public Task<bool> PromptUpdateSettingsAsync()
        {
            _input = new UpdateSettingsDialogInput()
            {
                FeeLimit = DefaultsService.FeeLimit,
                TransactionDuration = (int)DefaultsService.TransactionDuration.TotalSeconds,
                ReceiptWaitDuration = (int)DefaultsService.ReceiptWaitDuration.TotalSeconds,
                ReceiptRetryCount = DefaultsService.ReceiptRetryCount
            };
            _taskCompletionSource = new TaskCompletionSource<bool>();
            StateHasChanged();
            return _taskCompletionSource.Task;
        }
        private void HandleValidSubmit()
        {
            DefaultsService.FeeLimit = _input.FeeLimit;
            DefaultsService.TransactionDuration = TimeSpan.FromSeconds(_input.TransactionDuration);
            DefaultsService.ReceiptWaitDuration = TimeSpan.FromSeconds(_input.ReceiptWaitDuration);
            DefaultsService.ReceiptRetryCount = _input.ReceiptRetryCount;
            _taskCompletionSource.SetResult(true);
            _taskCompletionSource = default!;
            _input = default!;
            StateHasChanged();
        }
        private void Close()
        {
            _input = default!;
            _taskCompletionSource.SetResult(false);
            _taskCompletionSource = default!;
            StateHasChanged();
        }
    }
    public class UpdateSettingsDialogInput
    {
        [Required(ErrorMessage = "The Maximum Allowed Fee (in tℏ) is required.")]
        [Range(1, double.MaxValue, ErrorMessage = "The Maximum Allowed Fee must be greater than zero.")]
        public long FeeLimit { get; set; }
        [Required(ErrorMessage = "The Valid Transaction Duration (in seconds) is required.")]
        [Range(1, 360, ErrorMessage = "The Valid Transaciton Duration must be greater than zero and not more than 3 minutes (180 seconds).")]
        public int TransactionDuration { get; set; }
        [Required(ErrorMessage = "The Receipt Max Wait Duration (in seconds) is required.")]
        [Range(10, int.MaxValue, ErrorMessage = "The Receipt Max Wait Duration must be greater than 10 seconds.")]
        public int ReceiptWaitDuration { get; set; }
        [Required(ErrorMessage = "The Get Receipt Retry Count is required.")]
        [Range(2, int.MaxValue, ErrorMessage = "The Get Receipt Retry Count must be greater than one.")]
        public int ReceiptRetryCount { get; set; }
    }
}
