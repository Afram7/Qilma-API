using Qilma_API.DTOs;

namespace Qilma_API.Services;

public interface IGuestService
{
    // Create a new guest
    Task<CreateGuestResult> CreateGuestAsync();
}
