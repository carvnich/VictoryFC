using Microsoft.EntityFrameworkCore;
using VictoryFC.Models;

namespace VictoryFC.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        DbSet<User> Users { get; set; }
    }
}
