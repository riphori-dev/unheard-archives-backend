using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tywynh.Application.Interfaces;
using Tywynh.Domain.Entities;
using Tywynh.Domain.Enums;

namespace Tywynh.Infrastructure.Persistence
{
    public class AppDbContext : DbContext, IUnitOfWork
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Confession> Confessions => Set<Confession>();
        public DbSet<Resonance> Resonances => Set<Resonance>();
        public DbSet<DailyEcho> DailyEchoes => Set<DailyEcho>();
        public DbSet<DailyEchoInteraction> DailyEchoInteractions => Set<DailyEchoInteraction>();

        // Database-scoped locks work across API instances and serialize read-modify-write counters.
        public async Task<T> ExecuteSerializedAsync<T>(string key, Func<Task<T>> action, CancellationToken ct = default)
        {
            await using var transaction = await Database.BeginTransactionAsync(ct);
            await Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock(hashtextextended({key}, 0))", ct);
            var result = await action();
            await transaction.CommitAsync(ct);
            return result;
        }

        public DbSet<SubmissionReceipt> SubmissionReceipts => Set<SubmissionReceipt>();
        public DbSet<PublicationRecord> Publications => Set<PublicationRecord>();
        public DbSet<ContentReport> ContentReports => Set<ContentReport>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
            modelBuilder.Entity<SubmissionReceipt>().HasIndex(r => r.IdempotencyHash).IsUnique();
            modelBuilder.Entity<SubmissionReceipt>().HasIndex(r => r.ConfessionId).IsUnique();
            modelBuilder.Entity<SubmissionReceipt>().HasOne<Confession>().WithMany().HasForeignKey(r => r.ConfessionId);
            modelBuilder.Entity<PublicationRecord>().HasOne<Confession>().WithMany().HasForeignKey(p => p.ConfessionId);
            modelBuilder.Entity<ContentReport>().HasOne<Confession>().WithMany().HasForeignKey(r => r.ConfessionId);
            base.OnModelCreating(modelBuilder);
        }
    }
}
