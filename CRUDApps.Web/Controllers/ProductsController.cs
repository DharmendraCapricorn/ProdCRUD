using CRUDApps.Web.Models;
using CRUDApps.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace CRUDApps.Web.Controllers;

public class ProductsController : Controller
{
    private readonly IProductApiClient _api;

    public ProductsController(IProductApiClient api)
    {
        _api = api;
    }

    // GET: /Products
    public async Task<IActionResult> Index(ProductFilterModel filter, CancellationToken ct)
    {
        try
        {
            var page = await _api.GetProductsAsync(filter, ct);
            return View(new ProductIndexViewModel { Page = page, Filter = filter });
        }
        catch (Exception ex) when (ex is HttpRequestException or ApiException)
        {
            TempData["Error"] = $"Could not reach the API. Is CRUDApps running? ({ex.Message})";
            return View(new ProductIndexViewModel { Filter = filter });
        }
    }

    // GET: /Products/Details/5
    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        var product = await _api.GetProductAsync(id, ct);
        if (product is null) return NotFound();
        return View(product);
    }

    // GET: /Products/Create
    public IActionResult Create() => View(new ProductFormModel());

    // POST: /Products/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductFormModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(model);

        try
        {
            var created = await _api.CreateAsync(model, ct);
            TempData["Success"] = "Product created — you can now upload an image.";
            return RedirectToAction(nameof(Edit), new { id = created.Id });
        }
        catch (ApiException ex)
        {
            ApplyApiErrors(ex);
            return View(model);
        }
    }

    // GET: /Products/Edit/5
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var product = await _api.GetProductAsync(id, ct);
        if (product is null) return NotFound();
        return View(ToForm(product));
    }

    // POST: /Products/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductFormModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(model);

        try
        {
            await _api.UpdateAsync(id, model, ct);
            TempData["Success"] = "Product updated.";
            return RedirectToAction(nameof(Index));
        }
        catch (ApiException ex) when (ex.IsNotFound)
        {
            return NotFound();
        }
        catch (ApiException ex)
        {
            ApplyApiErrors(ex);
            return View(model);
        }
    }

    // GET: /Products/Delete/5
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var product = await _api.GetProductAsync(id, ct);
        if (product is null) return NotFound();
        return View(product);
    }

    // POST: /Products/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken ct)
    {
        await _api.DeleteAsync(id, ct);
        TempData["Success"] = "Product deleted.";
        return RedirectToAction(nameof(Index));
    }

    // POST: /Products/UploadImage/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadImage(int id, IFormFile? file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
        {
            TempData["Error"] = "Please choose a file to upload.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        try
        {
            await _api.UploadImageAsync(id, file, ct);
            TempData["Success"] = "Image uploaded.";
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Edit), new { id });
    }

    // POST: /Products/RemoveImage/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveImage(int id, CancellationToken ct)
    {
        try
        {
            await _api.RemoveImageAsync(id, ct);
            TempData["Success"] = "Image removed.";
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Edit), new { id });
    }

    private static ProductFormModel ToForm(ProductViewModel p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        Description = p.Description,
        Price = p.Price,
        Stock = p.Stock,
        ImagePath = p.ImagePath
    };

    /// <summary>Maps field-level API errors into ModelState; falls back to a model-level error.</summary>
    private void ApplyApiErrors(ApiException ex)
    {
        if (ex.Errors.Count > 0)
        {
            foreach (var (field, messages) in ex.Errors)
                foreach (var message in messages)
                    ModelState.AddModelError(field, message);
        }
        else
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }
    }
}
