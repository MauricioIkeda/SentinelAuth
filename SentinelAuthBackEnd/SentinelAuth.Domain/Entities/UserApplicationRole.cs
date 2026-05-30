using SentinelAuth.Domain.Shared;

namespace SentinelAuth.Domain.Entities;

public class UserApplicationRole : BaseEntity
{
    public long UserId { get; private set; }
    public long ApplicationClientId { get; private set; }
    public long RoleId { get; private set; }

    private UserApplicationRole()
    {
    }

    private UserApplicationRole(
        long userId,
        long applicationClientId,
        long roleId)
    {
        UserId = userId;
        ApplicationClientId = applicationClientId;
        RoleId = roleId;
    }

    public static Result<UserApplicationRole> Create(
        long userId,
        long applicationClientId,
        long roleId)
    {
        if (userId <= 0)
        {
            return Result<UserApplicationRole>.Failure(
                Error.Validation(
                    "UserApplicationRole.UserId",
                    "UserId is invalid"
                )
            );
        }

        if (applicationClientId <= 0)
        {
            return Result<UserApplicationRole>.Failure(
                Error.Validation(
                    "UserApplicationRole.ApplicationClientId",
                    "ApplicationClientId is invalid"
                )
            );
        }

        if (roleId <= 0)
        {
            return Result<UserApplicationRole>.Failure(
                Error.Validation(
                    "UserApplicationRole.RoleId",
                    "RoleId is invalid"
                )
            );
        }

        return Result<UserApplicationRole>.Success(
            new UserApplicationRole(
                userId,
                applicationClientId,
                roleId
            )
        );
    }
}