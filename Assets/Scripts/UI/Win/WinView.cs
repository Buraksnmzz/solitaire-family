using System;
using UnityEngine.UI;

namespace UI.Win
{
    public class WinView: BaseView
    {
        public Button continueButton;
        
        public event Action ContinueButtonClicked;

        private void Start()
        {
            continueButton.onClick.AddListener(()=>ContinueButtonClicked?.Invoke());
        }
    }
}