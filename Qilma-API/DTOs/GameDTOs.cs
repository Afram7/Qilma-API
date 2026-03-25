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