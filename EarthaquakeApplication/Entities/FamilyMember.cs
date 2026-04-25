using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EarthaquakeApplication.Entities
{
    public class FamilyMember
    {
        [Key] // Id'nin Primary Key olduğunu açıkça belirtmek iyi bir pratik
        public int Id { get; set; }

        [Required(ErrorMessage = "Ad alanı zorunludur.")] // Zorunlu ve hata mesajı
        public string FirstName { get; set; } = string.Empty; // Nullable olmayan string'lere varsayılan değer atamak iyi bir pratik

        [Required(ErrorMessage = "Soyad alanı zorunludur.")] // Zorunlu ve hata mesajı
        public string LastName { get; set; } = string.Empty;

        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";

        public string? Province { get; set; } // Nullable olarak kalsın
        public string? Country { get; set; } // Nullable olarak kalsın

        public double? Latitude { get; set; }   // ← eklenecek
        public double? Longitude { get; set; }

        // Foreign Key: Bu aile üyesinin hangi kullanıcıya ait olduğunu belirtir
        //[Required(ErrorMessage = "Kullanıcı ID'si zorunludur.")] // Zorunlu olmalı
        public string ApplicationUserId { get; set; } = string.Empty;

        public ApplicationUser? ApplicationUser { get; set; }
    }
}
