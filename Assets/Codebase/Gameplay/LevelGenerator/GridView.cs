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

        public async UniTask AnimateSwipe(Prop swipePair, Prop prop, Vector2Int pairPos, Vector2Int mainPos)
        {
            if (swipePair != null)
            {
                AlignToPos(swipePair, pairPos);
            }

            var tween = AlignToPos(prop, mainPos);
            await tween.AsyncWaitForCompletion();
        }

        private TweenerCore<Vector3, Vector3, VectorOptions> AlignToPos(Prop swipePair, Vector2Int vector2Int)
        {
            return _propsContainer.Props[swipePair].transform
                .DOLocalMove(new Vector3(vector2Int.x, vector2Int.y), _swipeTime);
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

        public async UniTask AlignField(List<(Prop prop, Vector2Int pos)> propsToMove)
        {
            Tween tween = null;
            foreach ((Prop prop, Vector2Int pos) propInfo in propsToMove)
            {
                var propObject = _propsContainer.Props[propInfo.prop];
                tween = propObject.transform.DOLocalMoveY(propInfo.pos.y, _swipeTime);
            }

            if (tween != null)
            {
                await tween.AsyncWaitForCompletion();
            }
        }
    }
}