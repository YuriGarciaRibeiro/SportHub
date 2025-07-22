using FluentResults;

namespace Application.Common.Errors;

public class BadRequest : Error
{
    public BadRequest(string message) : base(message)
    {
        WithMetadata("StatusCode", 422);
    }
}
