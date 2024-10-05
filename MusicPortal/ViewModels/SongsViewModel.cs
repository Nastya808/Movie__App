using Microsoft.AspNetCore.Mvc.Rendering;
using MusicPortal.Models;
using System.Collections.Generic;

namespace MusicPortal.ViewModels
{
    public class SongsViewModel
    {
        public IEnumerable<Song> Songs { get; set; } = new List<Song>();
        public SelectList Genres { get; set; } 
        public string CurrentFilter { get; set; } = string.Empty;
        public string CurrentSort { get; set; } = string.Empty;
        public string TitleSortParm { get; set; } = string.Empty;
        public string ArtistSortParm { get; set; } = string.Empty;
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
        public int? CurrentGenre { get; set; }
    }
}
