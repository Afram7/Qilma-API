using Microsoft.AspNetCore.Mvc;
using Qilma_API.DTOs;
using Qilma_API.Services;

namespace Qilma_API.Controllers;

[Route("[controller]")]
[ApiController]
public class UsersController : ControllerBase
{

    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDTO newUser)
    {
        var result = await _userService.CreateUserAsync(newUser);
        if (result.Failed)
        {
            return StatusCode(500, result.ErrorMessage);
        }
        if (!result.IsValid)
        {
            return BadRequest(result.ErrorMessage);
        }
        if (result.Conflict)
        {
            return Conflict(result.ErrorMessage);
        }
        return Created("" ,result.User); // temporary
    }
}
