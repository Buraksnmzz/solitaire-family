using System;
using System.Collections.Generic;
using System.Linq;
using Card;
using DG.Tweening;
using Gameplay;
using Levels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameplayView : BaseView
{
    private const float JokerSelectionButtonWidthMultiplier = 1.05f;
    private const float JokerSelectionButtonHeightMultiplier = 1.6f;
    private const string JokerSelectionTweenId = "JokerSelection";
    private const float JokerSelectionScale = 1.08f;
    private const float JokerSelectionScaleDuration = 0.6f;

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
    private Sequence _coinSequence;
    private readonly List<CardView> _jokerSelectionVisuals = new();
    private readonly List<int> _jokerSelectionVisualPileIndices = new();
    private RectTransform _jokerSelectionButtonsRoot;
    private readonly List<Button> _jokerSelectionButtons = new();

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
    public event Action<int> JokerPileSelected;


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
        SetGameplayInputBlocked(false);
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        ApplicationPaused?.Invoke(pauseStatus);
    }

    public Board Board => board;

    public void SetGameplayInputBlocked(bool isBlocked)
    {
        _inputBlocker.blocksRaycasts = isBlocked;
        _inputBlocker.interactable = isBlocked;
        _inputBlocker.gameObject.SetActive(isBlocked);
    }

    public void ShowPersistentErrorMessage(string errorMessage)
    {
        if (_sequence != null && _sequence.IsActive())
        {
            _sequence.Kill();
        }

        errorText.SetText(errorMessage);
        errorImage.alpha = 1f;
        errorImage.gameObject.SetActive(true);
    }

    public void HideErrorMessage()
    {
        if (_sequence != null && _sequence.IsActive())
        {
            _sequence.Kill();
        }

        errorImage.gameObject.SetActive(false);
    }

    public void ShowJokerPileSelection(IReadOnlyList<int> activePileIndices)
    {
        SetGameplayInputBlocked(true);
        ClearJokerSelectionVisuals();
        EnsureJokerSelectionButtonsRoot();
        EnsureJokerSelectionButtons(board != null ? board.Piles.Count : 0);

        if (board == null || board.jokerCardView == null)
            return;

        for (var i = 0; i < activePileIndices.Count; i++)
        {
            var pileIndex = activePileIndices[i];
            if (pileIndex < 0) continue;
            if (board.Piles == null) continue;
            if (pileIndex >= board.Piles.Count) continue;

            var pile = board.Piles[pileIndex];
            if (pile == null) continue;
            if (!pile.gameObject.activeInHierarchy) continue;

            var localTargetOnPile = pile.GetCardLocalPosition(pile.GetCardsCount());

            var copy = Instantiate(board.jokerCardView, pile.transform);
            copy.SetRaycastTarget(false);
            var dragger = copy.GetComponent<CardDragger>();
            if (dragger != null) dragger.enabled = false;
            copy.SetState(CardViewState.JokerTop);

            var copyRect = copy.transform as RectTransform;
            var pileRect = pile.transform as RectTransform;
            if (copyRect != null)
            {
                copyRect.anchorMin = new Vector2(0.5f, 0.5f);
                copyRect.anchorMax = new Vector2(0.5f, 0.5f);
                copyRect.pivot = new Vector2(0.5f, 0.5f);

                if (pileRect != null)
                {
                    var targetWidth = pileRect.rect.width;
                    var targetHeight = pileRect.rect.height;
                    if (targetWidth <= 0f) targetWidth = pileRect.sizeDelta.x;
                    if (targetHeight <= 0f) targetHeight = pileRect.sizeDelta.y;

                    if (targetWidth > 0f)
                        copyRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetWidth);
                    if (targetHeight > 0f)
                        copyRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetHeight);
                }

                copyRect.localPosition = localTargetOnPile;
                copyRect.localScale = Vector3.one;
                copyRect
                    .DOScale(Vector3.one * JokerSelectionScale, JokerSelectionScaleDuration)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetId(JokerSelectionTweenId);
            }

            copy.transform.SetAsLastSibling();

            _jokerSelectionVisuals.Add(copy);
            _jokerSelectionVisualPileIndices.Add(pileIndex);

            var button = pileIndex < _jokerSelectionButtons.Count ? _jokerSelectionButtons[pileIndex] : null;
            if (button == null) continue;
            var buttonRect = button.transform as RectTransform;
            if (buttonRect == null) continue;

            button.gameObject.SetActive(true);
            buttonRect.position = pile.transform.TransformPoint(localTargetOnPile);

            var width = 0f;
            if (pileRect != null)
            {
                width = pileRect.rect.width;
                if (width <= 0f)
                    width = pileRect.sizeDelta.x;
            }

            if (width <= 0f && copyRect != null)
            {
                width = copyRect.sizeDelta.x;
                if (width <= 0f)
                    width = copyRect.rect.width;
            }

            if (width > 0f)
            {
                buttonRect.sizeDelta = new Vector2(width, width * 5f);
            }

            button.transform.SetAsLastSibling();
        }

        for (var i = 0; i < _jokerSelectionButtons.Count; i++)
        {
            var button = _jokerSelectionButtons[i];
            if (button == null) continue;
            if (activePileIndices.Contains(i)) continue;
            button.gameObject.SetActive(false);
        }
    }

    public void HideJokerPileSelection()
    {
        ClearJokerSelectionVisuals();
        if (_jokerSelectionButtonsRoot != null)
        {
            _jokerSelectionButtonsRoot.gameObject.SetActive(false);
        }

        SetGameplayInputBlocked(false);
    }

    private void ClearJokerSelectionVisuals()
    {
        for (var i = 0; i < _jokerSelectionVisuals.Count; i++)
        {
            var view = _jokerSelectionVisuals[i];
            if (view == null) continue;
            //view.transform.DOKill(JokerSelectionTweenId);
            view.transform.DOKill();
            Destroy(view.gameObject);
        }

        _jokerSelectionVisuals.Clear();
        _jokerSelectionVisualPileIndices.Clear();
    }

    private void EnsureJokerSelectionButtonsRoot()
    {
        if (_jokerSelectionButtonsRoot != null) return;
        if (_inputBlocker == null) return;

        var go = new GameObject("JokerSelectionButtons", typeof(RectTransform));
        var rect = go.GetComponent<RectTransform>();
        rect.SetParent(_inputBlocker.transform, false);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        _jokerSelectionButtonsRoot = rect;
    }

    private void EnsureJokerSelectionButtons(int requiredCount)
    {
        if (_jokerSelectionButtonsRoot == null) return;
        if (requiredCount <= 0) return;

        _jokerSelectionButtonsRoot.gameObject.SetActive(true);

        while (_jokerSelectionButtons.Count < requiredCount)
        {
            var index = _jokerSelectionButtons.Count;

            var go = new GameObject($"JokerSelectButton_{index}", typeof(RectTransform), typeof(Image), typeof(Button));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(_jokerSelectionButtonsRoot, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);

            var image = go.GetComponent<Image>();
            image.color = new Color(1f, 1f, 1f, 0f);
            image.raycastTarget = true;

            var button = go.GetComponent<Button>();
            button.onClick.AddListener(() => JokerPileSelected?.Invoke(index));
            _jokerSelectionButtons.Add(button);
        }
    }

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

    public void SetLevelText(string level)
    {
        topLevelText.SetText(level);
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
        else if (totalCoins >= coinCost)
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
        else if (totalCoins >= coinCost)
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
        else if (totalCoins >= coinCost)
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
