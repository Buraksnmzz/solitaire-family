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
    [SerializeField] public Board board;
    [SerializeField] private Button undoButton;
    [SerializeField] private Button jokerButton;
    [SerializeField] private Button hintButton;
    [SerializeField] private Button coinButton;
    [SerializeField] private CanvasGroup errorImage;
    [SerializeField] private TextMeshProUGUI errorText;
    [SerializeField] private CanvasGroup _inputBlocker;
    [SerializeField] private TextMeshProUGUI hintCountText;
    [SerializeField] private TextMeshProUGUI undoCountText;
    [SerializeField] private TextMeshProUGUI jokerCountText;
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private TextMeshProUGUI topLevelText;
    [SerializeField] private GameObject hintRewardedImage;
    [SerializeField] private GameObject undoRewardedImage;
    [SerializeField] private GameObject jokerRewardedImage;
    [SerializeField] private GameObject hintCountImage;
    [SerializeField] private GameObject undoCountImage;
    [SerializeField] private GameObject jokerCountImage;
    [SerializeField] private Image undoButtonImage;
    [SerializeField] private Image jokerButtonImage;
    [SerializeField] private Image undoButtonIcon;
    [SerializeField] private Image jokerButtonIcon;
    [SerializeField] private Sprite undoButtonActiveSprite;
    [SerializeField] private Sprite undoIconActiveSprite;
    [SerializeField] private Sprite undoButtonPassiveSprite;
    [SerializeField] private Sprite undoIconPassiveSprite;
    [SerializeField] private Sprite jokerButtonActiveSprite;
    [SerializeField] private Sprite jokerIconActiveSprite;
    [SerializeField] private Sprite jokerButtonPassiveSprite;
    [SerializeField] private Sprite jokerIconPassiveSprite;
    [SerializeField] private Button debugRestartButton;
    [SerializeField] private Button debugNextButton;
    
    private Sequence _sequence;
    private Sequence _inputBlockerSequence;
    public event Action UndoButtonClicked;
    public event Action HintButtonClicked;
    public event Action JokerButtonClicked;
    public event Action CoinButtonClicked;
    public event Action<bool> ApplicationPaused;
    public event Action DegubRestartButtonClicked;
    public event Action DegubNextButtonClicked;

    private void Start()
    {
        undoButton.onClick.AddListener(() => UndoButtonClicked?.Invoke());
        jokerButton.onClick.AddListener(() => JokerButtonClicked?.Invoke());
        hintButton.onClick.AddListener(() => HintButtonClicked?.Invoke());
        debugRestartButton.onClick.AddListener(() => DegubRestartButtonClicked?.Invoke());
        debugNextButton.onClick.AddListener(() => DegubNextButtonClicked?.Invoke());
        coinButton.onClick.AddListener(() => CoinButtonClicked?.Invoke());
        SetUndoButtonInteractable(false);
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        ApplicationPaused?.Invoke(pauseStatus);
    }

    public Board Board => board;

    public void SetUndoButtonInteractable(bool interactable)
    {
        undoButton.interactable = interactable;
        undoButtonIcon.sprite = interactable ? undoIconActiveSprite : undoIconPassiveSprite;
        undoButtonImage.sprite = interactable ? undoButtonActiveSprite : undoButtonPassiveSprite;
    }

    public void SetJokerButtonInteractable(bool interactable)
    {
        jokerButton.interactable = interactable;
        jokerButtonIcon.sprite = interactable ? jokerIconActiveSprite : jokerIconPassiveSprite;
        jokerButtonImage.sprite = interactable ? jokerButtonActiveSprite : jokerButtonPassiveSprite;
    }

    public void SetMovesCount(int totalMovesCount)
    {
        movesCount.SetText(totalMovesCount.ToString());
        movesCount.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 5);
    }

    public void SetLevelText(int level)
    {
        topLevelText.SetText("Level " + level.ToString());
    }

    public void SetupBoard(LevelData levelData, int currentLevelIndex, SnapShotModel snapshot = null)
    {
        board.Setup(levelData, currentLevelIndex, panel, snapshot);
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
        _sequence.AppendInterval(2.4f);
        _sequence.Append(errorImage.DOFade(0f, 0.2f));
        _sequence.OnComplete(() => errorImage.gameObject.SetActive(false));

    }

    public void SetCoinText(int totalCoins)
    {
        coinText.SetText(totalCoins.ToString());
    }

    public void SetUndoAmount(int totalUndo)
    {
        if (totalUndo > 0)
        {
            undoCountText.SetText(totalUndo.ToString());
            undoCountImage.SetActive(true);
            undoRewardedImage.SetActive(false);
        }
        else
        {
            undoCountImage.SetActive(false);
            undoRewardedImage.SetActive(true);
        }
    }

    public void SetHintAmount(int totalHints)
    {
        if (totalHints > 0)
        {
            hintCountText.SetText(totalHints.ToString());
            hintCountImage.SetActive(true);
            hintRewardedImage.SetActive(false);
        }
        else
        {
            hintCountImage.SetActive(false);
            hintRewardedImage.SetActive(true);
        }
    }

    public void SetJokerAmount(int totalJokers)
    {
        if (totalJokers > 0)
        {
            jokerCountText.SetText(totalJokers.ToString());
            jokerCountImage.SetActive(true);
            jokerRewardedImage.SetActive(false);
        }
        else
        {
            jokerCountImage.SetActive(false);
            jokerRewardedImage.SetActive(true);
        }
    }
}
