using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Codebase.Levels;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;

namespace Codebase.Gameplay.LevelGenerator
{
    public class GamePlayProcessor : IDisposable
    {
        private readonly LevelGenerator _levelGenerator;
        private readonly GridAnalyser _gridAnalyser;
        private readonly GridView _gridView;
        private readonly PropsContainer _propsContainer;

        private readonly ISubject<Unit> _onWinCalculated = new Subject<Unit>();
        private readonly ISubject<Unit> _onLoseCalculated = new Subject<Unit>();
        private readonly ISubject<Unit> _onAnimationsCompleted = new Subject<Unit>();
        
        private int _animationCounter = 0;
        private CancellationTokenSource _animationCancellationToken = new CancellationTokenSource();

        public IObservable<Unit> OnWinCalculated => _onWinCalculated;
        public IObservable<Unit> OnLoseCalculated => _onLoseCalculated;
        public IObservable<Unit> OnAnimationsCompleted => _onAnimationsCompleted;

        public GamePlayProcessor(LevelGenerator levelGenerator, GridAnalyser gridAnalyser, GridView gridView,
            PropsContainer propsContainer)
        {
            _propsContainer = propsContainer;
            _gridView = gridView;
            _gridAnalyser = gridAnalyser;
            _levelGenerator = levelGenerator;

            propsContainer.OnSwiped.Subscribe(value => ProcessSwipe(value.Item1, value.Item2).Forget());
        }

        public void CreateLevel(Level level)
        {
            _levelGenerator.GenerateLevel(level);
            _gridAnalyser.Initialize(level.Props);
        }

        private async UniTaskVoid ProcessSwipe(Prop swipedProp, Vector2Int direction)
        {
            if (!_gridAnalyser.CanSwipe(swipedProp.Position, direction) || swipedProp.State is not Prop.PropState.Idle)
            {
                return;
            }

            Vector2Int propSwipedPos = swipedProp.Position + direction;
            Vector2Int propInitialPos = swipedProp.Position;
            var swipePair = _gridAnalyser.ProcessSwipe(swipedProp.Position, direction);

            List<Prop> propsToMove = _gridAnalyser.UpdatePositions();

            List<List<(Prop, Vector2Int)>> propsSequenceToMove = new List<List<(Prop, Vector2Int)>>();
            List<List<Prop>> propsSequenceToDestroy = new List<List<Prop>>();

            do
            {
                List<Prop> propsToDestroy = _gridAnalyser.AnalyseMerges();

                SetPropsState(propsToMove, Prop.PropState.WaitingAnimation);
                SetPropsState(propsToDestroy, Prop.PropState.WaitingAnimation);

                var propsToMoveList = new List<(Prop, Vector2Int)>();
                for (int i = 0; i < propsToMove.Count; i++)
                {
                    propsToMoveList.Add((propsToMove[i], propsToMove[i].Position));
                }

                propsSequenceToMove.Add(propsToMoveList);

                if (propsToDestroy.Count > 0)
                {
                    propsSequenceToDestroy.Add(propsToDestroy);
                }

                propsToMove = _gridAnalyser.UpdatePositions();
            } while (propsToMove.Count > 0);

            CheckLevelState();

            await CompleteAnimations(swipedProp, propSwipedPos, swipePair, propInitialPos, propsSequenceToMove,
                propsSequenceToDestroy, _animationCancellationToken.Token);

            TriggerFinishAnimations();
        }

        private async UniTask CompleteAnimations(Prop swipedProp, Vector2Int propSwipedPos,
            Prop swipePair, Vector2Int propInitialPos,
            List<List<(Prop, Vector2Int)>> propsSequenceToMove,
            List<List<Prop>> propsSequenceToDestroy, CancellationToken cancellationToken)
        {
            _animationCounter++;
            await _gridView.AnimateSwipe(swipePair, swipedProp, propInitialPos, propSwipedPos);
            List<Prop> processedProps = new List<Prop>();

            for (int i = 0, j = 0; i + j < propsSequenceToMove.Count + propsSequenceToDestroy.Count;)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
                
                if (propsSequenceToMove.Count > i)
                {
                    if (propsSequenceToMove[i].Count > 0)
                    {
                        await _gridView.AlignField(propsSequenceToMove[i]);
                        processedProps.AddRange(propsSequenceToMove[i].Select(x => x.Item1));
                    }
                    i++;
                }
                
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                if (propsSequenceToDestroy.Count > j)
                {
                    await _gridView.AnimateDestroyObjects(propsSequenceToDestroy[j]);
                    processedProps.AddRange(propsSequenceToDestroy[j]);
                    _propsContainer.RemoveProps(propsSequenceToDestroy[j]);
                    j++;
                }
            }

            for (int i = 0; i < processedProps.Count; i++)
            {
                processedProps[i].SetState(Prop.PropState.Idle);
            }

            await UniTask.Delay(TimeSpan.FromSeconds(0.3f), cancellationToken: cancellationToken);
            _animationCounter--;
        }

        private void TriggerFinishAnimations()
        {
            if (_animationCounter == 0)
            {
                _onAnimationsCompleted.OnNext(Unit.Default);
            }
        }

        private void CheckLevelState()
        {
            if (_gridAnalyser.PropsCount == 0)
            {
                WinLevel();
            }
            else if (_gridAnalyser.NoMatchesLeft)
            {
                LoseLevel();
            }
        }

        private void LoseLevel()
        {
            _onLoseCalculated.OnNext(Unit.Default);
        }

        private void WinLevel()
        {
            _onWinCalculated.OnNext(Unit.Default);
        }

        private static void SetPropsState(List<Prop> propsToMove, Prop.PropState state)
        {
            foreach (Prop prop in propsToMove)
            {
                prop.SetState(state);
            }
        }

        public void Dispose()
        {
            _animationCancellationToken.Cancel();
        }
    }
}