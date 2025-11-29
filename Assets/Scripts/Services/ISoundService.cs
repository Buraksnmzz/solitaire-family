namespace Core.Scripts.Services
{
    public interface ISoundService: IService
    {
        void PlaySound(ClipName clipName, float delay = 0);
    }
}