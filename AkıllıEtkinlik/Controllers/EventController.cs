
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AkıllıEtkinlik.Models; // Modellerinizi içeri almak için gerekli olabilir

namespace AkıllıEtkinlik.Controllers
{
    public class EventController : Controller
    {
       
        private readonly ApplicationDbContext db;

        // Constructor
        public EventController()
        {
            db = DbContextFactory.Create(); // DbContextFactory kullanarak ApplicationDbContext oluşturuyoruz

        }

        // GET: Event
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult JoinEvent(int id)
        {
            // Kullanıcı kimliğini al
            var userId = Session["UserId"];
            if (userId == null)
            {
                // Kullanıcı giriş yapmamışsa, giriş sayfasına yönlendir
                return RedirectToAction("Login", "Home");
            }

            int kullaniciId = (int)userId;

            // Katılmak istenen etkinliği al
            var etkinlik = db.Etkinlikler.FirstOrDefault(e => e.Id == id);
            if (etkinlik == null)
            {
                return HttpNotFound();
            }

            // Kullanıcının katıldığı etkinlikleri al
            var katildigiEtkinlikler = db.Katilimcilar
                .Where(k => k.KullaniciId == kullaniciId)
                .Select(k => k.Etkinlik)
                .ToList();

            // Kullanıcının katıldığı etkinliklerin tarih ve saatlerini kontrol et
            foreach (var mevcutEtkinlik in katildigiEtkinlikler)
            {
                // Mevcut etkinliğin başlangıç zamanı ve bitiş zamanı
                var mevcutEtkinlikBaslangic = mevcutEtkinlik.Tarih + mevcutEtkinlik.Saat;
                var mevcutEtkinlikBitis = mevcutEtkinlikBaslangic.Add(TimeSpan.FromHours(mevcutEtkinlik.EtkinlikSuresi));

                // Katılmak istenen etkinliğin başlangıç zamanı ve bitiş zamanı
                var etkinlikBaslangic = etkinlik.Tarih + etkinlik.Saat;
                var etkinlikBitis = etkinlikBaslangic.Add(TimeSpan.FromHours(etkinlik.EtkinlikSuresi));

                // Çakışma kontrolü
                if (mevcutEtkinlikBaslangic < etkinlikBitis && etkinlikBaslangic < mevcutEtkinlikBitis)
                {
                    TempData["ErrorMessage"] = "Bu etkinliğe katılamazsınız çünkü bu saat aralığında başka bir etkinliğe zaten katıldınız.";
                    return RedirectToAction("EventDetailsCard", "Home", new { id = id });
                }
            }

            // Eğer kullanıcı daha önce bu etkinliğe katılmışsa tekrar eklenmesini engelle
            var existingEntry = db.Katilimcilar.FirstOrDefault(k => k.KullaniciId == kullaniciId && k.EtkinlikId == id);
            if (existingEntry == null)
            {
                // Yeni bir katılımcı kaydı oluştur ve veritabanına ekle
                Katilimci yeniKatilimci = new Katilimci
                {
                    KullaniciId = kullaniciId,
                    EtkinlikId = id
                };
                db.Katilimcilar.Add(yeniKatilimci);
                db.SaveChanges();

                // Kullanıcıya 10 puan ekleyin
                var puan = new Puan
                {
                    KullaniciID = kullaniciId,
                    PuanMiktari = 10,  // Puan yerine PuanMiktari kullanıldı.
                    KazanilanTarih = DateTime.Now
                };

                db.Puanlar.Add(puan);
                db.SaveChanges();
            }

            // Kullanıcıyı UserDashboard sayfasına geri yönlendir
            return RedirectToAction("UserDashboard", "Home");
        }
       
    }
}
