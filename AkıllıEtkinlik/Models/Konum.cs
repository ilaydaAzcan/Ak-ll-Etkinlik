using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AkıllıEtkinlik.Models
{
    public class Konum
    {
        [Key]
        public int KonumId { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        public string District { get; set; }
        [Required]
        public string Neighborhood { get; set; } // Mahalle bilgisi
        // Kullanıcı ile ilişki
        public ICollection<Kullanici> Kullanicilar { get; set; }

        [NotMapped] // Bu alan veritabanına kaydedilmeyecek
        public string konum => $"{City}, {District}, {Neighborhood}";
    }
}
