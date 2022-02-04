using Hashgraph.Components.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Hashgraph.Components;

public static class ComponentExtensions
{
    public static void AddHashgraphComponentServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<GatewayListService>();
        serviceCollection.AddScoped<ClipboardService>();
    }
}

