using Qilma_API.DTOs;

namespace Qilma_API.Services.Interfaces;

public interface IGameService
{
    // Create a new game
    Task<CreateGameResult> CreateGameAsync(int ownerId, string ownerType);

    // Submit a guess for a specific game
    Task<SubmitGuessResult> SubmitGuessAsync(int guestId, int gameId, string guessWord);

    // Delete all games associated with a specific user
    Task DeleteGamesByUserIdAsync(int userId);
}
