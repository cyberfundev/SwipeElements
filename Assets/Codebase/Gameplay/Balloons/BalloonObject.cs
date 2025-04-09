using System;
using DG.Tweening;
using UniRx;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Codebase.Gameplay.Balloons
{
    public class BalloonObject : MonoBehaviour
    {
        [SerializeField] private Vector2 _verticalSpeedRange;
        [SerializeField] private Vector2 _verticalDistanceRange;
        [SerializeField] private Vector2 _horizontalSpeedRange;

        private readonly ISubject<Unit> _reachedRoutEnd = new Subject<Unit>();
        public IObservable<Unit> ReachedRoutEnd => _reachedRoutEnd;

        public void StartMove(bool fromLeftPoint, float flightDistance)
        {
            float horizontalDuration = Random.Range(_horizontalSpeedRange.x, _horizontalSpeedRange.y);
            transform
                .DOMoveX(transform.position.x + flightDistance * (fromLeftPoint ? -1 : 1), horizontalDuration)
                .OnComplete(
                    () => { _reachedRoutEnd.OnNext(Unit.Default); })
                .SetEase(Ease.Linear);

            float verticalRange = Random.Range(_verticalDistanceRange.x, _verticalDistanceRange.y);
            float verticalSpeed = Random.Range(_verticalSpeedRange.x, _verticalSpeedRange.y);
            float initialYPos = transform.position.y;
            transform.DOMoveY(initialYPos + verticalRange, verticalSpeed)
                .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
        }

        private void OnDisable()
        {
            transform.DOKill();
        }
    }
}