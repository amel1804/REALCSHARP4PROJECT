using System;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using BasketballLiveScore.Services.Interfaces;

namespace BasketballLiveScore.Services
{
    /// <summary>
    /// Interface pour le service de hashage des mots de passe
    /// </summary>
    public interface IPasswordHasherService
    {
        string HashPassword(string password);
        bool VerifyPassword(string password, string hashedPassword);
    }

    /// <summary>
    /// Service de hashage s�curis� des mots de passe utilisant PBKDF2-SHA256
    /// </summary>
    public class PasswordHasherService : IPasswordHasherService
    {
        // Configuration du hashage
        private const int SALT_SIZE = 128 / 8; // 128 bits
        private const int HASH_SIZE = 256 / 8; // 256 bits
        private const int ITERATIONS = 10000;  // Nombre d'it�rations PBKDF2

        /// <summary>
        /// Hash un mot de passe avec un salt al�atoire
        /// </summary>
        public string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentNullException(nameof(password));
            }

            // G�n�ration d'un salt al�atoire
            byte[] salt = new byte[SALT_SIZE];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Hashage du mot de passe
            byte[] hash = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: ITERATIONS,
                numBytesRequested: HASH_SIZE);

            // Combinaison salt + hash en base64
            return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        /// <summary>
        /// V�rifie qu'un mot de passe correspond � son hash
        /// </summary>
        public bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hashedPassword))
            {
                return false;
            }

            try
            {
                // Support pour migration des anciens mots de passe non hash�s
                if (!hashedPassword.Contains("."))
                {
                    // Ancien mot de passe en clair (migration)
                    return password == hashedPassword;
                }

                // Extraction du salt et du hash
                var parts = hashedPassword.Split('.');
                if (parts.Length != 2)
                {
                    return false;
                }

                byte[] salt = Convert.FromBase64String(parts[0]);
                byte[] storedHash = Convert.FromBase64String(parts[1]);

                // Hashage du mot de passe fourni
                byte[] computedHash = KeyDerivation.Pbkdf2(
                    password: password,
                    salt: salt,
                    prf: KeyDerivationPrf.HMACSHA256,
                    iterationCount: ITERATIONS,
                    numBytesRequested: HASH_SIZE);

                // Comparaison s�curis�e
                return SlowEquals(storedHash, computedHash);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Comparaison s�curis�e r�sistante aux timing attacks
        /// </summary>
        private bool SlowEquals(byte[] a, byte[] b)
        {
            if (a == null || b == null || a.Length != b.Length)
                return false;

            uint diff = 0;
            for (int i = 0; i < a.Length; i++)
            {
                diff |= (uint)(a[i] ^ b[i]);
            }
            return diff == 0;
        }
    }
}