using System.Collections.Generic;
using UnityEngine;

namespace Codebase.Gameplay.Props
{
    [CreateAssetMenu(menuName = "PropsConfig", fileName = "PropsConfig", order = 0)]
    public class PropsConfig : ScriptableObject
    {
        [SerializeField] private List<PropInfo> _props;

        public PropObject GetPropObject(int id)
        {
            return _props.Find(x => x.Id == id).Prefab;
        }
    }

    
    [System.Serializable]
    public struct PropInfo
    {
        public int Id;
        public PropObject Prefab;
    }
}
