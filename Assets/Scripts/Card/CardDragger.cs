using Card;
using Gameplay;
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

        public void Setup(CardPresenter presenter)
        {
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
            _startPosition = transform.localPosition;

            transform.SetParent(_canvasTransform);
            transform.SetAsLastSibling();
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

            transform.position = _canvas.transform.TransformPoint(localPoint);
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
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestContainer = container;
                }
            }

            if (closestContainer != null && closestDistance <= snapDistance)
            {
                var targetTop = closestContainer.GetTopCardModel();
                if (closestContainer.CanPlaceCard(_presenter.CardModel))
                {
                    if (_presenter.CardModel.Container != null)
                    {
                        _presenter.CardModel.Container.RemoveCard(_presenter.CardModel);
                    }

                    closestContainer.AddCard(_presenter.CardView, _presenter.CardModel);
                    _presenter.CardModel.Container = closestContainer;
                    return;
                }
            }

            transform.SetParent(_startParent);
            transform.SetSiblingIndex(_startSiblingIndex);
            transform.localPosition = _startPosition;
        }

        bool IsDraggable()
        {
            if (_presenter == null) return false;
            var containerType = _presenter.CardModel.ContainerType;

            if (containerType == CardContainerType.Foundation) return false;
            if (containerType == CardContainerType.Dealer) return false;

            return containerType == CardContainerType.OpenDealer || containerType == CardContainerType.Pile;
        }
    }
}
