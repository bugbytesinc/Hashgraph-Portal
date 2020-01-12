namespace Hashgraph.Portal.Services
{
    public class DefaultsService
    {
        public Address Payer { get; set; }
        public Gateway Gateway { get; set; }
        public Client RootClient { get; set; } = new Client();
    }
}
