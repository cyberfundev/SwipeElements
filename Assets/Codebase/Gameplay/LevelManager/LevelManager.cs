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

        public LevelManager(GamePlayProcessor gamePlayProcessor, 
            LevelsService levelsService, 
            PlayerProfile playerProfile, 
            LevelArgs levelArgs, 
            LevelModule levelModule,
            LoosePopup loosePopup)
        {
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

            _gamePlayProcessor.OnWinCalculated.Subscribe(_ => AppendLevelProgress(_levelsService, _playerProfile)).AddTo(_disposables);
            _gamePlayProcessor.OnCompleted.Subscribe(_ => WinLevel()).AddTo(_disposables);
            _gamePlayProcessor.OnLost.Subscribe(_ => LooseLevel().Forget()).AddTo(_disposables);
        }

        public static void AppendLevelProgress(LevelsService levelsService, PlayerProfile playerProfile)
        {
            if (levelsService.RepeatMode)
            {
                playerProfile.CurrentRepeatLevel =
                    playerProfile.CurrentLevelsAmount == playerProfile.CurrentRepeatLevel
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
        }

        public void Dispose()
        {
            _disposables.Dispose();
            _disposables = new CompositeDisposable();
        }

        private void WinLevel()
        {
            _levelModule.Finish(LevelResult.Win);
        }

        private async UniTaskVoid LooseLevel()
        {
            switch (await _loosePopup.ShowAndWaitForResult())
            {
                case Popup.PopupResult.Close:
                    _levelModule.Finish(LevelResult.Restart);
                    break;
            }
        }
    }
}
