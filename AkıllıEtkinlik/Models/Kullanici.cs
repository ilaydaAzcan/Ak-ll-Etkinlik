using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AkıllıEtkinlik.Models
{
    public class Kullanici
    {
        [Key]
        public int Id { get; set; }  // Benzersiz kullanıcı kimliği

        [Required]
        public string KullaniciAdi { get; set; }

        [Required]
        public string Sifre { get; set; }

        [Required]
        public string Ad { get; set; }

        [Required]
        public string Soyad { get; set; }

        [Required]
        public DateTime DogumTarihi { get; set; }

        [Required]
        public string Cinsiyet { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Phone]
        public string TelefonNumarasi { get; set; }

        public string IlgiAlanlari { get; set; }

        public string ProfilFoto { get; set; }  // Profil fotoğrafı yolu

        // Yeni eklenen alan: Konum bilgisi
        // [ForeignKey("Konum")]
        // Konum ilişkisi
        [ForeignKey("Konum")]
        public int KonumId { get; set; }

        public virtual Konum Konum { get; set; }

        public string Rol { get; set; } = "User";  // Varsayılan rol "User"
    }
}
