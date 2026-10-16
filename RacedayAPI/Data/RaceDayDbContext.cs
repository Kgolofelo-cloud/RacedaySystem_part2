using Microsoft.EntityFrameworkCore;

using RacedayAPI.Models;
using RacedayAPI.Models;

namespace RacedayAPI.Data

{

    public class RaceDayDbContext : DbContext

    {

        public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options) : base(options) { }

        public DbSet<AppUser> AppUsers { get; set; }

        public DbSet<EventLocation> EventLocations { get; set; }

        public DbSet<Event> Events { get; set; }

        public DbSet<RaceCategory> RaceCategories { get; set; }

        public DbSet<Registration> Registrations { get; set; }

        public DbSet<RaceResult> RaceResults { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)

        {

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AppUser>().HasIndex(u => u.Email).IsUnique();

            modelBuilder.Entity<RaceCategory>()

                .Property(c => c.EntryFee)

                .HasDefaultValue(0.00m);

            modelBuilder.Entity<Registration>()

                .Property(r => r.RegisteredAt)

                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<Registration>()

                .HasOne(r => r.RaceResult)

                .WithOne(rr => rr.Registration)

                .HasForeignKey<RaceResult>(rr => rr.RegistrationID);

            modelBuilder.Entity<RaceCategory>()

                .HasOne(c => c.Event)

                .WithMany(e => e.RaceCategories)

                .HasForeignKey(c => c.EventID)

                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Event>()

                .HasOne(e => e.Organiser)

                .WithMany(u => u.OrganisedEvents)

                .HasForeignKey(e => e.OrganiserID)

                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Event>()

                .HasOne(e => e.Location)

                .WithMany(l => l.Events)

                .HasForeignKey(e => e.LocationID)

                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Registration>()

                .HasOne(r => r.User)

                .WithMany(u => u.Registrations)

                .HasForeignKey(r => r.UserID)

                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Registration>()

                .HasOne(r => r.RaceCategory)

                .WithMany(c => c.Registrations)

                .HasForeignKey(r => r.CategoryID)

                .OnDelete(DeleteBehavior.Restrict);

        }

    }

}
