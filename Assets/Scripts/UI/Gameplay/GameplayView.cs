using Gameplay;
using Levels;
using TMPro;
using UnityEngine;

public class GameplayView : BaseView
{
    [SerializeField] private TextMeshProUGUI goalCount;
    [SerializeField] private Board board;
    
    
    public void SetGoalCount(int totalGoalCount)
    {
        goalCount.SetText(totalGoalCount.ToString());
    }

    public void SetupBoard(LevelData levelData, int currentLevelIndex)
    {
        board.Setup(levelData, currentLevelIndex);
    }
}
