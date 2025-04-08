using Codebase.Gameplay.Props;
using Codebase.Levels;
using UnityEngine;

namespace Codebase.Gameplay.LevelGenerator
{
    public class LevelGenerator
    {
        private readonly PropsConfig _propsConfig;
        private readonly Transform _propsParent;
        private readonly PropsContainer _propsContainer;

        public LevelGenerator(PropsConfig propsConfig, Transform propsParent, PropsContainer propsContainer)
        {
            _propsContainer = propsContainer;
            _propsParent = propsParent;
            _propsConfig = propsConfig;
        }
        
        public void GenerateLevel(Level level)
        {
            for (int x = 0; x < level.Props.GetLength(0); x++)
            {
                for (int y = 0; y < level.Props.GetLength(1); y++)
                {
                    if (level.Props[x, y] != null)
                    {
                        CreateProp(level.Props[x, y], new Vector2Int(x, y));
                    }
                }
            }
        }

        private void CreateProp(Prop levelProp, Vector2Int position)
        {
            PropObject propObject = Object.Instantiate(_propsConfig.GetPropObject(levelProp.PropId), _propsParent);
            propObject.transform.localPosition = new Vector3(position.x, position.y);
            levelProp.Position = position;
            propObject.Initialize().Forget();
            _propsContainer.RegisterProp(levelProp, propObject);
        }
    }
}