using System.Security.Claims;
using EticaretSitesi.Data;
using EticaretSitesi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EticaretSitesi.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly AppDbContext _db;
    public AdminController(AppDbContext db) => _db = db;

    // ---------- Ürünler ----------
    public async Task<IActionResult> Urunler()
        => View(await _db.Urunler.OrderBy(u => u.Id).ToListAsync());

    [HttpGet]
    public IActionResult UrunEkle()
    {
        ViewBag.Baslik = "Yeni Ürün";
        return View("UrunForm", new Urun());
    }

    [HttpPost]
    public async Task<IActionResult> UrunEkle(Urun u)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Baslik = "Yeni Ürün";
            return View("UrunForm", u);
        }
        u.Id = 0;
        _db.Urunler.Add(u);
        await _db.SaveChangesAsync();
        TempData["Mesaj"] = "Ürün eklendi.";
        return RedirectToAction("Urunler");
    }

    [HttpGet]
    public async Task<IActionResult> UrunDuzenle(int id)
    {
        var u = await _db.Urunler.FindAsync(id);
        if (u == null) return NotFound();
        ViewBag.Baslik = "Ürün Düzenle (fiyat / stok güncelle)";
        return View("UrunForm", u);
    }

    [HttpPost]
    public async Task<IActionResult> UrunDuzenle(int id, Urun form)
    {
        var u = await _db.Urunler.FindAsync(id);
        if (u == null) return NotFound();

        if (!ModelState.IsValid)
        {
            ViewBag.Baslik = "Ürün Düzenle (fiyat / stok güncelle)";
            return View("UrunForm", form);
        }

        u.Ad = form.Ad;
        u.Aciklama = form.Aciklama;
        u.Fiyat = form.Fiyat;
        u.Stok = form.Stok;
        u.SatistaMi = form.SatistaMi;
        u.ResimUrl = form.ResimUrl;
        await _db.SaveChangesAsync();

        TempData["Mesaj"] = "Ürün güncellendi.";
        return RedirectToAction("Urunler");
    }

    [HttpPost]
    public async Task<IActionResult> SatisDurumu(int id)
    {
        var u = await _db.Urunler.FindAsync(id);
        if (u != null)
        {
            u.SatistaMi = !u.SatistaMi;
            await _db.SaveChangesAsync();
            TempData["Mesaj"] = u.SatistaMi ? "Ürün satışa alındı." : "Ürün satıştan kaldırıldı.";
        }
        return RedirectToAction("Urunler");
    }

    [HttpPost]
    public async Task<IActionResult> UrunSil(int id)
    {
        var u = await _db.Urunler.FindAsync(id);
        if (u != null)
        {
            _db.Urunler.Remove(u);
            await _db.SaveChangesAsync();
            TempData["Mesaj"] = "Ürün silindi.";
        }
        return RedirectToAction("Urunler");
    }

    // ---------- Kullanıcılar ----------
    public async Task<IActionResult> Kullanicilar()
        => View(await _db.Kullanicilar.OrderBy(k => k.Id).ToListAsync());

    [HttpPost]
    public async Task<IActionResult> OnayDegistir(int id)
    {
        if (id == AktifId()) return RedirectToAction("Kullanicilar");
        var k = await _db.Kullanicilar.FindAsync(id);
        if (k != null)
        {
            k.Onayli = !k.Onayli;
            await _db.SaveChangesAsync();
        }
        return RedirectToAction("Kullanicilar");
    }

    [HttpPost]
    public async Task<IActionResult> RolDegistir(int id)
    {
        if (id == AktifId()) return RedirectToAction("Kullanicilar");
        var k = await _db.Kullanicilar.FindAsync(id);
        if (k != null)
        {
            k.Rol = k.Rol == "Admin" ? "Kullanici" : "Admin";
            await _db.SaveChangesAsync();
        }
        return RedirectToAction("Kullanicilar");
    }

    // ---------- Siparişler ----------
    public async Task<IActionResult> Siparisler()
    {
        var liste = await _db.Siparisler
            .Include(s => s.Kalemler)
            .Include(s => s.Kullanici)
            .OrderByDescending(s => s.Id)
            .ToListAsync();
        return View(liste);
    }

    private int AktifId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
