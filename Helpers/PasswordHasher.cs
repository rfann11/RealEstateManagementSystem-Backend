
using System.Security.Cryptography; // SHA256 ve RandomNumberGenerator için
using System.Text; // Encoding için

namespace REMS.Backend.Helpers
{
    public static class PasswordHasher
    {
        // Şifreyi hash'lemek için
        public static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            // Salt oluşturma
            const int SALT_SIZE = 16;
            passwordSalt = new byte[SALT_SIZE];
            RandomNumberGenerator.Fill(passwordSalt);
                                                      

            // Şifreyi ve salt'ı birleştirip SHA256 ile hash'leme
            using (var sha256 = SHA256.Create())
            {
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] saltedPasswordBytes = new byte[passwordBytes.Length + passwordSalt.Length];

                Buffer.BlockCopy(passwordBytes, 0, saltedPasswordBytes, 0, passwordBytes.Length);
                Buffer.BlockCopy(passwordSalt, 0, saltedPasswordBytes, passwordBytes.Length, passwordSalt.Length);

                passwordHash = sha256.ComputeHash(saltedPasswordBytes);
            }
        }

        // Şifreyi doğrulamak için
        public static bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] saltedPasswordBytes = new byte[passwordBytes.Length + storedSalt.Length];

                Buffer.BlockCopy(passwordBytes, 0, saltedPasswordBytes, 0, passwordBytes.Length);
                Buffer.BlockCopy(storedSalt, 0, saltedPasswordBytes, passwordBytes.Length, storedSalt.Length);

                var computedHash = sha256.ComputeHash(saltedPasswordBytes);

                if (computedHash.Length != storedHash.Length) return false;

                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != storedHash[i]) return false;
                }
            }
            return true;
        }
    }
}