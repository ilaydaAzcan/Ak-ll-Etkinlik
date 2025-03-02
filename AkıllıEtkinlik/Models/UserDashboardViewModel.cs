using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AkıllıEtkinlik.Models
{
    public class UserDashboardViewModel
    {
        
        
            public Kullanici Kullanici { get; set; }
           // public List<Event> Etkinlikler { get; set; }
       
        public IEnumerable<Event> Etkinlikler { get; set; }
        public int ToplamPuan { get; set; }


    }
}