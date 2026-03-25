using CrypticWizard.RandomWordGenerator;
using Qilma_API.Constants;
using Qilma_API.Data;
using Qilma_API.DTOs;
using Qilma_API.Models;

namespace Qilma_API.Services;

public class GameService : IGameService
{
    private readonly AppDbContext _db;

    public GameService(AppDbContext db)
    {
        _db = db;
    }

    // Generates a random 5-letter valid english word
    private string GenerateRandomWord()
    {
        var wordGenerator = new WordGenerator();
        string? word = null;
        while(word == null || word.Length != 5)
        {
            try
            {
                word = wordGenerator.GetWord();
            }
            catch
            {
                continue;
            }
        }

        return word;
    }

    public async Task<CreateGameResult> CreateGameAsync(int ownerId, string ownerType)
    {
        try
        {
            var word = GenerateRandomWord();
            var game = new GameModel
            {
                OwnerId = ownerId,
                OwnerType = ownerType,
                Attempts = 0,
                Word = word,
                Date = DateTime.UtcNow,
                Result = "pending"
            };

            _db.Games.Add(game);
            await _db.SaveChangesAsync();

            return new CreateGameResult
            {
                Game = new GameDTO
                {
                    GameId = game.GameId,
                    OwnerId = game.OwnerId,
                    OwnerType = game.OwnerType,
                    Attempts = game.Attempts,
                    Date = game.Date,
                    Result = game.Result
                }
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return new CreateGameResult
            {
                Failed = true,
                ErrorMessage = HttpErrorMessages.INTERNAL_ERROR_MESSAGE
            };
        }
    }
}
