using Gameplay;
using Levels;
using TMPro;
using Tutorial;
using UnityEngine;

namespace UI.Gameplay
{
    public class TutorialGameplayView : BaseView
    {
        [SerializeField] private Board board;
        [SerializeField] private CanvasGroup errorImage;
        [SerializeField] private TextMeshProUGUI errorText;
        [SerializeField] private TutorialConfig classicTutorialConfig;
        [SerializeField] private TutorialConfig mathTutorialConfig;
        [SerializeField] private ParticleSystem confettiParticle;
        [SerializeField] private Sprite classicBackground;
        [SerializeField] private Sprite mathBackground;
        public Board Board => board;

        public TutorialConfig GetTutorialConfig(GameMode gameMode)
        {
            switch (gameMode)
            {
                case GameMode.Math:
                    return mathTutorialConfig;
                default:
                    return classicTutorialConfig;
            }
        }

        public void SetupBoardWithoutShuffle(LevelData levelData, int currentLevelIndex, TutorialConfig tutorialConfig)
        {
            var deckConfig = tutorialConfig != null ? tutorialConfig.deckConfig : null;
            board.Setup(levelData, currentLevelIndex, panel, null, false, deckConfig);
        }

        public void SetBackgroundImage(GameMode gameMode)
        {
            if (backgroundImage == null)
                return;

            backgroundImage.sprite = gameMode == GameMode.Math ? mathBackground : classicBackground;
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
