using SentinelAuth.Domain.Shared;

namespace SentinelAuth.Domain.Entities;

public class ApplicationClient : BaseEntity
{
    public string Name { get; private set; } = null!;
    public string ClientId { get; private set; } = null!;
    public string Audience { get; private set; } = null!;
    public string? ClientSecretHash { get; private set; }
    public bool AllowRoleAssignment { get; private set; }
    public bool IsActive { get; private set; }

    private ApplicationClient()
    {
    }

    private ApplicationClient(
        string name,
        string clientId,
        string audience,
        string? clientSecretHash,
        bool allowRoleAssignment)
    {
        Name = name;
        ClientId = clientId;
        Audience = audience;
        ClientSecretHash = clientSecretHash;
        AllowRoleAssignment = allowRoleAssignment;
        IsActive = true;
    }

    public static Result<ApplicationClient> Create(
        string name,
        string clientId,
        string audience,
        string? clientSecretHash = null,
        bool allowRoleAssignment = false)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result<ApplicationClient>.Failure(
                Error.Validation("ApplicationClient.Name", "Name is required")
            );
        }

        if (string.IsNullOrWhiteSpace(clientId))
        {
            return Result<ApplicationClient>.Failure(
                Error.Validation("ApplicationClient.ClientId", "ClientId is required")
            );
        }

        if (string.IsNullOrWhiteSpace(audience))
        {
            return Result<ApplicationClient>.Failure(
                Error.Validation("ApplicationClient.Audience", "Audience is required")
            );
        }

        return Result<ApplicationClient>.Success(
            new ApplicationClient(
                name.Trim(),
                clientId.Trim(),
                audience.Trim(),
                string.IsNullOrWhiteSpace(clientSecretHash) ? null : clientSecretHash.Trim(),
                allowRoleAssignment
            )
        );
    }

    public void ConfigureClientSecret(string clientSecretHash)
    {
        ClientSecretHash = string.IsNullOrWhiteSpace(clientSecretHash)
            ? null
            : clientSecretHash.Trim();
    }

    public void SetRoleAssignmentPermission(bool allowRoleAssignment)
    {
        AllowRoleAssignment = allowRoleAssignment;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }
}
