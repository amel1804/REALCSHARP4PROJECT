using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using BasketballLiveScore.Services.Interfaces;
using BasketballLiveScore.Models;
using BasketballLiveScore.Data;

namespace BasketballLiveScore.Services
{
    /// <summary>
    /// Service pour la suppression des entités
    /// </summary>
    public class DeleteService : IDeleteService
    {
        private readonly BasketballDbContext _context; // Point-virgule manquant

        /// <summary>
        /// Constructeur avec injection du DbContext
        /// </summary>
        public DeleteService(BasketballDbContext context)
        {
            _context = context; // Correction : utiliser _context au lieu de _dataContext
        }

        /// <summary>
        /// Supprime un utilisateur par son identifiant
        /// </summary>
        public string Delete(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user != null)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
                return "Deleted";
            }
            return "User not found";
        }
    }
}