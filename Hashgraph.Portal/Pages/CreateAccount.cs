﻿using Hashgraph.Portal.Components;
using Hashgraph.Portal.Services;
using Microsoft.AspNetCore.Components;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Hashgraph.Portal.Pages
{
    public partial class CreateAccount : ComponentBase
    {
        [Inject] public DefaultsService DefaultsService { get; set; }

        private Network _network = null;
        private CreateAccountInput _input = new CreateAccountInput();
        private CreateAccountReceipt _output = null;

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
                var createParams = new CreateAccountParams
                {
                    Endorsement = _input.Endorsement,
                    InitialBalance = (ulong)_input.InitialBalance.GetValueOrDefault(),
                    RequireReceiveSignature = _input.RequireReceiveSignature,
                    Proxy = _input.Proxy
                };
                _output = await client.CreateAccountAsync(createParams, ctx => ctx.Memo = _input.Memo?.Trim());
            });
        }
    }
    public class CreateAccountInput
    {
        [Required(ErrorMessage = "Please select a Network Gateway Node.")]
        public Gateway Gateway { get; set; }
        [Required(ErrorMessage = "Please enter the account that will pay the Transaction Fees.")]
        public Address Payer { get; set; }
        [Required(ErrorMessage = "Please enter an initial balance, this will be taken from the Payer account, it can be zero.")]
        [Range(0, long.MaxValue, ErrorMessage = "The initial balance must be greater than or equal to zero.")]
        public long? InitialBalance { get; set; }
        [Required] public Endorsement Endorsement { get; set; }
        public bool RequireReceiveSignature { get; set; }
        public Address Proxy { get; set; }
        [MaxLength(100, ErrorMessage = "The memo field cannot exceed 100 characters.")]
        public string Memo { get; set; }
    }
}
