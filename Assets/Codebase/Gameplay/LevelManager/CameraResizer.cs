using JetBrains.Annotations;
using UnityEngine;

namespace Codebase.Gameplay.LevelManager
{
    [UsedImplicitly]
    public class CameraResizer
    {
        private readonly Vector2 _baseResolution = new(1080f, 1920f);
        private float _baseRatio => _baseResolution.x / _baseResolution.y;
        private Camera _camera;
        private const float BaseOrthographicSize = 11.3f;
        
        public float OrthographicSizeChange => _camera.orthographicSize / BaseOrthographicSize;
        public float WidthChange => _camera.aspect < _baseRatio ? _camera.aspect / _baseRatio : 1;

        public void Initialize(Camera camera)
        {
            _camera = camera;
            if (camera.aspect > _baseRatio)
            {
                camera.orthographicSize = BaseOrthographicSize * Screen.width / Screen.height;
            }
        }
    }
}