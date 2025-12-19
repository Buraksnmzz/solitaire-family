using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Shop
{
    public class ShopView: BaseView
    {
        [SerializeField] private Button closeButton;

        private void Start()
        {
            closeButton.onClick.AddListener(Hide);
        }
    }
}