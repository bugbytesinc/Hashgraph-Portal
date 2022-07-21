using System.ComponentModel;

namespace Hashgraph.Portal.Components
{
    public enum StakingSelection
    {
        [Description("Staking Preferences not Declared")]
        Unset,
        [Description("Staking is Delcined")]
        Declined,
        [Description("Stake to Node")]
        Node,
        [Description("Proxy Stake to Account")]
        Account
    }
}
