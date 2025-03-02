using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.EntityFrameworkCore;

namespace AkıllıEtkinlik.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Kullanici> Kullanicilar { get; set; }// Mevcut Kullanıcılar için DbSet
        public DbSet<Konum> Konumlar { get; set; }// Konumlar için DbSet
        public DbSet<Event> Etkinlikler { get; set; } // Etkinlikleri veritabanında saklamak için kullanılan DbSet
        public DbSet<Katilimci> Katilimcilar { get; set; }  // Katilimcilar tablosunu ekliyoruz
        public DbSet<Puan> Puanlar { get; set; }

        // Mesajlar tablosu için DbSet tanımı
        public DbSet<Mesaj> Mesajlar { get; set; }
    }
}
