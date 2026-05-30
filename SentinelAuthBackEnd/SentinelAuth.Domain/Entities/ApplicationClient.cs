using SentinelAuth.Domain.Shared;

namespace SentinelAuth.Domain.Entities;

public class ApplicationClient : BaseEntity
{
    public string Name { get; private set; }
    public string ClientId { get; private set; }
    public string Audience { get; private set; }
    public bool IsActive { get; private set; }

    private ApplicationClient()
    {
    }

    private ApplicationClient(string name, string clientId, string audience)
    {
        Name = name;
        ClientId = clientId;
        Audience = audience;
        IsActive = true;
    }

    public static Result<ApplicationClient> Create(
        string name,
        string clientId,
        string audience)
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
                audience.Trim()
            )
        );
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