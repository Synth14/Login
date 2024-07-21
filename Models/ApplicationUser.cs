using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Login.Models
{
    public class ApplicationUser:IdentityUser<Guid>
    {
        [PersonalData]
        [StringLength(4)]
        public string? Civility { get; set; }

        [PersonalData]
        [StringLength(128)]
        public string? FirstName { get; set; }

        [PersonalData]
        [StringLength(128)]
        public string? LastName { get; set; }
    }
}
