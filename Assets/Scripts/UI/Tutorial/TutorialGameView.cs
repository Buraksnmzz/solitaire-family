using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TutorialGameView : BaseView
{
    [SerializeField] private Button firstTargetButton;
    [SerializeField] private RectTransform firstTarget;
    [SerializeField] private Button secondTargetButton;
    [SerializeField] private RectTransform secondTarget;

    [SerializeField] private Button firstValueButton;
    [SerializeField] private RectTransform firstValue;
    [SerializeField] private Button secondValueButton;
    [SerializeField] private RectTransform secondValue;
    [SerializeField] private RectTransform highlight;

    [SerializeField] private Image handPointer;
    [SerializeField] private TextMeshProUGUI instructionText;

    [SerializeField] private float moveDuration = 0.3f;
    [SerializeField] private float handPulse = 0.12f;
    [SerializeField] private float dragYOffset = 200f; // Value will appear this many UI units above finger
    [SerializeField] private float handTapYOffset = 0f; // Fine tune tap pointer vertical offset
    [SerializeField] private List<Transform> cells;

    public event Action TargetClicked;
    public event Action ValueMoved;
    public event Action OnTutorialCompleted;

    private enum Step { None, FirstSelectCell, FirstSelectValue, SecondDragValue, Completed }
    private Step _step = Step.None;

    private Vector2 _secondValueStartAnchored;
    private Transform _originalParentFirstValue;
    private Transform _originalParentSecondValue;
    private Canvas _canvas;
    private bool _isCompleteTriggered;


    protected override void Awake()
    {
        base.Awake();

        _canvas = GetComponentInParent<Canvas>();

        if (firstTargetButton) firstTargetButton.onClick.AddListener(OnFirstTargetClicked);
        if (firstValueButton) firstValueButton.onClick.AddListener(OnFirstValueClicked);

        if (secondValueButton)
        {
            secondValueButton.onClick.RemoveAllListeners();
        }

        EnsureDragEvents(secondValue.gameObject);
    }

    public override void Show()
    {
        base.Show();
        PrepareInitialStates();
        StartStep1();
    }

    private void PrepareInitialStates()
    {
        _originalParentFirstValue = firstValue.parent;
        _originalParentSecondValue = secondValue.parent;
        _secondValueStartAnchored = secondValue.anchoredPosition;

        SetInteractable(false);

        if (handPointer)
        {
            handPointer.gameObject.SetActive(false);
            handPointer.transform.DOKill();
        }
    }

    private void SetInteractable(bool enabled)
    {
        if (firstTargetButton) firstTargetButton.interactable = enabled;
        if (secondTargetButton) secondTargetButton.interactable = enabled;
        if (firstValueButton) firstValueButton.interactable = enabled;
        if (secondValueButton) secondValueButton.interactable = enabled;
    }

    private void StartStep1()
    {
        _step = Step.FirstSelectCell;
        SetInteractable(false);
        if (firstTargetButton) firstTargetButton.interactable = true;

        ShowHandTapOver(firstTarget);
    }

    private void OnFirstTargetClicked()
    {
        if (_step != Step.FirstSelectCell) return;
        TargetClicked?.Invoke();
        highlight.position = firstTarget.position;
        highlight.sizeDelta = firstTarget.sizeDelta * 1.25f;
        highlight.gameObject.SetActive(true);
        _step = Step.FirstSelectValue;
        SetInteractable(false);
        if (firstValueButton) firstValueButton.interactable = true;

        ShowHandTapOver(firstValue);
    }

    private void OnFirstValueClicked()
    {
        if (_step != Step.FirstSelectValue) return;

        HideHand();

        AnimateValueToTarget(firstValue, firstTarget, StartStep2);
    }

    private void StartStep2()
    {
        _step = Step.SecondDragValue;
        SetInteractable(false);
        if (secondValueButton)
            secondValueButton.interactable = true;

        ShowHandDrag(secondValue, secondTarget);
    }

    private void CompleteTutorial()
    {
        _step = Step.Completed;
        HideHand();
        DoCompletedAnimation();
    }

    private void DoCompletedAnimation()
    {
        foreach (Transform cell in cells)
        {
            cell.DOPunchScale(-Vector3.one * 0.1f, 0.6f, 1).OnComplete(() =>
            {
                if (!_isCompleteTriggered)
                {
                    OnTutorialCompleted?.Invoke();
                    _isCompleteTriggered = true;
                }
            });
        }
    }

    private void AnimateValueToTarget(RectTransform value, RectTransform target, Action onComplete)
    {
        if (value == null || target == null) { onComplete?.Invoke(); return; }

        var canvas = _canvas != null ? _canvas : GetComponentInParent<Canvas>();
        var rect = value;
        var targetSize = target.rect.size;

        var worldPos = rect.position;
        rect.SetParent(canvas.transform, worldPositionStays: true);
        rect.position = worldPos;
        rect.SetAsLastSibling();

        Sequence s = DOTween.Sequence();
        s.Append(rect.DOMove(target.position, moveDuration).SetEase(Ease.OutQuad));
        s.Join(rect.DOSizeDelta(targetSize, moveDuration).SetEase(Ease.OutQuad));
        s.OnComplete(() =>
        {
            rect.SetParent(target, worldPositionStays: false);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = targetSize;
            rect.localScale = Vector3.one;
            onComplete?.Invoke();
            highlight.gameObject.SetActive(false);
            ValueMoved?.Invoke();
        });
    }

    private void ShowHandTapOver(RectTransform target)
    {
        if (handPointer == null || target == null) return;
        handPointer.gameObject.SetActive(true);
        handPointer.transform.DOKill();

        Vector3 worldCenter;
        {
            Vector3[] corners = new Vector3[4];
            target.GetWorldCorners(corners); // 0:BL 1:TL 2:TR 3:BR
            worldCenter = (corners[0] + corners[2]) * 0.5f;
        }

        // Place pointer at center (optional small adjustable offset)
        handPointer.transform.position = worldCenter + new Vector3(0f, handTapYOffset, 0f);
        handPointer.transform.localScale = Vector3.one;

        handPointer.transform
            .DOScale(1f + handPulse, 0.35f)
            .SetEase(Ease.OutQuad)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void ShowHandDrag(RectTransform from, RectTransform to)
    {
        if (handPointer == null || from == null || to == null) return;
        handPointer.gameObject.SetActive(true);
        handPointer.transform.DOKill();
        handPointer.transform.localScale = Vector3.one;

        Sequence s = DOTween.Sequence();
        s.AppendCallback(() => handPointer.transform.position = from.position);
        s.Append(handPointer.transform.DOMove(to.position, 0.8f).SetEase(Ease.InOutQuad));
        s.AppendInterval(0.2f);
        s.SetLoops(-1);
    }

    private void HideHand()
    {
        if (!handPointer) return;
        handPointer.transform.DOKill();
        handPointer.gameObject.SetActive(false);
    }

    private void EnsureDragEvents(GameObject go)
    {
        if (go == null) return;
        var trigger = go.GetComponent<EventTrigger>();
        if (trigger == null) trigger = go.AddComponent<EventTrigger>();
        trigger.triggers ??= new System.Collections.Generic.List<EventTrigger.Entry>();
        trigger.triggers.Clear();

        AddTrigger(trigger, EventTriggerType.BeginDrag, OnSecondValueBeginDrag);
        AddTrigger(trigger, EventTriggerType.Drag, OnSecondValueDrag);
        AddTrigger(trigger, EventTriggerType.EndDrag, OnSecondValueEndDrag);
    }

    private void AddTrigger(EventTrigger trigger, EventTriggerType type, Action<BaseEventData> cb)
    {
        var entry = new EventTrigger.Entry { eventID = type };
        entry.callback.AddListener((data) => cb(data));
        trigger.triggers.Add(entry);
    }

    private void OnSecondValueBeginDrag(BaseEventData data)
    {
        if (_step != Step.SecondDragValue) return;

        TargetClicked?.Invoke();
        highlight.parent = secondValue;
        highlight.position = secondValue.position;
        highlight.sizeDelta = secondValue.sizeDelta * 1.25f;
        highlight.gameObject.SetActive(true);

        HideHand();

        var ped = (PointerEventData)data;
        var rect = secondValue;
        var worldPos = rect.position;
        rect.SetParent(_canvas.transform, worldPositionStays: true);
        rect.position = worldPos;
        rect.SetAsLastSibling();
    }

    private void OnSecondValueDrag(BaseEventData data)
    {
        if (_step != Step.SecondDragValue) return;

        var ped = (PointerEventData)data;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)_canvas.transform, ped.position, _canvas.worldCamera, out var localPoint);

        localPoint.y += dragYOffset;
        secondValue.position = _canvas.transform.TransformPoint(localPoint);
    }

    private void OnSecondValueEndDrag(BaseEventData data)
    {
        if (_step != Step.SecondDragValue) return;

        highlight.gameObject.SetActive(false);


        float dist = Vector3.Distance(secondValue.position, secondTarget.position);
        float acceptRadius = Mathf.Max(secondTarget.rect.width, secondTarget.rect.height) * 1.5f;

        if (dist <= acceptRadius)
        {
            AnimateValueToTarget(secondValue, secondTarget, CompleteTutorial);
        }
        else
        {
            secondValue.SetParent(_originalParentSecondValue, worldPositionStays: false);
            secondValue.DOAnchorPos(_secondValueStartAnchored, 0.25f).SetEase(Ease.OutQuad);
        }
    }
}
