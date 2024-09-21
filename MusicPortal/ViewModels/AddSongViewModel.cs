using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace MusicPortal.ViewModels
{
    public class AddSongViewModel
    {
        [Required(ErrorMessage = "Title is required.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Artist is required.")]
        public string Artist { get; set; } = string.Empty;

        [Required(ErrorMessage = "Genre is required.")]
        public int GenreId { get; set; }

        public IFormFile? SongFile { get; set; }

        public string? SongFilePath { get; set; }

        public IEnumerable<SelectListItem>? Genres { get; set; }
    }
}
