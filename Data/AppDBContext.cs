namespace SensoreAPPMVC.Data
{
    using SensoreAPPMVC.Models;
    using Microsoft.EntityFrameworkCore;
   

    public class AppDBContext : DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
    }
}