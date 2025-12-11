namespace SensoreAPPMVC.Data
{
    using SensoreAPPMVC.Models;
    using SensoreAPPMVC.Utilities;
    using Microsoft.EntityFrameworkCore;

    public class AppDBContext : DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<PressureMap> PressureMaps { get; set; }
        public DbSet<Comment> Comments { get; set; }

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

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.PressureMap)
                .WithMany()
                .HasForeignKey(c => c.PressureMapId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}