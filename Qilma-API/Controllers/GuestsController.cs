using Microsoft.AspNetCore.Mvc;
using Qilma_API.Services;

namespace Qilma_API.Controllers;

[Route("[controller]")]
[ApiController]
public class GuestsController : ControllerBase
{
    
    private readonly IGuestService _guestService;

    public GuestsController(IGuestService guestService)
    {
        _guestService = guestService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateGuest()
    {
        var result = await _guestService.CreateGuestAsync();
        if (result.Failed)
        {
            return StatusCode(500, result.ErrorMessage);
        }
        return Created("", result.Guest);
    }
}
