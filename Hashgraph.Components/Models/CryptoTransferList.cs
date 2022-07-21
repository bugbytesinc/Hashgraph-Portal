namespace Hashgraph.Components.Models;

public sealed class CryptoTransferList : IEquatable<CryptoTransferList>
{
    public List<CryptoTransferModel> From { get; } = new List<CryptoTransferModel>(new[] { new CryptoTransferModel() });
    public List<CryptoTransferModel> To { get; } = new List<CryptoTransferModel>(new[] { new CryptoTransferModel() });
    public bool Equals(CryptoTransferList? other)
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
    public override bool Equals(object? obj)
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
    public CryptoTransfer[] ToCryptoTransferList()
    {
        var xferMap = new Dictionary<Address, long>();
        foreach (var xfer in From)
        {
            if (xfer.Address != null)
            {
                if (xferMap.TryGetValue(xfer.Address, out long value))
                {
                    xferMap[xfer.Address] = value - (long)xfer.Amount.GetValueOrDefault();
                }
                else
                {
                    xferMap[xfer.Address] = -(long)xfer.Amount.GetValueOrDefault();
                }
            }
        }
        foreach (var xfer in To)
        {
            if (xfer.Address != null)
            {
                if (xferMap.TryGetValue(xfer.Address, out long value))
                {
                    xferMap[xfer.Address] = value + (long)xfer.Amount.GetValueOrDefault();
                }
                else
                {
                    xferMap[xfer.Address] = (long)xfer.Amount.GetValueOrDefault();
                }
            }
        }
        // Note: this will need to change when we enable spending allowances
        return xferMap.Select(pair => new CryptoTransfer(pair.Key, pair.Value, false)).ToArray();
    }
}
public sealed class CryptoTransferModel : IEquatable<CryptoTransferModel>
{
    public Address? Address { get; set; } = null;
    public long? Amount { get; set; }
    public bool Equals(CryptoTransferModel? other)
    {
        if (other is null)
        {
            return false;
        }
        return
            Address == other.Address &&
            Amount == other.Amount;
    }
    public override bool Equals(object? obj)
    {
        if (obj is null)
        {
            return false;
        }
        if (ReferenceEquals(this, obj))
        {
            return true;
        }
        if (obj is CryptoTransferModel other)
        {
            return Equals(other);
        }
        return false;
    }
    public override int GetHashCode()
    {
        return $"CryptoTransfer:{Address}:{Amount}".GetHashCode();
    }
    public static bool operator ==(CryptoTransferModel left, CryptoTransferModel right)
    {
        if (left is null)
        {
            return right is null;
        }
        return left.Equals(right);
    }
    public static bool operator !=(CryptoTransferModel left, CryptoTransferModel right)
    {
        return !(left == right);
    }
}