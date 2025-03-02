using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AkıllıEtkinlik.Models
{
    public class Katilimci
    {
        public int Id { get; set; }
        public int KullaniciId { get; set; }
        public int EtkinlikId { get; set; }

        // İlişkiler
        public Kullanici Kullanici { get; set; }
        public Event Etkinlik { get; set; }
    }
}