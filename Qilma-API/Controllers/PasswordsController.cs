using Microsoft.AspNetCore.Mvc;
using Qilma_API.DTOs;
using Qilma_API.Services.Interfaces;

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
    public async Task<IActionResult> ResetPasswordRequest([FromBody] ResetPasswordRequestDTO request)
    {
        var result = await _passwordService.ResetPasswordRequestAsync(request);
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
    
    [HttpPost("reset/confirm")]
    public async Task<IActionResult> ResetPasswordConfirm([FromBody] ResetPasswordDTO ResetPasswordDTO)
    {
        var result = await _passwordService.ResetPasswordConfirmAsync(ResetPasswordDTO);
        if (result.Failed)
        {
            return StatusCode(500, result.ErrorMessage);
        }
        if (result.Forbidden)
        {
            return StatusCode(403, result.ErrorMessage);
        }
        if (!result.IsValid)
        {
            return BadRequest(result.ErrorMessage);
        }
        
        return Ok(result.SuccessMessage);
    }
}
