using JetBrains.Annotations;
using UnityEngine;

namespace Codebase.Levels
{
    [UsedImplicitly]
    public class LevelResizer
    {
        private readonly Vector2 _defaultLevelSize = new Vector2Int(5, 7);
        private readonly Transform _elementsParent;

        public LevelResizer(Transform elementsParent)
        {
            _elementsParent = elementsParent;
        }

        public void SetupLevel(Vector2Int levelSize)
        {
            var change = levelSize.x - _defaultLevelSize.x;
            Vector3 offset = _elementsParent.localPosition;
            var verticalChange = _defaultLevelSize.y / levelSize.y;

            offset.x -= change;

            _elementsParent.localPosition = offset;
            _elementsParent.localScale *= verticalChange < 1 ? verticalChange : 1;
        }
    }
}
