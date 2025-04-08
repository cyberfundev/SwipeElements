using UnityEngine;

namespace LevelManagement
{
    public class LevelResizer : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector2 _defaultLevelSize = new Vector2Int(5, 5);

        public float SetupLevel(Vector2Int levelSize)
        {
            var change = Mathf.Min(_defaultLevelSize.x / levelSize.x, _defaultLevelSize.y / levelSize.y);
            change *= 1.3f;
            target.localScale = Vector3.one * change;
            Vector3 offset = Vector3.zero;
            
            if (levelSize.x % 2 == 0) {
                offset -= Vector3.right * 0.5f * change;
            }
            
            if (levelSize.y % 2 == 0) {
                offset -= Vector3.forward * 0.5f * change;
            }

            target.localPosition += offset;
            return change;
        }
    }
}
