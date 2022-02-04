using Hashgraph.Portal.Services;
using Microsoft.AspNetCore.Components;

namespace Hashgraph.Portal.Components
{
    public partial class SettingsDisplay : ComponentBase
    {
        [Inject] public DefaultsService DefaultsService { get; set; } = default!;

        private UpdateSettingsDialog _updateSettingsDialog = default!;

        private async Task OnClick()
        {
            if (await _updateSettingsDialog.PromptUpdateSettingsAsync())
            {
                StateHasChanged();
            }
        }
    }
}
