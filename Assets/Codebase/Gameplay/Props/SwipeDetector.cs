using System;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Codebase.Gameplay.Props
{
    public class SwipeDetector : MonoBehaviour
    {
        [SerializeField] private float _dragDistance = 2;

        private readonly ISubject<Vector2Int> _dragListener = new Subject<Vector2Int>();
        private Vector3? _dragStartPosition;
        private bool _endedDrag;

        public IObservable<Vector2Int> DragListener => _dragListener;

        private void Update()
        {
            if (Input.GetMouseButtonUp(0))
            {
                _endedDrag = false;
                _dragStartPosition = null;
            }
        }

        public void OnMouseDrag()
        {
            if (_endedDrag)
            {
                return;
            }
            
            _dragStartPosition ??= Input.mousePosition;

            if (Vector2.Distance(Input.mousePosition, _dragStartPosition.Value) > _dragDistance)
            {
                Vector2 resultSwipeDirection = (Input.mousePosition - _dragStartPosition.Value).normalized;
                bool horizontalDirection = Mathf.Abs(resultSwipeDirection.x) > Mathf.Abs(resultSwipeDirection.y);
                var vector2Int = horizontalDirection
                    ?
                    resultSwipeDirection.x > 0 ? Vector2Int.right : Vector2Int.left
                    : resultSwipeDirection.y > 0
                        ? Vector2Int.up
                        : Vector2Int.down;
                _dragListener.OnNext(vector2Int);
                _endedDrag = true;
            }
        }
    }
}