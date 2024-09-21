using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicPortal.Data;
using MusicPortal.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[Authorize(Roles = "Administrator")]
public class RegistrationRequestsController : Controller
{
    private readonly ApplicationDbContext _context;

    public RegistrationRequestsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        List<RegistrationRequest> requests = await _context.RegistrationRequests
            .Where(r => !r.IsProcessed)
            .ToListAsync();

        return View(requests);
    }

    public async Task<IActionResult> Process(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var request = await _context.RegistrationRequests.FindAsync(id);
        if (request == null)
        {
            return NotFound();
        }

        request.IsProcessed = true;
        _context.Update(request);
        await _context.SaveChangesAsync();


        return RedirectToAction(nameof(Index));
    }
}
