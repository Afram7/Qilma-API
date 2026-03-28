using Microsoft.AspNetCore.Mvc;
using Qilma_API.DTOs;
using Qilma_API.Services;

namespace Qilma_API.Controllers;

[Route("[controller]")]
[ApiController]
public class PasswordsController : ControllerBase
{
    private readonly IPasswordService _passwordService;

    public PasswordsController(IPasswordService passwordService)
    {
        _passwordService = passwordService;
    }

    [HttpPost("reset/request")]
    public async Task<IActionResult> ResetPasswordRequest([FromBody] ResetPasswordRequestDto request)
    {
        var result = await _passwordService.ResetPasswordRequestAsync(request.Eamil);
        if (result.Failed)
        {
            return StatusCode(500, result.ErrorMessage);
        }
        if (!result.IsValid)
        {
            return BadRequest(result.ErrorMessage);
        }
        return Ok(result.SuccessMessage);
    }
}
