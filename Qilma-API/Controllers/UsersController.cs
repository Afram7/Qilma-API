using Microsoft.AspNetCore.Authorization;
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
        return CreatedAtAction(nameof(GetUserById), new { id = result.User!.UserId }, result.User);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetUserById()
    {
        var userId = int.Parse(User.FindFirst("UserId")!.Value);
        var result = await _userService.GetUserByIdAsync(userId);
        if (result.Failed)
        {
            return StatusCode(500, result.ErrorMessage);
        }
        if (result.NotFound)
        {
            return NotFound(result.ErrorMessage);
        }
        return Ok(result.User);
    }

    [Authorize]
    [HttpPut("me")]
    public async Task<IActionResult> UpdateUserById([FromBody] UpdateUserDTO updatedUser)
    {
        var userId = int.Parse(User.FindFirst("UserId")!.Value);
        var result = await _userService.UpdateUserByIdAsync(userId, updatedUser);
        if (result.Failed)        {
            return StatusCode(500, result.ErrorMessage);
        }
        if (!result.IsValid)
        {
            return BadRequest(result.ErrorMessage);
        }
        if (result.NotFound)
        {
            return NotFound(result.ErrorMessage);
        }
        return Ok(result.User);
    }

    [Authorize]
    [HttpDelete("me")]
    public async Task<IActionResult> DeleteUserById()
    {
        var userId = int.Parse(User.FindFirst("UserId")!.Value);
        var result = await _userService.DeleteUserByIdAsync(userId);
        if (result.Failed)
        {
            return StatusCode(500, result.ErrorMessage);
        }
        if (result.NotFound)
        {
            return NotFound(result.ErrorMessage);
        }
        return NoContent();
    }

    [Authorize]
    [HttpPut("me/password")]
    public async Task<IActionResult> UpdateUserPassword(UpdateUserPasswordDTO updatedPassword)
    {
        var userId = int.Parse(User.FindFirst("UserId")!.Value);
        var result = await _userService.UpdateUserPasswordAsync(userId, updatedPassword);
        if (result.Failed)
        {
            return StatusCode(500, result.ErrorMessage);
        }
        if (result.NotFound)
        {
            return NotFound(result.ErrorMessage);
        }
        if (!result.IsValid)
        {
            return BadRequest(result.ErrorMessage);
        }
        return Ok();
    }

    [Authorize]
    [HttpGet("me/statistics")]
    public async Task<IActionResult> GetUserStatistcs()
    {
        var userId = int.Parse(User.FindFirst("UserId")!.Value);
        var result = await _userService.GetUserStatistcsAsync(userId);
        if (result.Failed)
        {
            return StatusCode(500, result.ErrorMessage);
        }
        if (result.NotFound)
        {
            return NotFound(result.ErrorMessage);
        }
        return Ok(result.Statistic);
    }
}
