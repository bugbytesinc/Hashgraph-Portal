using Hashgraph.Portal.Services;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Hashgraph.Portal.Components
{
    public partial class SettingsDisplay : ComponentBase
    {
        [Inject] public DefaultsService DefaultsService { get; set; }

        private UpdateSettingsDialog UpdateSettingsDialog { get; set; }

        private async Task OnClick()
        {
            if (await UpdateSettingsDialog.PromptUpdateSettingsAsync())
            {
                StateHasChanged();
            }
        }
    }
}
