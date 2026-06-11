using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace SURVEY.Service.Configs
{
    public static class SecureConfigurationExtensions
    {
        private const string EncryptedPrefix = "enc:";

        public static string GetSecureValue(this IConfiguration configuration, string key)
        {
            var value = configuration[key];
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            if (!value.StartsWith(EncryptedPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return value;
            }

            try
            {
                var encryptedValue = value[EncryptedPrefix.Length..];
                var encryptedBytes = Convert.FromBase64String(encryptedValue);
                var decryptedBytes = ProtectedData.Unprotect(encryptedBytes, null, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(decryptedBytes);
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
