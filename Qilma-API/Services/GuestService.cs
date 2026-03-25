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
            var guest = new GuestModel();
            _db.Guests.Add(guest);
            await _db.SaveChangesAsync();

            return new CreateGuestResult
            {
                Guest = new GuestDTO
                {
                    GuestId = guest.GuestId
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
