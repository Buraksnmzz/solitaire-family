using Services.Persistence;

namespace Levels
{
    public static class MathLevelMapProtection
    {
        private static readonly ILocalDataProtector Protector = new AesCbcLocalDataProtector(new EmbeddedMathLevelSecretProvider());

        public static string Protect(string plainText)
        {
            return string.IsNullOrWhiteSpace(plainText) ? string.Empty : Protector.Protect(plainText);
        }

        public static bool TryUnprotect(string protectedText, out string plainText)
        {
            plainText = string.Empty;

            if (string.IsNullOrWhiteSpace(protectedText))
            {
                return false;
            }

            return Protector.TryUnprotect(protectedText, out plainText);
        }
    }
}