using Microsoft.EntityFrameworkCore;
using TripVista.Models.Entities;

namespace TripVista.Data
{
    public class SmartTripContext : DbContext
    {
        public SmartTripContext(DbContextOptions<SmartTripContext> options)
            : base(options)
        {
        }

        // ============================
        //       DB SETS
        // ============================
        public DbSet<Users> Users { get; set; }
        public DbSet<Destinations> Destinations { get; set; }
        public DbSet<Packages> Packages { get; set; }
        public DbSet<PackageDestinations> PackageDestinations { get; set; }
        public DbSet<Bookings> Bookings { get; set; }
        public DbSet<BookingTravelers> BookingTravelers { get; set; }
        public DbSet<OtpVerification> OtpVerifications { get; set; }
        public DbSet<Polls> Polls { get; set; }
        public DbSet<PollOptions> PollOptions { get; set; }
        public DbSet<PollVotes> PollVotes { get; set; }

        // ============================
        //     MODEL CONFIGURATION
        // ============================
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // -----------------------------
            // USERS
            // -----------------------------
            modelBuilder.Entity<Users>()
                .ToTable("users")
                .HasMany(u => u.Bookings)
                .WithOne(b => b.User)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Users>()
                .HasMany(u => u.SubmittedDestinations)
                .WithOne(d => d.SubmittedByUser)
                .HasForeignKey(d => d.SubmittedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // -----------------------------
            // DESTINATIONS
            // -----------------------------
            modelBuilder.Entity<Destinations>()
                .ToTable("destinations");

            modelBuilder.Entity<Destinations>()
                .HasOne(d => d.ReviewedByAdmin)
                .WithMany()
                .HasForeignKey(d => d.ReviewedByAdminId)
                .OnDelete(DeleteBehavior.Restrict);

            // -----------------------------
            // PACKAGES
            // -----------------------------
            modelBuilder.Entity<Packages>()
                .ToTable("packages")
                .HasMany(p => p.Bookings)
                .WithOne(b => b.Package)
                .HasForeignKey(b => b.PackageId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Packages>()
                .HasOne(p => p.CreatedByAdmin)
                .WithMany()
                .HasForeignKey(p => p.CreatedByAdminId)
                .OnDelete(DeleteBehavior.Restrict);

            // -----------------------------
            // PACKAGE DESTINATIONS
            // -----------------------------
            modelBuilder.Entity<PackageDestinations>()
                .ToTable("package_destinations");

            modelBuilder.Entity<PackageDestinations>()
                .HasOne(pd => pd.Package)
                .WithMany(p => p.PackageDestinations)
                .HasForeignKey(pd => pd.PackageId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PackageDestinations>()
                .HasOne(pd => pd.Destination)
                .WithMany(d => d.PackageDestinations)
                .HasForeignKey(pd => pd.DestinationId)
                .OnDelete(DeleteBehavior.Restrict);

            // -----------------------------
            // BOOKINGS
            // -----------------------------
            modelBuilder.Entity<Bookings>()
                .ToTable("bookings")
                .HasMany(b => b.BookingTravelers)
                .WithOne(t => t.Booking)
                .HasForeignKey(t => t.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            // -----------------------------
            // BOOKING TRAVELERS
            // -----------------------------
            modelBuilder.Entity<BookingTravelers>()
                .ToTable("booking_travelers");

            modelBuilder.Entity<BookingTravelers>()
                .HasIndex(t => new { t.BookingId, t.CNICOrPassportNumber })
                .IsUnique();

            // -----------------------------
            // OTP VERIFICATIONS
            // -----------------------------
            modelBuilder.Entity<OtpVerification>()
                .ToTable("otp_verifications");

            // -----------------------------
            // POLLS
            // -----------------------------
            modelBuilder.Entity<Polls>()
                .ToTable("polls")
                .HasMany(p => p.Options)
                .WithOne(o => o.Poll)
                .HasForeignKey(o => o.PollId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Polls>()
                .HasMany(p => p.Votes)
                .WithOne(v => v.Poll)
                .HasForeignKey(v => v.PollId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Polls>()
                .HasOne(p => p.CreatedByAdmin)
                .WithMany()
                .HasForeignKey(p => p.CreatedByAdminId)
                .OnDelete(DeleteBehavior.Restrict);

            // -----------------------------
            // POLL OPTIONS
            // -----------------------------
            modelBuilder.Entity<PollOptions>()
                .ToTable("poll_options")
                .HasMany(o => o.Votes)
                .WithOne(v => v.PollOption)
                .HasForeignKey(v => v.PollOptionId)
                .OnDelete(DeleteBehavior.Restrict);

            // -----------------------------
            // POLL VOTES
            // -----------------------------
            modelBuilder.Entity<PollVotes>()
                .ToTable("poll_votes");

            modelBuilder.Entity<PollVotes>()
                .HasOne(v => v.User)
                .WithMany()
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PollVotes>()
                .HasIndex(v => new { v.PollId, v.UserId })
                .IsUnique();
        }
    }
}
