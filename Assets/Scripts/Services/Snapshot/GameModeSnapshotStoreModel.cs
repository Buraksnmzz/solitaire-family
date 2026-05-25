using System;
using Levels;

[Serializable]
public class GameModeSnapshotStoreModel : IModel
{
    public SnapShotModel ClassicSnapshot;
    public SnapShotModel MathSnapshot;

    public SnapShotModel GetSnapshot(GameMode gameMode)
    {
        switch (gameMode)
        {
            case GameMode.Math:
                return MathSnapshot;
            default:
                return ClassicSnapshot;
        }
    }

    public void SetSnapshot(GameMode gameMode, SnapShotModel snapshot)
    {
        switch (gameMode)
        {
            case GameMode.Math:
                MathSnapshot = snapshot;
                return;
            default:
                ClassicSnapshot = snapshot;
                return;
        }
    }
}