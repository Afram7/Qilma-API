using Qilma_API.DTOs;

namespace Qilma_API.Services.Interfaces;

public interface IGuestService
{
    // Create a new guest
    Task<CreateGuestResult> CreateGuestAsync();

    // Get guest's statistics
    Task<GetStatisticsResult> GetGuestStatistcsAsync(int guestId);
}
