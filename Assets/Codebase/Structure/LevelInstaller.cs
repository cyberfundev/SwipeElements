using Codebase.Gameplay.LevelGenerator;
using Codebase.Gameplay.LevelManager;
using Codebase.Gameplay.Props;
using Codebase.UI;
using UnityEngine;
using Zenject;

namespace Codebase.Structure
{
    public class LevelInstaller : MonoInstaller
    {
        [SerializeField] private PropsConfig _propsConfig;
        [SerializeField] private Transform _propsParent;
        [SerializeField] private LoosePopup _loosePopup;
        public override void InstallBindings()
        {
            Container.Bind<PropsContainer>().AsCached();
            Container.Bind<LevelGenerator>().AsCached();
            Container.Bind<GridAnalyser>().AsCached();
            Container.Bind<GridView>().AsCached();
            Container.Bind<GamePlayProcessor>().AsCached();
        
            Container.BindInterfacesAndSelfTo<LevelManager>().AsCached();

            Container.BindInstance(_propsConfig);
            Container.BindInstance(_propsParent);
            Container.BindInstance(_loosePopup);
        }
    }
}
