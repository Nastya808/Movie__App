using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace MusicPortal.ViewModels
{
    public class SongUploadViewModel
    {
        public string Title { get; set; } = string.Empty; 

        [Required]
        public int GenreId { get; set; }

        public IFormFile File { get; set; } = null!;
    }
}
