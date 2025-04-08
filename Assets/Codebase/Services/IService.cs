using System;
using Cysharp.Threading.Tasks;

namespace Core.Services
{
    public interface IService : IDisposable
    {
        bool Initialized { get; set; }
        UniTask InitializeAsync();
    }
}