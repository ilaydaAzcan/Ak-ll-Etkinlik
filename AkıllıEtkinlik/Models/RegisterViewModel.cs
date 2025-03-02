using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AkıllıEtkinlik.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Kullanıcı adı gerekli")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Şifre gerekli")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Şifre doğrulama gerekli")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Şifreler eşleşmiyor")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Ad gerekli")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Soyad gerekli")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Doğum tarihi gerekli")]
        [DataType(DataType.Date)]
        public DateTime BirthDate { get; set; }

        [Required(ErrorMessage = "Cinsiyet gerekli")]
        public string Gender { get; set; } // "Erkek" veya "Kadın" gibi değerler olabilir.

        [Required(ErrorMessage = "E-posta gerekli")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Telefon numarası gerekli")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası girin")]
        public string PhoneNumber { get; set; }

        public string Interests { get; set; } // Kullanıcı ilgi alanlarını serbest metin olarak girebilir.

        [Display(Name = "Profil Fotoğrafı")]
        public HttpPostedFileBase ProfilePhoto { get; set; } // Kullanıcıdan dosya yüklemek için kullanılır.


        // Yeni eklenen özellikler
        [Required(ErrorMessage = "İl gerekli")]
        public string City { get; set; }

        [Required(ErrorMessage = "İlçe gerekli")]
        public string District {get; set;}
        public string Neighborhood { get; set; } // Yeni eklenen alan
    }
}