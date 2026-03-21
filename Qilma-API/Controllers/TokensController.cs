using Microsoft.AspNetCore.Mvc;
using Qilma_API.DTOs;
using Qilma_API.Services;

namespace Qilma_API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class TokensController : ControllerBase
    {
        
        private readonly ITokenService _tokenService;

        public TokensController(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }
        
        [HttpPost]
        public async Task<IActionResult> GenerateToken([FromBody] TokenRequestDTO request)
        {
            var result = await _tokenService.GenerateTokenAsync(request.Email, request.Password);
            if (result.Failed)
            {
                return StatusCode(500, result.ErrorMessage);
            }
            if (!result.IsValid)
            {
                return Unauthorized(result.ErrorMessage);
            }
            return Ok(result.TokenResponse);
        }
    }
}
