namespace SensoreAPPMVC.Data
{
    using SensoreAPPMVC.Models;
    using Microsoft.EntityFrameworkCore;

    public class AppDBContext : DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Patient> Patients { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasDiscriminator<string>("UserType")
                .HasValue<User>("User")
                .HasValue<Patient>("Patient");

            modelBuilder.Entity<User>()
                .Property<string>("UserType")
                .HasDefaultValue("User");
        }
    }
}