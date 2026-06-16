using CRUDApps.Web.Models;

namespace CRUDApps.Web.Services;

public interface IProductApiClient
{
    Task<PagedResult<ProductViewModel>> GetProductsAsync(ProductFilterModel filter, CancellationToken ct = default);
    Task<ProductViewModel?> GetProductAsync(int id, CancellationToken ct = default);
    Task<ProductViewModel> CreateAsync(ProductFormModel model, CancellationToken ct = default);
    Task<ProductViewModel> UpdateAsync(int id, ProductFormModel model, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    Task<ProductViewModel> UploadImageAsync(int id, IFormFile file, CancellationToken ct = default);
    Task<ProductViewModel> RemoveImageAsync(int id, CancellationToken ct = default);
}
