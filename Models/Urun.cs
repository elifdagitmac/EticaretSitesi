using System.ComponentModel.DataAnnotations;

namespace EticaretSitesi.Models;

public class Urun
{
    public int Id { get; set; }

    [Display(Name = "Ürün adı")]
    [Required(ErrorMessage = "Ürün adı zorunlu"), StringLength(100)]
    public string Ad { get; set; } = "";

    [Display(Name = "Açıklama")]
    [StringLength(500)]
    public string? Aciklama { get; set; }

    [Display(Name = "Fiyat (₺)")]
    [Range(0.01, 1000000, ErrorMessage = "Fiyat 0'dan büyük olmalı")]
    public decimal Fiyat { get; set; }

    [Display(Name = "Stok")]
    [Range(0, 100000, ErrorMessage = "Stok 0 veya daha büyük olmalı")]
    public int Stok { get; set; }

    [Display(Name = "Satışta mı?")]
    public bool SatistaMi { get; set; } = true;

    [Display(Name = "Resim adresi (URL)")]
    public string? ResimUrl { get; set; }
}
