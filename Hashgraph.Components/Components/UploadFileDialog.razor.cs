using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Hashgraph.Components
{
    public partial class UploadFileDialog : ComponentBase
    {
        private const int MAX_FILESIZE = 5120;
        private UploadFileInput _input = default!;
        private TaskCompletionSource<ReadOnlyMemory<byte>> _taskCompletionSource = default!;
        public Task<ReadOnlyMemory<byte>> PromptToUploadFile()
        {
            _input = new UploadFileInput();
            _taskCompletionSource = new TaskCompletionSource<ReadOnlyMemory<byte>>();
            StateHasChanged();
            return _taskCompletionSource.Task;
        }
        private async Task OnFileInputChanged(InputFileChangeEventArgs evt)
        {
            var file = evt.File;
            if (file is null)
            {
                _input.Contents = ReadOnlyMemory<byte>.Empty;
                _input.StatusMessage = "Please select a file to upload...";
            }
            else if (file.Size > MAX_FILESIZE)
            {
                _input.Contents = ReadOnlyMemory<byte>.Empty;
                _input.StatusMessage = "File is too big...";
            }
            else
            {
                await using var ms = new MemoryStream();
                await file.OpenReadStream(MAX_FILESIZE).CopyToAsync(ms);
                _input.Contents = ms.ToArray();
                if (_input.Contents.IsEmpty)
                {
                    _input.StatusMessage = $"The file {file.Name} appears to be empty.";
                }
                else
                {
                    _input.StatusMessage = $"Loaded {file.Size} bytes from {file.Name}";
                }
            }
        }
        private void Submit()
        {
            if (!_input.Contents.IsEmpty)
            {
                _taskCompletionSource.SetResult(_input.Contents);
                _taskCompletionSource = default!;
                _input = default!;
                StateHasChanged();
            }
            else
            {
                _input.StatusMessage = "Please upload a file before continuing.";
            }
        }
        private void Close()
        {
            _input = default!;
            _taskCompletionSource.SetResult(ReadOnlyMemory<byte>.Empty);
            _taskCompletionSource = default!;
            StateHasChanged();
        }
    }
    public class UploadFileInput
    {
        public string StatusMessage { get; set; } = string.Empty;
        public ReadOnlyMemory<byte> Contents { get; set; }
    }
}
