using AkıllıEtkinlik.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using BCrypt.Net;



namespace AkıllıEtkinlik.Controllers
    {
        public class HomeController : Controller
        {
            private readonly ApplicationDbContext _context;

            // Parametresiz constructor, _context'i başlatır.

            public HomeController()
            {
                // _context = new ApplicationDbContext(new DbContextOptions<ApplicationDbContext>());
                _context = DbContextFactory.Create();
            }


            [HttpGet]
            public ActionResult Login()
            {
                return View();
            }

      
        [HttpPost]
        public ActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.UserType == "Admin" && model.Username == "admin" && model.Password == "admin")
                {
                    // Admin başarılı giriş
                    Session["UserId"] = "admin"; // Admin için sabit bir değer
                    return RedirectToAction("AdminDashboard");
                }

                // Kullanıcı türü "User" ise, veritabanında sorgu yap
                if (model.UserType == "User")
                {
                    var kullanici = _context.Kullanicilar
                        .FirstOrDefault(k => k.KullaniciAdi == model.Username && k.Rol == "User");

                    if (kullanici != null && BCrypt.Net.BCrypt.Verify(model.Password, kullanici.Sifre))
                    {
                        // Kullanıcı başarılı giriş
                        Session["UserId"] = kullanici.Id;
                        return RedirectToAction("UserDashboard");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Geçersiz kullanıcı adı veya şifre.");
                        return View(model);
                    }
                }

                // Eğer yukarıdaki koşullar sağlanmadıysa, kullanıcı türü veya bilgiler hatalı
                ModelState.AddModelError("", "Geçersiz kullanıcı türü veya bilgiler.");
            }

            return View(model);
        }




        [HttpGet]
            public ActionResult Register()
            {
                return View();
            }


            [HttpPost]
            public async Task<ActionResult> Register(RegisterViewModel model)
            {
                if (ModelState.IsValid)
                {
                // Aynı kullanıcı adı var mı kontrol et
                var usernameExists = _context.Kullanicilar.Any(k => k.KullaniciAdi == model.Username);
                if (usernameExists)
                {
                    ModelState.AddModelError("Username", "Bu kullanıcı adı daha önce kullanılmıştır.");
                    return View(model);
                }

                // Aynı e-posta var mı kontrol et
                var emailExists = _context.Kullanicilar.Any(k => k.Email == model.Email);
                if (emailExists)
                {
                    ModelState.AddModelError("Email", "Bu e-posta adresi daha önce kullanılmıştır.");
                    return View(model);
                }
                //Yeni konum ekle
                var konum = new Konum
                    {
                        City = model.City,
                        District = model.District,
                        Neighborhood = model.Neighborhood
                    };
                    _context.Konumlar.Add(konum);
                    await _context.SaveChangesAsync();

                    //Kullanıcıyı ekle ve KonumId'yi kullan
                    var kullanici = new Kullanici
                    {
                        KullaniciAdi = model.Username,
                        Sifre = BCrypt.Net.BCrypt.HashPassword(model.Password), // Şifreyi hashle
                        Ad = model.FirstName,
                        Soyad = model.LastName,
                        DogumTarihi = model.BirthDate,
                        Cinsiyet = model.Gender,
                        Email = model.Email,
                        TelefonNumarasi = model.PhoneNumber,
                        IlgiAlanlari = model.Interests,
                        KonumId = konum.KonumId,
                        Rol = "User" // Varsayılan olarak "User" rolü atanıyor
                    };

                    // Profil fotoğrafını kaydetme işlemi
                    if (model.ProfilePhoto != null && model.ProfilePhoto.ContentLength > 0)
                    {
                        // Uploads klasörünü kontrol et ve oluştur
                        string uploadsPath = Server.MapPath("~/Content/Uploads/");
                        if (!Directory.Exists(uploadsPath))
                        {
                            Directory.CreateDirectory(uploadsPath);
                        }

                        // Dosyayı kaydet
                        string fileName = Guid.NewGuid() + Path.GetExtension(model.ProfilePhoto.FileName);
                        string path = Path.Combine(uploadsPath, fileName);
                        model.ProfilePhoto.SaveAs(path);

                        kullanici.ProfilFoto = "/Content/Uploads/" + fileName;
                    }

                    // Kullanıcıyı veritabanına kaydet
                    _context.Kullanicilar.Add(kullanici);
                    await _context.SaveChangesAsync();

                // Kullanıcı kayıt olduktan sonra 20 puan ekleyin
                var puan = new Puan
                {
                    KullaniciID = kullanici.Id,
                    PuanMiktari = 20,
                    KazanilanTarih = DateTime.Now
                };

                _context.Puanlar.Add(puan);
                await _context.SaveChangesAsync();


                // Kayıt başarılıysa giriş sayfasına yönlendirme yapılır
                return RedirectToAction("Login");
                }

                // Model doğrulanmazsa formu tekrar göster
                return View(model);
            }


        [HttpGet]
        public ActionResult UserDashboard(string category = "all")
        {
            if (Session["UserId"] == null)
            {
                // Kullanıcı giriş yapmamışsa, giriş sayfasına yönlendir
                return RedirectToAction("Login", "Home");
            }

            int userId = (int)Session["UserId"];
            var kullanici = _context.Kullanicilar.FirstOrDefault(k => k.Id == userId);

            if (kullanici == null)
            {
                return HttpNotFound();
            }

            IQueryable<Event> etkinlikler;

            if (category.ToLower() == "onerilenler")
            {
                // Kullanıcının katıldığı etkinliklerin kategorilerini al
                var katildigiKategoriler = _context.Katilimcilar
                    .Where(k => k.KullaniciId == userId)
                    .Select(k => k.Etkinlik.Kategori)
                    .Distinct()
                    .ToList();

                // Aynı kategorideki etkinlikleri önerilen olarak al
                etkinlikler = _context.Etkinlikler
                    .Where(e => katildigiKategoriler.Contains(e.Kategori) && e.Tarih >= DateTime.Now && e.OnayliMi)
                    .AsQueryable();
            }
            else if (category.ToLower() != "all")
            {
                // Belirli kategoriye göre etkinlikleri al
                etkinlikler = _context.Etkinlikler
                    .Where(e => e.Kategori.ToLower() == category.ToLower() && e.Tarih >= DateTime.Now && e.OnayliMi)
                    .AsQueryable();
            }
            else
            {
                // Tüm etkinlikleri al
                etkinlikler = _context.Etkinlikler
                    .Where(e => e.Tarih >= DateTime.Now && e.OnayliMi)
                    .AsQueryable();
            }

            var etkinlikList = etkinlikler.ToList();


            // Kullanıcının toplam puanını hesapla
            var toplamPuan = _context.Puanlar
           .Where(p => p.KullaniciID == userId)
           .Sum(p => p.PuanMiktari);

            // ViewModel oluştur ve toplam puanı ekle
            var viewModel = new UserDashboardViewModel
            {
                Kullanici = kullanici,
                Etkinlikler = etkinlikList,
                ToplamPuan = toplamPuan
            };

            // View'e ViewModel'i gönder
            return View(viewModel);
        }


        [HttpGet]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ForgetPassword(string email, string firstName, string lastName, string phoneNumber, string password, string confirmPassword)
        {
            if (password != confirmPassword)
            {
                ModelState.AddModelError("", "Şifreler uyuşmuyor. Lütfen tekrar deneyiniz.");
                return View("ForgotPassword");
            }

            var kullanici = _context.Kullanicilar
                .FirstOrDefault(k => k.Email == email && k.Ad == firstName && k.Soyad == lastName && k.TelefonNumarasi == phoneNumber);

            if (kullanici == null)
            {
                ModelState.AddModelError("", "Girilen bilgilerle eşleşen bir kullanıcı bulunamadı. Lütfen bilgilerinizi kontrol ediniz.");
                return View("ForgotPassword");
            }

            // Önceki şifre ile aynı olup olmadığını kontrol edin
            if (!string.IsNullOrEmpty(kullanici.Sifre))
            {
                try
                {
                    if (BCrypt.Net.BCrypt.Verify(password, kullanici.Sifre))
                    {
                        ModelState.AddModelError("", "Önceki şifrenizle aynı bir şifre kullanamazsınız. Lütfen farklı bir şifre deneyin.");
                        return View("ForgotPassword");
                    }
                }
                catch (BCrypt.Net.SaltParseException ex)
                {
                    // Hatalı formatları logla
                    System.Diagnostics.Debug.WriteLine($"Hatalı şifre formatı: {ex.Message}");
                    ModelState.AddModelError("", "Depolanan şifre formatı hatalı. Lütfen sistem yöneticinize başvurun.");
                    return View("ForgotPassword");
                }
            }

            // Şifreyi hashle ve veritabanına kaydet
            kullanici.Sifre = BCrypt.Net.BCrypt.HashPassword(password);
            _context.SaveChanges();

            ViewBag.SuccessMessage = "Şifreniz başarıyla güncellenmiştir. Lütfen giriş yapın.";
            return RedirectToAction("Login");
        }

        [HttpGet]
        public ActionResult UpdateProfile()
        {
            if (Session["UserId"] == null)
            {
                // Kullanıcı giriş yapmamışsa giriş sayfasına yönlendir
                return RedirectToAction("Login", "Home");
            }

            int userId = (int)Session["UserId"];
            var kullanici = _context.Kullanicilar.FirstOrDefault(k => k.Id == userId);

            if (kullanici == null)
            {
                return HttpNotFound();
            }

            // Kullanıcı bilgilerini View'a gönder
            return View(kullanici);
        }

        [HttpPost]
        public async Task<ActionResult> UpdateProfile(Kullanici model, HttpPostedFileBase ProfilFoto)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Home");
            }

            if (ModelState.IsValid)
            {
                var kullanici = _context.Kullanicilar.FirstOrDefault(k => k.Id == model.Id);
                if (kullanici != null)
                {
                    // Kullanıcı bilgilerini güncelle
                    kullanici.KullaniciAdi = model.KullaniciAdi;
                    kullanici.Email = model.Email;
                    kullanici.Ad = model.Ad;
                    kullanici.Soyad = model.Soyad;
                    kullanici.DogumTarihi = model.DogumTarihi;
                    kullanici.Cinsiyet = model.Cinsiyet;
                    kullanici.TelefonNumarasi = model.TelefonNumarasi;
                    kullanici.IlgiAlanlari = model.IlgiAlanlari;

                    // Mevcut şifre kontrolü
                    string mevcutSifre = Request["MevcutSifre"];
                    if (!string.IsNullOrEmpty(mevcutSifre) && !BCrypt.Net.BCrypt.Verify(mevcutSifre, kullanici.Sifre))
                    {
                        TempData["ErrorMessage"] = "Mevcut şifre yanlış.";
                        return View(model);
                    }

                    // Yeni şifre güncellemesi
                    string yeniSifre = Request["Sifre"];
                    string confirmSifre = Request["ConfirmSifre"];
                    if (!string.IsNullOrEmpty(yeniSifre))
                    {
                        if (yeniSifre == confirmSifre)
                        {
                            kullanici.Sifre = BCrypt.Net.BCrypt.HashPassword(yeniSifre); // Şifreyi hashle
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Yeni şifreler uyuşmuyor.";
                            return View(model);
                        }
                    }

                    // Profil fotoğrafını güncelle
                    if (ProfilFoto != null && ProfilFoto.ContentLength > 0)
                    {
                        string uploadsPath = Server.MapPath("~/Content/Uploads/");
                        if (!Directory.Exists(uploadsPath))
                        {
                            Directory.CreateDirectory(uploadsPath);
                        }

                        string fileName = Guid.NewGuid() + Path.GetExtension(ProfilFoto.FileName);
                        string path = Path.Combine(uploadsPath, fileName);
                        ProfilFoto.SaveAs(path);

                        kullanici.ProfilFoto = "/Content/Uploads/" + fileName;
                    }

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Bilgileriniz başarıyla güncellendi.";
                    return RedirectToAction("UpdateProfile");
                }
            }

            TempData["ErrorMessage"] = "Bilgiler güncellenirken bir hata oluştu. Lütfen tekrar deneyiniz.";
            return View(model);
        }



        [HttpGet]
            public ActionResult CreateEvent()
            {
                // Boş bir Event nesnesi oluşturuluyor ve View'e gönderiliyor
                var eventModel = new Event();
                var model = new Event
                {
                    Tarih = DateTime.Now // Bugünün tarihini varsayılan olarak ayarlayın
                };
                return View(eventModel);
            }
        [HttpPost]
        public ActionResult CreateEvent(EventViewModel model, HttpPostedFileBase Resim)
        {
            if (ModelState.IsValid)
            {
              
                // Yeni bir etkinlik oluşturun
                var etkinlik = new Event
                {
                    EtkinlikAdi = model.EtkinlikAdi,
                    Aciklama = model.Aciklama,
                    Tarih = model.Tarih,
                    Saat = model.Saat,
                    EtkinlikSuresi = model.EtkinlikSuresi,
                    Konum = model.Konum,
                    Kategori = model.Kategori,
                    OlusturanKullaniciId = (int)Session["UserId"] // Oturumdan kullanıcının ID'sini al
                };

                // Resim yüklendi mi kontrol et
                if (Resim != null && Resim.ContentLength > 0)
                {
                    string uploadsPath = Server.MapPath("~/Content/Uploads/");
                    if (!Directory.Exists(uploadsPath))
                    {
                        Directory.CreateDirectory(uploadsPath);
                    }

                    string fileName = Guid.NewGuid() + Path.GetExtension(Resim.FileName);
                    string path = Path.Combine(uploadsPath, fileName);
                    Resim.SaveAs(path);

                    etkinlik.ResimYolu = "/Content/Uploads/" + fileName; // Resim yolunu etkinlik modeline ekle
                }

                _context.Etkinlikler.Add(etkinlik);
                _context.SaveChanges();

                // Kullanıcıya 15 puan ekle
                var puan = new Puan
                {
                    KullaniciID = etkinlik.OlusturanKullaniciId,
                    PuanMiktari = 15, // Etkinlik ekleme puanı
                    KazanilanTarih = DateTime.Now
                };

                _context.Puanlar.Add(puan);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Etkinlik başarıyla oluşturuldu ve 15 puan kazandınız!";
                return RedirectToAction("UserDashboard");
            }

            return View(model);
        }

        [HttpGet]
            public ActionResult ManageEvents()
            {
                if (Session["UserId"] == null)
                {
                    return RedirectToAction("Login", "Home");
                }

                int userId = (int)Session["UserId"];
                var kullaniciEtkinlikleri = _context.Etkinlikler
                    .Where(e => e.OlusturanKullaniciId == userId && e.OnayliMi == true && e.Tarih >= DateTime.Now)
                    .ToList();

                return View(kullaniciEtkinlikleri);
            }

            [HttpGet]
            public ActionResult ManageEventsAdmin()
            {
                if (Session["UserId"] == null)
                {
                    return RedirectToAction("Login", "Home");
                }

                var adminEtkinlikler = _context.Etkinlikler
                    .Where(e => e.OnayliMi == true && e.Tarih >= DateTime.Now)
                    .ToList();

                return View(adminEtkinlikler);
            }


            [HttpPost]
            public ActionResult DeleteEventAdmin(int id)
            {
                if (Session["UserId"] == null)
                {
                    return RedirectToAction("Login", "Home");
                }

                var etkinlik = _context.Etkinlikler.FirstOrDefault(e => e.Id == id);
                if (etkinlik == null)
                {
                    return HttpNotFound();
                }
            // Etkinliğe katılan herhangi bir katılımcı var mı kontrol et
            var katilimciVarMi = _context.Katilimcilar.Any(k => k.EtkinlikId == id);
            if (katilimciVarMi)
            {
                TempData["ErrorMessage"] = "Bu etkinlikte katılımcılar olduğu için silinemez.";
                return RedirectToAction("ManageEventsAdmin");
            }
            _context.Etkinlikler.Remove(etkinlik);
                _context.SaveChanges();

                return RedirectToAction("ManageEventsAdmin");
            }


            [HttpPost]
            public ActionResult DeleteEvent(int id)
            {
                if (Session["UserId"] == null)
                {
                    return RedirectToAction("Login", "Home");
                }

                var etkinlik = _context.Etkinlikler.FirstOrDefault(e => e.Id == id);
                if (etkinlik == null || etkinlik.OlusturanKullaniciId != (int)Session["UserId"])
                {
                    return HttpNotFound();
                }

            // Etkinlikte herhangi bir katılımcı var mı kontrol et
            var katilimciVarMi = _context.Katilimcilar.Any(k => k.EtkinlikId == id);
            if (katilimciVarMi)
            {
                TempData["ErrorMessage"] = "Bu etkinlikte katılımcılar olduğu için silinemez.";
                return RedirectToAction("ManageEvents");
            }

            _context.Etkinlikler.Remove(etkinlik);
                _context.SaveChanges();

                return RedirectToAction("ManageEvents");
            }

            [HttpGet]
            public ActionResult EditEvent(int? id)
            {
                if (Session["UserId"] == null)
                {
                    return RedirectToAction("Login", "Home");
                }

                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                var etkinlik = _context.Etkinlikler.FirstOrDefault(e => e.Id == id);
                if (etkinlik == null || etkinlik.OlusturanKullaniciId != (int)Session["UserId"])
                {
                    return HttpNotFound();
                }

                return View(etkinlik);
            }



            [HttpPost]
            public ActionResult EditEvent(Event model, HttpPostedFileBase Resim)
            {
                if (ModelState.IsValid)
                {
                    var etkinlik = _context.Etkinlikler.FirstOrDefault(e => e.Id == model.Id);
                    if (etkinlik != null && etkinlik.OlusturanKullaniciId == (int)Session["UserId"])
                    {
                        etkinlik.EtkinlikAdi = model.EtkinlikAdi;
                        etkinlik.Aciklama = model.Aciklama;
                        etkinlik.Tarih = model.Tarih;
                        etkinlik.Saat = model.Saat;
                        etkinlik.Konum = model.Konum;
                        etkinlik.Kategori = model.Kategori;

                        // Eğer kullanıcı yeni bir resim yüklüyorsa
                        if (Resim != null && Resim.ContentLength > 0)
                        {
                            if (Resim.ContentType == "image/jpeg" || Resim.ContentType == "image/png")
                            {
                                string uploadsPath = Server.MapPath("~/Content/Uploads/");
                                if (!Directory.Exists(uploadsPath))
                                {
                                    Directory.CreateDirectory(uploadsPath);
                                }

                                string fileName = Guid.NewGuid() + Path.GetExtension(Resim.FileName);
                                string path = Path.Combine(uploadsPath, fileName);
                                Resim.SaveAs(path);

                                etkinlik.ResimYolu = "/Content/Uploads/" + fileName;
                            }
                            else
                            {
                                ModelState.AddModelError("", "Sadece JPEG veya PNG formatında resim yükleyebilirsiniz.");
                                return View(model);
                            }
                        }

                        _context.SaveChanges();
                        return RedirectToAction("ManageEvents");
                    }
                }

                return View(model);
            }

            [HttpGet]
            public ActionResult EditEventAdmin(int? id)
            {
                
                if (Session["UserId"] == null)
                {
                    return RedirectToAction("Login", "Home");
                }

                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                //Veritabanından etkinliği bul
                var etkinlik = _context.Etkinlikler.FirstOrDefault(e => e.Id == id);
                if (etkinlik == null)
                {
                    return HttpNotFound();
                }

                return View(etkinlik);
            }

            [HttpPost]
            public ActionResult EditEventAdmin(Event model, HttpPostedFileBase Resim)
            {
                if (ModelState.IsValid)
                {
                    //Etkinliği veritabanından bul
                    var etkinlik = _context.Etkinlikler.FirstOrDefault(e => e.Id == model.Id);
                    if (etkinlik != null)
                    {
                        etkinlik.EtkinlikAdi = model.EtkinlikAdi;
                        etkinlik.Aciklama = model.Aciklama;
                        etkinlik.Tarih = model.Tarih;
                        etkinlik.Saat = model.Saat;
                        etkinlik.Konum = model.Konum;
                        etkinlik.Kategori = model.Kategori;

                        //Yeni bir resim yüklenmişse 
                        if (Resim != null && Resim.ContentLength > 0)
                        {
                            string uploadsPath = Server.MapPath("~/Content/Uploads/");
                            if (!Directory.Exists(uploadsPath))
                            {
                                Directory.CreateDirectory(uploadsPath);
                            }

                            string fileName = Guid.NewGuid() + Path.GetExtension(Resim.FileName);
                            string path = Path.Combine(uploadsPath, fileName);
                            Resim.SaveAs(path);

                            etkinlik.ResimYolu = "/Content/Uploads/" + fileName;
                        }

                        _context.SaveChanges();
                        return RedirectToAction("ManageEventsAdmin");
                    }
                }

                return View(model);
            }
       
        [HttpGet]
        public ActionResult AdminDashboard(string category = "all")
        {
            if (Session["UserId"] == null)
            {
                // Kullanıcı giriş yapmamışsa, giriş sayfasına yönlendir
                return RedirectToAction("Login", "Home");
            }

            // Sadece onaylı etkinlikleri getir ve kategoriye göre filtrele
            var etkinlikler = _context.Etkinlikler.Where(e => e.OnayliMi && e.Tarih >= DateTime.Now);

            if (!string.IsNullOrEmpty(category) && category.ToLower() != "all")
            {
                etkinlikler = etkinlikler.Where(e => e.Kategori.ToLower() == category.ToLower());
            }

            var etkinlikList = etkinlikler.ToList();

            return View(etkinlikList);
        }



        [HttpGet]
            public ActionResult Logout()
            {
                // Oturumdaki bilgileri temizle
                Session.Clear();
                return RedirectToAction("Login", "Home");
            }

           
            [HttpGet]
            public ActionResult EtkinlikleriOnayla()
            {
                // Onaylanmamış etkinlikleri getiriyoruz
                var etkinlikler = _context.Etkinlikler.Where(e => !e.OnayliMi).ToList();

                if (etkinlikler == null || !etkinlikler.Any())
                {
                    etkinlikler = new List<Event>(); // Eğer etkinlik yoksa boş liste döndür
                }

                return View(etkinlikler);
            }

            [HttpPost]
            public ActionResult OnaylaEtkinlik(int id)
            {
                var etkinlik = _context.Etkinlikler.FirstOrDefault(e => e.Id == id);
                if (etkinlik != null)
                {
                    etkinlik.OnayliMi = true;
                    _context.SaveChanges();
                }
                return RedirectToAction("EtkinlikleriOnayla");
            }

            [HttpPost]
            public ActionResult ReddetEtkinlik(int id)
            {
                var etkinlik = _context.Etkinlikler.FirstOrDefault(e => e.Id == id);
                if (etkinlik != null)
                {
                    _context.Etkinlikler.Remove(etkinlik);
                    _context.SaveChanges();
                }
                return RedirectToAction("EtkinlikleriOnayla");
            }


            [HttpGet]
            public ActionResult ManageUsers()
            {
                if (Session["UserId"] == null)
                {
                    return RedirectToAction("Login", "Home");
                }

                // Tüm kullanıcıları getiriyoruz (Admin kullanıcı hariç)
                var kullanicilar = _context.Kullanicilar
                    .Where(k => k.Rol != "Admin")
                    .ToList();

                return View(kullanicilar);
            }


        [HttpGet]
        public ActionResult EditUser(int id)
        {
            // Kullanıcıyı veritabanından çek
            var kullanici = _context.Kullanicilar.FirstOrDefault(k => k.Id == id);

            if (kullanici == null)
            {
                TempData["ErrorMessage"] = "Kullanıcı bulunamadı.";
                return RedirectToAction("ManageUsers");
            }

            return View(kullanici); // Kullanıcı bilgilerini View'a gönder
        }

        [HttpPost]
        public async Task<ActionResult> EditUser(Kullanici model, HttpPostedFileBase ProfilFoto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Kullanıcıyı veritabanından bul
                    var kullanici = _context.Kullanicilar.FirstOrDefault(k => k.Id == model.Id);
                    if (kullanici == null)
                    {
                        TempData["ErrorMessage"] = "Kullanıcı bulunamadı.";
                        return View(model);
                    }

                    // Kullanıcı bilgilerini güncelle
                    kullanici.KullaniciAdi = model.KullaniciAdi;
                    kullanici.Email = model.Email;
                    kullanici.Ad = model.Ad;
                    kullanici.Soyad = model.Soyad;
                    kullanici.DogumTarihi = model.DogumTarihi;
                    kullanici.Cinsiyet = model.Cinsiyet;
                    kullanici.TelefonNumarasi = model.TelefonNumarasi;
                    kullanici.IlgiAlanlari = model.IlgiAlanlari;

                    // Şifreyi kontrol et ve güncelle (eğer girilmişse)
                    if (!string.IsNullOrEmpty(model.Sifre))
                    {
                        kullanici.Sifre = BCrypt.Net.BCrypt.HashPassword(model.Sifre);
                    }

                    // Profil fotoğrafını güncelle
                    if (ProfilFoto != null && ProfilFoto.ContentLength > 0)
                    {
                        string uploadsPath = Server.MapPath("~/Content/Uploads/");
                        if (!Directory.Exists(uploadsPath))
                        {
                            Directory.CreateDirectory(uploadsPath);
                        }

                        string fileName = Guid.NewGuid() + Path.GetExtension(ProfilFoto.FileName);
                        string path = Path.Combine(uploadsPath, fileName);
                        ProfilFoto.SaveAs(path);

                        kullanici.ProfilFoto = "/Content/Uploads/" + fileName;
                    }

                    // Değişiklikleri kaydet
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Kullanıcı bilgileri başarıyla güncellendi.";
                    return RedirectToAction("ManageUsers");
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Bilgiler güncellenirken bir hata oluştu: " + ex.Message;
                }
            }
            else
            {
                // ModelState hatalarını logla
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                foreach (var error in errors)
                {
                    System.Diagnostics.Debug.WriteLine(error);
                }

                TempData["ErrorMessage"] = "Bilgiler güncellenirken bir hata oluştu. Lütfen tüm alanları kontrol edin.";
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult DeleteUser(int id)
        {
            try
            {
                // Kullanıcı oturumu kontrolü
                if (Session["UserId"] == null)
                {
                    return RedirectToAction("Login", "Home");
                }

                // Silinecek kullanıcıyı veritabanından bul
                var kullanici = _context.Kullanicilar.FirstOrDefault(k => k.Id == id);

                if (kullanici == null)
                {
                    TempData["ErrorMessage"] = "Kullanıcı bulunamadı.";
                    return RedirectToAction("ManageUsers");
                }

                // Kullanıcıyla ilişkili diğer verileri sil

                // 1. Kullanıcıya ait puanları sil
                var puanlar = _context.Puanlar.Where(p => p.KullaniciID == id).ToList();
                _context.Puanlar.RemoveRange(puanlar);

                // 2. Kullanıcının katıldığı etkinlikleri sil
                var katilimcilar = _context.Katilimcilar.Where(k => k.KullaniciId == id).ToList();
                _context.Katilimcilar.RemoveRange(katilimcilar);

                // 3. Kullanıcının oluşturduğu mesajları sil
                var mesajlar = _context.Mesajlar.Where(m => m.GondericiID == id).ToList();
                _context.Mesajlar.RemoveRange(mesajlar);

                // 4. Kullanıcının oluşturduğu etkinlikleri sil
                var etkinlikler = _context.Etkinlikler.Where(e => e.OlusturanKullaniciId == id).ToList();
                _context.Etkinlikler.RemoveRange(etkinlikler);

                // Kullanıcıyı sil
                _context.Kullanicilar.Remove(kullanici);

                // Tüm değişiklikleri kaydet
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Kullanıcı ve ilişkili tüm veriler başarıyla silindi.";
                return RedirectToAction("ManageUsers");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Bir hata oluştu: " + ex.Message;
                return RedirectToAction("ManageUsers");
            }
        }

        [HttpGet]
        public ActionResult EventDetailsCard(int id)
        {
            var etkinlik = _context.Etkinlikler.FirstOrDefault(e => e.Id == id);
            if (etkinlik == null)
            {
                return HttpNotFound();
            }

            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Home");
            }

            int userId = (int)Session["UserId"];

            // Kullanıcının etkinliğe katılıp katılmadığını kontrol et
            var isUserJoined = _context.Katilimcilar.Any(k => k.KullaniciId == userId && k.EtkinlikId == id);

            if (!isUserJoined)
            {
                TempData["ErrorMessage"] = "Bu etkinliğe katılmadığınız için mesajları göremezsiniz veya gönderemezsiniz.";
            }

            var messages = _context.Mesajlar
                .Where(m => m.EtkinlikId == id)
                .Include(m => m.Gonderici)
                .OrderBy(m => m.GonderimZamani)
                .ToList();

            ViewBag.Messages = messages;

            return View(etkinlik);
        }



        [HttpPost]
         public ActionResult SendMessage(int EventId, string MessageText)
         {
             // Kullanıcı oturum kontrolü
             if (Session["UserId"] == null)
             {
                 TempData["ErrorMessage"] = "Mesaj göndermek için giriş yapmalısınız.";
                 return RedirectToAction("EventDetailsCard", new { id = EventId });
             }

             // Kullanıcı ID'sini al
             int userId = (int)Session["UserId"];

             // Mesajı oluştur
             var message = new Mesaj
             {
                 GondericiID = userId,
                 AliciID = null, // Grup mesajı  
                 EtkinlikId = EventId, // Hangi etkinlikte mesaj atıldığı belirtiliyor
                 MesajMetni = MessageText,
                 GonderimZamani = DateTime.Now
             };

             // Mesajı veritabanına ekle
             _context.Mesajlar.Add(message);
             _context.SaveChanges();

             return RedirectToAction("EventDetailsCard", new { id = EventId });
         }

        


         public ActionResult EventDetailsCardAdmin(int id)
         {
             var etkinlik = _context.Etkinlikler.FirstOrDefault(e => e.Id == id);
             if (etkinlik == null)
             {
                 return HttpNotFound();
             }
             // Etkinlik ile ilişkili mesajları kullanıcı bilgisiyle birlikte getir
             var messages = _context.Mesajlar
                 .Where(m => m.EtkinlikId == id) // Sadece bu etkinliğe ait mesajlar
                 .Include(m => m.Gonderici)      // Gönderen kullanıcı bilgisiyle birlikte getir
                 .OrderBy(m => m.GonderimZamani)
                 .ToList();

             // View'e Model ve Mesajları gönder
             ViewBag.Messages = messages;
             return View(etkinlik);
         }

        [HttpPost]
        public ActionResult DeleteMessage(int messageId, int eventId)
        {
            // Admin oturum kontrolü yapıyoruz
            if (Session["UserId"] == null || !Session["UserId"].Equals("admin"))
            {
                return RedirectToAction("Login", "Home"); // Admin olmayan birisi giriş yapmaya çalışıyorsa yönlendir
            }

            var message = _context.Mesajlar.FirstOrDefault(m => m.MesajID == messageId);
            if (message != null)
            {
                // Mesajı veritabanından sil
                _context.Mesajlar.Remove(message);
                _context.SaveChanges();
            }

            // Silme işleminden sonra admini aynı etkinliğin detaylarına yönlendir
            return RedirectToAction("EventDetailsCardAdmin", new { id = eventId });
        }


        [HttpGet]
        public ActionResult MyJoinedEvents()
        {
            if (Session["UserId"] == null)
            {
                // Kullanıcı giriş yapmamışsa, giriş sayfasına yönlendir
                return RedirectToAction("Login", "Home");
            }

            int userId = (int)Session["UserId"];

            // Katıldığı etkinlikleri almak
            var katildigimEtkinlikler = _context.Katilimcilar
                .Where(k => k.KullaniciId == userId)
                .Select(k => k.Etkinlik) // Etkinlik bilgilerini al
                .ToList();

            return View(katildigimEtkinlikler);
        }

        [HttpPost]
        public ActionResult JoinEvent(int eventId)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Home");
            }

            int userId = (int)Session["UserId"];

            // Yeni etkinlik bilgilerini al
            var yeniEtkinlik = _context.Etkinlikler.FirstOrDefault(e => e.Id == eventId);
            if (yeniEtkinlik == null)
            {
                TempData["ErrorMessage"] = "Etkinlik bulunamadı.";
                return RedirectToAction("UserDashboard");
            }

            // Kullanıcının katıldığı etkinlikleri al
            var kullaniciEtkinlikleri = _context.Katilimcilar
                .Where(k => k.KullaniciId == userId)
                .Select(k => k.Etkinlik)
                .ToList();

            // Yeni etkinlik saatlerini hesapla
            var yeniEtkinlikBaslangicSaati = yeniEtkinlik.Saat;
            var yeniEtkinlikBitisSaati = yeniEtkinlik.Saat.Add(TimeSpan.FromHours(yeniEtkinlik.EtkinlikSuresi));

            // Çakışma kontrolü yap
            foreach (var etkinlik in kullaniciEtkinlikleri)
            {
                var etkinlikBaslangicSaati = etkinlik.Saat;
                var etkinlikBitisSaati = etkinlik.Saat.Add(TimeSpan.FromHours(etkinlik.EtkinlikSuresi));

                if ((yeniEtkinlikBaslangicSaati < etkinlikBitisSaati) && (yeniEtkinlikBitisSaati > etkinlikBaslangicSaati))
                {
                    TempData["ErrorMessage"] = $"Bu etkinlik saatinde '{etkinlik.EtkinlikAdi}' adlı etkinliğe katıldığınız için bu etkinliğe katılamazsınız. ({etkinlikBaslangicSaati:hh\\:mm} - {etkinlikBitisSaati:hh\\:mm})";
                    return RedirectToAction("EventDetailsCard", new { id = eventId });
                }
            }

            // Kullanıcıyı etkinliğe ekle
            var katilimci = new Katilimci
            {
                KullaniciId = userId,
                EtkinlikId = eventId
            };
            _context.Katilimcilar.Add(katilimci);
            _context.SaveChanges();

            // Kullanıcıya 10 puan ekle
            var puan = new Puan
            {
                KullaniciID = userId,
                PuanMiktari = 10,
                KazanilanTarih = DateTime.Now
            };

            _context.Puanlar.Add(puan);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Başarıyla etkinliğe katıldınız ve 10 puan kazandınız!";
            return RedirectToAction("EventDetailsCard", new { id = eventId });
        }

        [HttpGet]
        public ActionResult EventMap(int? eventId)
        {
            // Eğer eventId sağlanmamışsa, hata mesajı döndür
            if (eventId == null)
            {
                TempData["ErrorMessage"] = "Etkinlik bilgisi sağlanmadı.";
                return RedirectToAction("UserDashboard");
            }

            // Kullanıcı oturum kontrolü
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Home");
            }

            // Giriş yapan kullanıcı ID'sini alın
            int userId = (int)Session["UserId"];

            // Kullanıcının konum bilgilerini alın
            var userLocation = _context.Kullanicilar
                .Where(k => k.Id == userId)
                .Select(k => new
                {
                    k.Konum.City,
                    k.Konum.District,
                    k.Konum.Neighborhood
                })
                .FirstOrDefault();

            // Eğer kullanıcının konumu bulunamazsa hata mesajı döndür
            if (userLocation == null)
            {
                TempData["ErrorMessage"] = "Kullanıcı konum bilgisi bulunamadı.";
                return RedirectToAction("UserDashboard");
            }

            // Etkinliğin konum bilgilerini alın
            var eventLocation = _context.Etkinlikler
                .Where(e => e.Id == eventId)
                .Select(e => new
                {
                    e.Konum
                })
                .FirstOrDefault();

            // Eğer etkinlik konumu bulunamazsa hata mesajı döndür
            if (eventLocation == null)
            {
                TempData["ErrorMessage"] = "Etkinlik konum bilgisi bulunamadı.";
                return RedirectToAction("UserDashboard");
            }

            // Kullanıcı ve etkinlik konumlarını JSON formatında ViewBag'e gönder
            ViewBag.UserLocation = Newtonsoft.Json.JsonConvert.SerializeObject(userLocation);
            ViewBag.EventLocation = Newtonsoft.Json.JsonConvert.SerializeObject(eventLocation);

            return View();
        }



    }
}




