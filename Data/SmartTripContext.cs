using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Emit;
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
        public DbSet<UserSubmissions> UserSubmissions { get; set; }
        public DbSet<Packages> Packages { get; set; }
        public DbSet<PackageDestinations> PackageDestinations { get; set; }
        public DbSet<Bookings> Bookings { get; set; }
        public DbSet<Reviews> Reviews { get; set; }
        

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
                .HasMany(u => u.Reviews)
                .WithOne(r => r.User)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            

            modelBuilder.Entity<Users>()
                .HasMany(u => u.UserSubmissions)
                .WithOne(s => s.User)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // -----------------------------
            // DESTINATIONS
            // -----------------------------
            modelBuilder.Entity<Destinations>()
                .ToTable("destinations")
                .HasMany(d => d.PackageDestinations)
                .WithOne(pd => pd.Destination)
                .HasForeignKey(pd => pd.DestinationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Destinations>()
                .HasMany(d => d.Reviews)
                .WithOne(r => r.Destination)
                .HasForeignKey(r => r.DestinationId)
                .OnDelete(DeleteBehavior.Cascade);

           

            // -----------------------------
            // PACKAGES
            // -----------------------------
            modelBuilder.Entity<Packages>()
                .ToTable("packages")
                .HasMany(p => p.PackageDestinations)
                .WithOne(pd => pd.Package)
                .HasForeignKey(pd => pd.PackageId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Packages>()
                .HasMany(p => p.Bookings)
                .WithOne(b => b.Package)
                .HasForeignKey(b => b.PackageId)
                .OnDelete(DeleteBehavior.Cascade);

            // -----------------------------
            // PACKAGE DESTINATIONS
            // -----------------------------
            modelBuilder.Entity<PackageDestinations>()
                .ToTable("package_destinations");

            // -----------------------------
            // USER SUBMISSIONS
            // -----------------------------
            modelBuilder.Entity<UserSubmissions>()
                .ToTable("user_submissions");

            // -----------------------------
            // BOOKINGS
            // -----------------------------
            modelBuilder.Entity<Bookings>()
                .ToTable("bookings");

            // -----------------------------
            // REVIEWS
            // -----------------------------
            modelBuilder.Entity<Reviews>()
                .ToTable("reviews");

            

            
        }
    }
}
