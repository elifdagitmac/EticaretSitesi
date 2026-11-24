using System.Globalization;
using EticaretSitesi.Data;
using EticaretSitesi.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(o =>
    o.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));

builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlite(builder.Configuration.GetConnectionString("Varsayilan")));

builder.Services.AddScoped<IPasswordHasher<Kullanici>, PasswordHasher<Kullanici>>();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.LoginPath = "/Account/Giris";
        o.AccessDeniedPath = "/Account/ErisimEngellendi";
    });

var app = builder.Build();

// Veritabanı + başlangıç verileri
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<Kullanici>>();
    db.Database.EnsureCreated();

    if (!db.Kullanicilar.Any())
    {
        var admin = new Kullanici
        {
            AdSoyad = "Yönetici",
            Email = "admin@site.com",
            Rol = "Admin",
            Onayli = true
        };
        admin.SifreHash = hasher.HashPassword(admin, "Admin123!");
        db.Kullanicilar.Add(admin);
    }

    if (!db.Urunler.Any())
    {
        db.Urunler.AddRange(
            new Urun { Ad = "Kablosuz Mouse", Aciklama = "Ergonomik, sessiz tıklama", Fiyat = 349.90m, Stok = 25 },
            new Urun { Ad = "Mekanik Klavye", Aciklama = "RGB aydınlatmalı", Fiyat = 1299.00m, Stok = 10 },
            new Urun { Ad = "USB-C Kablo", Aciklama = "1 metre, hızlı şarj", Fiyat = 89.50m, Stok = 100 }
        );
    }
    db.SaveChanges();
}

app.UseStaticFiles();

// Fiyat girişlerinde nokta/virgül karışmasın diye sabit kültür
var kultur = new CultureInfo("en-US");
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(kultur),
    SupportedCultures = new[] { kultur },
    SupportedUICultures = new[] { kultur }
});

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
