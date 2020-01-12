using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hashgraph.Portal.Models
{
    public enum NetworkActivityEventType
    {
        SendingRequest,
        ResponseReceived,
        WaitingForSignature
    }
}
