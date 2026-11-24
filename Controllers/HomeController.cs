using EticaretSitesi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EticaretSitesi.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _db;
    public HomeController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index(string? ara)
    {
        var q = _db.Urunler.Where(u => u.SatistaMi);
        if (!string.IsNullOrWhiteSpace(ara))
            q = q.Where(u => u.Ad.Contains(ara));

        ViewBag.Ara = ara;
        return View(await q.OrderBy(u => u.Ad).ToListAsync());
    }
}
