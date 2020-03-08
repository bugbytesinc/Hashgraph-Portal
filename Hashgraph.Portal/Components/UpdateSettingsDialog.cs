﻿using Hashgraph.Portal.Services;
using Microsoft.AspNetCore.Components;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Hashgraph.Portal.Components
{
    public partial class UpdateSettingsDialog : ComponentBase
    {
        [Inject] public DefaultsService DefaultsService { get; set; }

        private UpdateSettingsDialogInput _input = null;
        private TaskCompletionSource<bool> _taskCompletionSource = null;
        public Task<bool> PromptUpdateSettingsAsync()
        {
            _input = new UpdateSettingsDialogInput()
            {
                FeeLimit = DefaultsService.FeeLimit,
                TransactionDuration = (int)DefaultsService.TransactionDuration.TotalSeconds
            };
            _taskCompletionSource = new TaskCompletionSource<bool>();
            StateHasChanged();
            return _taskCompletionSource.Task;
        }
        private void HandleValidSubmit()
        {
            DefaultsService.FeeLimit = _input.FeeLimit;
            DefaultsService.TransactionDuration = TimeSpan.FromSeconds(_input.TransactionDuration);
            _taskCompletionSource.SetResult(true);
            _taskCompletionSource = null;
            _input = null;
            StateHasChanged();
        }
        private void Close()
        {
            _input = null;
            _taskCompletionSource.SetResult(false);
            _taskCompletionSource = null;
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
    }
}
