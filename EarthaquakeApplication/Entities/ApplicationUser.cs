using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EarthaquakeApplication.Entities
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }


        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";

        public DateTime? Birthday { get; set; }
        public string? Gender { get; set; }
        public string? Province { get; set; }
        public string? Country { get; set; }

    }
}
