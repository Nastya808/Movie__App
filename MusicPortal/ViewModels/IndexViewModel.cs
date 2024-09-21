using MusicPortal.Models;

namespace MusicPortal.ViewModels
{
    public class IndexViewModel
    {
        public List<Genre> Genres { get; set; } = new List<Genre>();
        public List<Song> Songs { get; set; } = new List<Song>();
    }
}
