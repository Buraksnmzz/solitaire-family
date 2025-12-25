using DG.Tweening;

namespace Gameplay
{
    public class DealerTutorial: Dealer
    {
        public override void PlayHintCue(float fadeDuration = 0.4f, float holdDuration = 1f)
        {
            if (dealerHint == null) return;

            _hintSequence?.Kill();
            dealerHint.DOKill();
            dealerHint.alpha = 0f;
            dealerHint.gameObject.SetActive(true);

            _hintSequence = DOTween.Sequence();
            _hintSequence.Append(dealerHint.DOFade(1f, fadeDuration));
            _hintSequence.AppendInterval(holdDuration);
            _hintSequence.Append(dealerHint.DOFade(0f, fadeDuration));
            _hintSequence.OnComplete(() => { dealerHint.gameObject.SetActive(false); _hintSequence = null; });
        }
    }
}