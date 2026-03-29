namespace Qilma_API.DTOs;

// DTO for returning user's statistics
public class StatisticDTO
{
    required public int StatisticId { get; set; }
    required public int OwnerId { get; set; }
    required public string OwnerType { get; set; }
    required public int GamesPlayed { get; set; }
    required public int GamesWon { get; set; }
}

public class GetStatisticsResult
{
    public bool NotFound { get; set; }
    public bool Failed { get; set; }
    public string? ErrorMessage { get; set; }
    public StatisticDTO? Statistic { get; set;}
}