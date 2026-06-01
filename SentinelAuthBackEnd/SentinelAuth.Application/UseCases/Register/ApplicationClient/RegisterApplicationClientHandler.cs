using MediatR;
using SentinelAuth.Application.Interfaces;
using SentinelAuth.Domain.Repositories;
using SentinelAuth.Domain.Shared;
using ApplicationClientDomain = SentinelAuth.Domain.Entities.ApplicationClient;

namespace SentinelAuth.Application.UseCases.Register.ApplicationClient;

public sealed class RegisterApplicationClientHandler
    : IRequestHandler<RegisterApplicationClientCommand, Result<RegisterApplicationClientResult>>
{
    private readonly IApplicationClientRepository _applicationClientRepositoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;

    public RegisterApplicationClientHandler(
        IApplicationClientRepository applicationClientRepositoryRepository,
        IUnitOfWork unitOfWork,
        ITokenService tokenService)
    {
        _applicationClientRepositoryRepository = applicationClientRepositoryRepository;
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
    }

    public async Task<Result<RegisterApplicationClientResult>> Handle(
        RegisterApplicationClientCommand command,
        CancellationToken cancellationToken)
    {
        var clientSecretHash = string.IsNullOrWhiteSpace(command.ClientSecret)
            ? null
            : _tokenService.HashClientSecret(command.ClientSecret);

        var applicationClient = ApplicationClientDomain.Create(
            command.Name,
            command.ClientId,
            command.Audience,
            clientSecretHash,
            command.AllowRoleAssignment && !string.IsNullOrWhiteSpace(clientSecretHash)
        );

        if (!applicationClient.IsSuccess)
        {
            return Result<RegisterApplicationClientResult>.Failure(applicationClient.Error);
        }

        var clientIdAlreadyExists = await _applicationClientRepositoryRepository.ExistsByClientIdAsync(
            applicationClient.Value.ClientId,
            cancellationToken
        );

        if (clientIdAlreadyExists)
        {
            return Result<RegisterApplicationClientResult>.Failure(
                Error.Conflict(
                    "ApplicationClient.ClientIdAlreadyExists",
                    "The client id is already registered."
                )
            );
        }

        var audienceAlreadyExists = await _applicationClientRepositoryRepository.ExistsByAudienceAsync(
            applicationClient.Value.Audience,
            cancellationToken
        );

        if (audienceAlreadyExists)
        {
            return Result<RegisterApplicationClientResult>.Failure(
                Error.Conflict(
                    "ApplicationClient.AudienceAlreadyExists",
                    "The audience is already registered."
                )
            );
        }

        await _applicationClientRepositoryRepository.AddAsync(applicationClient.Value, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result<RegisterApplicationClientResult>.Success(
            new RegisterApplicationClientResult(
                applicationClient.Value.Id,
                applicationClient.Value.Name,
                applicationClient.Value.ClientId,
                applicationClient.Value.Audience,
                applicationClient.Value.IsActive
            )
        );
    }
}
