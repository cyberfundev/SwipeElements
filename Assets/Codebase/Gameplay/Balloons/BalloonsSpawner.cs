using System;
using System.Collections.Generic;
using Codebase.Gameplay.LevelManager;
using UniRx;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace Codebase.Gameplay.Balloons
{
    public class BalloonsSpawner : MonoBehaviour
    {
        [SerializeField] private Transform _balloonsParent;
        [SerializeField] private BalloonObject _balloon1Prefab;
        [SerializeField] private BalloonObject _balloon2Prefab;
        [Space] 
        [SerializeField] private Vector2 _leftStartPoint;
        [SerializeField] private float _heightRange;
        [SerializeField] private int _maxAmount;

        private readonly float _spawnInterval = 3f;
        private float _timeSinceSpawn = 0;

        private float _flightDistance => _leftStartPoint.x * 2;
        private int _balloonsCount => _balloonsDisposables.Count;

        private SimplePool<BalloonObject> _pool1;
        private SimplePool<BalloonObject> _pool2;

        private readonly Dictionary<BalloonObject, IDisposable> _balloonsDisposables = new();

        [Inject]
        private void Construct(CameraResizer cameraResizer)
        {
            _leftStartPoint.x *= cameraResizer.WidthChange;
        }

        private void Start()
        {
            _pool1 = new SimplePool<BalloonObject>(_balloonsParent, _balloon1Prefab, 3);
            _pool2 = new SimplePool<BalloonObject>(_balloonsParent, _balloon2Prefab, 3);

        }

        private void Update()
        {
            if (_timeSinceSpawn <= 0 && _balloonsCount < _maxAmount)
            {
                SpawnBalloon();
                _timeSinceSpawn = _spawnInterval;
            }
            else
            {
                _timeSinceSpawn -= Time.deltaTime;
            }
        }

        private void SpawnBalloon()
        {
            bool fromLeftPoint = Random.Range(0, 2) == 0;
            bool firstBalloonVariant = Random.Range(0, 2) == 0;

            BalloonObject spawnedBalloonObject = firstBalloonVariant ? _pool1.GetElement() : _pool2.GetElement();

            Vector2 spawnPosition = _leftStartPoint;
            spawnPosition *= fromLeftPoint ? Vector2.one : new Vector2(-1, 1);
            spawnPosition.y += Random.Range(-_heightRange, _heightRange);
            spawnedBalloonObject.transform.position = spawnPosition;

            spawnedBalloonObject.StartMove(fromLeftPoint, _flightDistance);
            _balloonsDisposables[spawnedBalloonObject] = spawnedBalloonObject.ReachedRoutEnd
                .Subscribe(_ => DespawnBalloon(firstBalloonVariant, spawnedBalloonObject));
        }

        private void DespawnBalloon(bool firstBalloonVariant, BalloonObject spawnedBalloonObject)
        {
            if (firstBalloonVariant)
            {
                _pool1.Despawn(spawnedBalloonObject);
            }
            else
            {
                _pool2.Despawn(spawnedBalloonObject);
            }
            
            _balloonsDisposables[spawnedBalloonObject].Dispose();
            _balloonsDisposables.Remove(spawnedBalloonObject);
        }

        private void OnDestroy()
        {
            foreach (var disposable in _balloonsDisposables)
            {
                disposable.Value.Dispose();
            }
        }
    }
}