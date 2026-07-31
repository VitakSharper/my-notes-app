using Microsoft.EntityFrameworkCore;
using ProfileService.Models;

namespace ProfileService.Data;

public class ProfileDbContext(DbContextOptions<ProfileDbContext> options) : DbContext(options)
{
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
}
