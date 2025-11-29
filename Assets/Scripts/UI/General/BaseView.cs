using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// Base class for all UI views in the MVP pattern
/// </summary>
public abstract class BaseView : MonoBehaviour, IView
{
    [SerializeField] protected Image backgroundImage;
    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private PopupAnimationType animationType = PopupAnimationType.Fade;
    [SerializeField] protected RectTransform panel;
    [SerializeField] private float fadeEndValue = 0.8f;

    private bool _isVisible;
    private Sequence _currentAnimation;
    private Vector2 _originalPanelPosition;
    private Vector3 _originalPanelScale;
    private float _moveOffset;

    protected virtual void Awake()
    {
        if (panel != null)
        {
            _originalPanelPosition = panel.anchoredPosition;
            _originalPanelScale = panel.localScale;

            var rect = panel.rect;
            _moveOffset = rect.height;
        }

        if (panel != null)
        {
            switch (animationType)
            {
                case PopupAnimationType.Fade:
                    var panelCanvasGroup = panel.GetComponent<CanvasGroup>();
                    if (panelCanvasGroup == null)
                    {
                        panelCanvasGroup = panel.gameObject.AddComponent<CanvasGroup>();
                    }
                    panelCanvasGroup.alpha = 0f;
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
    }

    public virtual void Show()
    {
        if (_isVisible) return;

        transform.SetAsLastSibling();
        _currentAnimation?.Kill();
        gameObject.SetActive(true);

        _currentAnimation = DOTween.Sequence()
            .OnStart(() =>
            {
                backgroundImage.raycastTarget = true;
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
    }
}
