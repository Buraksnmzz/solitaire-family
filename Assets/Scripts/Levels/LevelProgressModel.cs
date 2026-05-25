namespace Levels
{
    public class LevelProgressModel : IModel
    {
        public int CurrentLevelIndex { get; set; }
        public int CurrentLevelIndexMath { get; set; }
        public int CurrentLevelAttemptCount { get; set; }
        public int CurrentLevelAttemptCountMath { get; set; }
        public bool TutorialCompleted { get; set; }

        public bool EnsurePlayableLevelInitialized(GameMode gameMode)
        {
            if (!TutorialCompleted)
                return false;

            var currentLevelIndex = GetCurrentLevelIndex(gameMode);
            if (currentLevelIndex > 0)
                return false;

            SetCurrentLevelIndex(gameMode, 1);
            return true;
        }

        public int GetCurrentLevelIndex(GameMode gameMode)
        {
            switch (gameMode)
            {
                case GameMode.Math:
                    return CurrentLevelIndexMath;
                default:
                    return CurrentLevelIndex;
            }
        }

        public void SetCurrentLevelIndex(GameMode gameMode, int value)
        {
            switch (gameMode)
            {
                case GameMode.Math:
                    CurrentLevelIndexMath = value;
                    return;
                default:
                    CurrentLevelIndex = value;
                    return;
            }
        }

        public int GetCurrentLevelAttemptCount(GameMode gameMode)
        {
            switch (gameMode)
            {
                case GameMode.Math:
                    return CurrentLevelAttemptCountMath;
                default:
                    return CurrentLevelAttemptCount;
            }
        }

        public void SetCurrentLevelAttemptCount(GameMode gameMode, int value)
        {
            switch (gameMode)
            {
                case GameMode.Math:
                    CurrentLevelAttemptCountMath = value;
                    return;
                default:
                    CurrentLevelAttemptCount = value;
                    return;
            }
        }
    }
}