using Newtonsoft.Json;
using UnityEngine;

namespace Codebase.Levels
{
    public class Prop
    {
        public int PropId { get; set; }
        [JsonIgnore]
        public Vector2Int Position { get; set; }

        [JsonIgnore] public bool Interactable { get; set; } = true;

        public void SetInteractable(bool value)
        {
            Interactable = value;
        }
    }
}