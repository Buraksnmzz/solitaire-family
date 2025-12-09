using DG.Tweening;
using Gameplay;
using Services;
using Services.Drag;
using UI.Signals;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Card
{
    public class CardDragger : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] float snapDistance = 70f;

        CardPresenter _presenter;
        RectTransform _canvasTransform;
        Canvas _canvas;
        Vector3 _startPosition;
        Transform _startParent;
        int _startSiblingIndex;
        CardPresenter[] _draggedPresenters;
        Vector3[] _startLocalPositions;
        IEventDispatcherService _eventDispatcherService;
        IDragStateService _dragStateService;
        bool _isDragging;
        private readonly float _moveDuration = 0.25f;

        public void Setup(CardPresenter presenter, Transform parent)
        {
            _eventDispatcherService = ServiceLocator.GetService<IEventDispatcherService>();
            _dragStateService = ServiceLocator.GetService<IDragStateService>();
            _presenter = presenter;
            _canvasTransform = parent as RectTransform;
            var canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                _canvas = canvas;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_dragStateService == null) return;
            if (!_dragStateService.CanStartDrag()) return;
            if (!IsDraggable()) return;

            _dragStateService.StartDrag();
            _isDragging = true;


            _startParent = transform.parent;
            _startSiblingIndex = transform.GetSiblingIndex();

            var container = _presenter.GetContainer();
            if (container == null) return;

            var stack = container.GetCardsFrom(_presenter);
            _draggedPresenters = stack.ToArray();
            _startLocalPositions = new Vector3[_draggedPresenters.Length];

            for (var i = 0; i < _draggedPresenters.Length; i++)
            {
                var view = _draggedPresenters[i].CardView;
                if (view == null) continue;
                var rectTransform = view.transform as RectTransform;
                if (rectTransform == null) continue;
                _startLocalPositions[i] = rectTransform.localPosition;
                rectTransform.SetParent(_canvasTransform);
                rectTransform.SetAsLastSibling();
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDragging) return;
            if (!IsDraggable()) return;
            if (_canvasTransform == null || _canvas == null) return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvasTransform,
                eventData.position,
                _canvas.worldCamera,
                out var localPoint);

            var worldPoint = _canvas.transform.TransformPoint(localPoint);
            var delta = worldPoint - transform.position;

            if (_draggedPresenters == null || _draggedPresenters.Length == 0)
            {
                transform.position = worldPoint;
                return;
            }

            for (var i = 0; i < _draggedPresenters.Length; i++)
            {
                var view = _draggedPresenters[i].CardView;
                if (view == null) continue;
                view.transform.position += delta;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_isDragging) return;
            if (_dragStateService == null) return;
            if (!IsDraggable())
            {
                _dragStateService.EndDrag();
                _isDragging = false;
                return;
            }

            _eventDispatcherService.Dispatch(new CardMovementStateChangedSignal(true));

            var containers = FindObjectsOfType<CardContainer>();
            CardContainer closestContainer = null;
            var closestDistance = float.MaxValue;

            foreach (var container in containers)
            {
                var rectTransform = container.transform as RectTransform;
                if (rectTransform == null) continue;

                var distance = Vector3.Distance(transform.position, rectTransform.position);
                if (distance < closestDistance && _presenter.GetContainer() != container)
                {
                    closestDistance = distance;
                    closestContainer = container;
                }
            }

            if (closestContainer != null && closestDistance <= snapDistance)
            {
                if (closestContainer.CanPlaceCard(_presenter))
                {
                    var currentContainer = _presenter.GetContainer();
                    if (currentContainer != null)
                    {
                        var stack = currentContainer.GetCardsFrom(_presenter).ToArray();

                        var movedFaceUpStates = new bool[stack.Length];
                        for (var i = 0; i < stack.Length; i++)
                        {
                            movedFaceUpStates[i] = stack[i].IsFaceUp;
                        }

                        var before = currentContainer.GetCardsBefore(_presenter);
                        var previousCard = before.Count > 0 ? before[^1] : null;
                        var previousCardWasFaceUp = previousCard is { IsFaceUp: true };

                        currentContainer.RemoveCardsFrom(_presenter);
                        for (var i = 0; i < stack.Length; i++)
                        {
                            var presenter = stack[i];
                            closestContainer.AddCard(presenter);
                        }

                        _eventDispatcherService.Dispatch(
                            new CardMovePerformedSignal(
                                currentContainer,
                                closestContainer,
                                stack,
                                movedFaceUpStates,
                                previousCard,
                                previousCardWasFaceUp));

                        _eventDispatcherService.Dispatch(new MoveCountRequestedSignal());

                        if (currentContainer is Pile)
                        {
                            currentContainer.RevealTopCardIfNeeded();
                        }

                        _dragStateService.EndDrag();
                        _isDragging = false;
                    }
                    return;
                }
            }

            if (_draggedPresenters != null)
            {
                var remainingAnimations = _draggedPresenters.Length;
                for (var i = 0; i < _draggedPresenters.Length; i++)
                {
                    var presenter = _draggedPresenters[i];
                    var view = presenter.CardView;
                    if (view == null) continue;
                    var viewRectTransform = view.transform as RectTransform;
                    if (viewRectTransform == null) continue;

                    var targetLocalInStartParent = _startLocalPositions[i];
                    var startParentRect = _startParent as RectTransform;
                    if (startParentRect == null) continue;

                    var worldTargetInStartParent = startParentRect.TransformPoint(targetLocalInStartParent);
                    var canvasLocalTarget = _canvasTransform.InverseTransformPoint(worldTargetInStartParent);

                    viewRectTransform.DOKill();
                    viewRectTransform.SetAsLastSibling();
                    viewRectTransform.DOLocalMove(canvasLocalTarget, _moveDuration)
                        .SetEase(DG.Tweening.Ease.OutQuad)
                        .OnComplete(() =>
                        {
                            viewRectTransform.SetParent(_startParent, true);
                            viewRectTransform.localPosition = targetLocalInStartParent;
                            viewRectTransform.SetSiblingIndex(_startSiblingIndex + i);

                            remainingAnimations--;
                            if (remainingAnimations <= 0)
                            {
                                _eventDispatcherService.Dispatch(new CardMovementStateChangedSignal(false));
                                _dragStateService.EndDrag();
                                _isDragging = false;
                            }
                        });
                }
            }

            _draggedPresenters = null;
            _startLocalPositions = null;
            _isDragging = false;
        }

        bool IsDraggable()
        {
            if (_presenter == null) return false;

            var container = _presenter.GetContainer();
            if (container == null) return false;

            var containerType = container.GetType();

            if (containerType == typeof(Foundation)) return false;
            if (containerType == typeof(Dealer)) return false;

            if (containerType == typeof(OpenDealer))
            {
                var topCard = container.GetTopCard();
                return topCard == _presenter;
            }

            if (containerType == typeof(Pile))
            {
                var pile = container as Pile;
                return pile != null && pile.CanDrag(_presenter);
            }

            return false;
        }
    }
}
