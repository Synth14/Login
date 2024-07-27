using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Login.Models
{
    [Index(nameof(DomainName), IsUnique = true)]
    public class AllowedDomain
    {
        public int Id { get; set; }

        [Required]
        [StringLength(256)]
        public string DomainName { get; set; }
    }
}
