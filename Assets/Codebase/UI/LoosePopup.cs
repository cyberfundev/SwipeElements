using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Codebase.UI
{
    public class LoosePopup : Popup
    {
        [SerializeField] private Button _restartButton;

        private CompositeDisposable _disposables = new();

        protected override void Initialize()
        {
            base.Initialize();
            _restartButton.OnClickAsObservable().Subscribe(_ => Restart()).AddTo(_disposables);
        }

        private void Restart()
        {
            HideWithResult(PopupResult.Close);
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
            _disposables = new CompositeDisposable();
        }
    }
}