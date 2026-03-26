namespace Qilma_API.DTOs;

// DTO for returning game information
public class GameDTO
{
    required public int GameId { get;  set; }
    required public int OwnerId { get;  set; }
    required public string OwnerType { get;  set; }
    required public int Attempts { get;  set; }
    required public DateTime Date { get;  set; }
    required public string Result { get;  set; }
}

// DTO for returning the result of creating a game
public class CreateGameResult
{
    public bool Failed { get; set; }
    public string? ErrorMessage { get; set; }
    public GameDTO? Game { get; set; }
}

// DTO for submitting a guess
public class GuessWordDTO
{
    required public string Word { get; set; }
}

// DTO for returning the result of a guess submission
public class GuessResultDTO
{
    required public int GameId { get;  set; }
    required public string GuessWord { get;  set; }
    required public string[] Result { get;  set; }
    required public int AttemptsLeft { get;  set; }
    required public bool GameOver { get;  set; }
    public string? CorrectWord { get; set; }
}

// DTO for returning the result of submitting a guess
public class SubmitGuessResult
{
    public bool IsValid { get; set; }
    public bool Conflict { get; set; }
    public bool NotFound { get; set; }
    public bool Forbidden { get; set; }
    public bool WordNotRecognized { get; set; }
    public bool Failed { get; set; }
    public string? ErrorMessage { get; set; }
    public GuessResultDTO? GuessResult { get; set; }
}