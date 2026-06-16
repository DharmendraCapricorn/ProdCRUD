using System.ComponentModel.DataAnnotations;

namespace CRUDApps.Web.Models;

/// <summary>Mirrors the API's ProductResponseDto.</summary>
public class ProductViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string? ImagePath { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>Form model used for both Create and Edit. Validated client- and server-side.</summary>
public class ProductFormModel
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Price cannot be negative.")]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative.")]
    public int Stock { get; set; }

    // Carried through on the Edit screen so the image panel can render.
    public string? ImagePath { get; set; }
}

/// <summary>Search / filter / sort options bound from the query string.</summary>
public class ProductFilterModel
{
    public string? Search { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool? InStock { get; set; }
    public string? SortBy { get; set; }
    public bool Desc { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 2;

    /// <summary>True when any search/filter (not paging) is active.</summary>
    public bool HasFilters =>
        !string.IsNullOrWhiteSpace(Search) || MinPrice.HasValue || MaxPrice.HasValue ||
        InStock.HasValue || !string.IsNullOrWhiteSpace(SortBy) || Desc;

    /// <summary>Builds the API query string from the populated filter and paging values.</summary>
    public string ToQueryString()
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(Search)) parts.Add($"search={Uri.EscapeDataString(Search.Trim())}");
        if (MinPrice.HasValue) parts.Add($"minPrice={MinPrice.Value}");
        if (MaxPrice.HasValue) parts.Add($"maxPrice={MaxPrice.Value}");
        if (InStock.HasValue) parts.Add($"inStock={InStock.Value.ToString().ToLowerInvariant()}");
        if (!string.IsNullOrWhiteSpace(SortBy)) parts.Add($"sortBy={Uri.EscapeDataString(SortBy)}");
        if (Desc) parts.Add("desc=true");
        parts.Add($"page={(Page < 1 ? 1 : Page)}");
        parts.Add($"pageSize={(PageSize < 1 ? 10 : PageSize)}");
        return "?" + string.Join("&", parts);
    }

    /// <summary>Filter values as route-value pairs, for preserving them in pagination links.</summary>
    public IDictionary<string, string?> ToRouteValues() => new Dictionary<string, string?>
    {
        ["Search"] = Search,
        ["MinPrice"] = MinPrice?.ToString(),
        ["MaxPrice"] = MaxPrice?.ToString(),
        ["InStock"] = InStock?.ToString(),
        ["SortBy"] = SortBy,
        ["Desc"] = Desc ? "true" : null,
        ["PageSize"] = PageSize.ToString()
    };
}

/// <summary>Backing model for the product list page.</summary>
public class ProductIndexViewModel
{
    public PagedResult<ProductViewModel> Page { get; set; } = new();
    public ProductFilterModel Filter { get; set; } = new();
}
