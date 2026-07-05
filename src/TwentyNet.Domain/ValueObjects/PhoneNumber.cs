using TwentyNet.Domain.Common;

namespace TwentyNet.Domain.ValueObjects;

public sealed class PhoneNumber : ValueObject
{
    public string Value { get; }

    public PhoneNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Phone number cannot be empty.", nameof(value));

        Value = value.Trim();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
