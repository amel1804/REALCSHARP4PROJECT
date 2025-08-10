using System;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace BasketballLiveScore.Services
{
    /// <summary>
    /// Interface pour le service de hashage des mots de passe
    /// </summary>
    public interface IPasswordHasherService
    {
        /// <summary>
        /// Hash un mot de passe avec un salt aléatoire
        /// </summary>
        string HashPassword(string password);

        /// <summary>
        /// Vérifie qu'un mot de passe correspond à un hash
        /// </summary>
        bool VerifyPassword(string password, string hashedPassword);
    }

    /// <summary>
    /// Service pour le hashage sécurisé des mots de passe
    /// Utilise PBKDF2 avec salt aléatoire pour une sécurité maximale
    /// </summary>
    public class PasswordHasherService : IPasswordHasherService
    {
        // Constantes pour la configuration du hashage
        private const int SALT_SIZE = 128 / 8; // 16 bytes
        private const int HASH_SIZE = 256 / 8; // 32 bytes
        private const int ITERATIONS = 100000; // Nombre d'itérations PBKDF2
        private const char DELIMITER = '.';

        /// <summary>
        /// Hash un mot de passe avec un salt aléatoire
        /// Format du résultat: salt.hash (encodé en base64)
        /// </summary>
        public string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentNullException(nameof(password));
            }

            // Générer un salt aléatoire
            byte[] salt = GenerateSalt();

            // Hasher le mot de passe avec le salt
            byte[] hash = HashPasswordWithSalt(password, salt);

            // Combiner salt et hash avec un délimiteur
            string saltBase64 = Convert.ToBase64String(salt);
            string hashBase64 = Convert.ToBase64String(hash);

            return $"{saltBase64}{DELIMITER}{hashBase64}";
        }

        /// <summary>
        /// Vérifie qu'un mot de passe correspond à un hash
        /// </summary>
        public bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrEmpty(password))
            {
                return false;
            }

            if (string.IsNullOrEmpty(hashedPassword))
            {
                return false;
            }

            try
            {
                // Séparer le salt et le hash
                string[] parts = hashedPassword.Split(DELIMITER);
                if (parts.Length != 2)
                {
                    return false;
                }

                byte[] salt = Convert.FromBase64String(parts[0]);
                byte[] storedHash = Convert.FromBase64String(parts[1]);

                // Hasher le mot de passe fourni avec le même salt
                byte[] computedHash = HashPasswordWithSalt(password, salt);

                // Comparer les deux hashs de manière sécurisée
                return CryptographicEquals(storedHash, computedHash);
            }
            catch
            {
                // En cas d'erreur (format invalide, etc.), retourner false
                return false;
            }
        }

        /// <summary>
        /// Génère un salt aléatoire cryptographiquement sécurisé
        /// </summary>
        private byte[] GenerateSalt()
        {
            byte[] salt = new byte[SALT_SIZE];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }

        /// <summary>
        /// Hash un mot de passe avec un salt donné
        /// </summary>
        private byte[] HashPasswordWithSalt(string password, byte[] salt)
        {
            return KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: ITERATIONS,
                numBytesRequested: HASH_SIZE
            );
        }

        /// <summary>
        /// Comparaison sécurisée de deux tableaux de bytes
        /// Résistant aux attaques de timing
        /// </summary>
        private bool CryptographicEquals(byte[] a, byte[] b)
        {
            if (a == null || b == null || a.Length != b.Length)
            {
                return false;
            }

            var result = 0;
            for (int i = 0; i < a.Length; i++)
            {
                result |= a[i] ^ b[i];
            }
            return result == 0;
        }
    }
}