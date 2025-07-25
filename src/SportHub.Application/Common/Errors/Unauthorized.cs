namespace Application.Common.Errors;

public class Unauthorized : Error
{
    public Unauthorized(string message) : base(message)
    {
        WithMetadata("StatusCode", 401);
    }
}
