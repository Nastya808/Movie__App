using Microsoft.AspNetCore.Identity;
using MusicPortal.Models;

namespace MusicPortal.ViewModels
{
    public class AdminProfileViewModel
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public IList<ApplicationUser> Users { get; set; } // Список пользователей
        public IList<IdentityRole> Roles { get; set; } // Список ролей
        // Добавьте другие свойства, которые будут отображены в профиле администратора
    }
}
