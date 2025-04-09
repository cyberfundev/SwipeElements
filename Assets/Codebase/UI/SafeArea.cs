using UnityEngine;

namespace Codebase.UI
{
    public class SafeArea : MonoBehaviour
    {
        [SerializeField] private RectTransform _rect;
        [SerializeField] private Canvas _canvas;
        
        private void Start()
        {
            UpdateSafeArea();
        }

        private void UpdateSafeArea()
        {
            var safeArea = Screen.safeArea;
            

            var anchorMin = safeArea.position;
            var anchorMax = safeArea.position + safeArea.size;
            anchorMin.x /= _canvas.pixelRect.width;
            anchorMin.y /= _canvas.pixelRect.height;
            anchorMax.x /= _canvas.pixelRect.width;
            anchorMax.y /= _canvas.pixelRect.height;

            _rect.anchorMin = anchorMin;
            _rect.anchorMax = anchorMax;
        }
    }
}