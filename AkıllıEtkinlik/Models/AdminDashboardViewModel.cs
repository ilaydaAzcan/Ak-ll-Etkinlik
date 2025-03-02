using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AkıllıEtkinlik.Models
{
    public class AdminDashboardViewModel
    {
        public Kullanici Kullanici { get; set; }
        public IEnumerable<Event> Etkinlikler { get; set; }
        public IEnumerable<Mesaj> Mesajlar { get; set; }
       
    }
}