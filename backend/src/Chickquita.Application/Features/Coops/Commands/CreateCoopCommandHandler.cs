using AutoMapper;
using Chickquita.Domain.Common;
using Chickquita.Domain.Entities;
using Chickquita.Application.DTOs;
using Chickquita.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Chickquita.Application.Features.Coops.Commands;

/// <summary>
/// Handler for CreateCoopCommand that creates a new coop for the current tenant.
/// </summary>
public sealed class CreateCoopCommandHandler : IRequestHandler<CreateCoopCommand, Result<CoopDto>>
{
    private readonly ICoopRepository _coopRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateCoopCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateCoopCommandHandler"/> class.
    /// </summary>
    /// <param name="coopRepository">The coop repository.</param>
    /// <param name="currentUserService">The current user service.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    /// <param name="logger">The logger instance.</param>
    public CreateCoopCommandHandler(
        ICoopRepository coopRepository,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<CreateCoopCommandHandler> logger,
        IUnitOfWork unitOfWork)
    {
        _coopRepository = coopRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _logger = logger;
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <summary>
    /// Handles the CreateCoopCommand by creating a new coop.
    /// </summary>
    /// <param name="request">The create coop command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the coop DTO.</returns>
    public async Task<Result<CoopDto>> Handle(CreateCoopCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing CreateCoopCommand - Name: {Name}, Location: {Location}",
            request.Name,
            request.Location);

        try
        {
            var tenantId = _currentUserService.TenantId;

            // Check if a coop with this name already exists for the current tenant
            var nameExists = await _coopRepository.ExistsByNameAsync(request.Name);
            if (nameExists)
            {
                _logger.LogWarning(
                    "CreateCoopCommand: Coop with name '{Name}' already exists for tenant {TenantId}",
                    request.Name,
                    tenantId.Value);
                return Result<CoopDto>.Failure(Error.Conflict("A coop with this name already exists"));
            }

            // Create the coop entity
            var coopResult = Coop.Create(tenantId.Value, request.Name, request.Location);
            if (coopResult.IsFailure)
                return Result<CoopDto>.Failure(coopResult.Error);
            var coop = coopResult.Value;

            // Save to database
            var addedCoop = await _coopRepository.AddAsync(coop);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Created new coop with ID: {CoopId} for tenant: {TenantId}",
                addedCoop.Id,
                tenantId.Value);

            var coopDto = _mapper.Map<CoopDto>(addedCoop);

            // New coops have no flocks
            coopDto.FlocksCount = 0;

            return Result<CoopDto>.Success(coopDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error occurred while creating coop with name: {Name}",
                request.Name);

            return Result<CoopDto>.Failure(
                Error.Failure("An unexpected error occurred. Please try again."));
        }
    }
}
