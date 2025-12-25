using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StokDepo.Data;
using StokDepo.Models;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;



namespace StokDepo.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    [AllowAnonymous]
    public IActionResult AccessDenied()
{
    return View();
}

    public async Task<IActionResult> Index()
    {

        var totalProducts = await _context.Products.CountAsync();
        var criticalStock = await _context.Products.CountAsync(p => p.Quantity > 0 && p.Quantity <= 5);
        var outOfStock = await _context.Products.CountAsync(p => p.Quantity == 0);
        // Son 14 gün için günlük net stok değişimi (giriş-çıkış toplamı)
        var days = 14;
        var startDateUtc = DateTime.UtcNow.Date.AddDays(-(days - 1));
        var endDateUtc = DateTime.UtcNow.Date.AddDays(1); 
        var dailyNet = await _context.StockMovements



    .AsNoTracking()
    .Where(m => m.CreatedAt >= startDateUtc && m.CreatedAt < endDateUtc)
    .GroupBy(m => m.CreatedAt.Date)
    .Select(g => new
    {
        Day = g.Key,
        Net = g.Sum(x => x.QuantityChange)
    })
    .OrderBy(x => x.Day)
    .ToListAsync();

// Eksik günleri 0 ile doldur (grafikte boşluk olmasın)
var labels = new List<string>();
var values = new List<int>();

for (int i = 0; i < days; i++)
{
    var day = startDateUtc.AddDays(i);
    labels.Add(day.ToString("dd.MM"));

    var item = dailyNet.FirstOrDefault(x => x.Day == day);
    values.Add(item?.Net ?? 0);
}

// View'a JSON olarak gönder (JS tarafı direkt kullanacak)
ViewBag.ChartLabelsJson = JsonSerializer.Serialize(labels);
ViewBag.ChartValuesJson = JsonSerializer.Serialize(values);


        ViewBag.TotalProducts = totalProducts;
        ViewBag.CriticalStock = criticalStock;
        ViewBag.OutOfStock = outOfStock;

        var normalStock = totalProducts - criticalStock - outOfStock;
if (normalStock < 0) normalStock = 0;

ViewBag.StatusLabelsJson = JsonSerializer.Serialize(new[] { "Normal", "Kritik", "Stok Yok" });
ViewBag.StatusValuesJson = JsonSerializer.Serialize(new[] { normalStock, criticalStock, outOfStock });

        var recentMovements = await _context.StockMovements
    .Include(s => s.Product)
    .OrderByDescending(s => s.Id)
    .Take(5)
    .AsNoTracking()
    .ToListAsync();

ViewBag.RecentMovements = recentMovements;


        return View();
    }

    [AllowAnonymous]
    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]

    [AllowAnonymous]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

