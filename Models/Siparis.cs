namespace EticaretSitesi.Models;

public class Siparis
{
    public int Id { get; set; }
    public int KullaniciId { get; set; }
    public Kullanici? Kullanici { get; set; }
    public DateTime Tarih { get; set; }
    public decimal Toplam { get; set; }
    public List<SiparisKalemi> Kalemler { get; set; } = new();
}

public class SiparisKalemi
{
    public int Id { get; set; }
    public int SiparisId { get; set; }
    public int UrunId { get; set; }
    public string UrunAdi { get; set; } = "";
    public decimal BirimFiyat { get; set; }
    public int Adet { get; set; }
}
