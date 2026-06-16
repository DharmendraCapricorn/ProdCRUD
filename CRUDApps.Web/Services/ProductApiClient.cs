using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CRUDApps.Web.Models;

namespace CRUDApps.Web.Services;

/// <summary>Typed HttpClient wrapper over the CRUDApps REST API.</summary>
public class ProductApiClient : IProductApiClient
{
    private const string BasePath = "api/products";
    private readonly HttpClient _http;

    public ProductApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<PagedResult<ProductViewModel>> GetProductsAsync(ProductFilterModel filter, CancellationToken ct = default)
    {
        var response = await _http.GetAsync($"{BasePath}{filter.ToQueryString()}", ct);
        await EnsureSuccessAsync(response, ct);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<ProductViewModel>>(cancellationToken: ct);
        return result ?? new PagedResult<ProductViewModel>();
    }

    public async Task<ProductViewModel?> GetProductAsync(int id, CancellationToken ct = default)
    {
        var response = await _http.GetAsync($"{BasePath}/{id}", ct);
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        await EnsureSuccessAsync(response, ct);
        return await response.Content.ReadFromJsonAsync<ProductViewModel>(cancellationToken: ct);
    }

    public async Task<ProductViewModel> CreateAsync(ProductFormModel model, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync(BasePath, ToPayload(model), ct);
        await EnsureSuccessAsync(response, ct);
        return (await response.Content.ReadFromJsonAsync<ProductViewModel>(cancellationToken: ct))!;
    }

    public async Task<ProductViewModel> UpdateAsync(int id, ProductFormModel model, CancellationToken ct = default)
    {
        var response = await _http.PutAsJsonAsync($"{BasePath}/{id}", ToPayload(model), ct);
        await EnsureSuccessAsync(response, ct);
        return (await response.Content.ReadFromJsonAsync<ProductViewModel>(cancellationToken: ct))!;
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var response = await _http.DeleteAsync($"{BasePath}/{id}", ct);
        await EnsureSuccessAsync(response, ct);
    }

    public async Task<ProductViewModel> UploadImageAsync(int id, IFormFile file, CancellationToken ct = default)
    {
        using var content = new MultipartFormDataContent();
        using var stream = file.OpenReadStream();
        var fileContent = new StreamContent(stream);
        if (!string.IsNullOrEmpty(file.ContentType))
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
        content.Add(fileContent, "file", file.FileName);

        var response = await _http.PostAsync($"{BasePath}/{id}/image", content, ct);
        await EnsureSuccessAsync(response, ct);
        return (await response.Content.ReadFromJsonAsync<ProductViewModel>(cancellationToken: ct))!;
    }

    public async Task<ProductViewModel> RemoveImageAsync(int id, CancellationToken ct = default)
    {
        var response = await _http.DeleteAsync($"{BasePath}/{id}/image", ct);
        await EnsureSuccessAsync(response, ct);
        return (await response.Content.ReadFromJsonAsync<ProductViewModel>(cancellationToken: ct))!;
    }

    private static object ToPayload(ProductFormModel m) =>
        new { name = m.Name, description = m.Description, price = m.Price, stock = m.Stock };

    /// <summary>Throws an <see cref="ApiException"/> carrying any field errors / detail from the API body.</summary>
    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode) return;

        var status = (int)response.StatusCode;
        var title = response.ReasonPhrase ?? "Request failed";
        var errors = new Dictionary<string, string[]>();

        try
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            if (!string.IsNullOrWhiteSpace(body))
            {
                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;

                // Validation problem details: { "errors": { "Name": ["..."] } }
                if (root.TryGetProperty("errors", out var errs) && errs.ValueKind == JsonValueKind.Object)
                {
                    foreach (var prop in errs.EnumerateObject())
                    {
                        var messages = prop.Value.ValueKind == JsonValueKind.Array
                            ? prop.Value.EnumerateArray().Select(e => e.GetString() ?? "").ToArray()
                            : [prop.Value.GetString() ?? ""];
                        errors[prop.Name] = messages;
                    }
                }

                // Our middleware shape: { "detail": "..." } / { "title": "..." }
                if (root.TryGetProperty("detail", out var detail) && detail.ValueKind == JsonValueKind.String)
                    title = detail.GetString() ?? title;
                else if (root.TryGetProperty("title", out var t) && t.ValueKind == JsonValueKind.String)
                    title = t.GetString() ?? title;
            }
        }
        catch (JsonException)
        {
            // Non-JSON error body; fall back to the reason phrase.
        }

        throw new ApiException(status, title, errors.Count > 0 ? errors : null);
    }
}
