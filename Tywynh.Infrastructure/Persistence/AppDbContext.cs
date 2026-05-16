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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresEnum<ConfessionCategory>();
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}
