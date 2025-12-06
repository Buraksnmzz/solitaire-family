using System;
using DG.Tweening;
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
    [SerializeField] private CanvasGroup errorImage;
    [SerializeField] private TextMeshProUGUI errorText;
    private Sequence _sequence;
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

    public void ShowErrorMessage(string errorMessage)
    {
        if (_sequence != null && _sequence.IsActive())
        {
            _sequence.Kill();
        }
        errorText.SetText(errorMessage);
        errorImage.gameObject.SetActive(true);
        _sequence = DOTween.Sequence();
        _sequence.Append(errorImage.DOFade(1f, 0.2f));
        _sequence.AppendInterval(2);
        _sequence.Append(errorImage.DOFade(0f, 0.2f));
        _sequence.OnComplete(() => errorImage.gameObject.SetActive(false));
        
    }
}
