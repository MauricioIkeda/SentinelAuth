using SentinelAuth.Domain.Shared;

namespace SentinelAuth.Domain.Entities;

public class Role : BaseEntity
{
    public long ApplicationClientId { get; private set; }
    public string Name { get; private set; }
    public string NormalizedName { get; private set; }

    private Role()
    {
    }

    private Role(long applicationClientId, string name)
    {
        ApplicationClientId = applicationClientId;
        Name = name.Trim();
        NormalizedName = name.Trim().ToUpperInvariant();
    }

    public static Result<Role> Create(long applicationClientId, string name)
    {
        if (applicationClientId <= 0)
        {
            return Result<Role>.Failure(
                Error.Validation(
                    "Role.ApplicationClientId",
                    "ApplicationClientId is invalid"
                )
            );
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return Result<Role>.Failure(
                Error.Validation(
                    "Role.Name",
                    "Name is required"
                )
            );
        }

        if (name.Length > 100)
        {
            return Result<Role>.Failure(
                Error.Validation(
                    "Role.Name",
                    "Name must have at most 100 characters"
                )
            );
        }

        return Result<Role>.Success(new Role(applicationClientId, name));
    }
}