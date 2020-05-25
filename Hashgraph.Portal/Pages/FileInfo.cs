using Hashgraph.Portal.Components;
using Hashgraph.Portal.Services;
using Microsoft.AspNetCore.Components;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Hashgraph.Portal.Pages
{
    public partial class FileInfo : ComponentBase
    {
        [Inject] public DefaultsService DefaultsService { get; set; }

        private Network _network = null;
        private FileInfoInput _input = new FileInfoInput();
        private Hashgraph.FileInfo _output = null;

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
                _output = await client.GetFileInfoAsync(_input.File, ctx => ctx.Memo = _input.Memo?.Trim());
            });
        }
    }
    public class FileInfoInput
    {
        [Required(ErrorMessage = "Please select a Network Gateway Node.")]
        public Gateway Gateway { get; set; }
        [Required(ErrorMessage = "Please enter the account that will pay the Query Transaction Fees.")]
        public Address Payer { get; set; }
        [Required(ErrorMessage = "Please enter the File you wish to query.")]
        public Address File { get; set; }
        [MaxLength(100, ErrorMessage = "The memo field cannot exceed 100 characters.")]
        public string Memo { get; set; }
    }
}
