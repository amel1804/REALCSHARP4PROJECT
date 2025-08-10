using BasketballLiveScore.Models;
using BasketballLiveScore.Models.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.Collections.Generic;

namespace BasketballLiveScore.Data
{
    /// <summary>
    /// Contexte de base de données pour l'application Basketball LiveScore
    /// Gère la configuration et l'accès aux données selon les patterns Entity Framework
    /// </summary>
    public class BasketballDbContext : DbContext
    {
        // Constructeur par défaut nécessaire pour les migrations
        public BasketballDbContext()
        {
        }

        // Constructeur avec options pour l'injection de dépendances
        public BasketballDbContext(DbContextOptions<BasketballDbContext> options)
            : base(options)
        {
        }

        // DbSets pour chaque entité du modèle
        public DbSet<User> Users { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<Models.Match> Matches { get; set; } // Qualification explicite pour éviter le conflit
        public DbSet<MatchEvent> MatchEvents { get; set; }
        public DbSet<FoulEvent> FoulEvents { get; set; }
        public DbSet<SubstitutionEvent> SubstitutionEvents { get; set; }
        public DbSet<TimeoutEvent> TimeoutEvents { get; set; }
        public DbSet<GameAction> GameActions { get; set; }

        // Ajout du DbSet pour MatchLineup
        public DbSet<MatchLineup> MatchLineups { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- Héritage TPH (Table Per Hierarchy) pour MatchEvent ---
            modelBuilder.Entity<MatchEvent>()
                .HasDiscriminator<string>("Discriminator")
                .HasValue<MatchEvent>("MatchEvent")
                .HasValue<FoulEvent>("FoulEvent")
                .HasValue<SubstitutionEvent>("SubstitutionEvent")
                .HasValue<TimeoutEvent>("TimeoutEvent");

            // --- Relations avec cascade autorisée ---

            // Player -> Team (Cascade)
            modelBuilder.Entity<Player>()
                .HasOne(p => p.Team)
                .WithMany(t => t.Players)
                .HasForeignKey(p => p.TeamId)
                .OnDelete(DeleteBehavior.Cascade);

            // GameAction -> Match (Cascade)
            modelBuilder.Entity<GameAction>()
                .HasOne(ga => ga.Match)
                .WithMany(m => m.GameActions)
                .HasForeignKey(ga => ga.MatchId)
                .OnDelete(DeleteBehavior.Cascade);

            // MatchEvent -> Match (Cascade)
            modelBuilder.Entity<MatchEvent>()
                .HasOne(me => me.Match)
                .WithMany(m => m.MatchEvents)
                .HasForeignKey(me => me.MatchId)
                .OnDelete(DeleteBehavior.Cascade);

            // --- Relations sans cascade (NoAction) pour éviter conflits ---

            // Many-to-Many Match.LiveEncoders <-> User.EncodedMatches
            modelBuilder.Entity<Models.Match>()
                .HasMany(m => m.LiveEncoders)
                .WithMany(u => u.EncodedMatches)
                .UsingEntity<Dictionary<string, object>>(
                    "MatchLiveEncoders",
                    j => j.HasOne<User>().WithMany().HasForeignKey("UserId"),
                    j => j.HasOne<Models.Match>().WithMany().HasForeignKey("MatchId"),
                    j =>
                    {
                        j.ToTable("MatchLiveEncoders");
                        j.HasKey("MatchId", "UserId");
                    });

            // User.PreparedMatches <-> Match.PreparedByUser (One-to-Many)
            modelBuilder.Entity<Models.Match>()
                .HasOne(m => m.PreparedByUser)
                .WithMany(u => u.PreparedMatches)
                .HasForeignKey(m => m.PreparedByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Team -> HomeMatches (NoAction)
            modelBuilder.Entity<Team>()
                .HasMany(t => t.HomeMatches)
                .WithOne(m => m.HomeTeam)
                .HasForeignKey(m => m.HomeTeamId)
                .OnDelete(DeleteBehavior.NoAction);

            // Team -> AwayMatches (NoAction)
            modelBuilder.Entity<Team>()
                .HasMany(t => t.AwayMatches)
                .WithOne(m => m.AwayTeam)
                .HasForeignKey(m => m.AwayTeamId)
                .OnDelete(DeleteBehavior.NoAction);

            // MatchEvent -> Player (NoAction)
            modelBuilder.Entity<MatchEvent>()
                .HasOne(me => me.Player)
                .WithMany()
                .HasForeignKey(me => me.PlayerId)
                .OnDelete(DeleteBehavior.NoAction);

            // SubstitutionEvent -> PlayerIn (NoAction)
            modelBuilder.Entity<SubstitutionEvent>()
                .HasOne(se => se.PlayerIn)
                .WithMany()
                .HasForeignKey(se => se.PlayerInId)
                .OnDelete(DeleteBehavior.NoAction);

            // SubstitutionEvent -> PlayerOut (NoAction)
            modelBuilder.Entity<SubstitutionEvent>()
                .HasOne(se => se.PlayerOut)
                .WithMany()
                .HasForeignKey(se => se.PlayerOutId)
                .OnDelete(DeleteBehavior.NoAction);

            // TimeoutEvent -> Team (NoAction)
            modelBuilder.Entity<TimeoutEvent>()
                .HasOne(te => te.Team)
                .WithMany()
                .HasForeignKey(te => te.TeamId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<MatchLineup>(entity =>
            {
                entity.HasKey(ml => ml.Id);

                entity.HasOne(ml => ml.Match)
                      .WithMany(m => m.MatchLineups)
                      .HasForeignKey(ml => ml.MatchId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ml => ml.Player)
                      .WithMany(p => p.MatchLineups)
                      .HasForeignKey(ml => ml.PlayerId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ml => ml.Team)
                      .WithMany(t => t.MatchLineups)
                      .HasForeignKey(ml => ml.TeamId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.NoAction);
            });

            // --- Indexes pour la performance ---

            // User.Username unique
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // User.Email unique
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Player : numéro de maillot unique par équipe
            modelBuilder.Entity<Player>()
                .HasIndex(p => new { p.TeamId, p.JerseyNumber })
                .IsUnique();

            // Ajout index sur Foreign Keys pour optimiser les jointures (optionnel)
            modelBuilder.Entity<Player>()
                .HasIndex(p => p.TeamId);

            modelBuilder.Entity<GameAction>()
                .HasIndex(ga => ga.MatchId);

            modelBuilder.Entity<MatchEvent>()
                .HasIndex(me => me.MatchId);

            modelBuilder.Entity<MatchEvent>()
                .HasIndex(me => me.PlayerId);

            modelBuilder.Entity<MatchEvent>()
                .HasIndex(me => me.CreatedById);

            modelBuilder.Entity<SubstitutionEvent>()
                .HasIndex(se => se.PlayerInId);

            modelBuilder.Entity<SubstitutionEvent>()
                .HasIndex(se => se.PlayerOutId);

            modelBuilder.Entity<TimeoutEvent>()
                .HasIndex(te => te.TeamId);

            // --- Contraintes supplémentaires (exemples) ---

            modelBuilder.Entity<User>()
                .Property(u => u.Username)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<User>()
                .Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Team>()
                .Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Player>()
                .Property(p => p.FirstName)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<Player>()
                .Property(p => p.LastName)
                .IsRequired()
                .HasMaxLength(50);

            // --- Valeurs par défaut ---

            modelBuilder.Entity<Models.Match>()
                .Property(m => m.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<Team>()
                .Property(t => t.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<User>()
                .Property(u => u.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<User>()
                .Property(u => u.IsActive)
                .HasDefaultValue(true);

            modelBuilder.Entity<MatchEvent>()
                .Property(me => me.CreatedAt)
                .HasDefaultValueSql("GETDATE()");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(
                    "Server=(localdb)\\MSSQLLocalDB;Database=BasketballLiveScoreDb;Trusted_Connection=True;");
            }
        }
    }

    /// <summary>
    /// Classe pour la création du contexte à la conception (migrations)
    /// </summary>
    public class BasketballDbContextFactory : IDesignTimeDbContextFactory<BasketballDbContext>
    {
        public BasketballDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<BasketballDbContext>();
            optionsBuilder.UseSqlServer(
                "Server=(localdb)\\MSSQLLocalDB;Database=BasketballLiveScoreDb;Trusted_Connection=True;");

            return new BasketballDbContext(optionsBuilder.Options);
        }
    }
}
