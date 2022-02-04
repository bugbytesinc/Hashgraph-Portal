using Hashgraph.Components.Services;
using Microsoft.AspNetCore.Components;

namespace Hashgraph.Components;

public partial class SelectGatewayDialog : ComponentBase
{
    [Inject] public GatewayListService GatewayListService { get; set; } = default!;
    [Parameter] public EventCallback<Gateway> SelectedEventCallback { get; set; }
    private SelectGatewayInput _input = default!;
    private Dictionary<string, Gateway[]> _gateways = default!;

    public async Task ShowAsync(Gateway? selected)
    {
        _gateways = await GatewayListService.GetNetworkGateways();
        var network = FindNetwork(selected);
        var list = FindGatewayList(network);

        _input = new SelectGatewayInput()
        {
            SelectedGateway = selected,
            SelectedNetwork = network,
            Networks = _gateways.Keys.OrderBy(n => n).ToArray(),
            Gateways = list
        };
        StateHasChanged();
    }

    public void Close()
    {
        _input = default!;
        StateHasChanged();
    }

    public async void Select(Gateway gateway)
    {
        await SelectedEventCallback.InvokeAsync(gateway);
        _input = default!;
        StateHasChanged();
    }

    internal void NetworkChanged(string network)
    {
        _input.SelectedNetwork = network;
        _input.Gateways = FindGatewayList(network);
    }

    private string FindNetwork(Gateway? selected)
    {
        foreach (var pair in _gateways)
        {
            if (pair.Value.Contains(selected))
            {
                return pair.Key;
            }
        }
        return _gateways.Keys.FirstOrDefault() ?? "Main";
    }

    private Gateway[] FindGatewayList(string network)
    {
        if (_gateways.TryGetValue(network, out Gateway[]? list))
        {
            return list;
        }
        return Array.Empty<Gateway>();
    }
}
public class SelectGatewayInput
{
    public Gateway? SelectedGateway { get; set; }
    public string SelectedNetwork { get; set; } = default!;
    public string[] Networks { get; set; } = default!;
    public Gateway[] Gateways { get; set; } = default!;
}