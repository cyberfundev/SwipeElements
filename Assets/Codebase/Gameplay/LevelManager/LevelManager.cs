using System;
using Codebase.Gameplay.LevelGenerator;
using Codebase.Levels;
using Codebase.Profiles;
using Codebase.Structure;
using Codebase.UI;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UniRx;
using Zenject;

namespace Codebase.Gameplay.LevelManager
{
    [UsedImplicitly]
    public class LevelManager : IInitializable, IDisposable
    {
        private readonly GamePlayProcessor _gamePlayProcessor;
        private readonly PlayerProfile _playerProfile;
        private CompositeDisposable _disposables = new();
        private readonly LevelArgs _levelArgs;
        private readonly LevelModule _levelModule;
        private readonly LevelsService _levelsService;
        private readonly LoosePopup _loosePopup;
        private readonly LevelUI _levelUI;
        
        private LevelState _levelState;
        
        private enum LevelState
        {
            Idle,
            LostWaitingAnimation,
            WonWaitingAnimation,
            Completed,
        }

        public LevelManager(GamePlayProcessor gamePlayProcessor, 
            LevelsService levelsService, 
            PlayerProfile playerProfile, 
            LevelArgs levelArgs, 
            LevelModule levelModule,
            LoosePopup loosePopup,
            LevelUI levelUI)
        {
            _levelUI = levelUI;
            _loosePopup = loosePopup;
            _levelsService = levelsService;
            _levelModule = levelModule;
            _levelArgs = levelArgs;
            _playerProfile = playerProfile;
            _gamePlayProcessor = gamePlayProcessor;
        }

        public void Initialize()
        {
            _playerProfile.SavedLevelState = _levelArgs.Level;
            
            _gamePlayProcessor.CreateLevel(_levelArgs.Level);
            
            _levelUI.OnNextClicked
                .Subscribe(_ => GoNextLevel())
                .AddTo(_disposables);
            _levelUI.OnRestartClicked
                .Subscribe(_ => RestartLevel())
                .AddTo(_disposables);


            _gamePlayProcessor.OnWinCalculated
                .Where(_ => _levelState is LevelState.Idle)
                .Subscribe(_ => StartWaitLevelEndAnimate(true))
                .AddTo(_disposables);
            _gamePlayProcessor.OnLoseCalculated
                .Where(_ => _levelState is LevelState.Idle)
                .Subscribe(_ => StartWaitLevelEndAnimate(false))
                .AddTo(_disposables);
            _gamePlayProcessor.OnAnimationsCompleted
                .Where(_ => _levelState is LevelState.WonWaitingAnimation || _levelState is LevelState.LostWaitingAnimation)
                .Subscribe(_ => EndLevel())
                .AddTo(_disposables);

            _levelState = LevelState.Idle;
        }

        public static void AppendLevelProgress(LevelsService levelsService, PlayerProfile playerProfile)
        {
            if (levelsService.RepeatMode)
            {
                playerProfile.CurrentRepeatLevel =
                    playerProfile.CurrentLevelsAmount == playerProfile.CurrentRepeatLevel + 1
                        ? 0
                        : playerProfile.CurrentRepeatLevel + 1;
            }
            else
            {
                playerProfile.CurrentLevel++;

                if (playerProfile.CurrentLevel >= playerProfile.CurrentLevelsAmount)
                {
                    levelsService.RepeatMode = true;
                }
            }

            playerProfile.SavedLevelState = null;
        }

        public void Dispose()
        {
            _disposables.Dispose();
            _disposables = new CompositeDisposable();
        }

        private void StartWaitLevelEndAnimate(bool completed)
        {
            if (completed)
            {
                _levelState = LevelState.WonWaitingAnimation;
                AppendLevelProgress(_levelsService, _playerProfile);
            }
            else
            {
                _levelState = LevelState.LostWaitingAnimation;
            }
        }

        private void EndLevel()
        {
            if (_levelState is LevelState.WonWaitingAnimation)
            {
                WinLevel();
            }
            else
            {
                LooseLevel().Forget();
            }

            _levelState = LevelState.Completed;
        }

        private void WinLevel()
        {
            _levelState = LevelState.Completed;
            _levelModule.Finish(LevelResult.Win);
        }

        private async UniTaskVoid LooseLevel()
        {
            _levelState = LevelState.Completed;
            switch (await _loosePopup.ShowAndWaitForResult())
            {
                case Popup.PopupResult.Close:
                    _levelModule.Finish(LevelResult.Restart);
                    break;
            }
        }

        private void GoNextLevel()
        {
            _levelModule.Finish(LevelResult.Next);
        }

        private void RestartLevel()
        {
            _levelModule.Finish(LevelResult.Restart);
        }
    }
}
