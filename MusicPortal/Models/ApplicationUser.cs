﻿using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MusicPortal.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public bool IsActive { get; set; } = true;

        public virtual ICollection<Song> Songs { get; set; } = new HashSet<Song>();
    }
}
