using System.ComponentModel.DataAnnotations;

namespace MusicPortal.Models
{
    public class Song
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, ErrorMessage = "Title length can't be more than 100.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Artist is required")]
        [StringLength(100, ErrorMessage = "Artist length can't be more than 100.")]
        public string Artist { get; set; } = string.Empty;

        [Required(ErrorMessage = "FilePath is required")]
        [StringLength(255, ErrorMessage = "FilePath length can't be more than 255.")]
        public string FilePath { get; set; } = string.Empty;

        [Required(ErrorMessage = "GenreId is required")]
        public int GenreId { get; set; }

        public Genre Genre { get; set; } = new Genre();

        [Required(ErrorMessage = "UserId is required")]
        public string UserId { get; set; } = string.Empty;

        public ApplicationUser User { get; set; } = new ApplicationUser();


    }
}
