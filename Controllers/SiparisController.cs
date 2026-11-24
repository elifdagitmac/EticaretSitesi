using System.Security.Claims;
using EticaretSitesi.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EticaretSitesi.Controllers;

[Authorize]
public class SiparisController : Controller
{
    private readonly AppDbContext _db;
    public SiparisController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var kid = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var liste = await _db.Siparisler
            .Include(s => s.Kalemler)
            .Where(s => s.KullaniciId == kid)
            .OrderByDescending(s => s.Id)
            .ToListAsync();
        return View(liste);
    }
}
