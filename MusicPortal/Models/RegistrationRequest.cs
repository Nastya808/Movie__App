using System.ComponentModel.DataAnnotations;

namespace MusicPortal.Models
{
    public class RegistrationRequest
    {
        public int Id { get; set; }

        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        public bool IsApproved { get; set; }

        public string? RequestedBy { get; set; }

        public DateTime RequestDate { get; set; } = DateTime.UtcNow;

        public bool IsProcessed { get; set; } = false;
    }
}
