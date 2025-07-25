namespace Application.Common.Errors;

public class NotFound : Error
{
    public NotFound(string message) : base(message)
    {
        WithMetadata("StatusCode", 404);
    }
}
