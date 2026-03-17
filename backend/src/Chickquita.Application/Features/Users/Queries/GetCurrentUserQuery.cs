using Chickquita.Domain.Common;
using Chickquita.Application.DTOs;
using MediatR;

namespace Chickquita.Application.Features.Users.Queries;

public record GetCurrentUserQuery : IRequest<Result<UserDto>>;
