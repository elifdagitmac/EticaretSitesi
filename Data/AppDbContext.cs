using EticaretSitesi.Models;
using Microsoft.EntityFrameworkCore;

namespace EticaretSitesi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Kullanici> Kullanicilar => Set<Kullanici>();
    public DbSet<Urun> Urunler => Set<Urun>();
    public DbSet<SepetOgesi> SepetOgeleri => Set<SepetOgesi>();
    public DbSet<Siparis> Siparisler => Set<Siparis>();
    public DbSet<SiparisKalemi> SiparisKalemleri => Set<SiparisKalemi>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Kullanici>().HasIndex(k => k.Email).IsUnique();
    }
}
