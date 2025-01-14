using System.ComponentModel.DataAnnotations;

namespace PueblaApi.Entities
{
    public class ActivationToken
    {
        [Key]
        public long Id { get; set; }
        [Required]
        public string Token { get; set; }
        [Required]
        public long UserId { get; set; }
        public ApplicationUser User { get; set; } = null!;
    }
}
