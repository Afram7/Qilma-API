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
        var createdUser = await _userService.CreateUserAsync(newUser);
        return Ok(createdUser); // Temporary
    }
}
