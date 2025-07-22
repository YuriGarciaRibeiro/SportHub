using FluentResults;

namespace Application.Common.Errors;

public class Forbidden : Error
{
    public Forbidden(string message) : base(message)
    {
        WithMetadata("StatusCode", 403);
    }
}
