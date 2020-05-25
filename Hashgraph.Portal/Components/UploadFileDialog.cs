#pragma warning disable CA1031
using BlazorInputFile;
using Microsoft.AspNetCore.Components;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Hashgraph.Portal.Components
{
    public partial class UploadFileDialog : ComponentBase
    {
        private UploadFileInput _input = null;
        private TaskCompletionSource<ReadOnlyMemory<byte>> _taskCompletionSource = null;
        public Task<ReadOnlyMemory<byte>> PromptToUploadFile()
        {
            _input = new UploadFileInput();
            _taskCompletionSource = new TaskCompletionSource<ReadOnlyMemory<byte>>();
            StateHasChanged();
            return _taskCompletionSource.Task;
        }
        private async Task OnFileInputChanged(IFileListEntry[] files)
        {
            // Todo check for 4K
            var file = files.FirstOrDefault();
            if (file != null)
            {
                await using var ms = new MemoryStream();
                await file.Data.CopyToAsync(ms);
                _input.Contents = ms.ToArray();
                if(_input.Contents.IsEmpty)
                {
                    _input.StatusMessage = $"The file {file.Name} appears to be empty.";
                }
                else
                {
                    _input.StatusMessage = $"Loaded {file.Size} bytes from {file.Name}";
                }
            }
            else
            {
                _input.Contents = ReadOnlyMemory<byte>.Empty;
                _input.StatusMessage = "Please select a file to upload...";
            }
        }
        private void Submit()
        {
            if (!_input.Contents.IsEmpty)
            {
                _taskCompletionSource.SetResult(_input.Contents);
                _taskCompletionSource = null;
                _input = null;
                StateHasChanged();
            }
            else
            {
                _input.StatusMessage = "Please upload a file before continuing.";
            }
        }
        private void Close()
        {
            _input = null;
            _taskCompletionSource.SetResult(ReadOnlyMemory<byte>.Empty);
            _taskCompletionSource = null;
            StateHasChanged();
        }
    }
    public class UploadFileInput
    {
        public string StatusMessage { get; set; }
        public ReadOnlyMemory<byte> Contents { get; set; }
    }
}
