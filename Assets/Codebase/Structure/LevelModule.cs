using Codebase.Levels;

namespace Codebase.Structure
{
    public class LevelModule : GameModule<LevelResult>
    {
        
    }

    public class LevelArgs
    {
        public LevelArgs(Level level)
        {
            Level = level;
        }

        public Level Level { get; }
    }
}