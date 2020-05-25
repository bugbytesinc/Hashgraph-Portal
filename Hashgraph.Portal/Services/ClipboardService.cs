﻿using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace Hashgraph.Portal.Services
{
    public class ClipboardService
    {
        private readonly IJSRuntime _runtime;

        public bool Enabled { get; private set; } = true;

        public ClipboardService(IJSRuntime runtime)
        {
            _runtime = runtime;
        }

        public async Task QueryCapabilitiesAsync()
        {
            Enabled = await _runtime.InvokeAsync<bool>("window.hashgraph.supportsClipboard");
        }

        public async Task WriteToClipboardAsync(string data)
        {
            if (Enabled)
            {
                await _runtime.InvokeVoidAsync("navigator.clipboard.writeText", data);
            }
        }

        public async Task<string> ReadFromClipboardAsync()
        {
            if (Enabled)
            {
                return await _runtime.InvokeAsync<string>("navigator.clipboard.readText");
            }
            return string.Empty;
        }
    }
}
