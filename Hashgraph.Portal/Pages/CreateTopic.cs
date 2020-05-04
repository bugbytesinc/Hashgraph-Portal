using Hashgraph.Portal.Components;
using Hashgraph.Portal.Services;
using Microsoft.AspNetCore.Components;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Hashgraph.Portal.Pages
{
    public partial class CreateTopic : ComponentBase
    {
        [Inject] public DefaultsService DefaultsService { get; set; }

        private Network _network = null;
        private CreateTopicInput _input = new CreateTopicInput();
        private CreateTopicReceipt _output = null;

        protected override void OnInitialized()
        {
            _input.Gateway = DefaultsService.Gateway;
            _input.Payer = DefaultsService.Payer;
            base.OnInitialized();
        }

        protected async Task HandleValidSubmit()
        {
            _output = null;
            await _network.ExecuteAsync(_input.Gateway, _input.Payer, async client =>
            {
                var createParams = new CreateTopicParams
                {
                    Memo = _input.Desciption?.Trim(),
                    Administrator = _input.Administrator != Endorsement.None ? _input.Administrator : null,
                    Participant = _input.Participant != Endorsement.None ? _input.Participant : null,
                    RenewAccount = _input.RenewAccount != Address.None ? _input.RenewAccount : null
                };
                _output = await client.CreateTopicAsync(createParams, ctx => ctx.Memo = _input.Memo?.Trim());
            });
        }
    }
    public class CreateTopicInput
    {
        [Required(ErrorMessage = "Please select a Network Gateway Node.")]
        public Gateway Gateway { get; set; }
        [Required(ErrorMessage = "Please enter the Account that will pay the Transaction Fees.")]
        public Address Payer { get; set; }
        [Required(ErrorMessage = "Please a short description or name of the topic.")]
        [MaxLength(100, ErrorMessage = "The topic description field cannot exceed 100 characters.")]
        public string Desciption { get; set; }
        public Endorsement Administrator { get; set; }
        public Endorsement Participant { get; set; }
        public Address RenewAccount { get; set; }
        [MaxLength(100, ErrorMessage = "The memo field cannot exceed 100 characters.")]
        public string Memo { get; set; }
    }
}
