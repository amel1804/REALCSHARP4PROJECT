using System;
using BasketballLiveScore.Models.Enums;

namespace BasketballLiveScore.Models.Events
{
    /// <summary>
    /// Classe de base abstraite pour tous les �v�nements d'un match
    /// Utilise le pattern Template Method vu en cours
    /// </summary>
    public abstract class MatchEvent
    {
        public int Id { get; set; }

        /// <summary>
        /// R�f�rence au match concern�
        /// </summary>
        public int MatchId { get; set; }
        public virtual Match Match { get; set; }

        /// <summary>
        /// Quart-temps o� l'�v�nement s'est produit
        /// </summary>
        public int Quarter { get; set; }

        /// <summary>
        /// Temps dans le quart-temps (format MM:SS)
        /// </summary>
        public TimeSpan GameTime { get; set; }

        /// <summary>
        /// Identifiant du joueur concern� (si applicable)
        /// </summary>
        public int? PlayerId { get; set; }
        public virtual Player? Player { get; set; }

        /// <summary>
        /// Nombre de points associ�s � l'�v�nement (si applicable)
        /// </summary>
        public int Points { get; set; }

        /// <summary>
        /// Type d'�v�nement - Propri�t� abstraite � impl�menter dans les classes d�riv�es
        /// </summary>
        public abstract MatchEventType EventType { get; }

        /// <summary>
        /// Description textuelle de l'�v�nement
        /// M�thode abstraite selon le pattern Template Method vu en cours
        /// </summary>
        public abstract string GetDescription();

        /// <summary>
        /// Timestamp de cr�ation de l'�v�nement
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Utilisateur qui a cr�� l'�v�nement
        /// CORRECTION: Changement de CreatedByUserId vers CreatedById pour correspondre au DbContext
        /// </summary>
        public int CreatedById { get; set; }
        public virtual User CreatedBy { get; set; }
    }
}