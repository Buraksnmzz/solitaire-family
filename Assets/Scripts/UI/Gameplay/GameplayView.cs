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
    [SerializeField] private Button jokerButton;
    [SerializeField] private Image undoButtonIcon;
    [SerializeField] private CanvasGroup errorImage;
    [SerializeField] private TextMeshProUGUI errorText;
    [SerializeField] private CanvasGroup _inputBlocker;
    private Sequence _sequence;
    private Sequence _inputBlockerSequence;
    public event Action UndoButtonClicked;

    private void Start()
    {
        undoButton.onClick.AddListener(() => UndoButtonClicked?.Invoke());
        jokerButton.onClick.AddListener(() => board.GenerateJokerCard());
        SetUndoButtonInteractable(false);
    }

    public void SetUndoButtonInteractable(bool interactable)
    {
        undoButton.interactable = interactable;
        undoButtonIcon.color = interactable ? new Color(1, 1, 1, 1) : new Color(1, 1, 1, 0.4f);
    }

    public void SetMovesCount(int totalMovesCount)
    {
        movesCount.SetText(totalMovesCount.ToString());
    }

    public void SetupBoard(LevelData levelData, int currentLevelIndex)
    {
        board.Setup(levelData, currentLevelIndex, panel);
    }

    public void SetInputBlocked(bool blocked)
    {
        if (_inputBlocker == null) return;
        if (_inputBlockerSequence != null && _inputBlockerSequence.IsActive())
        {
            _inputBlockerSequence.Kill();
            _inputBlockerSequence = null;
        }

        _inputBlocker.blocksRaycasts = blocked;
        _inputBlocker.interactable = blocked;

        if (blocked)
        {
            _inputBlockerSequence = DOTween.Sequence();
            _inputBlockerSequence.AppendInterval(2f);
            _inputBlockerSequence.OnComplete(() =>
            {
                _inputBlocker.blocksRaycasts = false;
                _inputBlocker.interactable = false;
                _inputBlockerSequence = null;
            });
        }
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
