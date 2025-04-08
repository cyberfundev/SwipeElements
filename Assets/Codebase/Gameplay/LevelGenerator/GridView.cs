using System;
using System.Collections.Generic;
using Codebase.Levels;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace Codebase.Gameplay.LevelGenerator
{
    public class GridView
    {
        private readonly GridAnalyser _gridAnalyser;
        private readonly PropsContainer _propsContainer;

        private readonly float _swipeTime = 0.5f;

        public GridView(GridAnalyser gridAnalyser, PropsContainer propsContainer)
        {
            _propsContainer = propsContainer;
            _gridAnalyser = gridAnalyser;
        }

        public async UniTask AnimateSwipe(Prop swipePair, Prop prop)
        {
            if (swipePair != null)
            {
                AlignToPos(swipePair);
            }

            var tween = AlignToPos(prop);
            await tween.AsyncWaitForCompletion();
        }

        private TweenerCore<Vector3, Vector3, VectorOptions> AlignToPos(Prop swipePair)
        {
            return _propsContainer.Props[swipePair].transform
                .DOLocalMove(new Vector3(swipePair.Position.x, swipePair.Position.y), _swipeTime);
        }

        public async UniTask AnimateDestroyObjects(List<Prop> propsToDestroy)
        {
            float destroyTime = 0;
            foreach (Prop prop in propsToDestroy)
            {
                var propObject = _propsContainer.Props[prop];
                propObject.AnimateDestroy();
                destroyTime = Mathf.Max(destroyTime, propObject.DestroyTime);
            }

            await UniTask.Delay(TimeSpan.FromSeconds(destroyTime));
        }

        public async UniTask AlignField(List<Prop> propsToMove)
        {
            Tween tween = null;
            foreach (Prop prop in propsToMove)
            {
                var propObject = _propsContainer.Props[prop];
                tween = propObject.transform.DOLocalMoveY(prop.Position.y, _swipeTime);
            }

            if (tween != null)
            {
                await tween.AsyncWaitForCompletion();
            }
        }
    }
}