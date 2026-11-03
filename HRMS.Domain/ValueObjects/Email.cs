using HRMS.Domain.Common;

namespace HRMS.Domain.ValueObjects;

public class Email : ValueObject
{
    public string Value { get; }

    private Email(string value) => Value = value;

    public static Result<Email> Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result<Email>.Failure("Email cannot be empty.");

        if (!email.Contains('@') || !email.Contains('.'))
            return Result<Email>.Failure("Invalid email format.");

        return Result<Email>.Success(new Email(email.Trim().ToLowerInvariant()));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
