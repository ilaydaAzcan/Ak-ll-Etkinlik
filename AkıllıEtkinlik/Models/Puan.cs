using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AkıllıEtkinlik.Models;


namespace AkıllıEtkinlik.Models
{
    public class Puan
    {
        public int Id { get; set; }
        public int KullaniciID { get; set; }
        public int PuanMiktari { get; set; }
        public DateTime KazanilanTarih { get; set; }

        public virtual Kullanici Kullanici { get; set; }
    }
}       