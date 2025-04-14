using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace WebsiteHotrohoctap.Models
{
    public class User : IdentityUser
    {
        [Required]
        public string UserType { get; set; }
        // Chỉ dùng trong View
        [NotMapped]
        public string CurrentRole { get; set; }

        [NotMapped]
        public List<string> Roles { get; set; }
    }
}
