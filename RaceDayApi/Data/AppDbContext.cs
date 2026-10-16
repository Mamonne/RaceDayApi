using Microsoft.EntityFrameworkCore;
using RaceDayApi.Models;

namespace RaceDayApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<RaceRoute> Routes { get; set; }
        public DbSet<Weather> Weather { get; set; }
        public DbSet<Enrolment> Enrolments { get; set; }
        public DbSet<Result> Results { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Table names and keys
            modelBuilder.Entity<Weather>().ToTable("WeatherInfo");
            modelBuilder.Entity<RaceRoute>().HasKey(r => r.RouteId);

            // Email must be unique
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();

            // Check constraints (same as the SQL script)
            modelBuilder.Entity<User>().ToTable(t =>
                t.HasCheckConstraint("CK_Users_Role", "Role IN ('Organiser', 'Participant')"));

            modelBuilder.Entity<Event>().ToTable(t =>
                t.HasCheckConstraint("CK_Events_EventType", "EventType IN ('Running', 'Walking', 'Cycling')"));

            modelBuilder.Entity<Enrolment>().ToTable(t =>
                t.HasCheckConstraint("CK_Enrolments_PaymentStatus", "PaymentStatus IN ('Pending', 'Paid', 'Cancelled')"));

            modelBuilder.Entity<Result>().ToTable(t =>
                t.HasCheckConstraint("CK_Results_Status", "Status IN ('Finished', 'DNF', 'DSQ')"));

            // Default values (same as the SQL script)
            modelBuilder.Entity<User>().Property(u => u.CreatedDate).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<Event>().Property(e => e.CreatedDate).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<Enrolment>().Property(e => e.RegistrationDate).HasDefaultValueSql("GETDATE()");

            // Event -> Organiser (User)
            modelBuilder.Entity<Event>()
                .HasOne<User>().WithMany()
                .HasForeignKey(e => e.OrganiserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Category, Route, Weather belong to an Event
            modelBuilder.Entity<Category>()
                .HasOne<Event>().WithMany()
                .HasForeignKey(c => c.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RaceRoute>()
                .HasOne<Event>().WithMany()
                .HasForeignKey(r => r.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Weather>()
                .HasOne<Event>().WithMany()
                .HasForeignKey(w => w.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            // Enrolment links a Participant, an Event and a Category
            modelBuilder.Entity<Enrolment>()
                .HasOne<User>().WithMany()
                .HasForeignKey(e => e.ParticipantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Enrolment>()
                .HasOne<Event>().WithMany()
                .HasForeignKey(e => e.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Enrolment>()
                .HasOne<Category>().WithMany()
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Result is 1:1 with Enrolment
            modelBuilder.Entity<Result>()
                .HasOne<Enrolment>().WithOne()
                .HasForeignKey<Result>(r => r.EnrolmentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}