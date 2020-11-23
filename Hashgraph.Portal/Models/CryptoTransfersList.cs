#pragma warning disable CA1307
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hashgraph.Portal.Models
{
    public sealed class CryptoTransferList : IEquatable<CryptoTransferList>
    {
        public List<CryptoTransfer> From { get; } = new List<CryptoTransfer>(new []{new CryptoTransfer()});
        public List<CryptoTransfer> To { get; } = new List<CryptoTransfer>(new[] { new CryptoTransfer() });
        public bool Equals(CryptoTransferList other)
        {
            if (other is null || From.Count != other.From.Count || To.Count != other.To.Count)
            {
                return false;
            }
            for (int i = 0; i < From.Count; i++)
            {
                if (From[i] != other.From[i])
                {
                    return false;
                }
            }
            for (int i = 0; i < To.Count; i++)
            {
                if (To[i] != other.To[i])
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
            if (obj is CryptoTransferList other)
            {
                return Equals(other);
            }
            return false;
        }
        public override int GetHashCode()
        {
            return $"CryptoTransferList:{string.Join('.', From.Select(c => c.GetHashCode()))}:{string.Join('.', To.Select(c => c.GetHashCode()))}".GetHashCode();
        }
        public static bool operator ==(CryptoTransferList left, CryptoTransferList right)
        {
            if (left is null)
            {
                return right is null;
            }
            return left.Equals(right);
        }
        public static bool operator !=(CryptoTransferList left, CryptoTransferList right)
        {
            return !(left == right);
        }
        public Dictionary<Address, long> ToTransferDictionary()
        {
            var xferMap = new Dictionary<Address, long>();
            foreach (var xfer in From)
            {
                if (xfer.Address != null)
                {
                    if (xferMap.TryGetValue(xfer.Address, out long value))
                    {
                        xferMap[xfer.Address] = value - (long)xfer.Amount;
                    }
                    else
                    {
                        xferMap[xfer.Address] = -(long)xfer.Amount;
                    }
                }
            }
            foreach (var xfer in To)
            {
                if (xfer.Address != null)
                {
                    if (xferMap.TryGetValue(xfer.Address, out long value))
                    {
                        xferMap[xfer.Address] = value + (long)xfer.Amount;
                    }
                    else
                    {
                        xferMap[xfer.Address] = (long)xfer.Amount;
                    }
                }
            }
            return xferMap;
        }
    }
    public sealed class CryptoTransfer : IEquatable<CryptoTransfer>
    {
        public Address Address { get; set; }
        public long Amount { get; set; }
        public bool Equals(CryptoTransfer other)
        {
            if (other is null)
            {
                return false;
            }
            return
                Address == other.Address &&
                Amount == other.Amount;
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
            if (obj is CryptoTransfer other)
            {
                return Equals(other);
            }
            return false;
        }
        public override int GetHashCode()
        {
            return $"CryptoTransfer:{Address}:{Amount}".GetHashCode();
        }
        public static bool operator ==(CryptoTransfer left, CryptoTransfer right)
        {
            if (left is null)
            {
                return right is null;
            }
            return left.Equals(right);
        }
        public static bool operator !=(CryptoTransfer left, CryptoTransfer right)
        {
            return !(left == right);
        }
    }
}
