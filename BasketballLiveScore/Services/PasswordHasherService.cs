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
        /// Hash un mot de passe avec un salt al�atoire
        /// </summary>
        string HashPassword(string password);

        /// <summary>
        /// V�rifie qu'un mot de passe correspond � un hash
        /// </summary>
        bool VerifyPassword(string password, string hashedPassword);
    }

    /// <summary>
    /// Service pour le hashage s�curis� des mots de passe
    /// Utilise PBKDF2 avec salt al�atoire pour une s�curit� maximale
    /// </summary>
    public class PasswordHasherService : IPasswordHasherService
    {
        // Constantes pour la configuration du hashage
        private const int SALT_SIZE = 128 / 8; // 16 bytes
        private const int HASH_SIZE = 256 / 8; // 32 bytes
        private const int ITERATIONS = 100000; // Nombre d'it�rations PBKDF2
        private const char DELIMITER = '.';

        /// <summary>
        /// Hash un mot de passe avec un salt al�atoire
        /// Format du r�sultat: salt.hash (encod� en base64)
        /// </summary>
        public string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentNullException(nameof(password));
            }

            // G�n�rer un salt al�atoire
            byte[] salt = GenerateSalt();

            // Hasher le mot de passe avec le salt
            byte[] hash = HashPasswordWithSalt(password, salt);

            // Combiner salt et hash avec un d�limiteur
            string saltBase64 = Convert.ToBase64String(salt);
            string hashBase64 = Convert.ToBase64String(hash);

            return $"{saltBase64}{DELIMITER}{hashBase64}";
        }

        /// <summary>
        /// V�rifie qu'un mot de passe correspond � un hash
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
                // S�parer le salt et le hash
                string[] parts = hashedPassword.Split(DELIMITER);
                if (parts.Length != 2)
                {
                    return false;
                }

                byte[] salt = Convert.FromBase64String(parts[0]);
                byte[] storedHash = Convert.FromBase64String(parts[1]);

                // Hasher le mot de passe fourni avec le m�me salt
                byte[] computedHash = HashPasswordWithSalt(password, salt);

                // Comparer les deux hashs de mani�re s�curis�e
                return CryptographicEquals(storedHash, computedHash);
            }
            catch
            {
                // En cas d'erreur (format invalide, etc.), retourner false
                return false;
            }
        }

        /// <summary>
        /// G�n�re un salt al�atoire cryptographiquement s�curis�
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
        /// Hash un mot de passe avec un salt donn�
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
        /// Comparaison s�curis�e de deux tableaux de bytes
        /// R�sistant aux attaques de timing
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