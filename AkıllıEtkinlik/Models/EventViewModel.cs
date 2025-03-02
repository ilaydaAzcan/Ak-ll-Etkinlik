using System;
using System.ComponentModel.DataAnnotations;

namespace AkıllıEtkinlik.Models
{
    public class EventViewModel
    {
        [Required(ErrorMessage = "Etkinlik adı gerekli")]
        public string EtkinlikAdi { get; set; }

        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir")]
        public string Aciklama { get; set; }

        [Required(ErrorMessage = "Tarih gerekli")]
        public DateTime Tarih { get; set; }

        [Required(ErrorMessage = "Saat gerekli")]
        public TimeSpan Saat { get; set; }

        [Required(ErrorMessage = "Etkinlik süresi gerekli")]
        public int EtkinlikSuresi { get; set; }

        [Required(ErrorMessage = "Konum gerekli")]
        public string Konum { get; set; }

        [Required(ErrorMessage = "Kategori gerekli")]
        public string Kategori { get; set; }
    }
}
