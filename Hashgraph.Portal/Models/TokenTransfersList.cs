#pragma warning disable CA1307
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hashgraph.Portal.Models
{
    public sealed class TokenTransferList : List<TokenTransferGroup>, IEquatable<TokenTransferList> 
    {
        public bool Equals(TokenTransferList other)
        {
            if (other is null || Count != other.Count)
            {
                return false;
            }
            for (int i = 0; i < Count; i++)
            {
                if ( this[i] != other[i])
                {
                    return false;
                }
            }
            return true;
        }
        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj is TokenTransferList other)
            {
                return Equals(other);
            }
            return false;
        }
        public override int GetHashCode()
        {
            return $"TokenTransferList:{string.Join('.', this.Select(c => c.GetHashCode()))}".GetHashCode();
        }
        public static bool operator ==(TokenTransferList left, TokenTransferList right)
        {
            if (left is null)
            {
                return right is null;
            }
            return left.Equals(right);
        }
        public static bool operator !=(TokenTransferList left, TokenTransferList right)
        {
            return !(left == right);
        }
        public IEnumerable<TokenTransfer> ToTransferList()
        {
            var list = new List<TokenTransfer>();
            foreach (var group in this)
            {
                foreach (var xfer in group.Transfers.ToTransferDictionary())
                {
                    list.Add(new TokenTransfer(group.Token, xfer.Key, xfer.Value));
                }
            }
            return list;
        }
    }
    public sealed class TokenTransferGroup : IEquatable<TokenTransferGroup>
    {
        public Address Token { get; set; }
        public CryptoTransferList Transfers { get; set; } = new CryptoTransferList();
        public bool Equals(TokenTransferGroup other)
        {
            if (other is null)
            {
                return false;
            }
            return
                Token == other.Token &&
                Transfers == other.Transfers;
        }
        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj is TokenTransferGroup other)
            {
                return Equals(other);
            }
            return false;
        }
        public override int GetHashCode()
        {
            return $"TokenTransferGroup:{Token}:{Transfers.GetHashCode()}".GetHashCode();
        }
        public static bool operator ==(TokenTransferGroup left, TokenTransferGroup right)
        {
            if (left is null)
            {
                return right is null;
            }
            return left.Equals(right);
        }
        public static bool operator !=(TokenTransferGroup left, TokenTransferGroup right)
        {
            return !(left == right);
        }
    }
}

