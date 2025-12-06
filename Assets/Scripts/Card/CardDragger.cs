using Card;
using Gameplay;
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

        public void Setup(CardPresenter presenter)
        {
            _eventDispatcherService = ServiceLocator.GetService<IEventDispatcherService>();
            _presenter = presenter;
            var canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                _canvas = canvas;
                _canvasTransform = canvas.transform as RectTransform;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!IsDraggable()) return;

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
            if (!IsDraggable()) return;

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
                        // 1) Taşınacak stack ve faceUp snapshot
                        var stack = currentContainer.GetCardsFrom(_presenter).ToArray();

                        var movedFaceUpStates = new bool[stack.Length];
                        for (var i = 0; i < stack.Length; i++)
                        {
                            movedFaceUpStates[i] = stack[i].IsFaceUp;
                        }

                        // 2) Stack’in altındaki “previous” kart ve eski yüz durumu
                        var before = currentContainer.GetCardsBefore(_presenter);
                        var previousCard = before.Count > 0 ? before[before.Count - 1] : null;
                        var previousCardWasFaceUp = previousCard != null && previousCard.IsFaceUp;

                        // 3) Gerçek taşıma
                        currentContainer.RemoveCardsFrom(_presenter);
                        for (var i = 0; i < stack.Length; i++)
                        {
                            var presenter = stack[i];
                            closestContainer.AddCard(presenter);
                        }

                        // 4) Snapshot sinyalini gönder
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
                    }

                    _draggedPresenters = null;
                    _startLocalPositions = null;
                    return;
                }
            }

            if (_draggedPresenters != null)
            {
                for (var i = 0; i < _draggedPresenters.Length; i++)
                {
                    var presenter = _draggedPresenters[i];
                    var view = presenter.CardView;
                    if (view == null) continue;
                    view.transform.SetParent(_startParent);
                    view.transform.SetSiblingIndex(_startSiblingIndex + i);
                    presenter.MoveToLocalPosition(_startLocalPositions[i], 0.2f);
                }
            }

            _draggedPresenters = null;
            _startLocalPositions = null;
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
                return _presenter.IsFaceUp;
            }

            return false;
        }
    }
}
