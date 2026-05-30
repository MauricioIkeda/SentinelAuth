using MediatR;
using SentinelAuth.Application.Interfaces;
using SentinelAuth.Domain.Repositories;
using SentinelAuth.Domain.Shared;
using ApplicationClientDomain = SentinelAuth.Domain.Entities.ApplicationClient;

namespace SentinelAuth.Application.UseCases.Register.ApplicationClient;

public sealed class RegisterApplicationClientHandler
    : IRequestHandler<RegisterApplicationClientCommand, Result<RegisterApplicationClientResult>>
{
    private readonly IApplicationClient _applicationClientRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterApplicationClientHandler(
        IApplicationClient applicationClientRepository,
        IUnitOfWork unitOfWork)
    {
        _applicationClientRepository = applicationClientRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<RegisterApplicationClientResult>> Handle(
        RegisterApplicationClientCommand command,
        CancellationToken cancellationToken)
    {
        var applicationClient = ApplicationClientDomain.Create(
            command.Name,
            command.ClientId,
            command.Audience
        );

        if (!applicationClient.IsSuccess)
        {
            return Result<RegisterApplicationClientResult>.Failure(applicationClient.Error);
        }

        var clientIdAlreadyExists = await _applicationClientRepository.ExistsByClientIdAsync(
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

        var audienceAlreadyExists = await _applicationClientRepository.ExistsByAudienceAsync(
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

        await _applicationClientRepository.AddAsync(applicationClient.Value, cancellationToken);
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
