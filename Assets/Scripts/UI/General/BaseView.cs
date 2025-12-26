using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Core.Scripts.Services;

public abstract class BaseView : MonoBehaviour, IView
{
    [SerializeField] protected Image backgroundImage;
    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private PopupAnimationType animationType = PopupAnimationType.Fade;
    [SerializeField] protected RectTransform panel;
    [SerializeField] private float fadeEndValue = 0.8f;
    [SerializeField] private bool shouldMoveBackgroundToParent = false;

    private bool _isVisible;
    private Sequence _currentAnimation;
    private Vector2 _originalPanelPosition;
    private Vector3 _originalPanelScale;
    private float _moveOffset;
    private RectTransform _backgroundRectTransform;
    private bool _isBackgroundAttachedToCanvas;
    private Transform _backgroundOriginalParent;
    private int _backgroundOriginalSiblingIndex;
    private Transform _backgroundsRoot;
    private CanvasGroup _panelCanvasGroup;
    private ISoundService _soundService;
    private List<Button> _subscribedButtons;

    protected virtual void Awake()
    {
        if (backgroundImage != null)
        {
            _backgroundRectTransform = backgroundImage.rectTransform;
        }

        if (panel != null)
        {
            _originalPanelPosition = panel.anchoredPosition;
            _originalPanelScale = panel.localScale;

            var rect = panel.rect;
            _moveOffset = rect.height;

            _panelCanvasGroup = panel.GetComponent<CanvasGroup>();
            if (_panelCanvasGroup == null)
            {
                _panelCanvasGroup = panel.gameObject.AddComponent<CanvasGroup>();
            }
            _panelCanvasGroup.interactable = false;
            _panelCanvasGroup.blocksRaycasts = false;
        }

        if (panel != null)
        {
            switch (animationType)
            {
                case PopupAnimationType.Fade:
                    if (_panelCanvasGroup != null)
                    {
                        _panelCanvasGroup.alpha = 0f;
                    }
                    break;
                case PopupAnimationType.MoveUp:
                    panel.anchoredPosition = _originalPanelPosition + Vector2.down * _moveOffset;
                    break;
                case PopupAnimationType.MoveDown:
                    panel.anchoredPosition = _originalPanelPosition + Vector2.up * _moveOffset;
                    break;
                case PopupAnimationType.Scale:
                    panel.localScale = Vector3.zero;
                    break;
            }
        }

        RegisterButtonClickSounds();
    }

    private void RegisterButtonClickSounds()
    {
        _subscribedButtons = new List<Button>();
        try
        {
            _soundService = ServiceLocator.GetService<ISoundService>();
        }
        catch
        {
            _soundService = null;
        }

        if (_soundService == null) return;

        var buttons = GetComponentsInChildren<Button>(true);
        foreach (var btn in buttons)
        {
            if (btn == null) continue;
            if (_subscribedButtons.Contains(btn)) continue;
            btn.onClick.AddListener(PlayButtonClickSound);
            _subscribedButtons.Add(btn);
        }
    }

    private void PlayButtonClickSound()
    {
        _soundService?.PlaySound(ClipName.ButtonClick);
    }

    /// <summary>
    /// Exclude a specific button from the automatic click sound behavior.
    /// Call this from derived views (e.g., in Start) for buttons that should remain silent.
    /// </summary>
    public void ExcludeButtonFromClickSound(Button btn)
    {
        if (btn == null || _subscribedButtons == null) return;
        try
        {
            btn.onClick.RemoveListener(PlayButtonClickSound);
        }
        catch { }
        _subscribedButtons.Remove(btn);
    }

    /// <summary>
    /// Exclude multiple buttons from automatic click sound.
    /// </summary>
    public void ExcludeButtonsFromClickSound(params Button[] buttons)
    {
        if (buttons == null) return;
        foreach (var b in buttons)
            ExcludeButtonFromClickSound(b);
    }

