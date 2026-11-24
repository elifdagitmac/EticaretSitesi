using System.Security.Claims;
using EticaretSitesi.Data;
using EticaretSitesi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EticaretSitesi.Controllers;

[Authorize]
public class SepetController : Controller
{
    private readonly AppDbContext _db;
    public SepetController(AppDbContext db) => _db = db;

    private int AktifKullaniciId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public async Task<IActionResult> Index()
    {
        var kid = AktifKullaniciId;
        var liste = await _db.SepetOgeleri
            .Include(s => s.Urun)
            .Where(s => s.KullaniciId == kid)
            .ToListAsync();
        return View(liste);
    }

    [HttpPost]
    public async Task<IActionResult> Ekle(int urunId, int adet)
    {
        var kid = AktifKullaniciId;
        var urun = await _db.Urunler.FindAsync(urunId);

        if (urun == null || !urun.SatistaMi || adet < 1)
        {
            TempData["Hata"] = "Ürün sepete eklenemedi.";
            return RedirectToAction("Index", "Home");
        }

        var oge = await _db.SepetOgeleri
            .FirstOrDefaultAsync(s => s.KullaniciId == kid && s.UrunId == urunId);
        var yeniAdet = (oge?.Adet ?? 0) + adet;

        if (yeniAdet > urun.Stok)
        {
            TempData["Hata"] = $"Stokta yalnızca {urun.Stok} adet var.";
            return RedirectToAction("Index", "Home");
        }

        if (oge == null)
            _db.SepetOgeleri.Add(new SepetOgesi { KullaniciId = kid, UrunId = urunId, Adet = adet });
        else
            oge.Adet = yeniAdet;

        await _db.SaveChangesAsync();
        TempData["Mesaj"] = $"'{urun.Ad}' sepete eklendi.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Guncelle(int id, int adet)
    {
        var kid = AktifKullaniciId;
        var oge = await _db.SepetOgeleri.Include(s => s.Urun)
            .FirstOrDefaultAsync(s => s.Id == id && s.KullaniciId == kid);
        if (oge == null) return RedirectToAction("Index");

        if (adet < 1)
            _db.SepetOgeleri.Remove(oge);
        else if (adet > oge.Urun.Stok)
            TempData["Hata"] = $"Stokta yalnızca {oge.Urun.Stok} adet var.";
        else
            oge.Adet = adet;

        await _db.SaveChangesAsync();
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Sil(int id)
    {
        var kid = AktifKullaniciId;
        var oge = await _db.SepetOgeleri
            .FirstOrDefaultAsync(s => s.Id == id && s.KullaniciId == kid);
        if (oge != null)
        {
            _db.SepetOgeleri.Remove(oge);
            await _db.SaveChangesAsync();
        }
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> SatinAl()
    {
        var kid = AktifKullaniciId;
        var sepet = await _db.SepetOgeleri.Include(s => s.Urun)
            .Where(s => s.KullaniciId == kid).ToListAsync();

        if (!sepet.Any())
        {
            TempData["Hata"] = "Sepetiniz boş.";
            return RedirectToAction("Index");
        }

        foreach (var s in sepet)
        {
            if (!s.Urun.SatistaMi || s.Adet > s.Urun.Stok)
            {
                TempData["Hata"] = $"'{s.Urun.Ad}' için yeterli stok yok veya satıştan kaldırıldı.";
                return RedirectToAction("Index");
            }
        }

        var siparis = new Siparis
        {
            KullaniciId = kid,
            Tarih = DateTime.Now,
            Toplam = sepet.Sum(s => s.Urun.Fiyat * s.Adet)
        };

        foreach (var s in sepet)
        {
            siparis.Kalemler.Add(new SiparisKalemi
            {
                UrunId = s.UrunId,
                UrunAdi = s.Urun.Ad,
                BirimFiyat = s.Urun.Fiyat,
                Adet = s.Adet
            });
            s.Urun.Stok -= s.Adet;
        }

        _db.Siparisler.Add(siparis);
        _db.SepetOgeleri.RemoveRange(sepet);
        await _db.SaveChangesAsync();

        TempData["Mesaj"] = "Siparişiniz alındı. Teşekkürler!";
        return RedirectToAction("Index", "Siparis");
    }
}
