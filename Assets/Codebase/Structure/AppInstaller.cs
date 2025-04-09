using Codebase.Levels;
using Codebase.Profiles;
using Extensions;
using Profiles;
using UnityEngine;
using Zenject;

namespace Codebase.Structure
{
    public class AppInstaller : MonoInstaller
    {
        [SerializeField] private CanvasGroup _loadingScreen;
        [SerializeField] private Camera _mainCamera;

        public override void InstallBindings()
        {
            Container.BindProfile<PlayerProfile>();
            Container.BindService<ProfileService>();
            Container.BindService<LevelsService>();

            Container.BindInstance(_loadingScreen);
            Container.BindInstance(_mainCamera);

            Container.BindInterfacesAndSelfTo<AppStarter>().AsCached().NonLazy();
        }
    }
}