using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AkıllıEtkinlik.Models
{
    public class Mesaj
    {
       
        public int MesajID { get; set; } // Birincil anahtar
        public int GondericiID { get; set; }
        public int? AliciID { get; set; } // AliciID boş olabilir
        public string MesajMetni { get; set; }
        public DateTime GonderimZamani { get; set; }
        public int EtkinlikId { get; set; } // Etkinlik ilişkisi

        [ForeignKey("GondericiID")]
        public Kullanici Gonderici { get; set; } // Gönderen kullanıcı bilgisi
    }
}