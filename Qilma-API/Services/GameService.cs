using Microsoft.EntityFrameworkCore;
using Qilma_API.Constants;
using Qilma_API.Data;
using Qilma_API.DTOs;
using Qilma_API.Models;
using Qilma_API.Services.Interfaces;
using Qilma_API.Validators;

namespace Qilma_API.Services;

public class GameService : IGameService
{
    private const int MAX_ATTEMPTS = 7;
    private readonly GameValidator _gameValidator;
    private readonly IWordService _wordService;
    private readonly IStatisticService _statisticService;
    private readonly AppDbContext _db;
    private readonly ILogger<GameService> _logger;

    public GameService(GameValidator gameValidator, IWordService wordService, IStatisticService statisticService, AppDbContext db, ILogger<GameService> logger)
    {
        _gameValidator = gameValidator;
        _wordService = wordService;
        _statisticService = statisticService;
        _db = db;
        _logger = logger;
    }

    // Returns the game with the specified ID, or null if not found
    private async Task<GameModel?> FetchGameByIdAsync(int gameId)
    {
        var game = await _db.Games.FindAsync(gameId);

        return game;
    }

    // Compares the guess word with the target word and returns an array of "correct", "wrong-position", or "wrong" for each letter
    private string[] CalculateGuessResult(string guessWord, string targetWord)
    {
        guessWord = guessWord.ToUpper();
        targetWord = targetWord.ToUpper();
        int length = guessWord.Length;
        string[] resultArray = new string[length];
        int[] targetLetterCount = new int[26];

        // Count occurrences of each letter in target word
        for (int i = 0; i < length; i++)
        {
            targetLetterCount[targetWord[i] - 'A']++;
        }

        // First pass: mark correct letters
        for (int i = 0; i < length; i++)
        {
            if (guessWord[i] == targetWord[i])
            {
                resultArray[i] = "correct";
                targetLetterCount[guessWord[i] - 'A']--;
            }
        }

        // Second pass: mark wrong-position or wrong
        for (int i = 0; i < length; i++)
        {
            if (resultArray[i] != null)
            {
                continue; // already correct
            }

            int index = guessWord[i] - 'A';
            if (targetLetterCount[index] > 0)
            {
                resultArray[i] = "wrong-position";
                targetLetterCount[index]--;
            }
            else
            {
                resultArray[i] = "wrong";
            }
        }

        return resultArray;
    }

    public async Task<CreateGameResult> CreateGameAsync(int ownerId, string ownerType)
    {
        try
        {
            var word = await _wordService.GenerateRandomWordAsync();
            var gameModel = new GameModel
            {
                OwnerId = ownerId,
                OwnerType = ownerType,
                Attempts = 0,
                Word = word,
                Date = DateTime.UtcNow,
                Result = GameStatus.Pending
            };

            using var transaction = await _db.Database.BeginTransactionAsync();
            await _statisticService.UpdateStatisticsForNewGameAsync(ownerId, ownerType);
            _db.Games.Add(gameModel);
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return new CreateGameResult
            {
                Game = new GameDTO
                {
                    GameId = gameModel.GameId,
                    OwnerId = gameModel.OwnerId,
                    OwnerType = gameModel.OwnerType,
                    Attempts = gameModel.Attempts,
                    Date = gameModel.Date,
                    Result = gameModel.Result
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating game for ownerId: {OwnerId}", ownerId);
            return new CreateGameResult
            {
                Failed = true,
                ErrorMessage = ErrorMessages.INTERNAL_ERROR_MESSAGE
            };
        }
    }

    public async Task<SubmitGuessResult> SubmitGuessAsync(int guestId, int gameId, string guessWord)
    {
        try
        {
            if (gameId <= 0)
            {
                return new SubmitGuessResult
                {
                    IsValid = false,
                    ErrorMessage = ErrorMessages.INVALID_GAME_ID
                };
            }
            
            var game = await FetchGameByIdAsync(gameId);
            if (game == null)
            {
                return new SubmitGuessResult
                {
                    NotFound = true,
                    ErrorMessage = ErrorMessages.GAME_NOT_FOUND
                };
            }
            if (game.OwnerType == OwnerTypes.Guest && game.OwnerId != guestId)
            {
                return new SubmitGuessResult
                {
                    Forbidden = true,
                    ErrorMessage = ErrorMessages.GAME_ACCESS_DENIED
                };
            }
            if (game.Result != GameStatus.Pending)
            {
                return new SubmitGuessResult
                {
                    Conflict = true,
                    ErrorMessage = ErrorMessages.GAME_ALREADY_FINISHED
                };
            }

            var validation = _gameValidator.ValidateGuessWord(guessWord);
            if (!validation.IsValid)
            {
                return new SubmitGuessResult
                {
                    WordNotRecognized = true,
                    ErrorMessage = validation.ErrorMessage
                };
            }

            using var transaction = await _db.Database.BeginTransactionAsync();
            game.Attempts++;
            var resultArray = CalculateGuessResult(guessWord, game.Word);

            if (guessWord.Equals(game.Word, StringComparison.OrdinalIgnoreCase))
            {
                game.Result = GameStatus.Win;
                await _statisticService.UpdateStatisticsForGameWinAsync(game.OwnerId, game.OwnerType);
            }
            else if (game.Attempts >= MAX_ATTEMPTS)
            {
                game.Result = GameStatus.Lose;
            }

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return new SubmitGuessResult
            {
                IsValid = true,
                GuessResult = new GuessResultDTO
                {
                    GameId = game.GameId,
                    GuessWord = guessWord,
                    Result = resultArray,
                    AttemptsLeft = MAX_ATTEMPTS - game.Attempts,
                    GameOver = game.Result != GameStatus.Pending,
                    CorrectWord = game.Result != GameStatus.Pending ? game.Word : null
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting guess for gameId: {GameId}", gameId);
            return new SubmitGuessResult
            {
                Failed = true,
                ErrorMessage = ErrorMessages.INTERNAL_ERROR_MESSAGE
            };
        }
    }

    public async Task DeleteGamesByUserIdAsync(int userId)
    {
        try
        {
            var games = await _db.Games.Where(g => g.OwnerId == userId && g.OwnerType == OwnerTypes.User).ToListAsync();
            _db.Games.RemoveRange(games);
            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting games for userId: {UserId}", userId);
        }
    }
}
