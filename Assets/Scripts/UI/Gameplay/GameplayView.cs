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
    [SerializeField] private Transform coinImage;
    [SerializeField] private RectTransform earnedCoinIconPrefab;
    [SerializeField] private Button settingsButton;
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
    [SerializeField] private GameObject hintCoinCostImage;
    [SerializeField] private GameObject undoCoinCostImage;
    [SerializeField] private GameObject jokerCoinCostImage;
    [SerializeField] private TextMeshProUGUI hintCoinCostText;
    [SerializeField] private TextMeshProUGUI undoCoinCostText;
    [SerializeField] private TextMeshProUGUI jokerCoinCostText;
    [SerializeField] private Image undoButtonImage;
    [SerializeField] private Image undoButtonIcon;
    [SerializeField] private Sprite undoButtonActiveSprite;
    [SerializeField] private Sprite undoIconActiveSprite;
    [SerializeField] private Sprite undoButtonPassiveSprite;
    [SerializeField] private Sprite undoIconPassiveSprite;
    [SerializeField] private Button debugNextButton;
    [SerializeField] private Button debugCompleteButton;
    [SerializeField] private Button debugMoveButton;
    [SerializeField] private ParticleSystem coinParticle;
    [SerializeField] private ParticleSystem confettiParticle;
    [SerializeField] private ParticleSystem getMovesParticle;
    [SerializeField] private Transform bottomPanel;

    private Sequence _sequence;
    private Sequence _inputBlockerSequence;
    private Sequence _coinSequence;
    public event Action UndoButtonClicked;
    public event Action HintButtonClicked;
    public event Action JokerButtonClicked;
    public event Action CoinButtonClicked;
    public event Action<bool> ApplicationPaused;
    public event Action DegubNextButtonClicked;
    public event Action SettingsButtonClicked;
    public event Action DegubCompleteButtonClicked;
    public event Action DegubMoveButtonClicked;
    public event Action OnCoinMoved;


    private void Start()
    {
        undoButton.onClick.AddListener(() => UndoButtonClicked?.Invoke());
        jokerButton.onClick.AddListener(() => JokerButtonClicked?.Invoke());
        hintButton.onClick.AddListener(() => HintButtonClicked?.Invoke());
        debugNextButton.onClick.AddListener(() => DegubNextButtonClicked?.Invoke());
        debugCompleteButton.onClick.AddListener(() => DegubCompleteButtonClicked?.Invoke());
        debugMoveButton.onClick.AddListener(() => DegubMoveButtonClicked?.Invoke());
        coinButton.onClick.AddListener(() => CoinButtonClicked?.Invoke());
        settingsButton.onClick.AddListener(() => SettingsButtonClicked?.Invoke());
        // Exclude some gameplay buttons from playing the global click sound
        ExcludeButtonsFromClickSound(undoButton, jokerButton, hintButton);

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

    public void SetMovesCount(int totalMovesCount)
    {
        movesCount.SetText(totalMovesCount.ToString());
        movesCount.transform.DOKill(true);
        movesCount.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 5);
    }

    public void SetLevelText(int level)
    {
        topLevelText.SetText("Level " + level.ToString());
    }

    public void SetupBoard(LevelData levelData, int currentLevelIndex, SnapShotModel snapshot = null)
    {
        board.Setup(levelData, currentLevelIndex, panel, snapshot);

        var bottomPanelRectTransform = bottomPanel as RectTransform;
        if (bottomPanelRectTransform == null) return;

        var piles = board != null ? board.Piles : null;
        if (piles == null) return;

        for (var i = 0; i < piles.Count; i++)
        {
            if (piles[i] is Pile pile)
            {
                pile.ConfigureDynamicSpacing(bottomPanelRectTransform);
            }
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

    public void PlayEarnedMovesCoinAnimation(int remainingMoves, int coinsPerMoveLeft, int startingCoins, Action onCompleted)
    {
        if (remainingMoves <= 0)
        {
            SetCoinText(startingCoins);
            onCompleted?.Invoke();
            return;
        }

        if (_coinSequence != null && _coinSequence.IsActive())
        {
            _coinSequence.Kill();
        }

        _coinSequence = DOTween.Sequence();

        const float coinMoveDuration = 0.4f;
        const float coinScaleDuration = 0.2f;

        var currentCoins = startingCoins;
        var currentMoves = remainingMoves;

        for (var i = 0; i < remainingMoves; i++)
        {
            var icon = Instantiate(earnedCoinIconPrefab, panel);
            icon.localScale = Vector3.zero;
            icon.position = movesCount.transform.position;

            float delay;
            if (i < 10)
            {
                delay = 0.3f * i;
            }
            else if (i < 20)
            {
                delay = 0.3f * 10 + 0.15f * (i - 10);
            }
            else
            {
                delay = 0.3f * 10 + 0.15f * 10 + 0.05f * (i - 20);
            }

            var scaleTween = icon.DOScale(Vector3.one, coinScaleDuration).SetEase(Ease.OutBack);
            var moveTween = icon.DOMove(coinImage.position, coinMoveDuration).OnComplete(() =>
            {
                OnCoinMoved?.Invoke();
                currentCoins += coinsPerMoveLeft;
                SetCoinText(currentCoins);
                if (currentMoves > 0)
                {
                    currentMoves--;
                    if (currentMoves < 0)
                        currentMoves = 0;
                    if (currentMoves == remainingMoves - 1)
                        coinParticle.Play();
                    SetMovesCount(currentMoves);
                }

                coinImage.DOKill(true);
                coinImage.DOPunchScale(Vector3.one * 0.2f, 0.08f);
                Destroy(icon.gameObject);
            });

            _coinSequence.Insert(delay, scaleTween);
            _coinSequence.Insert(delay, moveTween);
        }

        _coinSequence.OnComplete(() =>
        {
            SetCoinText(startingCoins + remainingMoves * coinsPerMoveLeft);
            onCompleted?.Invoke();
        });
    }

    public void SetUndoAmount(int totalUndo, int totalCoins, int coinCost)
    {
        if (totalUndo > 0)
        {
            undoCountText.SetText(totalUndo.ToString());
            undoCountImage.SetActive(true);
            undoRewardedImage.SetActive(false);
            undoCoinCostImage.SetActive(false);
        }
        else if(totalCoins >= coinCost)
        {
            undoCountImage.SetActive(false);
            undoRewardedImage.SetActive(false);
            undoCoinCostImage.SetActive(true);
            undoCoinCostText.text = coinCost.ToString();
        }
        else
        {
            undoCountImage.SetActive(false);
            undoRewardedImage.SetActive(true);
            undoCoinCostImage.SetActive(false);
        }
    }

    public void SetHintAmount(int totalHints, int totalCoins, int coinCost)
    {
        if (totalHints > 0)
        {
            hintCountText.SetText(totalHints.ToString());
            hintCountImage.SetActive(true);
            hintRewardedImage.SetActive(false);
            hintCoinCostImage.SetActive(false);
        }
        else if(totalCoins >= coinCost)
        {
            hintCountImage.SetActive(false);
            hintRewardedImage.SetActive(false);
            hintCoinCostImage.SetActive(true);
            hintCoinCostText.text = coinCost.ToString();
        }
        else
        {
            hintCountImage.SetActive(false);
            hintRewardedImage.SetActive(true);
            hintCoinCostImage.SetActive(false);
        }
    }

    public void SetJokerAmount(int totalJokers, int totalCoins, int coinCost)
    {
        if (totalJokers > 0)
        {
            jokerCountText.SetText(totalJokers.ToString());
            jokerCountImage.SetActive(true);
            jokerRewardedImage.SetActive(false);
            jokerCoinCostImage.SetActive(false);
        }
        else if(totalCoins >= coinCost)
        {
            jokerCountImage.SetActive(false);
            jokerRewardedImage.SetActive(false);
            jokerCoinCostImage.SetActive(true);
            jokerCoinCostText.text = coinCost.ToString();
        }
        else
        {
            jokerCoinCostImage.SetActive(false);
            jokerCountImage.SetActive(false);
            jokerRewardedImage.SetActive(true);
        }
    }

    public void PlayConfetti()
    {
        confettiParticle.Play();
    }

    public void PlayGetMovesParticle()
    {
        getMovesParticle.Play();
    }
}
