using EarthaquakeApplication.Entities;
using EarthaquakeApplication.Entities; // FamilyMember için
using System.Collections.Generic;

namespace EarthaquakeApplication.FilterViewModel
{
    public class FamilyMembersViewModel
    {
        // Mevcut aile üyeleri listesi
        public List<FamilyMember> ExistingMembers { get; set; }

        // Yeni bir aile üyesi eklemek için kullanılan boş FamilyMember nesnesi
        public FamilyMember NewMember { get; set; } = new FamilyMember(); // Varsayılan olarak boş bir nesne oluştur

        // İsteğe bağlı: Başarılı mesajları veya hata mesajları için
        public string StatusMessage { get; set; }
    }
}