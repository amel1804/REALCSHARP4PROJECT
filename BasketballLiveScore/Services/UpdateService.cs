using System;
using System.Linq;
using BasketballLiveScore.Data;
using BasketballLiveScore.Services.Interfaces;

namespace BasketballLiveScore.Services
{
    /// <summary>
    /// Service pour la mise à jour des utilisateurs
    /// </summary>
    public class UpdateService : IUpdateService
    {
        private readonly BasketballDbContext _context;

        public UpdateService(BasketballDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Met à jour un utilisateur existant
        /// </summary>
        public string Update(int id, string name, string role)
        {
            // Validation des paramètres
            if (string.IsNullOrEmpty(name))
            {
                return "Name is required";
            }

            if (string.IsNullOrEmpty(role))
            {
                return "Role is required";
            }

            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user != null)
            {
                // Mise à jour du username et/ou du nom complet
                user.Username = name;  // Utiliser Username au lieu de Name

                // Optionnel : Si le paramètre 'name' contient un nom complet, le séparer
                var nameParts = name.Split(' ', 2);
                if (nameParts.Length > 0)
                {
                    user.FirstName = nameParts[0];
                    if (nameParts.Length > 1)
                    {
                        user.LastName = nameParts[1];
                    }
                }

                user.Role = role;

                try
                {
                    _context.Users.Update(user);
                    _context.SaveChanges();
                    return "OK";
                }
                catch (Exception ex)
                {
                    // En production, logger l'exception
                    return $"Update failed: {ex.Message}";
                }
            }

            return "User not found";
        }
    }
}