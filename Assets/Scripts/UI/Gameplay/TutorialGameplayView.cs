using Gameplay;
using Levels;
using TMPro;
using Tutorial;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Gameplay
{
    public class TutorialGameplayView : BaseView
    {
        public Board board;
        [SerializeField] private CanvasGroup errorImage;
        [SerializeField] private TextMeshProUGUI errorText;
        [SerializeField] private TutorialConfig tutorialConfig;
        [SerializeField] private ParticleSystem confettiParticle;
        public Board Board => board;
        public TutorialConfig TutorialConfig => tutorialConfig;

        public void SetupBoardWithoutShuffle(LevelData levelData, int currentLevelIndex)
        {
            var deckConfig = tutorialConfig != null ? tutorialConfig.deckConfig : null;
            board.Setup(levelData, currentLevelIndex, panel, null, false, deckConfig);
        }

        public void ShowInstructionMessage(string instruction)
        {
            errorText.SetText(instruction);
            errorImage.gameObject.SetActive(true);
            errorImage.alpha = 1f;
        }

        public void SetErrorImage(bool isShow)
        {
            errorImage.gameObject.SetActive(isShow);
        }

        public void PlayConfetti()
        {
            confettiParticle.Play();
        }
    }
}
