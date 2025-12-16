using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.OutOfMoves
{
    public class OutOfMovesView: BaseView
    {
        [SerializeField] private Button restartButton;
        [SerializeField] private Button addMovesButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private TextMeshProUGUI usageCountText;

        public event Action RestartButtonClicked;
        public event Action AddMovesClicked;
        public event Action ContinueButtonClicked;

        private void Start()
        {
            restartButton.onClick.AddListener(()=> RestartButtonClicked?.Invoke());
            addMovesButton.onClick.AddListener(()=> AddMovesClicked?.Invoke());
            continueButton.onClick.AddListener(()=> ContinueButtonClicked?.Invoke());
        }

        public void SetUsageText(int count)
        {
            usageCountText.text = count + "/9";
        }
    }
}