using UnityEngine;
using UnityEngine.UI;

namespace UI.Settings
{
    public class WaitingView : BaseView
    {
        [SerializeField] private Image spinnerImage;
        [SerializeField] private float rotationSpeedDegreesPerSecond = 360f;

        private bool _isRotating;

        private void Update()
        {
            if (!_isRotating || spinnerImage == null)
            {
                return;
            }

            spinnerImage.rectTransform.Rotate(0f, 0f, -rotationSpeedDegreesPerSecond * Time.unscaledDeltaTime);
        }

        protected override void OnShown()
        {
            base.OnShown();
            _isRotating = true;
        }

        protected override void OnHidden()
        {
            base.OnHidden();
            _isRotating = false;
        }
    }
}