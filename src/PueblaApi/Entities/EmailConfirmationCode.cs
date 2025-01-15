using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PueblaApi.Entities
{
    public class EmailConfirmationCode
    {
        // User the code belongs to. (PK and FK)
        [Key]
        [ForeignKey("Id")]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; } = null!;
        [Required]
        public Guid Code { get; set; }
        [Required]
        public DateTimeOffset ExpirationDate { set; get; }
    }
}
