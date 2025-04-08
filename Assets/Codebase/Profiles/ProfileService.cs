using System.Collections.Generic;
using System.IO;
using Codebase.Services;
using CyberFunCore.Core.Profiles;
using Cysharp.Threading.Tasks;
using Extensions;
using Newtonsoft.Json;
using UniRx;
using UnityEngine;

namespace Profiles
{
    public class ProfileService : Service
    {
        protected override string LogTag { get; } = typeof(ProfileService).ToString();
        private static readonly string Directory = Path.Combine(Application.persistentDataPath, "Profiles");
        private static readonly List<Profile> _profiles = new();

        private static CompositeDisposable _disposables = new();
        

        public override async UniTask InitializeAsync()
        {
            Observable.EveryApplicationFocus().Subscribe(OnFocus);
            Observable.EveryApplicationPause().Subscribe(OnPause);
            await base.InitializeAsync();
        }

        public override void Dispose()
        {
            _disposables.Dispose();
            _disposables = new CompositeDisposable();
        }

        public static T LoadProfile<T>() where T : Profile
        {
            var file = Directory + "/" + typeof(T) + ".json";
            if (!System.IO.Directory.Exists(Directory))
            {
                System.IO.Directory.CreateDirectory(Directory);
            }

            if (!File.Exists(file))
            {
                File.WriteAllText(file, new Profile().ToJson());
            }

            using (var openedFile = File.OpenText(file)) {
                JsonReader reader = new JsonTextReader(openedFile);
                JsonSerializer serializer = new();
                var profile = serializer.Deserialize<T>(reader);
                _profiles.Add(profile);
                profile?.OnSaveRequest
                    .Subscribe(_ => Save(profile))
                    .AddTo(_disposables);
                return profile;
            }
        }

        public void SaveAll()
        {
            foreach (Profile profile in _profiles)
            {
                Save(profile);
            }
        }

        private void OnFocus(bool hasFocus)
        {
            if(hasFocus)
                return;
            
            SaveAll();
        }

        private void OnPause(bool hasFocus)
        {
            if(!hasFocus)
                return;

            SaveAll();
        }

        private static void Save<T>(T profile) where T : Profile
        {
            Debug.Log("SAVE PROFILE " + profile.GetType());
            string path = Directory + "/" + profile.GetType() + ".json";
            if(File.Exists(path))
            {
                File.Delete(path);
            }
            File.WriteAllText(path, profile.GetJson());

        }
    }
}