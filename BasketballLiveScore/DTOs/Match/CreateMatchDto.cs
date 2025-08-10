using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BasketballLiveScore.DTOs.Match
{
    /// <summary>
    /// DTO pour la création d'un nouveau match de basketball
    /// Implémente TOUTES les fonctionnalités demandées dans l'énoncé
    /// </summary>
    public class CreateMatchDto
    {
        // ========== INFORMATIONS GÉNÉRALES DU MATCH ==========

        [Required(ErrorMessage = "La date du match est obligatoire")]
        [DataType(DataType.DateTime)]
        public DateTime ScheduledDate { get; set; }

        [Required(ErrorMessage = "L'équipe domicile est obligatoire")]
        [Range(1, int.MaxValue, ErrorMessage = "L'ID de l'équipe domicile doit être valide")]
        public int HomeTeamId { get; set; }

        [Required(ErrorMessage = "L'équipe visiteur est obligatoire")]
        [Range(1, int.MaxValue, ErrorMessage = "L'ID de l'équipe visiteur doit être valide")]
        public int AwayTeamId { get; set; }

        // ========== CONFIGURATION DU MATCH (selon énoncé) ==========

        /// <summary>
        /// Nombre de quart-temps (4 par défaut en basketball)
        /// </summary>
        [Required(ErrorMessage = "Le nombre de quart-temps est obligatoire")]
        [Range(1, 10, ErrorMessage = "Le nombre de quart-temps doit être entre 1 et 10")]
        public int NumberOfQuarters { get; set; } = 4;

        /// <summary>
        /// Durée de chaque quart-temps en minutes (10 minutes par défaut)
        /// </summary>
        [Required(ErrorMessage = "La durée des quart-temps est obligatoire")]
        [Range(1, 60, ErrorMessage = "La durée doit être entre 1 et 60 minutes")]
        public int QuarterDurationMinutes { get; set; } = 10;

        /// <summary>
        /// Durée des timeouts en secondes (60 secondes par défaut)
        /// </summary>
        [Required(ErrorMessage = "La durée des timeouts est obligatoire")]
        [Range(10, 300, ErrorMessage = "La durée des timeouts doit être entre 10 et 300 secondes")]
        public int TimeoutDurationSeconds { get; set; } = 60;

        /// <summary>
        /// Nombre de timeouts par équipe (2 par mi-temps généralement)
        /// </summary>
        [Required(ErrorMessage = "Le nombre de timeouts est obligatoire")]
        [Range(0, 10, ErrorMessage = "Le nombre de timeouts doit être entre 0 et 10")]
        public int TimeoutsPerTeam { get; set; } = 2;

        // ========== COMPOSITION DES ÉQUIPES - 5 DE BASE ==========

        /// <summary>
        /// Les 5 joueurs de base de l'équipe domicile (IDs des joueurs)
        /// </summary>
        [Required(ErrorMessage = "Le 5 de base de l'équipe domicile est obligatoire")]
        [MinLength(5, ErrorMessage = "Il faut exactement 5 joueurs dans le 5 de base")]
        [MaxLength(5, ErrorMessage = "Il faut exactement 5 joueurs dans le 5 de base")]
        public List<int> HomeTeamStartingLineup { get; set; } = new List<int>();

        /// <summary>
        /// Les 5 joueurs de base de l'équipe visiteur (IDs des joueurs)
        /// </summary>
        [Required(ErrorMessage = "Le 5 de base de l'équipe visiteur est obligatoire")]
        [MinLength(5, ErrorMessage = "Il faut exactement 5 joueurs dans le 5 de base")]
        [MaxLength(5, ErrorMessage = "Il faut exactement 5 joueurs dans le 5 de base")]
        public List<int> AwayTeamStartingLineup { get; set; } = new List<int>();

        /// <summary>
        /// Liste complète des joueurs de l'équipe domicile disponibles pour le match
        /// </summary>
        public List<int> HomeTeamBenchPlayers { get; set; } = new List<int>();

        /// <summary>
        /// Liste complète des joueurs de l'équipe visiteur disponibles pour le match
        /// </summary>
        public List<int> AwayTeamBenchPlayers { get; set; } = new List<int>();

        // ========== PERSONNEL DU MATCH (selon énoncé) ==========

        /// <summary>
        /// ID de la personne qui a géré l'encodage de la préparation du match
        /// </summary>
        [Required(ErrorMessage = "L'ID du préparateur du match est obligatoire")]
        public int PreparedByUserId { get; set; }

        /// <summary>
        /// IDs des personnes qui vont s'occuper de l'encodage en temps réel
        /// </summary>
        [Required(ErrorMessage = "Au moins un encodeur live est obligatoire")]
        [MinLength(1, ErrorMessage = "Il faut au moins un encodeur pour le match")]
        public List<int> LiveEncoderIds { get; set; } = new List<int>();

        // ========== INFORMATIONS COMPLÉMENTAIRES ==========

        /// <summary>
        /// Lieu du match (gymnase, salle, etc.)
        /// </summary>
        [MaxLength(200, ErrorMessage = "Le lieu ne peut pas dépasser 200 caractères")]
        public string? Venue { get; set; }

        /// <summary>
        /// Nom de la compétition ou du championnat
        /// </summary>
        [MaxLength(100, ErrorMessage = "La compétition ne peut pas dépasser 100 caractères")]
        public string? Competition { get; set; }

        /// <summary>
        /// Catégorie (Senior, Junior, Cadet, etc.)
        /// </summary>
        [MaxLength(50, ErrorMessage = "La catégorie ne peut pas dépasser 50 caractères")]
        public string? Category { get; set; }

        /// <summary>
        /// Notes ou remarques sur le match
        /// </summary>
        [MaxLength(500, ErrorMessage = "Les notes ne peuvent pas dépasser 500 caractères")]
        public string? Notes { get; set; }

        // ========== MÉTHODES DE VALIDATION ==========

        /// <summary>
        /// Valide que les données du DTO sont cohérentes
        /// </summary>
        public ValidationResult Validate()
        {
            var errors = new List<string>();

            // Vérifier que les équipes sont différentes
            if (HomeTeamId == AwayTeamId)
            {
                errors.Add("L'équipe domicile et visiteur doivent être différentes");
            }

            // Vérifier qu'il n'y a pas de doublons dans les 5 de base
            var homeLineupDistinct = HomeTeamStartingLineup?.Distinct().Count() ?? 0;
            if (HomeTeamStartingLineup != null && homeLineupDistinct != HomeTeamStartingLineup.Count)
            {
                errors.Add("Le 5 de base domicile contient des doublons");
            }

            var awayLineupDistinct = AwayTeamStartingLineup?.Distinct().Count() ?? 0;
            if (AwayTeamStartingLineup != null && awayLineupDistinct != AwayTeamStartingLineup.Count)
            {
                errors.Add("Le 5 de base visiteur contient des doublons");
            }

            // Vérifier que la date n'est pas dans le passé
            if (ScheduledDate < DateTime.Now.AddHours(-12))
            {
                errors.Add("La date du match ne peut pas être trop dans le passé");
            }

            return errors.Count > 0
                ? new ValidationResult(false, string.Join(", ", errors))
                : new ValidationResult(true, "Validation réussie");
        }
    }

    /// <summary>
    /// Résultat de validation personnalisé
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string Message { get; set; }

        public ValidationResult(bool isValid, string message)
        {
            IsValid = isValid;
            Message = message;
        }
    }
}