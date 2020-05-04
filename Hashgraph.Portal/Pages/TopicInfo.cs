using Hashgraph.Portal.Components;
using Hashgraph.Portal.Services;
using Microsoft.AspNetCore.Components;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Hashgraph.Portal.Pages
{
    public partial class TopicInfo : ComponentBase
    {
        [Inject] public DefaultsService DefaultsService { get; set; }

        private Network _network = null;
        private TopicInfoInput _input = new TopicInfoInput();
        private Hashgraph.TopicInfo _output = null;

        protected override void OnInitialized()
        {
            _input.Payer = DefaultsService.Payer;
            _input.Gateway = DefaultsService.Gateway;
            base.OnInitialized();
        }
        protected async Task HandleValidSubmit()
        {
            _output = null;
            await _network.ExecuteAsync(_input.Gateway, _input.Payer, async client =>
            {
                _output = await client.GetTopicInfoAsync(_input.Topic, ctx => ctx.Memo = _input.Memo?.Trim());
            });
        }
    }
    public class TopicInfoInput
    {
        [Required(ErrorMessage = "Please select a Network Gateway Node.")]
        public Gateway Gateway { get; set; }
        [Required(ErrorMessage = "Please enter the account that will pay the Query Transaction Fees.")]
        public Address Payer { get; set; }
        [Required(ErrorMessage = "Please enter the topic you wish to Query.")]
        public Address Topic { get; set; }
        [MaxLength(100, ErrorMessage = "The memo field cannot exceed 100 characters.")]
        public string Memo { get; set; }
    }
}
