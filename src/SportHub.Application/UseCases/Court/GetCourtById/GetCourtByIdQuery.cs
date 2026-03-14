using Application.Common.Interfaces;
using Application.CQRS;
using FluentResults;
using MediatR;

namespace Application.UseCases.Court.GetCourtById;

public record GetCourtByIdQuery(Guid Id) : IQuery<CourtPublicResponse>;
