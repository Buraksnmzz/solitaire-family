using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.NoMoreMoves
{
    public class NoMoreMovesView : BaseView
    {
        [SerializeField] private Button continueButton;
        [SerializeField] private Button jokerButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private TextMeshProUGUI usageCountText;
        [SerializeField] private TextMeshProUGUI jokerCostText;


        public event Action RestartButtonClicked;
        public event Action ContinueButtonClicked;
        public event Action JokerButtonClicked;
        public event Action CloseButtonClicked;

        private void Start()
        {
            restartButton.onClick.AddListener(() => RestartButtonClicked?.Invoke());
            continueButton.onClick.AddListener(() => ContinueButtonClicked?.Invoke());
            jokerButton.onClick.AddListener(() => JokerButtonClicked?.Invoke());
            closeButton.onClick.AddListener(() => CloseButtonClicked?.Invoke());
        }

        public void SetUsageText(int count, int total)
        {
            usageCountText.text = count + "/" + total;
        }

        public void SetJokerButtonActive(bool isActive)
        {
            jokerButton.gameObject.SetActive(isActive);
        }

        public void SetJokerCostText(int cost)
        {
            jokerCostText.text = cost.ToString();
        }
    }
}