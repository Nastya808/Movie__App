using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicPortal.Data;
using MusicPortal.Models;
using System.Linq;
using System.Threading.Tasks;

[Authorize(Roles = "Administrator")]
public class AdminSongsController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminSongsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Genres()
    {
        var genres = _context.Genres.ToList();
        return View(genres);
    }

    public IActionResult AddGenre()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> AddGenre(Genre genre)
    {
        if (ModelState.IsValid)
        {
            _context.Genres.Add(genre);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Genres));
        }
        return View(genre);
    }
}
