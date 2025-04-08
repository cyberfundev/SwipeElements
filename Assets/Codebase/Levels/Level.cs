using System.Collections.Generic;

namespace Codebase.Levels
{
    public class Level
    {
        public Prop[,] Props { get; set; }

        public Level(Prop[,] props)
        {
            Props = props;
        }
    }
}