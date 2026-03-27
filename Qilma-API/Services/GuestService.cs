using Microsoft.EntityFrameworkCore;
using Qilma_API.Constants;
using Qilma_API.Data;
using Qilma_API.DTOs;
using Qilma_API.Models;

namespace Qilma_API.Services;

public class GuestService : IGuestService
{

    private readonly AppDbContext _db;

    public GuestService(AppDbContext db)
    {
        _db = db;
    }

    // Returns the guest's statistics with the specified ID, or null if not found
    private async Task<StatisticModel?> FetchGuestStatisticsByIdAsync(int id)
    {
        var statistic = await _db.Statistics.FirstOrDefaultAsync(statistics => statistics.OwnerId == id && statistics.OwnerType == "guest");
        return statistic;
    }
    public async Task<CreateGuestResult> CreateGuestAsync()
    {
        try
        {
            var guestModel = new GuestModel();

            using var transaction = await _db.Database.BeginTransactionAsync();
            _db.Guests.Add(guestModel);
            await _db.SaveChangesAsync();

            var statisticModel = new StatisticModel
            {
                OwnerId = guestModel.GuestId,
                OwnerType = "guest",
                GamesPlayed = 0,
                GamesWon = 0
            };

            _db.Statistics.Add(statisticModel);
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return new CreateGuestResult
            {
                Guest = new GuestDTO
                {
                    GuestId = guestModel.GuestId
                }
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return new CreateGuestResult
            {
                Failed = true,
                ErrorMessage = HttpErrorMessages.INTERNAL_ERROR_MESSAGE
            };
        }
    }

    public async Task<GetStatistcsResult> GetGuestStatistcsAsync(int id)
    {
        try
        {
            var statistic = await FetchGuestStatisticsByIdAsync(id);
            if (statistic == null)
            {
                return new GetStatistcsResult
                {
                    NotFound = true,
                    ErrorMessage = HttpErrorMessages.STATISTIC_NOT_FOUND
                };
            }

            return new GetStatistcsResult
            {
                Statistic = new StatisticDTO
                {
                    StatisticId = statistic.StatisticId,
                    OwnerId = statistic.OwnerId,
                    OwnerType = statistic.OwnerType,
                    GamesPlayed = statistic.GamesPlayed,
                    GamesWon = statistic.GamesWon
                }
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return new GetStatistcsResult
            {
                Failed = true,
                ErrorMessage = HttpErrorMessages.INTERNAL_ERROR_MESSAGE
            };
        }
    }
}
