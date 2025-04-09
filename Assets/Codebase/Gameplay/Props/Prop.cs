using Newtonsoft.Json;
using UnityEngine;

namespace Codebase.Levels
{
    public class Prop
    {
        public int PropId { get; set; }
        [JsonIgnore]
        public Vector2Int Position { get; set; }
        [JsonIgnore]
        public PropState State { get; protected set; }

        public void SetState(PropState state)
        {
            State = state;
        }
        
        public enum PropState
        {
            WaitingAnimation,
            Merging,
            Idle
        }
    }
}