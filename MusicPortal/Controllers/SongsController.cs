using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MusicPortal.Data;
using MusicPortal.Models;
using MusicPortal.ViewModels;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MusicPortal.Controllers
{
    public class SongsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SongsController(ApplicationDbContext context)
        {
            _context = context;
        }

        private async Task<string?> SaveSongFileAsync(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", file.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                return "/uploads/" + file.FileName;
            }
            return null;
        }


        private async Task<string> SaveSongFile(IFormFile file)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", file.FileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return "/uploads/" + file.FileName;
        }

        public async Task<IActionResult> Index(string searchString, int? genreId, int pageNumber = 1)
        {
            var query = _context.Songs
                .Include(s => s.Genre)  
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(s => s.Title.Contains(searchString) || s.Artist.Contains(searchString));
            }

            if (genreId.HasValue)
            {
                query = query.Where(s => s.GenreId == genreId.Value);
            }

            var pageSize = 10;
            var totalCount = await query.CountAsync();
            var totalPages = (int)System.Math.Ceiling((double)totalCount / pageSize);

            var songs = await _context.Songs
                .Include(s => s.Genre) 
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();


            var model = new SongsViewModel
            {
                Songs = songs,
                Genres = new SelectList(await _context.Genres.ToListAsync(), "Id", "Name"),
                CurrentFilter = searchString,
                CurrentGenre = genreId,
                PageNumber = pageNumber,
                TotalPages = totalPages
            };

            return View(model);
        }



        [HttpGet]
        public IActionResult Add()
        {
            ViewBag.Genres = new SelectList(_context.Genres, "Id", "Name");
            return View(new AddSongViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(AddSongViewModel model)
        {
            if (ModelState.IsValid)
            {
                var song = new Song
                {
                    Title = model.Title,
                    Artist = model.Artist,
                    GenreId = model.GenreId,  
                    FilePath = await SaveSongFileAsync(model.SongFile),
                    UserId = User.Identity.Name
                };

                _context.Songs.Add(song);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Genres = new SelectList(_context.Genres, "Id", "Name", model.GenreId);
            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var song = await _context.Songs.FindAsync(id);
            if (song == null)
            {
                return NotFound();
            }

            ViewBag.Genres = new SelectList(await _context.Genres.ToListAsync(), "Id", "Name");

            var model = new AddSongViewModel
            {
                Title = song.Title,
                Artist = song.Artist,
                GenreId = song.GenreId,
                SongFilePath = song.FilePath
            };

            return View(model);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AddSongViewModel model)
        {
            if (ModelState.IsValid)
            {
                var song = await _context.Songs.FindAsync(id);
                if (song == null)
                {
                    return NotFound();
                }

                song.Title = model.Title;
                song.Artist = model.Artist;
                song.GenreId = model.GenreId;

                if (model.SongFile != null)
                {
                    song.FilePath = await SaveSongFileAsync(model.SongFile);
                }

                _context.Update(song);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Re-populate genres for the view model in case of invalid model state
            model.Genres = await _context.Genres
                                .Select(g => new ViewModels.SelectListItem
                                {
                                    Value = g.Id.ToString(),
                                    Text = g.Name
                                })
                                .ToListAsync();

            return View(model);
        }



        // Delete Action
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var song = await _context.Songs.FindAsync(id);
            if (song == null)
            {
                return NotFound();
            }

            return View(song);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var song = await _context.Songs.FindAsync(id);
            if (song != null)
            {
                _context.Songs.Remove(song);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        public string GetGenreName(MusicPortal.Models.Genre genre)
        {
            return genre != null && !string.IsNullOrEmpty(genre.Name) ? genre.Name : "Unknown";
        }

    }
}
