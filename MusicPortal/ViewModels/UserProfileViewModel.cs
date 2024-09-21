using MusicPortal.Models;

namespace MusicPortal.ViewModels
{
    public class UserProfileViewModel
    {
        public ApplicationUser User { get; set; } = null!;
        public IEnumerable<Song> UserSongs { get; set; } = new List<Song>();
        public IEnumerable<Genre> Genres { get; set; } = new List<Genre>();
    }
}
