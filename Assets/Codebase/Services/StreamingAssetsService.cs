using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Core.Helpers
{
    public class StreamingAssetsService
    {
        public static async UniTask ExtractFromStreamingAssets(string fileName, string extractPath)
        {
            if (File.Exists(extractPath))
            {
                File.Delete(extractPath);
            }

            var directory = Path.GetDirectoryName(extractPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            var assetPath = Path.Combine(Application.streamingAssetsPath, fileName);

            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.IPhonePlayer:
                    File.Copy(assetPath, extractPath);
                    break;
                case RuntimePlatform.Android:
                    var request = UnityWebRequest.Get(assetPath);
                    await request.SendWebRequest().ToUniTask();
                    await File.WriteAllBytesAsync(extractPath, request.downloadHandler.data);
                    break;
            }
        }

        public static bool Exists(string fileName)
        {
            fileName = $"{Application.streamingAssetsPath}/{fileName}";
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    var request = UnityWebRequest.Get(fileName);
                    return request.error == null;
                default:
                    return File.Exists(fileName);
            }
        }
    }
}
