using Qilma_API.Models;

namespace Qilma_API.Services.Interfaces;

public interface IStatisticService
{
    // Increases the GamesPlayed count for the specified owner and owner type when a new game is created
    Task UpdateStatisticsForNewGameAsync(int ownerId, string ownerType);

    // Increases the GamesWon count for the specified owner and owner type when a game is won
    Task UpdateStatisticsForGameWinAsync(int ownerId, string ownerType);

    // Fetches the statistics for a guest with the specified ID, or null if not found
    Task<StatisticModel?> FetchGuestStatisticsByIdAsync(int guestId);

    // Fetches the statistics for a user with the specified ID, or null if not found
    Task<StatisticModel?> FetchUserStatisticsByIdAsync(int userId);

    // Creates a new statistic entry for a newly created guest with the specified ID
    Task CreateStatisticForNewGuestAsync(int guestId);

    // Creates a new statistic entry for a newly created user with the specified ID
    Task CreateStatisticForNewUserAsync(int userId);

    // Deletes the statistic entry for a user with the specified ID
    Task DeleteStatisticForUserAsync(int userId);
}
