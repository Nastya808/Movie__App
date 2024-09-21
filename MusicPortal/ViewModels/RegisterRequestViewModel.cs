using System.ComponentModel.DataAnnotations;

namespace MusicPortal.ViewModels
{
    public class RegisterRequestViewModel
    {
        [Required]
        [Display(Name = "Username")]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}
