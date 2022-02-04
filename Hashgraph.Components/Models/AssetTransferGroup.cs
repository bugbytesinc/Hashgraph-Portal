namespace Hashgraph.Components.Models;

public sealed class AssetTransferList : List<AssetTransferGroup>, IEquatable<AssetTransferList>
{
    public bool Equals(AssetTransferList? other)
    {
        if (other is null || Count != other.Count)
        {
            return false;
        }
        for (int i = 0; i < Count; i++)
        {
            if (this[i] != other[i])
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
        if (obj is AssetTransferList other)
        {
            return Equals(other);
        }
        return false;
    }
    public override int GetHashCode()
    {
        return $"AssetTransferList:{string.Join('.', this.Select(c => c.GetHashCode()))}".GetHashCode();
    }
    public static bool operator ==(AssetTransferList left, AssetTransferList right)
    {
        if (left is null)
        {
            return right is null;
        }
        return left.Equals(right);
    }
    public static bool operator !=(AssetTransferList left, AssetTransferList right)
    {
        return !(left == right);
    }
    public IEnumerable<AssetTransfer> ToAssetTransferList()
    {
        return this.SelectMany(group => group.ToAssetTransfers());
    }
}

public sealed class AssetTransferGroup : IEquatable<AssetTransferGroup>
{
    public Address? Token { get; set; } = null;
    public Address? From { get; set; } = null;
    public Address? To { get; set; } = null;
    public string? SerialNumbers { get; set; } = null;
    public bool Equals(AssetTransferGroup? other)
    {
        if (other is null)
        {
            return false;
        }
        return
            Token == other.Token &&
            From == other.From &&
            To == other.To &&
            SerialNumbers == other.SerialNumbers;
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
        if (obj is AssetTransferGroup other)
        {
            return Equals(other);
        }
        return false;
    }
    public override int GetHashCode()
    {
        return $"AssetTransferGroup:{Token?.GetHashCode()}:{From?.GetHashCode()}:{To?.GetHashCode()}:{SerialNumbers}".GetHashCode();
    }
    public static bool operator ==(AssetTransferGroup left, AssetTransferGroup right)
    {
        if (left is null)
        {
            return right is null;
        }
        return left.Equals(right);
    }
    public static bool operator !=(AssetTransferGroup left, AssetTransferGroup right)
    {
        return !(left == right);
    }

    public IReadOnlyCollection<AssetTransfer> ToAssetTransfers()
    {
        return SerialNumbers!.Split(',').Select(s => new AssetTransfer(new Asset(Token!, long.Parse(s.Trim())), From!, To!)).ToArray();
    }
}