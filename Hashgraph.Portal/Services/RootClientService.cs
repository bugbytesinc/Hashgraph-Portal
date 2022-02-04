namespace Hashgraph.Portal.Services;

public class RootClientService
{
    public Client RootClient { get; set; }
    public RootClientService()
    {
        RootClient = new Client();
    }
}