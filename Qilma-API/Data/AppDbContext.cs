using Microsoft.EntityFrameworkCore;
using Qilma_API.Models;

namespace Qilma_API.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{

    // Define DbSet properties for each model
    public DbSet<UserModel> Users => Set<UserModel>();
    public DbSet<GuestModel> Guests => Set<GuestModel>();
}