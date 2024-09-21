using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MusicPortal.Data;
using MusicPortal.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

public static class SeedData
{
    public static async Task InitializeAsync(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager, ApplicationDbContext context)
    {
        string adminRole = "Administrator";
        string adminUserName = "admin";
        string adminEmail = "admin@example.com";
        string adminPassword = "Admin@123";

        if (!await roleManager.RoleExistsAsync(adminRole))
        {
            await roleManager.CreateAsync(new IdentityRole(adminRole));
        }

        var adminUser = await userManager.FindByNameAsync(adminUserName);

        if (adminUser == null)
        {
            adminUser = new ApplicationUser { UserName = adminUserName, Email = adminEmail };
            var result = await userManager.CreateAsync(adminUser, adminPassword);

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, adminRole);
            }
        }
        else if (!await userManager.IsInRoleAsync(adminUser, adminRole))
        {
            await userManager.AddToRoleAsync(adminUser, adminRole);
        }

        InitializeGenres(context);
    }

    private static void InitializeGenres(ApplicationDbContext context)
    {
        if (!context.Genres.Any())
        {
            context.Genres.AddRange(
                new Genre { Name = "Rock" },
                new Genre { Name = "Pop" },
                new Genre { Name = "Jazz" },
                new Genre { Name = "Classical" }
            );
            context.SaveChanges();
        }
    }
}
