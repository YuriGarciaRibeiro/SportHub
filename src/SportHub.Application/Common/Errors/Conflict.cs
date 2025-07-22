using FluentResults;

namespace Application.Common.Errors;

public class Conflict : Error
{
    public Conflict(string message) : base(message)
    {
        WithMetadata("StatusCode", 409);
    }
}
