using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Codebase.UI
{
    public class LevelUI : MonoBehaviour
    {
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _nextButton;

        private readonly ISubject<Unit> _restartListener = new Subject<Unit>();
        private readonly ISubject<Unit> _nextListener = new Subject<Unit>();
        
        public IObservable<Unit> OnRestartClicked => _restartListener;
        public IObservable<Unit> OnNextClicked => _nextListener;

        private CompositeDisposable _disposables = new();


        public void Start()
        {
            _restartButton
                .OnClickAsObservable()
                .Subscribe(_ => _restartListener.OnNext(Unit.Default))
                .AddTo(_disposables);
            _nextButton
                .OnClickAsObservable()
                .Subscribe(_ => _nextListener.OnNext(Unit.Default))
                .AddTo(_disposables);
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
            _disposables = new CompositeDisposable();
        }
    }
}
