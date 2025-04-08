using System;
using Extensions;
using Newtonsoft.Json;
using UniRx;

namespace CyberFunCore.Core.Profiles
{
    public class Profile
    {
        private ISubject<Unit> _saveRequestListener = new Subject<Unit>();
        
        [JsonIgnore]
        public IObservable<Unit> OnSaveRequest => _saveRequestListener;
        
        public string GetJson()
        {
            return this.ToJson();
        }

        public void RequestSave()
        {
            _saveRequestListener.OnNext(default);
        }
        
        
        public Profile()
        {
        }
    }
}