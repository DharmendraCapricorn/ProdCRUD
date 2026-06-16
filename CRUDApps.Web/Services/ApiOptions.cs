namespace CRUDApps.Web.Services;

/// <summary>Configuration for reaching the CRUDApps API, bound from the "Api" config section.</summary>
public class ApiOptions
{
    public const string SectionName = "Api";

    /// <summary>Base URL of the CRUDApps API, e.g. "http://localhost:5029".</summary>
    public string BaseUrl { get; set; } = "http://localhost:5029";
}
