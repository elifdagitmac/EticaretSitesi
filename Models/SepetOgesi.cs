namespace EticaretSitesi.Models;

public class SepetOgesi
{
    public int Id { get; set; }
    public int KullaniciId { get; set; }
    public int UrunId { get; set; }
    public Urun Urun { get; set; } = null!;
    public int Adet { get; set; }
}
