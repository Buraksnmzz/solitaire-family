using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SettingsCard : MonoBehaviour
{
    [SerializeField] private Image toggleBackground;
    [SerializeField] private RectTransform handle;

    [SerializeField] private Vector2 onPosition = new Vector2(20, 0);
    [SerializeField] private Vector2 offPosition = new Vector2(-20, 0);
    [SerializeField] private float tweenDuration = 1f;
    [SerializeField] private Ease tweenEase = Ease.OutQuad;

    private IStyleService _styleService;
    private StyleHelper _styleHelper;
    private Color _onColor;
    private Color _offColor;

    private bool _isOn;


    private void Awake()
    {
        _styleService = ServiceLocator.GetService<IStyleService>();
        _styleHelper = _styleService.StyleHelper;
        _onColor = _styleHelper.gameBlue;
        _offColor = _styleHelper.gameGrey;
    }

    public void SetState(bool isOn, bool animate = true)
    {
        _isOn = isOn;
        ApplyVisualState(animate);
    }

    private void ApplyVisualState(bool animate)
    {
        if (toggleBackground != null)
        {
            toggleBackground.color = _isOn ? _onColor : _offColor;
        }

        if (handle != null)
        {
            var target = _isOn ? onPosition : offPosition;
            if (animate)
            {
                handle.DOAnchorPos(target, tweenDuration).SetEase(tweenEase);
            }
            else
            {
                handle.anchoredPosition = target;
            }
        }
    }
}
