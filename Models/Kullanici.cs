namespace EticaretSitesi.Models;

public class Kullanici
{
    public int Id { get; set; }
    public string AdSoyad { get; set; } = "";
    public string Email { get; set; } = "";
    public string SifreHash { get; set; } = "";
    public string Rol { get; set; } = "Kullanici"; // "Admin" veya "Kullanici"
    public bool Onayli { get; set; } = false;
}
