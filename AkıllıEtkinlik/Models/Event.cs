using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AkıllıEtkinlik.Models
{
    public enum Kategori
    {
        Konser,
        Tiyatro,
        StandUp,
        Festival,
        ElektronikMuzik,
        Soylesi,
        Sergi,
        Spor,
        Egitim,
    }

    public class Event
    {
        [Key]
        public int Id { get; set; }  // Benzersiz etkinlik kimliği

        [Required(ErrorMessage = "Etkinlik adı gerekli")]
        [StringLength(100, ErrorMessage = "Etkinlik adı en fazla 100 karakter olabilir")]
        public string EtkinlikAdi { get; set; }

        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir")]
        public string Aciklama { get; set; }

        [Required(ErrorMessage = "Tarih gerekli")]
        [DataType(DataType.Date)]
        public DateTime Tarih { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Saat gerekli")]
        public TimeSpan Saat { get; set; }

        [Required(ErrorMessage = "Etkinlik süresi gerekli")]
        public int EtkinlikSuresi { get; set; }

        [Required(ErrorMessage = "Konum gerekli")]
        [StringLength(100, ErrorMessage = "Konum en fazla 100 karakter olabilir")]
        public string Konum { get; set; }

        [Required(ErrorMessage = "Kategori gerekli")]
        [StringLength(50, ErrorMessage = "Kategori en fazla 50 karakter olabilir")]
        public string Kategori { get; set; }

        // Yeni eklenen alan: Etkinliği oluşturan kullanıcının ID'si
        [Required]
        public int OlusturanKullaniciId { get; set; }

        public string ResimYolu { get; set; }
 
        public bool OnayliMi {  get; set; }


    }
}