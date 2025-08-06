using BasketballLiveScore.Models;
using BasketballLiveScore.Models.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using SystemMatch = System.Text.RegularExpressions.Match; // Alias pour éviter le conflit

namespace BasketballLiveScore.Data
{
	/// <summary>
	/// Contexte de base de données pour l'application Basketball LiveScore
	/// Gère la configuration et l'accès aux données selon les patterns Entity Framework vus en cours
	/// </summary>
	public class BasketballDbContext : DbContext
	{
		// Constructeur par défaut nécessaire pour les migrations
		public BasketballDbContext()
		{
		}

		// Constructeur avec options pour l'injection de dépendances selon les patterns vus en cours
		public BasketballDbContext(DbContextOptions<BasketballDbContext> options)
			: base(options)
		{
		}

		// DbSets pour chaque entité du modèle selon les conventions Entity Framework vues en cours
		public DbSet<User> Users { get; set; }
		public DbSet<Team> Teams { get; set; }
		public DbSet<Player> Players { get; set; }
		public DbSet<Models.Match> Matches { get; set; } // Qualification explicite pour éviter le conflit
		public DbSet<MatchEvent> MatchEvents { get; set; }
		public DbSet<FoulEvent> FoulEvents { get; set; }
		public DbSet<SubstitutionEvent> SubstitutionEvents { get; set; }
		public DbSet<TimeoutEvent> TimeoutEvents { get; set; }
		public DbSet<GameAction> GameActions { get; set; }

		/// <summary>
		/// Configuration du modèle et des relations Entity Framework
		/// Résout les problèmes de cascade multiples selon les bonnes pratiques vues en cours
		/// Utilise l'API Fluent pour les configurations complexes comme vu dans les exemples de cours
		/// </summary>
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			// Configuration de la hiérarchie MatchEvent avec TPH (Table Per Hierarchy)
			// Pattern d'héritage vu en cours pour gérer les différents types d'événements
			modelBuilder.Entity<MatchEvent>()
				.HasDiscriminator<string>("Discriminator")
				.HasValue<MatchEvent>("MatchEvent")
				.HasValue<FoulEvent>("FoulEvent")
				.HasValue<SubstitutionEvent>("SubstitutionEvent")
				.HasValue<TimeoutEvent>("TimeoutEvent");

			// === RELATIONS AVEC CASCADE AUTORISÉE (pas de conflit) ===
			// Ces relations peuvent utiliser CASCADE car elles sont directes et uniques

			// Player -> Team : CASCADE OK car relation directe unique
			// Un joueur appartient à une seule équipe, suppression en cascade logique
			modelBuilder.Entity<Player>()
				.HasOne(p => p.Team)
				.WithMany(t => t.Players)
				.HasForeignKey(p => p.TeamId)
				.OnDelete(DeleteBehavior.Cascade);

			// GameAction -> Match : CASCADE OK car relation directe unique
			// Les actions de jeu appartiennent au match, suppression en cascade logique
			modelBuilder.Entity<GameAction>()
				.HasOne(ga => ga.Match)
				.WithMany(m => m.GameActions)
				.HasForeignKey(ga => ga.MatchId)
				.OnDelete(DeleteBehavior.Cascade);

			// MatchEvent -> Match : CASCADE OK car relation directe unique
			// Les événements appartiennent au match, suppression en cascade logique
			modelBuilder.Entity<MatchEvent>()
				.HasOne(me => me.Match)
				.WithMany(m => m.MatchEvents)
				.HasForeignKey(me => me.MatchId)
				.OnDelete(DeleteBehavior.Cascade);

			// === RELATIONS SANS CASCADE (éviter les chemins multiples) ===
			// Ces relations utilisent NoAction pour éviter les cycles de cascade selon les bonnes pratiques vues en cours

			// **Configuration Many-to-Many BIDIRECTIONNELLE : Match.LiveEncoders <-> User.EncodedMatches**
			// Configuration Many-to-Many avec navigation inverse selon les patterns EF Core vus en cours
			modelBuilder.Entity<Models.Match>()
				.HasMany(m => m.LiveEncoders)
				.WithMany(u => u.EncodedMatches) // Navigation inverse bidirectionnelle
				.UsingEntity<Dictionary<string, object>>(
					"MatchLiveEncoders", // Nom de la table de jonction
					j => j.HasOne<User>().WithMany().HasForeignKey("UserId"),
					j => j.HasOne<Models.Match>().WithMany().HasForeignKey("MatchId"),
					j =>
					{
						j.ToTable("MatchLiveEncoders"); // Nom explicite de la table de jonction
						j.HasKey("MatchId", "UserId"); // Clé composite
					}
				);

			// **Configuration relation bidirectionnelle User.PreparedMatches <-> Match.PreparedByUser**
			// Configuration selon les patterns de relations One-to-Many vus en cours
			modelBuilder.Entity<Models.Match>()
				.HasOne(m => m.PreparedByUser)
				.WithMany(u => u.PreparedMatches) // Navigation inverse pour la relation bidirectionnelle
				.HasForeignKey(m => m.PreparedByUserId)
				.OnDelete(DeleteBehavior.NoAction); // NoAction pour éviter les cycles de cascade

			// **Configuration des relations inverses Team -> Match**
			// Configuration selon les patterns de relations multiples vus en cours
			// Une équipe peut jouer plusieurs matchs à domicile et à l'extérieur

			// Relation Team -> HomeMatches (quand l'équipe joue à domicile)
			modelBuilder.Entity<Team>()
				.HasMany(t => t.HomeMatches)
				.WithOne(m => m.HomeTeam)
				.HasForeignKey(m => m.HomeTeamId)
				.OnDelete(DeleteBehavior.NoAction); // NoAction pour éviter les conflits avec AwayTeam

