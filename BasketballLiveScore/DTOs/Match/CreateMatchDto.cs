using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BasketballLiveScore.DTOs.Match
{
    /// <summary>
    /// DTO pour la cr�ation d'un nouveau match de basketball
    /// Impl�mente TOUTES les fonctionnalit�s demand�es dans l'�nonc�
    /// </summary>
    public class CreateMatchDto
    {
        // ========== INFORMATIONS G�N�RALES DU MATCH ==========

        [Required(ErrorMessage = "La date du match est obligatoire")]
        [DataType(DataType.DateTime)]
        public DateTime ScheduledDate { get; set; }

        [Required(ErrorMessage = "L'�quipe domicile est obligatoire")]
        [Range(1, int.MaxValue, ErrorMessage = "L'ID de l'�quipe domicile doit �tre valide")]
        public int HomeTeamId { get; set; }

        [Required(ErrorMessage = "L'�quipe visiteur est obligatoire")]
        [Range(1, int.MaxValue, ErrorMessage = "L'ID de l'�quipe visiteur doit �tre valide")]
        public int AwayTeamId { get; set; }

        // ========== CONFIGURATION DU MATCH (selon �nonc�) ==========

        /// <summary>
        /// Nombre de quart-temps (4 par d�faut en basketball)
        /// </summary>
        [Required(ErrorMessage = "Le nombre de quart-temps est obligatoire")]
        [Range(1, 10, ErrorMessage = "Le nombre de quart-temps doit �tre entre 1 et 10")]
        public int NumberOfQuarters { get; set; } = 4;

        /// <summary>
        /// Dur�e de chaque quart-temps en minutes (10 minutes par d�faut)
        /// </summary>
        [Required(ErrorMessage = "La dur�e des quart-temps est obligatoire")]
        [Range(1, 60, ErrorMessage = "La dur�e doit �tre entre 1 et 60 minutes")]
        public int QuarterDurationMinutes { get; set; } = 10;

        /// <summary>
        /// Dur�e des timeouts en secondes (60 secondes par d�faut)
        /// </summary>
        [Required(ErrorMessage = "La dur�e des timeouts est obligatoire")]
        [Range(10, 300, ErrorMessage = "La dur�e des timeouts doit �tre entre 10 et 300 secondes")]
        public int TimeoutDurationSeconds { get; set; } = 60;

        /// <summary>
        /// Nombre de timeouts par �quipe (2 par mi-temps g�n�ralement)
        /// </summary>
        [Required(ErrorMessage = "Le nombre de timeouts est obligatoire")]
        [Range(0, 10, ErrorMessage = "Le nombre de timeouts doit �tre entre 0 et 10")]
        public int TimeoutsPerTeam { get; set; } = 2;

        // ========== COMPOSITION DES �QUIPES - 5 DE BASE ==========

        /// <summary>
        /// Les 5 joueurs de base de l'�quipe domicile (IDs des joueurs)
        /// </summary>
        [Required(ErrorMessage = "Le 5 de base de l'�quipe domicile est obligatoire")]
        [MinLength(5, ErrorMessage = "Il faut exactement 5 joueurs dans le 5 de base")]
        [MaxLength(5, ErrorMessage = "Il faut exactement 5 joueurs dans le 5 de base")]
        public List<int> HomeTeamStartingLineup { get; set; } = new List<int>();

        /// <summary>
        /// Les 5 joueurs de base de l'�quipe visiteur (IDs des joueurs)
        /// </summary>
        [Required(ErrorMessage = "Le 5 de base de l'�quipe visiteur est obligatoire")]
        [MinLength(5, ErrorMessage = "Il faut exactement 5 joueurs dans le 5 de base")]
        [MaxLength(5, ErrorMessage = "Il faut exactement 5 joueurs dans le 5 de base")]
        public List<int> AwayTeamStartingLineup { get; set; } = new List<int>();

        /// <summary>
        /// Liste compl�te des joueurs de l'�quipe domicile disponibles pour le match
        /// </summary>
        public List<int> HomeTeamBenchPlayers { get; set; } = new List<int>();

        /// <summary>
        /// Liste compl�te des joueurs de l'�quipe visiteur disponibles pour le match
        /// </summary>
        public List<int> AwayTeamBenchPlayers { get; set; } = new List<int>();

        // ========== PERSONNEL DU MATCH (selon �nonc�) ==========

        /// <summary>
        /// ID de la personne qui a g�r� l'encodage de la pr�paration du match
        /// </summary>
        [Required(ErrorMessage = "L'ID du pr�parateur du match est obligatoire")]
        public int PreparedByUserId { get; set; }

        /// <summary>
        /// IDs des personnes qui vont s'occuper de l'encodage en temps r�el
        /// </summary>
        [Required(ErrorMessage = "Au moins un encodeur live est obligatoire")]
        [MinLength(1, ErrorMessage = "Il faut au moins un encodeur pour le match")]
        public List<int> LiveEncoderIds { get; set; } = new List<int>();

        // ========== INFORMATIONS COMPL�MENTAIRES ==========

        /// <summary>
        /// Lieu du match (gymnase, salle, etc.)
        /// </summary>
        [MaxLength(200, ErrorMessage = "Le lieu ne peut pas d�passer 200 caract�res")]
        public string? Venue { get; set; }

        /// <summary>
        /// Nom de la comp�tition ou du championnat
        /// </summary>
        [MaxLength(100, ErrorMessage = "La comp�tition ne peut pas d�passer 100 caract�res")]
        public string? Competition { get; set; }

        /// <summary>
        /// Cat�gorie (Senior, Junior, Cadet, etc.)
        /// </summary>
        [MaxLength(50, ErrorMessage = "La cat�gorie ne peut pas d�passer 50 caract�res")]
        public string? Category { get; set; }

        /// <summary>
        /// Notes ou remarques sur le match
        /// </summary>
        [MaxLength(500, ErrorMessage = "Les notes ne peuvent pas d�passer 500 caract�res")]
        public string? Notes { get; set; }

        // ========== M�THODES DE VALIDATION ==========

        /// <summary>
        /// Valide que les donn�es du DTO sont coh�rentes
        /// </summary>
        public ValidationResult Validate()
        {
            var errors = new List<string>();

            // V�rifier que les �quipes sont diff�rentes
            if (HomeTeamId == AwayTeamId)
            {
                errors.Add("L'�quipe domicile et visiteur doivent �tre diff�rentes");
            }

            // V�rifier qu'il n'y a pas de doublons dans les 5 de base
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

            // V�rifier que la date n'est pas dans le pass�
            if (ScheduledDate < DateTime.Now.AddHours(-12))
            {
                errors.Add("La date du match ne peut pas �tre trop dans le pass�");
            }

            return errors.Count > 0
                ? new ValidationResult(false, string.Join(", ", errors))
                : new ValidationResult(true, "Validation r�ussie");
        }
    }

    /// <summary>
    /// R�sultat de validation personnalis�
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