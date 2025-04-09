using Codebase.Gameplay.LevelManager;
using Codebase.Levels;
using Codebase.Profiles;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Profiles;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;
using Object = UnityEngine.Object;

namespace Codebase.Structure
{
    public class AppStarter : IInitializable
    {
        private readonly LevelsService _levelsService;
        private readonly ProfileService _profileService;
        private readonly DiContainer _diContainer;
        private readonly CanvasGroup _loadingScreen;
        private readonly PlayerProfile _playerProfile;
        private readonly CameraResizer _cameraResizer;
        private readonly Camera _mainCamera;

        public AppStarter(LevelsService levelsService, ProfileService profileService, DiContainer diContainer,
            CanvasGroup loadingScreen, PlayerProfile playerProfile, CameraResizer cameraResizer, Camera mainCamera)
        {
            _mainCamera = mainCamera;
            _cameraResizer = cameraResizer;
            _playerProfile = playerProfile;
            _loadingScreen = loadingScreen;
            _profileService = profileService;
            _levelsService = levelsService;
            _diContainer = diContainer;
        }

        public void Initialize()
        {
            Application.targetFrameRate = 60;
            _cameraResizer.Initialize(_mainCamera);
            StartApp().Forget();
        }

        private async UniTaskVoid StartApp()
        {
            await _profileService.InitializeAsync();
            await _levelsService.InitializeAsync();
            
            int loadedLevelsAmount = await _levelsService.UpdateLevelsIfNeed(_playerProfile.CurrentLevelsAmount);

            _levelsService.RepeatMode = _playerProfile.CurrentLevel >= loadedLevelsAmount;

            if (loadedLevelsAmount > _playerProfile.CurrentLevelsAmount)
            {
                _playerProfile.CurrentRepeatLevel = 0;
                _playerProfile.SavedLevelState = null;
            }
            
            _playerProfile.CurrentLevelsAmount = loadedLevelsAmount;

            StartLevel().Forget();
        }

        private async UniTaskVoid StartLevel()
        {
            Level currentLevel;

            if (_playerProfile.SavedLevelState == null)
            {
                int levelNumber = _levelsService.RepeatMode
                    ? _playerProfile.CurrentRepeatLevel
                    : _playerProfile.CurrentLevel;
                currentLevel = _levelsService.GetLevel(levelNumber);
            }
            else
            {
                currentLevel = _playerProfile.SavedLevelState;
            }

            var module = await PlayModule<LevelModule, LevelArgs>("LevelScene", new LevelArgs(currentLevel));
            switch (module.Result)
            {
                case LevelResult.Restart:
                case LevelResult.Win:
                    _playerProfile.SavedLevelState = null;
                    StartLevel().Forget();
                    break;
                case LevelResult.Next:
                    AppendLevel();
                    StartLevel().Forget();
                    break;
            }
        }

        private void AppendLevel()
        {
            LevelManager.AppendLevelProgress(_levelsService, _playerProfile);
        }

        private async UniTask<TModule> PlayModule<TModule, TArgs>(string sceneName, TArgs args)
            where TModule : GameModule, new()
        {
            TModule module = new TModule();
            _diContainer.Rebind<TModule>().FromInstance(module);
            _diContainer.Rebind<TArgs>().FromInstance(args);

            await LoadScene(sceneName);

            foreach (SceneContext sceneContext in Object.FindObjectsOfType<SceneContext>())
            {
                if (!sceneContext.Initialized)
                {
                    sceneContext.Run();
                }
            }

            await _loadingScreen.DOFade(0, 0.15f).AsyncWaitForCompletion();
            await module.CompletionSource.Task;

            await _loadingScreen.DOFade(1, 0.15f).AsyncWaitForCompletion();
            await UnloadScene(sceneName);

            return module;
        }

        private async UniTask LoadScene(string sceneName)
        {
            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

            while (!asyncOperation.isDone)
            {
                await UniTask.Yield();
            }

            SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
        }

        private async UniTask UnloadScene(string sceneName)
        {
            AsyncOperation asyncOperation = SceneManager.UnloadSceneAsync(sceneName);

            while (!asyncOperation.isDone)
            {
                await UniTask.Yield();
            }

            SceneManager.SetActiveScene(SceneManager.GetSceneByName("Starter"));
        }
    }

    public enum LevelResult
    {
        Restart,
        Next,
        Win,
    }
}