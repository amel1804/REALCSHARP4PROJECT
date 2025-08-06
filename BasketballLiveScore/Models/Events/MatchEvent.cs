using System;
using BasketballLiveScore.Models.Enums;

namespace BasketballLiveScore.Models.Events
{
    /// <summary>
    /// Classe de base abstraite pour tous les événements d'un match
    /// Utilise le pattern Template Method vu en cours
    /// </summary>
    public abstract class MatchEvent
    {
        public int Id { get; set; }

        /// <summary>
        /// Référence au match concerné
        /// </summary>
        public int MatchId { get; set; }
        public virtual Match Match { get; set; }

        /// <summary>
        /// Quart-temps où l'événement s'est produit
        /// </summary>
        public int Quarter { get; set; }

        /// <summary>
        /// Temps dans le quart-temps (format MM:SS)
        /// </summary>
        public TimeSpan GameTime { get; set; }

        /// <summary>
        /// Identifiant du joueur concerné (si applicable)
        /// </summary>
        public int? PlayerId { get; set; }
        public virtual Player? Player { get; set; }

        /// <summary>
        /// Nombre de points associés à l'événement (si applicable)
        /// </summary>
        public int Points { get; set; }

        /// <summary>
        /// Type d'événement - Propriété abstraite à implémenter dans les classes dérivées
        /// </summary>
        public abstract MatchEventType EventType { get; }

        /// <summary>
        /// Description textuelle de l'événement
        /// Méthode abstraite selon le pattern Template Method vu en cours
        /// </summary>
        public abstract string GetDescription();

        /// <summary>
        /// Timestamp de création de l'événement
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Utilisateur qui a créé l'événement
        /// CORRECTION: Changement de CreatedByUserId vers CreatedById pour correspondre au DbContext
        /// </summary>
        public int CreatedById { get; set; }
        public virtual User CreatedBy { get; set; }
    }
}