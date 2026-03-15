using Application.Common.Models;
using Application.Security;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SportHub.Application.UseCases.Users.GetUsers;
using WebApi.Extensions.ResultExtensions;

namespace SportHub.Api.Endpoints;

public static class UsersEndpoints
{
    public static void MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users")
            .WithTags("Users")
            .RequireAuthorization(PolicyNames.IsOwner);

        group.MapGet("/", async (
            [FromQuery] int? page,
            [FromQuery] int? pageSize,
            [FromQuery] string? email,
            [FromQuery] string? firstName,
            [FromQuery] string? lastName,
            [FromQuery] UserRole? role,
            [FromQuery] bool? isActive,
            [FromQuery] string? searchTerm,
            ISender sender) =>
        {
            var filter = new GetUsersFilter
            {
                Page = page is > 0 ? page.Value : 1,
                PageSize = pageSize is > 0 ? pageSize.Value : 10,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                Role = role,
                IsActive = isActive,
                SearchTerm = searchTerm
            };

            var query = new GetUsersQuery(filter);
            var result = await sender.Send(query);
            return result.ToIResult();
        })
        .WithName("GetUsers")
        .WithSummary("Lista usuários com filtros e paginação")
        .WithDescription(@"
Retorna uma lista paginada de usuários com suporte a múltiplos filtros.

**Parâmetros de Paginação:**
- `page`: Número da página (padrão: 1)
- `pageSize`: Itens por página (padrão: 10, máximo: 100)

**Filtros Disponíveis:**
- `email`: Busca parcial por email
- `firstName`: Busca parcial por primeiro nome
- `lastName`: Busca parcial por sobrenome
- `role`: Filtra por role específico (Customer, Staff, Manager, Owner)
- `isActive`: Filtra por status ativo (true/false)
- `searchTerm`: Busca global em email, firstName e lastName

**Exemplo de uso:**
- `/api/users?page=1&pageSize=20`
- `/api/users?role=Staff&isActive=true`
- `/api/users?searchTerm=john&page=1`
")
        .Produces<PagedResult<GetUserDto>>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
        .Produces<ProblemDetails>(StatusCodes.Status403Forbidden);
    }
}
