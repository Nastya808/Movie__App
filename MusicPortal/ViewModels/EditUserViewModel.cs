// File: ViewModels/EditUserViewModel.cs
using System.Collections.Generic;

namespace MusicPortal.ViewModels
{
    public class EditUserViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<SelectListItem> Roles { get; set; } = new List<SelectListItem>();
        public IEnumerable<string> SelectedRoles { get; set; } = new List<string>();
    }

    public class SelectListItem
    {
        public string Value { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public bool Selected { get; set; } = false;
    }
}
