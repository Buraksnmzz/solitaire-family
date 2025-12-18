using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.OutOfMoves
{
    public class OutOfMovesView : BaseView
    {
        [SerializeField] private Button restartButton;
        [SerializeField] private Button addMovesButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private TextMeshProUGUI usageCountText;
        [SerializeField] private TextMeshProUGUI extraMovesCostText;

        public event Action RestartButtonClicked;
        public event Action AddMovesClicked;
        public event Action ContinueButtonClicked;
        public event Action CloseButtonClicked;

        private void Start()
        {
            restartButton.onClick.AddListener(() => RestartButtonClicked?.Invoke());
            addMovesButton.onClick.AddListener(() => AddMovesClicked?.Invoke());
            continueButton.onClick.AddListener(() => ContinueButtonClicked?.Invoke());
            closeButton.onClick.AddListener(()=> CloseButtonClicked?.Invoke());
        }

        public void SetUsageText(int count, int total)
        {
            usageCountText.text = count + "/" + total;
        }

        public void SetAddMovesButtonActive(bool isActive)
        {
            addMovesButton.gameObject.SetActive(isActive);
        }

        public void SetExtraMovesCostText(int extraMovesCost)
        {
            extraMovesCostText.text = extraMovesCost.ToString();
        }
    }
}