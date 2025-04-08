using Codebase.Levels;
using CyberFunCore.Core.Profiles;

namespace Codebase.Profiles
{
    public class PlayerProfile : Profile
    {
        public int CurrentLevel { get; set; }
        public int CurrentRepeatLevel { get; set; }
        public int CurrentLevelsAmount { get; set; }
        public Level SavedLevelState { get; set; }
    }
}