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
}
