using DG.Tweening;
using UnityEngine;

namespace Gameplay
{
    public class DealerTutorial : Dealer
    {
        [SerializeField] private Transform handImage;

        protected override float DefaultHintFadeDuration => 0.4f;
        protected override float DefaultHintHoldDuration => 1f;

        protected override void PlayHintCue(float fadeDuration, float holdDuration)
        {
            if (dealerHint == null) return;

            _hintSequence?.Kill();
            dealerHint.DOKill();
            dealerHint.alpha = 0f;
            dealerHint.gameObject.SetActive(true);

            _hintSequence = DOTween.Sequence();
            handImage.DOScale(Vector3.one * 1.2f, fadeDuration)
                .OnComplete(() => handImage.DOScale(Vector3.one * 1f, fadeDuration));
            _hintSequence.Append(dealerHint.DOFade(1f, fadeDuration));
            _hintSequence.AppendInterval(holdDuration);
            _hintSequence.Append(dealerHint.DOFade(0f, fadeDuration));
            _hintSequence.OnComplete(() => { dealerHint.gameObject.SetActive(false); _hintSequence = null; });
        }
    }
}