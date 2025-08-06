using System; 

namespace BasketballLiveScore.Services.Interfaces
{
    /// <summary>
    /// Interface pour le service de suppression
    /// </summary>
    public interface IDeleteService
    {
        /// <summary>
        /// Supprime une entité par son identifiant
        /// </summary>
        /// <param name="id">L'identifiant de l'entité à supprimer</param>
        /// <returns>Message de confirmation</returns>
        string Delete(int id);
    }
}