			// Relation Team -> AwayMatches (quand l'équipe joue à l'extérieur)
			modelBuilder.Entity<Team>()
				.HasMany(t => t.AwayMatches)
				.WithOne(m => m.AwayTeam)
				.HasForeignKey(m => m.AwayTeamId)
				.OnDelete(DeleteBehavior.NoAction); // NoAction pour éviter les conflits avec HomeTeam

			// MatchEvent -> Player : NO ACTION pour éviter cascade multiple
			// Un événement peut référencer un joueur, mais pas de cascade pour éviter les conflits
			modelBuilder.Entity<MatchEvent>()
				.HasOne(me => me.Player)
				.WithMany()
				.HasForeignKey(me => me.PlayerId)
				.OnDelete(DeleteBehavior.NoAction);

			// MatchEvent -> User (CreatedBy) : NO ACTION pour éviter cascade multiple
			// Un événement est créé par un utilisateur, mais pas de cascade
			modelBuilder.Entity<MatchEvent>()
				.HasOne<User>()
				.WithMany()
				.HasForeignKey(me => me.CreatedById)
				.OnDelete(DeleteBehavior.NoAction);

			// SubstitutionEvent -> PlayerIn : NO ACTION pour éviter cascade multiple
			// Référence au joueur qui entre dans la substitution
			modelBuilder.Entity<SubstitutionEvent>()
				.HasOne(se => se.PlayerIn)
				.WithMany()
				.HasForeignKey(se => se.PlayerInId)
				.OnDelete(DeleteBehavior.NoAction);

			// SubstitutionEvent -> PlayerOut : NO ACTION pour éviter cascade multiple
			// Référence au joueur qui sort dans la substitution
			modelBuilder.Entity<SubstitutionEvent>()
				.HasOne(se => se.PlayerOut)
				.WithMany()
				.HasForeignKey(se => se.PlayerOutId)
				.OnDelete(DeleteBehavior.NoAction);

			// TimeoutEvent -> Team : NO ACTION pour éviter cascade multiple
			// Un timeout appartient à une équipe mais pas de cascade
			modelBuilder.Entity<TimeoutEvent>()
				.HasOne(te => te.Team)
				.WithMany()
				.HasForeignKey(te => te.TeamId)
				.OnDelete(DeleteBehavior.NoAction);

			// === CONFIGURATION DES INDEX POUR LES PERFORMANCES ===
			// Configuration des index selon les bonnes pratiques vues en cours

			// Index unique sur le nom d'utilisateur pour l'authentification
			modelBuilder.Entity<User>()
				.HasIndex(u => u.Username)
				.IsUnique();

			// Index unique sur l'email pour éviter les doublons
			modelBuilder.Entity<User>()
				.HasIndex(u => u.Email)
				.IsUnique();

			// Index composite unique : un numéro de maillot unique par équipe
			// Respecte les règles métier du basketball
			modelBuilder.Entity<Player>()
				.HasIndex(p => new { p.TeamId, p.JerseyNumber })
				.IsUnique();

			// === CONFIGURATION DES VALEURS PAR DÉFAUT ===
			// Configuration des valeurs par défaut avec SQL Server selon les patterns vus en cours

			// Valeur par défaut pour la date de création des matchs
			modelBuilder.Entity<Models.Match>()
				.Property(m => m.CreatedAt)
				.HasDefaultValueSql("GETDATE()");

			// Valeur par défaut pour la date de création des équipes
			modelBuilder.Entity<Team>()
				.Property(t => t.CreatedAt)
				.HasDefaultValueSql("GETDATE()");

			// Valeur par défaut pour la date de création des utilisateurs
			modelBuilder.Entity<User>()
				.Property(u => u.CreatedAt)
				.HasDefaultValueSql("GETDATE()");

			// Valeur par défaut pour le statut actif des utilisateurs
			modelBuilder.Entity<User>()
				.Property(u => u.IsActive)
				.HasDefaultValue(true);

			// Valeur par défaut pour la date de création des événements
			modelBuilder.Entity<MatchEvent>()
				.Property(me => me.CreatedAt)
				.HasDefaultValueSql("GETDATE()");
		}

		/// <summary>
		/// Configuration de la connexion pour le mode développement
		/// Utilisé uniquement lors de la création des migrations selon les patterns vus en cours
		/// </summary>
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
	/// Factory pour la création du contexte lors des migrations Entity Framework
	/// Implémente IDesignTimeDbContextFactory selon les bonnes pratiques vues en cours
	/// Nécessaire pour les outils Entity Framework CLI et Package Manager Console
	/// </summary>
	public class BasketballDbContextFactory : IDesignTimeDbContextFactory<BasketballDbContext>
	{
		/// <summary>
		/// Crée une instance du contexte pour les migrations
		/// Utilise la configuration depuis appsettings.json selon les patterns vus en cours
		/// </summary>
		/// <param name="args">Arguments de ligne de commande (non utilisés)</param>
		/// <returns>Instance configurée du BasketballDbContext</returns>
		public BasketballDbContext CreateDbContext(string[] args)
		{
			// Configuration selon les patterns de configuration vus en cours
			var configuration = new ConfigurationBuilder()
				.AddJsonFile("appsettings.json")
				.Build();

			// Construction des options avec la chaîne de connexion
			var optionsBuilder = new DbContextOptionsBuilder<BasketballDbContext>();
			optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));

			// Retour de l'instance configurée
			return new BasketballDbContext(optionsBuilder.Options);
		}
	}
}