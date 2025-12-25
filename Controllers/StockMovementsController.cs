using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StokDepo.Data;
using StokDepo.Models;

namespace StokDepo.Controllers
{
    [Authorize] // giriş olmadan yok
    public class StockMovementsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StockMovementsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /StockMovements
        public async Task<IActionResult> Index(DateTime? from, DateTime? to, int? productId)
        {
            var q = _context.StockMovements
                .Include(x => x.Product)
                .AsNoTracking()
                .AsQueryable();

            if (productId.HasValue)
                q = q.Where(x => x.ProductId == productId.Value);

            if (from.HasValue)
                q = q.Where(x => x.CreatedAt >= from.Value);

            if (to.HasValue)
                q = q.Where(x => x.CreatedAt <= to.Value.AddDays(1).AddSeconds(-1)); // gün sonu

            var list = await q
                .OrderByDescending(x => x.Id)
                .Take(200)
                .ToListAsync();

            ViewBag.Products = await _context.Products
                .AsNoTracking()
                .OrderBy(p => p.Name)
                .Select(p => new SelectListItem($"{p.Name} ({p.Category})", p.Id.ToString()))
                .ToListAsync();

            ViewBag.From = from?.ToString("yyyy-MM-dd") ?? "";
            ViewBag.To = to?.ToString("yyyy-MM-dd") ?? "";
            ViewBag.ProductId = productId?.ToString() ?? "";

            return View(list);
        }

        // GET: /StockMovements/Create
        [Authorize(Roles = "Admin")] // sadece admin stok oynasın istiyorsan
        public async Task<IActionResult> Create(int? productId)
        {
            ViewBag.Products = await _context.Products
                .AsNoTracking()
                .OrderBy(p => p.Name)
                .Select(p => new SelectListItem($"{p.Name} ({p.Category})", p.Id.ToString(), p.Id == productId))
                .ToListAsync();

            return View();
        }

        // POST: /StockMovements/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(int productId, string type, int quantity, string? note)
        {
            if (quantity <= 0)
            {
                ModelState.AddModelError("", "Miktar 0'dan büyük olmalı.");
            }

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId);
            if (product == null)
            {
                ModelState.AddModelError("", "Ürün bulunamadı.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Products = await _context.Products
                    .AsNoTracking()
                    .OrderBy(p => p.Name)
                    .Select(p => new SelectListItem($"{p.Name} ({p.Category})", p.Id.ToString(), p.Id == productId))
                    .ToListAsync();

                return View();
            }

            // type: "in" veya "out"
            int diff = type == "out" ? -quantity : quantity;

            // stok eksiye düşmesin
            if (product!.Quantity + diff < 0)
            {
                ModelState.AddModelError("", "Çıkış miktarı mevcut stoktan fazla olamaz.");

                ViewBag.Products = await _context.Products
                    .AsNoTracking()
                    .OrderBy(p => p.Name)
                    .Select(p => new SelectListItem($"{p.Name} ({p.Category})", p.Id.ToString(), p.Id == productId))
                    .ToListAsync();

                return View();
            }

            using var tx = await _context.Database.BeginTransactionAsync();

            // 1) ürün stoğunu güncelle
            product.Quantity += diff;

            // 2) hareket kaydı ekle
            _context.StockMovements.Add(new StockMovement
            {
                ProductId = product.Id,
                QuantityChange = diff,
                Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim(),
                PerformedBy = User.Identity?.Name
            });

            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            TempData["Ok"] = "Stok hareketi kaydedildi.";
            return RedirectToAction(nameof(Index));
        }
    }
}
