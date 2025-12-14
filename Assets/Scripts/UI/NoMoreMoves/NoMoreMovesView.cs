using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI.NoMoreMoves
{
    public class NoMoreMovesView: BaseView
    {
        [SerializeField] private Button restartButton;
        [SerializeField] private Button addMoves;

        public event Action RestartButtonClicked;
        public event Action AddMovesClicked;

        private void Start()
        {
            restartButton.onClick.AddListener(()=> RestartButtonClicked?.Invoke());
            addMoves.onClick.AddListener(()=> AddMovesClicked?.Invoke());
        }
    }
}