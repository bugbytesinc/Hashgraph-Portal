namespace Hashgraph.Components.Models;

public sealed class RoyaltyList : List<RoyaltyDefinition>, IEquatable<RoyaltyList>
{
    public bool Equals(RoyaltyList? other)
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
        if (obj is RoyaltyList other)
        {
            return Equals(other);
        }
        return false;
    }
    public override int GetHashCode()
    {
        return $"RoyaltyList:{string.Join('.', this.Select(c => c.GetHashCode()))}".GetHashCode();
    }
    public static bool operator ==(RoyaltyList left, RoyaltyList right)
    {
        if (left is null)
        {
            return right is null;
        }
        return left.Equals(right);
    }
    public static bool operator !=(RoyaltyList left, RoyaltyList right)
    {
        return !(left == right);
    }
    public IReadOnlyCollection<IRoyalty>? ToRoyaltyList()
    {
        if (Count > 0)
        {
            return this.Select(r => r.ToRoyalty()).ToArray();
        }
        return null;
    }
}
public sealed class RoyaltyDefinition : IEquatable<RoyaltyDefinition>
{
    public RoyaltyType RoyaltyType { get; set; }
    public Address? Account { get; set; }
    public Address? Token { get; set; }
    public long? FixedAmount { get; set; }
    public long? Numerator { get; set; }
    public long? Denominator { get; set; }
    public long? FallbackAmount { get; set; }
    public Address? FallbackToken { get; set; }
    public long? Minimum { get; set; }
    public long? Maximum { get; set; }
    public bool AssessAsSurcharge { get; set; }

    public bool Equals(RoyaltyDefinition? other)
    {
        if (other is null)
        {
            return false;
        }
        return
            RoyaltyType == other.RoyaltyType &&
            Account == other.Account &&
            Token == other.Token &&
            FixedAmount == other.FixedAmount &&
            Numerator == other.Numerator &&
            Denominator == other.Denominator &&
            FallbackAmount == other.FallbackAmount &&
            FallbackToken == other.FallbackToken &&
            Minimum == other.Minimum &&
            Maximum == other.Maximum &&
            AssessAsSurcharge == other.AssessAsSurcharge;
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
        if (obj is RoyaltyDefinition other)
        {
            return Equals(other);
        }
        return false;
    }
    public override int GetHashCode()
    {
        return $"RoyaltyDefinition:{RoyaltyType.GetHashCode()}:{Account?.GetHashCode()}:{Token?.GetHashCode()}:{FixedAmount?.GetHashCode()}:{Numerator?.GetHashCode()}:{Denominator?.GetHashCode()}:{FallbackAmount?.GetHashCode()}:{FallbackToken?.GetHashCode()}:{Minimum?.GetHashCode()}:{Maximum?.GetHashCode()}:{AssessAsSurcharge.GetHashCode()}".GetHashCode();
    }

    public IRoyalty ToRoyalty()
    {
        return RoyaltyType switch
        {
            RoyaltyType.Fixed => new FixedRoyalty(Account!, Token ?? Address.None, FixedAmount.GetValueOrDefault()),
            RoyaltyType.Token => new TokenRoyalty(Account!, Numerator.GetValueOrDefault(), Denominator.GetValueOrDefault(), Minimum.GetValueOrDefault(), Maximum.GetValueOrDefault(), AssessAsSurcharge),
            RoyaltyType.Asset => new AssetRoyalty(Account!, Numerator.GetValueOrDefault(), Denominator.GetValueOrDefault(), FallbackAmount.GetValueOrDefault(), FallbackToken ?? Address.None),
            _ => throw new ArgumentOutOfRangeException("Unsupported Royalty Type"),
        };
    }

    public static bool operator ==(RoyaltyDefinition left, RoyaltyDefinition right)
    {
        if (left is null)
        {
            return right is null;
        }
        return left.Equals(right);
    }
    public static bool operator !=(RoyaltyDefinition left, RoyaltyDefinition right)
    {
        return !(left == right);
    }
}