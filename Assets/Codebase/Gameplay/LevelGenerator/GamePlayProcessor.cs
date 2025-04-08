using System;
using System.Collections.Generic;
using System.Linq;
using Codebase.Levels;
using UniRx;
using UnityEngine;

namespace Codebase.Gameplay.LevelGenerator
{
    public class GamePlayProcessor
    {
        private readonly LevelGenerator _levelGenerator;
        private readonly GridAnalyser _gridAnalyser;
        private readonly GridView _gridView;
        private readonly PropsContainer _propsContainer;

        private readonly ISubject<Unit> _onWinCalculated = new Subject<Unit>();
        private readonly ISubject<Unit> _onCompleted = new Subject<Unit>();
        private readonly ISubject<Unit> _onLost = new Subject<Unit>();

        public IObservable<Unit> OnWinCalculated => _onWinCalculated;
        public IObservable<Unit> OnCompleted => _onCompleted;
        public IObservable<Unit> OnLost => _onLost;

        public GamePlayProcessor(LevelGenerator levelGenerator, GridAnalyser gridAnalyser, GridView gridView,
            PropsContainer propsContainer)
        {
            _propsContainer = propsContainer;
            _gridView = gridView;
            _gridAnalyser = gridAnalyser;
            _levelGenerator = levelGenerator;

            propsContainer.OnSwiped.Subscribe(value => ProcessSwipe(value.Item1, value.Item2));
        }

        public void CreateLevel(Level level)
        {
            _levelGenerator.GenerateLevel(level);
            _gridAnalyser.Initialize(level.Props);
        }

        private async void ProcessSwipe(Prop swipedProp, Vector2Int direction)
        {
            if (!_gridAnalyser.CanSwipe(swipedProp.Position, direction) || !swipedProp.Interactable)
            {
                return;
            }

            var swipePair = _gridAnalyser.ProcessSwipe(swipedProp.Position, direction);
            await _gridView.AnimateSwipe(swipePair, swipedProp);

            List<Prop> propsToMove = _gridAnalyser.UpdatePositions();

            while (propsToMove.Count > 0)
            {
                if (propsToMove.Count > 0)
                {
                    await _gridView.AlignField(propsToMove);
                }

                List<Prop> propsToDestroy = _gridAnalyser.AnalyseMerges();

                SetPropsInteractable(propsToMove, false);

                if (propsToDestroy.Count > 0)
                {
                    await _gridView.AnimateDestroyObjects(propsToDestroy);
                }

                _propsContainer.RemoveProps(propsToDestroy);

                SetPropsInteractable(propsToMove, true);

                propsToMove = _gridAnalyser.UpdatePositions();
            }

            CheckLevelState();
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
            _onLost.OnNext(Unit.Default);
        }

        private void WinLevel()
        {
            _onWinCalculated.OnNext(Unit.Default);
            _onCompleted.OnNext(Unit.Default);
        }

        private static void SetPropsInteractable(List<Prop> propsToMove, bool value)
        {
            foreach (Prop prop in propsToMove)
            {
                prop.SetInteractable(value);
            }
        }
    }
}