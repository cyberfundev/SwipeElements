#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Codebase.Profiles
{
    public class EditorExtension : MonoBehaviour
    {
        [MenuItem("GameTools/Clear Profiles")]
        public static void ClearProfiles()
        {
            foreach (string file in Directory.GetFiles(Path.Combine(Application.persistentDataPath, "Profiles")))
            {
                File.Delete(file);
            }
        }
    }
}
#endif