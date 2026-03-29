using Microsoft.EntityFrameworkCore;
using Qilma_API.Constants;
using Qilma_API.Data;
using Qilma_API.Models;
using Qilma_API.Services.Interfaces;

namespace Qilma_API.Services;

public class StatisticService : IStatisticService
{
    private readonly AppDbContext _db;
    private readonly ILogger<StatisticService> _logger;
    public StatisticService(AppDbContext db, ILogger<StatisticService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task UpdateStatisticsForNewGameAsync(int ownerId, string ownerType)
    {
        try
        {
            var statistic = await _db.Statistics.FirstOrDefaultAsync(s => s.OwnerId == ownerId && s.OwnerType == ownerType);
            if (statistic != null)
            {
                statistic.GamesPlayed++;
                _db.Statistics.Update(statistic);
                await _db.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating statistics for new game");
        }
    }

    public async Task UpdateStatisticsForGameWinAsync(int ownerId, string ownerType)
    {
        try
        {
            var statistic = await _db.Statistics.FirstOrDefaultAsync(s => s.OwnerId == ownerId && s.OwnerType == ownerType);
            if (statistic != null)
            {
                statistic.GamesWon++;
                _db.Statistics.Update(statistic);
                await _db.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating statistics for game win");
        }
    }

    public async Task<StatisticModel?> FetchGuestStatisticsByIdAsync(int guestId)
    {
        try
        {
            var statistic = await _db.Statistics.FirstOrDefaultAsync(s => s.OwnerId == guestId && s.OwnerType == OwnerTypes.Guest);
            return statistic;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching guest statistics by ID");
            return null;
        }
    }

    public async Task<StatisticModel?> FetchUserStatisticsByIdAsync(int userId)
    {
        try
        {
            var statistic = await _db.Statistics.FirstOrDefaultAsync(s => s.OwnerId == userId && s.OwnerType == OwnerTypes.User);
            return statistic;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user statistics by ID");
            return null;
        }
    }

    public async Task CreateStatisticForNewGuestAsync(int guestId)
    {
        try
        {
            var statisticModel = new StatisticModel
            {
                OwnerId = guestId,
                OwnerType = OwnerTypes.Guest,
                GamesPlayed = 0,
                GamesWon = 0
            };

            _db.Statistics.Add(statisticModel);
            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating statistic for new guest");
        }
    }

    public async Task CreateStatisticForNewUserAsync(int userId)
    {
        try
        {
            var statisticModel = new StatisticModel
            {
                OwnerId = userId,
                OwnerType = OwnerTypes.User,
                GamesPlayed = 0,
                GamesWon = 0
            };

            _db.Statistics.Add(statisticModel);
            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating statistic for new user");
        }
    }

    public async Task DeleteStatisticForUserAsync(int userId)
    {
        try
        {
            var statistic = await _db.Statistics.FirstOrDefaultAsync(s => s.OwnerId == userId && s.OwnerType == OwnerTypes.User);
            if (statistic != null)
            {
                _db.Statistics.Remove(statistic);
                await _db.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting statistic for user");
        }
    }
}
