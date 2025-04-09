using Codebase.Gameplay.LevelManager;
using JetBrains.Annotations;
using UnityEngine;

namespace Codebase.Levels
{
    [UsedImplicitly]
    public class LevelResizer
    {
        private readonly Vector2 _defaultLevelSize = new Vector2Int(5, 7);
        private readonly Transform _elementsParent;
        private readonly float _baseScale = 2;
        private CameraResizer _resizer;

        public LevelResizer(Transform elementsParent, CameraResizer resizer)
        {
            _resizer = resizer;
            _elementsParent = elementsParent;
        }

        public void SetupLevel(Vector2Int levelSize)
        {
            var change = levelSize.x - _defaultLevelSize.x;
            Vector3 offset = _elementsParent.localPosition;
            var verticalChange = _defaultLevelSize.y / levelSize.y;


            _elementsParent.localScale = Vector2.one * (_baseScale * _resizer.WidthChange);
            _elementsParent.localScale *= verticalChange < 1 ? verticalChange : 1;
            offset.x = -(levelSize.x - 1) * _elementsParent.localScale.x / 2;
            _elementsParent.localPosition = offset;
        }
    }
}
