using System;
using System.ComponentModel.DataAnnotations;

namespace BasketballLiveScore.Models
{
    /// <summary>
    /// Repr�sente la participation d'un joueur dans un match
    /// G�re les 5 de base et les rempla�ants selon l'�nonc�
    /// </summary>
    public class MatchLineup
    {
        public int Id { get; set; }

        [Required]
        public int MatchId { get; set; }

        [Required]
        public int TeamId { get; set; }

        [Required]
        public int PlayerId { get; set; }

        /// <summary>
        /// Indique si le joueur fait partie du 5 de base
        /// Requis selon l'�nonc� : "des 5 de base (les 5 joueurs de chaque �quipe qui commencent le match)"
        /// </summary>
        [Required]
        public bool IsStarter { get; set; } = false;

        /// <summary>
        /// Position du joueur (1-5 pour les titulaires du 5 de base, 6+ pour les rempla�ants)
        /// </summary>
        public int Position { get; set; }

        /// <summary>
        /// Indique si le joueur est actuellement sur le terrain
        /// </summary>
        public bool IsOnCourt { get; set; } = false;

        /// <summary>
        /// Nombre de fautes personnelles (P0, P1, P2, P3)
        /// Maximum 5 fautes = disqualification
        /// </summary>
        public int PersonalFouls { get; set; } = 0;

        /// <summary>
        /// Type de la derni�re faute commise
        /// </summary>
        [MaxLength(10)]
        public string? LastFoulType { get; set; }

        /// <summary>
        /// Points marqu�s par le joueur dans ce match
        /// </summary>
        public int Points { get; set; } = 0;

        /// <summary>
        /// Paniers � 1 point (lancers francs) marqu�s
        /// </summary>
        public int FreeThrowsMade { get; set; } = 0;

        /// <summary>
        /// Paniers � 2 points marqu�s
        /// </summary>
        public int TwoPointersMade { get; set; } = 0;

        /// <summary>
        /// Paniers � 3 points marqu�s
        /// </summary>
        public int ThreePointersMade { get; set; } = 0;

        /// <summary>
        /// Temps de jeu total en secondes
        /// </summary>
        public int PlayingTimeSeconds { get; set; } = 0;

        /// <summary>
        /// Moment o� le joueur est entr� sur le terrain pour la derni�re fois
        /// </summary>
        public DateTime? LastEnteredCourt { get; set; }

        /// <summary>
        /// Quart-temps o� le joueur est entr� pour la premi�re fois
        /// </summary>
        public int? FirstQuarterPlayed { get; set; }

        /// <summary>
        /// Indique si le joueur est disqualifi� (5 fautes ou expulsion)
        /// </summary>
        public bool IsDisqualified { get; set; } = false;

        /// <summary>
        /// Raison de la disqualification si applicable
        /// </summary>
        [MaxLength(100)]
        public string? DisqualificationReason { get; set; }

        // Navigation properties
        public virtual Match? Match { get; set; }
        public virtual Team? Team { get; set; }
        public virtual Player? Player { get; set; }

        /// <summary>
        /// V�rifie si le joueur peut entrer sur le terrain
        /// </summary>
        public bool CanEnterCourt()
        {
            return !IsDisqualified &&
                   !IsOnCourt &&
                   PersonalFouls < 5; // Maximum 5 fautes personnelles
        }

        /// <summary>
        /// V�rifie si le joueur doit �tre disqualifi�
        /// </summary>
        public void CheckDisqualification()
        {
            if (PersonalFouls >= 5)
            {
                IsDisqualified = true;
                DisqualificationReason = "5 fautes personnelles";
                IsOnCourt = false;
            }
        }

        /// <summary>
        /// Ajoute une faute et v�rifie la disqualification
        /// </summary>
        public void AddFoul(string foulType)
        {
            PersonalFouls++;
            LastFoulType = foulType;
            CheckDisqualification();
        }

        /// <summary>
        /// Enregistre un panier marqu�
        /// </summary>
        public void AddBasket(int points)
        {
            Points += points;
            switch (points)
            {
                case 1:
                    FreeThrowsMade++;
                    break;
                case 2:
                    TwoPointersMade++;
                    break;
                case 3:
                    ThreePointersMade++;
                    break;
            }
        }

        /// <summary>
        /// Met � jour le temps de jeu quand le joueur sort
        /// </summary>
        public void UpdatePlayingTime()
        {
            if (LastEnteredCourt.HasValue && IsOnCourt)
            {
                var timePlayed = (int)(DateTime.UtcNow - LastEnteredCourt.Value).TotalSeconds;
                PlayingTimeSeconds += timePlayed;
                LastEnteredCourt = null;
            }
        }

        /// <summary>
        /// Fait entrer le joueur sur le terrain
        /// </summary>
        public void EnterCourt(int currentQuarter)
        {
            if (!CanEnterCourt())
                throw new InvalidOperationException("Le joueur ne peut pas entrer sur le terrain");

            IsOnCourt = true;
            LastEnteredCourt = DateTime.UtcNow;

            if (!FirstQuarterPlayed.HasValue)
                FirstQuarterPlayed = currentQuarter;
        }

        /// <summary>
        /// Fait sortir le joueur du terrain
        /// </summary>
        public void ExitCourt()
        {
            UpdatePlayingTime();
            IsOnCourt = false;
        }

        /// <summary>
        /// Obtient le temps de jeu format� (MM:SS)
        /// </summary>
        public string GetFormattedPlayingTime()
        {
            var minutes = PlayingTimeSeconds / 60;
            var seconds = PlayingTimeSeconds % 60;
            return $"{minutes:D2}:{seconds:D2}";
        }
    }
}