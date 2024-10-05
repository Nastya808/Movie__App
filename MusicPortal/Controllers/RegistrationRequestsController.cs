using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
    private readonly UserManager<ApplicationUser> _userManager;

    public RegistrationRequestsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager; 
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

    [HttpPost]
    public async Task<IActionResult> ApproveRequest([FromForm] int requestId)
    {
        var request = await _context.RegistrationRequests.FindAsync(requestId);
        if (request == null)
        {
            return Json(new { success = false, message = "Request not found" });
        }

        var user = new ApplicationUser
        {
            UserName = request.Username,
            Email = request.Email
        };

        var result = await _userManager.CreateAsync(user, request.Password);  
        if (result.Succeeded)
        {
            request.IsApproved = true;
            request.IsProcessed = true;
            _context.RegistrationRequests.Update(request);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }
        else
        {
            return Json(new { success = false, message = "Error creating user: " + string.Join(", ", result.Errors.Select(e => e.Description)) });
        }
    }


    [HttpPost]
    public async Task<IActionResult> RejectRequest([FromForm] int requestId)
    {
        var request = await _context.RegistrationRequests.FindAsync(requestId);
        if (request == null)
        {
            return Json(new { success = false, message = "Request not found" });
        }

        _context.RegistrationRequests.Remove(request);
        await _context.SaveChangesAsync();

        return Json(new { success = true });
    }


    public async Task<IActionResult> ViewRequest(int requestId)
    {
        var request = await _context.RegistrationRequests.FindAsync(requestId);
        if (request == null)
        {
            return NotFound();
        }
        return View(request);
    }
}
