using System;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Codebase.Gameplay.Props
{
    public class PropObject : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private SwipeDetector _swipeDetector;
        [SerializeField] private Animator _animator;
        [SerializeField] private float _destroyTime;

        private readonly int _explodeTrigger = Animator.StringToHash("Explode");
        private static readonly int Idle = Animator.StringToHash("Idle");
        private readonly ISubject<Vector2Int> _onSwiped = new Subject<Vector2Int>();
        
        private CompositeDisposable _disposables = new();

        public IObservable<Vector2Int> OnSwiped => _onSwiped;
        public float DestroyTime => _destroyTime;

        public async UniTaskVoid Initialize()
        {
            _swipeDetector.DragListener.Subscribe(_onSwiped.OnNext).AddTo(_disposables);

            await UniTask.Delay(TimeSpan.FromSeconds(Random.Range(0, 0.3f)));
            _animator.SetTrigger(Idle);
        }

        public void AnimateDestroy()
        {
            _animator.SetTrigger(_explodeTrigger);
        }

        public void OnDestroy()
        {
            _disposables.Dispose();
            _disposables = new CompositeDisposable();
        }
    }
}