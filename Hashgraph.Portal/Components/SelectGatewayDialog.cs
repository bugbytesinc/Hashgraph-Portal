using Hashgraph.Portal.Services;
using Microsoft.AspNetCore.Components;

namespace Hashgraph.Portal.Components
{
    public partial class SelectGatewayDialog : ComponentBase
    {
        [Inject] public GatewayListService GatewayListService { get; set; }
        [Parameter] public EventCallback<Gateway> SelectedEventCallback { get; set; }
        private Gateway _selected = null;
        private SelectGatewayInput _input = null;
        public void Show(Gateway selected)
        {
            _selected = selected;
            _input = new SelectGatewayInput();
            StateHasChanged();
        }
        public void Close()
        {
            _input = null;
            StateHasChanged();
        }

        public async void Select(Gateway gateway)
        {
            await SelectedEventCallback.InvokeAsync(gateway);
            _input = null;
            StateHasChanged();
        }
    }
    public class SelectGatewayInput
    {
    }
}
