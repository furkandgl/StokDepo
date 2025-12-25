using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StokDepo.Data;
using StokDepo.Models;
using Microsoft.AspNetCore.Authorization;
using System.Text;
using System.Globalization;
using System.Security.Claims;



namespace StokDepo.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Products
  [Authorize]
public async Task<IActionResult> Index(string? q, string? category, string? stock, int page = 1, int pageSize = 10)
{
    if (page < 1) page = 1;
    if (pageSize < 5) pageSize = 5;
    if (pageSize > 50) pageSize = 50;

    var productsQuery = _context.Products.AsNoTracking();

    // Arama
    if (!string.IsNullOrWhiteSpace(q))
    {
        q = q.Trim();
        productsQuery = productsQuery.Where(p => p.Name.Contains(q));
    }

    // Kategori
    if (!string.IsNullOrWhiteSpace(category))
    {
        category = category.Trim();
        productsQuery = productsQuery.Where(p => p.Category == category);
    }

    // Stok
    if (!string.IsNullOrWhiteSpace(stock))
    {
        stock = stock.Trim().ToLowerInvariant();

        if (stock == "out")
            productsQuery = productsQuery.Where(p => p.Quantity == 0);
        else if (stock == "critical")
            productsQuery = productsQuery.Where(p => p.Quantity > 0 && p.Quantity <= 5);
        else if (stock == "normal")
            productsQuery = productsQuery.Where(p => p.Quantity > 5);
    }

    // Dropdown kategoriler
    var categories = await _context.Products.AsNoTracking()
        .Select(p => p.Category)
        .Where(c => c != null && c != "")
        .Distinct()
        .OrderBy(c => c)
        .ToListAsync();

    // Toplam kayıt sayısı
    var totalCount = await productsQuery.CountAsync();
    var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
    if (totalPages == 0) totalPages = 1;
    if (page > totalPages) page = totalPages;

    var products = await productsQuery
        .OrderByDescending(p => p.CreatedDate)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    ViewBag.Query = q;
    ViewBag.SelectedCategory = category;
    ViewBag.SelectedStock = stock;
    ViewBag.Categories = categories;

    ViewBag.Page = page;
    ViewBag.PageSize = pageSize;
    ViewBag.TotalPages = totalPages;
    ViewBag.TotalCount = totalCount;

    return View(products);
}


        // GET: /Products/ExportCsv
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> ExportCsv(string? q, string? category, string? stock, DateTime? dateFrom, DateTime? dateTo)

        {
            var productsQuery = _context.Products.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                productsQuery = productsQuery.Where(p => p.Name.Contains(q));
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                category = category.Trim();
                productsQuery = productsQuery.Where(p => p.Category == category);
            }

            if (!string.IsNullOrWhiteSpace(stock))
            {
                stock = stock.Trim().ToLowerInvariant();

                if (stock == "out")
                    productsQuery = productsQuery.Where(p => p.Quantity == 0);
                else if (stock == "critical")
                    productsQuery = productsQuery.Where(p => p.Quantity > 0 && p.Quantity <= 5);
                else if (stock == "normal")
                    productsQuery = productsQuery.Where(p => p.Quantity > 5);
            }

            if (dateFrom.HasValue)
{
    var from = dateFrom.Value.Date;
    productsQuery = productsQuery.Where(p => p.CreatedDate >= from);
}

if (dateTo.HasValue)
{
    var to = dateTo.Value.Date.AddDays(1);
    productsQuery = productsQuery.Where(p => p.CreatedDate < to);
}


            var products = await productsQuery
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();

            string Escape(string s)
            {
                if (s.Contains('"') || s.Contains(',') || s.Contains('\n') || s.Contains('\r'))
                    return "\"" + s.Replace("\"", "\"\"") + "\"";
                return s;
            }

            var sb = new StringBuilder();
            sb.AppendLine("Id,Name,Category,Price,Quantity,CreatedDate");

          foreach (var p in products)
{
    sb.AppendLine(string.Join(",",
        p.Id.ToString(),
        Escape(p.Name ?? ""),
        Escape(p.Category ?? ""),
        Escape(p.Price.ToString()),   
        p.Quantity.ToString(),
        Escape(p.CreatedDate.ToString("yyyy-MM-dd HH:mm"))
    ));
}


var encoding = new UTF8Encoding(true);
return File(encoding.GetBytes(sb.ToString()), "text/csv; charset=utf-8", "products.csv");

        }

        // GET: /Products/Details/5
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products.AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null) return NotFound();

            return View(product);
        }

        // GET: /Products/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Name,Category,Price,Quantity,Description")] Product product)
        {
            if (!ModelState.IsValid) return View(product);  
    product.CreatedDate = DateTime.Now;
    _context.Add(product);
    await _context.SaveChangesAsync(); 

   // İlk stok hareketi (ürün oluşturulurken)
if (product.Quantity != 0)
{
    var performedBy = User.Identity?.Name ?? "system";

    _context.StockMovements.Add(new StockMovement
    {
        ProductId = product.Id,
        QuantityChange = product.Quantity,
        CreatedAt = DateTime.UtcNow,
        PerformedBy = performedBy,
        Note = $"Ürün oluşturuldu (ilk stok): +{product.Quantity}"
    });

    await _context.SaveChangesAsync();
}

    TempData["Success"] = "Ürün başarıyla eklendi.";

    return RedirectToAction(nameof(Index));
}

        // GET: /Products/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            return View(product);
        }

        // POST: /Products/Edit/5
      [HttpPost]
[ValidateAntiForgeryToken]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> Edit(int id, Product product)
{
    if (id != product.Id) return NotFound();
    if (!ModelState.IsValid) return View(product);

    var dbProduct = await _context.Products.FirstOrDefaultAsync(p => p.Id == product.Id);
    if (dbProduct == null) return NotFound();

    var oldQty = dbProduct.Quantity;

    dbProduct.Name = product.Name;
    dbProduct.Category = product.Category;
    dbProduct.Price = product.Price;
    dbProduct.Quantity = product.Quantity;
    dbProduct.Description = product.Description;

    await _context.SaveChangesAsync();

    var diff = dbProduct.Quantity - oldQty;

    if (diff != 0)
    {
        var performedBy = User.Identity?.Name ?? "system";

        _context.StockMovements.Add(new StockMovement
        {
            ProductId = dbProduct.Id,
            QuantityChange = diff,
            CreatedAt = DateTime.UtcNow,
            PerformedBy = performedBy,
            Note = null
        });

        await _context.SaveChangesAsync();
    }
    TempData["Success"] = "Ürün başarıyla güncellendi.";

    return RedirectToAction(nameof(Index));

}


        // GET: /Products/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products.AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null) return NotFound();

            return View(product);
        }

        // POST: /Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
            TempData["Success"] = "Ürün silindi.";
            return RedirectToAction(nameof(Index));
        }
    }
}