    public virtual void Show()
    {
        if (_isVisible) return;

        transform.SetAsLastSibling();
        _currentAnimation?.Kill();
        gameObject.SetActive(true);

        if (shouldMoveBackgroundToParent)
        {
            AttachBackgroundToCanvas();
            if (_backgroundRectTransform != null)
            {
                _backgroundRectTransform.SetAsLastSibling();
            }
        }

        _currentAnimation = DOTween.Sequence()
            .OnStart(() =>
            {
                backgroundImage.raycastTarget = true;
                if (_panelCanvasGroup != null)
                {
                    _panelCanvasGroup.interactable = false;
                    _panelCanvasGroup.blocksRaycasts = false;
                }
            });

        _currentAnimation.Join(backgroundImage.DOFade(fadeEndValue, animationDuration));

        if (panel != null)
        {
            switch (animationType)
            {
                case PopupAnimationType.Fade:
                    var panelCanvasGroup = panel.GetComponent<CanvasGroup>();
                    if (panelCanvasGroup != null)
                    {
                        _currentAnimation.Join(panelCanvasGroup.DOFade(1f, animationDuration)
                            .From(0f)
                            .SetEase(Ease.Linear));
                    }
                    break;

                case PopupAnimationType.MoveUp:
                    _currentAnimation.Join(panel.DOAnchorPos(_originalPanelPosition, animationDuration)
                        .From(_originalPanelPosition + Vector2.down * _moveOffset)
                        .SetEase(Ease.OutBack));
                    break;

                case PopupAnimationType.MoveDown:
                    _currentAnimation.Join(panel.DOAnchorPos(_originalPanelPosition, animationDuration)
                        .From(_originalPanelPosition + Vector2.up * _moveOffset)
                        .SetEase(Ease.OutBack));
                    break;

                case PopupAnimationType.Scale:
                    _currentAnimation.Join(panel.DOScale(_originalPanelScale, animationDuration)
                        .From(Vector3.zero)
                        .SetEase(Ease.OutBack));
                    break;
            }
        }

        _currentAnimation.OnComplete(() =>
        {
            backgroundImage.raycastTarget = true;
            if (_panelCanvasGroup != null)
            {
                _panelCanvasGroup.interactable = true;
                _panelCanvasGroup.blocksRaycasts = true;
            }
            _isVisible = true;
            OnShown();
        });
    }

    public virtual void Hide()
    {
        if (!_isVisible && !gameObject.activeSelf) return;

        _currentAnimation?.Kill();

        _currentAnimation = DOTween.Sequence()
            .OnStart(() =>
            {
                backgroundImage.raycastTarget = false;
                if (_panelCanvasGroup != null)
                {
                    _panelCanvasGroup.interactable = false;
                    _panelCanvasGroup.blocksRaycasts = false;
                }
            });

        _currentAnimation.Join(backgroundImage.DOFade(0, animationDuration));

        if (panel != null)
        {
            switch (animationType)
            {
                case PopupAnimationType.Fade:
                    var panelCanvasGroup = panel.GetComponent<CanvasGroup>();
                    if (panelCanvasGroup != null)
                    {
                        _currentAnimation.Join(panelCanvasGroup.DOFade(0f, animationDuration)
                            .SetEase(Ease.Linear));
                    }
                    break;

                case PopupAnimationType.MoveUp:
                    _currentAnimation.Join(panel.DOAnchorPos(_originalPanelPosition + Vector2.down * _moveOffset, animationDuration)
                        .SetEase(Ease.InBack));
                    break;

                case PopupAnimationType.MoveDown:
                    _currentAnimation.Join(panel.DOAnchorPos(_originalPanelPosition + Vector2.up * _moveOffset, animationDuration)
                        .SetEase(Ease.InBack));
                    break;

                case PopupAnimationType.Scale:
                    _currentAnimation.Join(panel.DOScale(Vector3.zero, animationDuration)
                        .SetEase(Ease.InBack));
                    break;
            }
        }

        _currentAnimation.OnComplete(() =>
        {
            _isVisible = false;
            gameObject.SetActive(false);
            OnHidden();
        });
    }

    protected virtual void OnShown() { }

    protected virtual void OnHidden() { }

    protected virtual void OnDestroy()
    {
        _currentAnimation?.Kill();
        if (backgroundImage != null)
        {
            Destroy(backgroundImage.gameObject);
        }
        if (_subscribedButtons != null)
        {
            foreach (var btn in _subscribedButtons)
            {
                if (btn != null)
                    btn.onClick.RemoveListener(PlayButtonClickSound);
            }
            _subscribedButtons.Clear();
            _subscribedButtons = null;
        }
    }

    private void AttachBackgroundToCanvas()
    {
        if (_backgroundRectTransform == null)
        {
            return;
        }

        var canvas = backgroundImage.canvas;
        if (canvas == null)
        {
            return;
        }

        if (!_isBackgroundAttachedToCanvas)
        {
            _backgroundOriginalParent = _backgroundRectTransform.parent;
            _backgroundOriginalSiblingIndex = _backgroundRectTransform.GetSiblingIndex();
        }

        if (_backgroundsRoot == null)
        {
            var backgroundsTransform = canvas.transform.Find("Backgrounds");
            _backgroundsRoot = backgroundsTransform != null ? backgroundsTransform : canvas.transform;
        }

        _backgroundRectTransform.SetParent(_backgroundsRoot, false);
        _isBackgroundAttachedToCanvas = true;
    }
}
