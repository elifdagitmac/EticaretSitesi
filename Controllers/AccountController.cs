using System.Security.Claims;
using EticaretSitesi.Data;
using EticaretSitesi.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EticaretSitesi.Controllers;

public class AccountController : Controller
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher<Kullanici> _hasher;

    public AccountController(AppDbContext db, IPasswordHasher<Kullanici> hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    [HttpGet]
    public IActionResult Giris(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Giris(string email, string sifre, string? returnUrl = null)
    {
        email = (email ?? "").Trim().ToLower();
        var k = await _db.Kullanicilar.FirstOrDefaultAsync(x => x.Email == email);

        if (k == null ||
            _hasher.VerifyHashedPassword(k, k.SifreHash, sifre ?? "") == PasswordVerificationResult.Failed)
        {
            ViewBag.Hata = "E-posta veya şifre hatalı.";
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        if (!k.Onayli)
        {
            ViewBag.Hata = "Hesabınız henüz yönetici tarafından onaylanmadı.";
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, k.Id.ToString()),
            new(ClaimTypes.Name, k.AdSoyad),
            new(ClaimTypes.Role, k.Rol)
        };
        var kimlik = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(kimlik));

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Kayit() => View();

    [HttpPost]
    public async Task<IActionResult> Kayit(string adSoyad, string email, string sifre)
    {
        adSoyad = (adSoyad ?? "").Trim();
        email = (email ?? "").Trim().ToLower();

        if (adSoyad == "" || email == "" || (sifre ?? "").Length < 6)
        {
            ViewBag.Hata = "Tüm alanlar zorunlu, şifre en az 6 karakter olmalı.";
            return View();
        }
        if (await _db.Kullanicilar.AnyAsync(x => x.Email == email))
        {
            ViewBag.Hata = "Bu e-posta ile zaten kayıt var.";
            return View();
        }

        var k = new Kullanici { AdSoyad = adSoyad, Email = email, Rol = "Kullanici", Onayli = false };
        k.SifreHash = _hasher.HashPassword(k, sifre!);
        _db.Kullanicilar.Add(k);
        await _db.SaveChangesAsync();

        TempData["Mesaj"] = "Kayıt alındı. Yönetici onayından sonra giriş yapabilirsiniz.";
        return RedirectToAction("Giris");
    }

    [HttpPost]
    public async Task<IActionResult> Cikis()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    public IActionResult ErisimEngellendi() => View();
}
