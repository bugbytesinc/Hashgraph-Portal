using Hashgraph.Portal.Components;
using Hashgraph.Portal.Services;
using Microsoft.AspNetCore.Components;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Hashgraph.Portal.Pages
{
    public partial class CreateToken : ComponentBase
    {
        [Inject] public DefaultsService DefaultsService { get; set; }

        private Network _network = null;
        private CreateTokenInput _input = new CreateTokenInput();
        private CreateTokenReceipt _output = null;

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
                var hasRenewalAccount = _input.RenewAccount != null && _input.RenewAccount != Address.None;
                var createParams = new CreateTokenParams
                {
                    Name = _input.Name,
                    Symbol = _input.Symbol,
                    Circulation = (ulong) _input.Circulation.GetValueOrDefault(),
                    Decimals = (uint) _input.Decimals.GetValueOrDefault(),
                    Treasury = _input.Treasury,
                    Administrator = _input.Administrator != Endorsement.None ? _input.Administrator : null,
                    GrantKycEndorsement = _input.GrantKycEndorsement != Endorsement.None ? _input.GrantKycEndorsement : null,
                    SuspendEndorsement = _input.SuspendEndorsement != Endorsement.None ? _input.SuspendEndorsement : null,
                    ConfiscateEndorsement = _input.ConfiscateEndorsement != Endorsement.None ? _input.ConfiscateEndorsement : null,
                    SupplyEndorsement = _input.SupplyEndorsement != Endorsement.None ? _input.SupplyEndorsement : null,
                    InitializeSuspended = _input.InitializeSuspended,
                    Expiration = DateTime.UtcNow.AddDays(90),
                    RenewPeriod = hasRenewalAccount ? TimeSpan.FromDays(90) : null,
                    RenewAccount = hasRenewalAccount ? _input.RenewAccount : null
                };
                _output = await client.CreateTokenAsync(createParams, ctx => ctx.Memo = _input.Memo?.Trim());
            });
        }
    }
    public class CreateTokenInput
    {
        [Required(ErrorMessage = "Please select a Network Gateway Node.")]
        public Gateway Gateway { get; set; }
        [Required(ErrorMessage = "Please enter the Account that will pay the Transaction Fees.")]
        public Address Payer { get; set; }
        [Required(ErrorMessage = "Please the name of the token.")]
        [MaxLength(100, ErrorMessage = "The token name field cannot exceed 100 characters.")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Please enter the symbol for this token.")]
        [MaxLength(100, ErrorMessage = "The token symbol field cannot exceed 100 characters.")]
        public string Symbol { get; set; }
        [Required(ErrorMessage = "Please enter the initial circulation of tokens (in the smallest denomination).")]
        [Range(1, long.MaxValue, ErrorMessage = "The initial circulation must be greater than or equal to zero.")]
        public long? Circulation { get; set; }
        [Range(0, long.MaxValue, ErrorMessage = "The number of decimal places must be zero or larger.")]
        public long? Decimals { get; set; }
        [Required(ErrorMessage = "Please enter the treasury account that will receive the initial tokens.")]
        public Address Treasury { get; set; }
        public Endorsement Administrator { get; set; }
        public Endorsement GrantKycEndorsement { get; set; }
        public Endorsement SuspendEndorsement { get; set; }
        public Endorsement ConfiscateEndorsement { get; set; }
        public Endorsement SupplyEndorsement { get; set; }
        public bool InitializeSuspended { get; set; }
        // Skipping Expiration and Renew Period for now
        public Address RenewAccount { get; set; }
        [MaxLength(100, ErrorMessage = "The memo field cannot exceed 100 characters.")]
        public string Memo { get; set; }
    }
}
