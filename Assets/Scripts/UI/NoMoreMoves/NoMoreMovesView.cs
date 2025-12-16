using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI.NoMoreMoves
{
    public class NoMoreMovesView: BaseView
    {
        [SerializeField] private Button continueButton;
        [SerializeField] private Button jokerButton;
        [SerializeField] private Button restartButton;
        

        public event Action RestartButtonClicked;
        public event Action ContinueButtonClicked;
        public event Action JokerButtonClicked;

        private void Start()
        {
            restartButton.onClick.AddListener(()=> RestartButtonClicked?.Invoke());
            continueButton.onClick.AddListener(() => ContinueButtonClicked?.Invoke());
            jokerButton.onClick.AddListener(() => JokerButtonClicked?.Invoke());
        }
    }
}