using Qilma_API.DTOs;

namespace Qilma_API.Services;

public interface IGuestService
{
    // Create a new guest
    Task<CreateGuestResult> CreateGuestAsync();

    // Get guest's statistics
    Task<GetStatistcsResult> GetGuestStatistcsAsync(int id);
}
