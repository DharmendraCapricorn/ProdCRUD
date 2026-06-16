namespace CRUDApps.Web.Services;

/// <summary>Raised when the API returns a non-success status. Carries field-level validation errors when present.</summary>
public class ApiException : Exception
{
    public int StatusCode { get; }

    /// <summary>Field name → error messages, when the API returned a validation problem.</summary>
    public IDictionary<string, string[]> Errors { get; }

    public ApiException(int statusCode, string message, IDictionary<string, string[]>? errors = null)
        : base(message)
    {
        StatusCode = statusCode;
        Errors = errors ?? new Dictionary<string, string[]>();
    }

    public bool IsNotFound => StatusCode == 404;
}
