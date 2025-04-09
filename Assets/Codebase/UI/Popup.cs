using Cysharp.Threading.Tasks;
using DG.Tweening;
using UniRx;
using UnityEngine;

namespace Codebase.UI
{
    public class Popup<TResult> : MonoBehaviour
    {
        private TResult _currentResult;
        private Sequence _animationSequence;

        protected readonly ReactiveProperty<PopupState> State = new();
        
        protected virtual void Initialize()
        {
            
        }

        public void Hide()
        {
            State.Value = PopupState.Hiding;
            gameObject.SetActive(false);
            State.Value = PopupState.Hidden;
        }

        public virtual void HideWithResult(TResult result)
        {
            _currentResult = result;
            Hide();
        }

        public virtual async UniTask<TResult> ShowAndWaitForResult(bool animated = true)
        {
            Initialize();
            
            await ShowPopup(animated);

            await UniTask.WaitUntil(() => State.Value == PopupState.Hidden, PlayerLoopTiming.PreLateUpdate);

            return _currentResult;
        }

        private async UniTask ShowPopup(bool animated)
        {
            State.Value = PopupState.Appear;
            gameObject.SetActive(true);
            
            if (animated) {
                _animationSequence?.Kill();
                _animationSequence = DOTween.Sequence();

                transform.localScale = Vector3.one * 0.5f;
                _animationSequence.Append(transform.DOScale(1.1f, 0.1f));
                _animationSequence.Append(transform.DOScale(1f, 0.1f));

                await _animationSequence.AsyncWaitForCompletion();
            }
            State.Value = PopupState.Show;
        }

        protected enum PopupState
        {
            Hidden,
            Appear,
            Show,
            Hiding,
        }
    }

    public class Popup : Popup<Popup.PopupResult>
    {
        public enum PopupResult
        {
            Ok,
            Close,
        }
    }
}