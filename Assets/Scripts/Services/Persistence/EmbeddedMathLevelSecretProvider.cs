namespace Services.Persistence
{
    public class EmbeddedMathLevelSecretProvider : ILocalSecretProvider
    {
        private static readonly string[] SecretParts =
        {
            "U29saXRhaXJl",
            "RmFtaWx5TWF0aA==",
            "TGV2ZWxNYXA=",
            "U2hpZWxkVjI="
        };

        public string GetOrCreateSecret()
        {
            return string.Concat(SecretParts);
        }
    }
}