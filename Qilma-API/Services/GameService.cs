using CrypticWizard.RandomWordGenerator;
using Microsoft.EntityFrameworkCore;
using Qilma_API.Constants;
using Qilma_API.Data;
using Qilma_API.DTOs;
using Qilma_API.Models;
using Qilma_API.Validators;

namespace Qilma_API.Services;

public class GameService : IGameService
{
    private const int MAX_ATTEMPTS = 7;
    private readonly GameValidator _gameValidator;
    private readonly WordGenerator _wordGenerator;
    private readonly AppDbContext _db;

    public GameService(WordGenerator wordGenerator, GameValidator gameValidator, AppDbContext db)
    {
        _gameValidator = gameValidator;
        _wordGenerator = wordGenerator;
        _db = db;
    }

    // Generates a random 5-letter valid english word
    private string GenerateRandomWord()
    {
        string? word = null;
        int attempts = 0;
        const int maxAttempts = 300;
        while((word == null || word.Length != 5) && attempts < maxAttempts)
        {
            try
            {
                word = _wordGenerator.GetWord();
            }
            catch{}
            attempts++;
        }

        if (word == null || word.Length != 5)
        {
            throw new Exception ("Failed to generate a valid word");
        }

        return word;
    }

    // Returns the game with the specified ID, or null if not found
    private async Task<GameModel?> FetchGameByIdAsync(int id)
    {
        var game = await _db.Games.FindAsync(id);
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
            var word = GenerateRandomWord();
            var gameModel = new GameModel
            {
                OwnerId = ownerId,
                OwnerType = ownerType,
                Attempts = 0,
                Word = word,
                Date = DateTime.UtcNow,
                Result = "pending"
            };

            using var transaction = await _db.Database.BeginTransactionAsync();
            var statistic = await _db.Statistics.FirstOrDefaultAsync(statistics => statistics.OwnerId == ownerId && statistics.OwnerType == ownerType);
            if (statistic != null)
            {
                statistic.GamesPlayed++;
            }

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
            Console.WriteLine(ex.Message);
            return new CreateGameResult
            {
                Failed = true,
                ErrorMessage = HttpErrorMessages.INTERNAL_ERROR_MESSAGE
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
                    ErrorMessage = HttpErrorMessages.INVALID_GAME_ID
                };
            }
            
            var game = await FetchGameByIdAsync(gameId);
            if (game == null)
            {
                return new SubmitGuessResult
                {
                    NotFound = true,
                    ErrorMessage = HttpErrorMessages.GAME_NOT_FOUND
                };
            }
            if (game.OwnerType == "guest" && game.OwnerId != guestId)
            {
                return new SubmitGuessResult
                {
                    Forbidden = true,
                    ErrorMessage = HttpErrorMessages.GAME_ACCESS_DENIED
                };
            }
            if (game.Result != "pending")
            {
                return new SubmitGuessResult
                {
                    Conflict = true,
                    ErrorMessage = HttpErrorMessages.GAME_ALREADY_FINISHED
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
                game.Result = "win";
                var statistic = await _db.Statistics.FirstOrDefaultAsync(statistics => statistics.OwnerId == game.OwnerId && statistics.OwnerType == game.OwnerType);
                if (statistic != null)
                {
                    statistic.GamesWon++;
                }
            }
            else if (game.Attempts >= MAX_ATTEMPTS)
            {
                game.Result = "lose";
            }

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            var guessResult = new GuessResultDTO
            {
                GameId = game.GameId,
                GuessWord = guessWord,
                Result = resultArray,
                AttemptsLeft = MAX_ATTEMPTS - game.Attempts,
                GameOver = game.Result != "pending",
                CorrectWord = game.Result != "pending" ? game.Word : null
            };

            return new SubmitGuessResult
            {
                IsValid = true,
                GuessResult = guessResult
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return new SubmitGuessResult
            {
                Failed = true,
                ErrorMessage = HttpErrorMessages.INTERNAL_ERROR_MESSAGE
            };
        }
    }
}
