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
        private CameraResizer _cameraResizer;
        private Camera _mainCamera;

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

            // CreateLevels();

            _playerProfile.CurrentLevelsAmount =
                await _levelsService.UpdateLevelsIfNeed(_playerProfile.CurrentLevelsAmount);
            _levelsService.RepeatMode = _playerProfile.CurrentLevel >= _playerProfile.CurrentLevelsAmount;

            StartLevel().Forget();
        }

        // private void CreateLevels()
        // {
        //     Level level1 = new Level(new List<List<Prop>>
        //     {
        //         new() { new Prop { PropId = 1 }, new Prop { PropId = 1 } },
        //         new() { null, null, },
        //         new() { new() { PropId = 1 }, new() { PropId = 2 } },
        //         new() { new() { PropId = 2 }, null },
        //         new() { new() { PropId = 2 }, null },
        //     });
        //     
        //     Level level2 = new Level(new List<List<Prop>>
        //     {
        //         new() { new() { PropId = 1 }, new() { PropId = 1 }, new() { PropId = 2 }, new() { PropId = 1 }, new() { PropId = 1 } },
        //         new() { new() { PropId = 2 }, new() { PropId = 2 }, new() { PropId = 1 }, new() { PropId = 2 }, new() { PropId = 1 } },
        //         new() { new() { PropId = 1 }, new() { PropId = 1 }, new() { PropId = 2 }, new() { PropId = 1 }, null }, 
        //         new() { new() { PropId = 1 }, new() { PropId = 1 }, new() { PropId = 2 }, new() { PropId = 1 }, null },
        //     });
        //     
        //     Level level3 = new Level(new List<List<Prop>>
        //     {
        //         new () { new() { PropId = 1 }, new() { PropId = 1 }, new() { PropId = 2 }, new() { PropId = 1 }, new() { PropId = 1 }, null },
        //         new () { new() { PropId = 2 }, new() { PropId = 2 }, new() { PropId = 1 }, new() { PropId = 1 }, new() { PropId = 2 }, new() { PropId = 1 } },
        //         new () { new() { PropId = 1 }, new() { PropId = 1 }, new() { PropId = 2 }, null, null, null },
        //         new () { new() { PropId = 1 }, new() { PropId = 1 }, new() { PropId = 2 }, new() { PropId = 1 }, null, null },
        //     });
        //
        //
        //     _levelsService.SaveLevel(level1, 0);
        //     _levelsService.SaveLevel(level2, 1);
        //     _levelsService.SaveLevel(level3, 2);
        // }

        private async UniTaskVoid StartLevel()
        {
            int levelNumber = _levelsService.RepeatMode
                ? _playerProfile.CurrentRepeatLevel
                : _playerProfile.CurrentLevel;
            var module =
                await PlayModule<LevelModule, LevelArgs>("LevelScene",
                    new LevelArgs(_levelsService.GetLevel(levelNumber)));

            switch (module.Result)
            {
                case LevelResult.Restart:
                case LevelResult.Win:
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