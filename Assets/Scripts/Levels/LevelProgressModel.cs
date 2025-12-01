namespace Levels
{
    public class LevelProgressModel: IModel
    {
        public int CurrentLevelIndex { get; set; }
        public bool TutorialCompleted { get; set; }
    }
}