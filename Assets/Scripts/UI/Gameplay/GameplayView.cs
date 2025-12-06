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
    [SerializeField] private Image undoButtonIcon;
    public event Action UndoButtonClicked;

    private void Start()
    {
        undoButton.onClick.AddListener(()=>UndoButtonClicked?.Invoke());
        SetUndoButtonInteractable(false);
    }

    public void SetUndoButtonInteractable(bool interactable)
    {
        undoButton.interactable = interactable;
        undoButtonIcon.color = interactable ? new Color(1,1,1,1) : new Color(1,1,1,0.4f);
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
