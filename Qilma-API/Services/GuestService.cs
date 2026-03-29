using Qilma_API.Constants;
using Qilma_API.Data;
using Qilma_API.DTOs;
using Qilma_API.Models;
using Qilma_API.Services.Interfaces;

namespace Qilma_API.Services;

public class GuestService : IGuestService
{
    private readonly AppDbContext _db;
    private readonly IStatisticService _statisticService;
    private readonly ILogger<GuestService> _logger;

    public GuestService(AppDbContext db, IStatisticService statisticService, ILogger<GuestService> logger)
    {
        _db = db;
        _statisticService = statisticService;
        _logger = logger;
    }

    public async Task<CreateGuestResult> CreateGuestAsync()
    {
        try
        {
            var guestModel = new GuestModel();
            using var transaction = await _db.Database.BeginTransactionAsync();
            _db.Guests.Add(guestModel);
            await _db.SaveChangesAsync();
            await _statisticService.CreateStatisticForNewGuestAsync(guestModel.GuestId);
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
            _logger.LogError(ex, "Error creating guest");
            return new CreateGuestResult
            {
                Failed = true,
                ErrorMessage = ErrorMessages.INTERNAL_ERROR_MESSAGE
            };
        }
    }

    public async Task<GetStatisticsResult> GetGuestStatistcsAsync(int guestId)
    {
        try
        {
            var statistic = await _statisticService.FetchGuestStatisticsByIdAsync(guestId);
            if (statistic == null)
            {
                return new GetStatisticsResult
                {
                    NotFound = true,
                    ErrorMessage = ErrorMessages.STATISTIC_NOT_FOUND
                };
            }

            return new GetStatisticsResult
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
            _logger.LogError(ex, "Error fetching statistics for guestId: {GuestId}", guestId);
            return new GetStatisticsResult
            {
                Failed = true,
                ErrorMessage = ErrorMessages.INTERNAL_ERROR_MESSAGE
            };
        }
    }
}
