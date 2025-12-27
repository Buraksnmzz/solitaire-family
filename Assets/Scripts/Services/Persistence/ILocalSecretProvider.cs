namespace Services.Persistence
{
    public interface ILocalSecretProvider
    {
        string GetOrCreateSecret();
    }
}
