using JetBrains.Annotations;
using UnityEngine;

namespace Codebase.Gameplay.LevelManager
{
    [UsedImplicitly]
    public class CameraResizer
    {
        private readonly Vector2 _baseResolution = new(1080f, 1920f);
        private const float BaseOrthographicSize = 11.3f;
        
        public void Initialize(Camera camera)
        {
            if (camera.aspect > _baseResolution.x / _baseResolution.y)
            {
                camera.orthographicSize = BaseOrthographicSize * Screen.width / Screen.height;
            }
            // else
            // {
                // camera.orthographicSize = BaseOrthographicSize * _baseResolution.y / (2f * Screen.height);;
            // }
        }
    }
}