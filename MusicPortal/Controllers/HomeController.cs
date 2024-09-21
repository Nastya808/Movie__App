using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicPortal.Data;
using MusicPortal.ViewModels;
using System.Threading.Tasks;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var model = new IndexViewModel
        {
            Genres = await _context.Genres.ToListAsync(),
            Songs = await _context.Songs.ToListAsync()
        };

        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }
}
