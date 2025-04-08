using Core.Services;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Codebase.Services
{
    public abstract class Service : IService
    {
        protected abstract string LogTag { get; }
        public bool Initialized { get; set; }

        protected void ServiceLog(string log)
        {
            Debug.Log($"[{LogTag}]: {log}");
        }

        public virtual async UniTask InitializeAsync()
        {
            Initialized = true;
        }

        public virtual void Dispose()
        {
            
        }
    }
}
