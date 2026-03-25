using Microsoft.AspNetCore.Mvc;
using Qilma_API.Services;

namespace Qilma_API.Controllers;

[Route("[controller]")]
[ApiController]
public class GamesController : ControllerBase
{

    private readonly IGameService _gameService;

    public GamesController(IGameService gameService)
    {
        _gameService = gameService;
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateGame()
    {
        int ownerId;
        string ownerType;
        if (User.Identity?.IsAuthenticated == true)
        {
            ownerId = int.Parse(User.FindFirst("UserId")!.Value);
            ownerType = "user";
        }
        else if (Request.Headers.TryGetValue("X-Guest-Id", out var guestId))
        {
            ownerId = int.Parse(guestId!);
            ownerType = "guest";
        }
        else
        {
            return Unauthorized();
        }

        var result = await _gameService.CreateGameAsync(ownerId, ownerType);
        if (result.Failed)
        {
            return StatusCode(500, result.ErrorMessage);
        }
        return Created("", result.Game);
    }
}
