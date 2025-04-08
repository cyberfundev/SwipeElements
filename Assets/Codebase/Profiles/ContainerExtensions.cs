using Core.Services;
using CyberFunCore.Core.Profiles;
using Profiles;
using Zenject;

namespace Extensions
{
    public static class ContainerExtensions
    {
        public static void BindProfile<T>(this DiContainer container) where T : Profile
        {
            container
                .Bind<T>()
                .FromInstance(ProfileService.LoadProfile<T>())
                .AsCached();
        }
        public static void BindService<T>(this DiContainer container) where T : IService
        {
            container
                .Bind<T>()
                .AsCached()
                .NonLazy();
        }
    }
}
