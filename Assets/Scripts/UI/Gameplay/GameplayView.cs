using System;
using Gameplay;
using Levels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameplayView : BaseView
{
    [SerializeField] private TextMeshProUGUI movesCount;
    [SerializeField] private Board board;
    [SerializeField] private Button undoButton;
    public event Action UndoButtonClicked;

    private void Start()
    {
        undoButton.onClick.AddListener(()=>UndoButtonClicked?.Invoke());
        SetUndoButtonInteractable(false);
    }

    public void SetUndoButtonInteractable(bool interactable)
    {
        undoButton.interactable = interactable;
    }

    public void SetMovesCount(int totalMovesCount)
    {
        movesCount.SetText(totalMovesCount.ToString());
    }

    public void SetupBoard(LevelData levelData, int currentLevelIndex)
    {
        board.Setup(levelData, currentLevelIndex);
    }
}
