using System.Collections.Generic;
using System.IO;
using System.Text;
using Codebase.Services;
using Core.Helpers;
using Cysharp.Threading.Tasks;
using Extensions;
using UnityEngine;
using System.Linq;

namespace Codebase.Levels
{
    public class LevelsService : Service
    {
        private readonly string _levelsPath = Application.persistentDataPath + "/Levels/";

        public bool RepeatMode;

        public Level GetLevel(int id)
        {
            return File.ReadAllText(_levelsPath + GetFileName(id + 1)).ToDeserialized<Level>();
        }

        public void SaveLevel(Level level, int id)
        {
            string levelPath = _levelsPath + GetFileName(id + 1);

            if (File.Exists(levelPath))
            {
                File.Delete(levelPath);
            }

            File.WriteAllText(levelPath, level.ToJson());
        }

        public async UniTask<int> UpdateLevelsIfNeed(int levelsAmount)
        {
            if (!Directory.Exists(_levelsPath))
            {
                Directory.CreateDirectory(_levelsPath);
            }

            var result = await CanLoadNewLevels(levelsAmount);
            if (result.Item1)
            {
                RepeatMode = false;

                return await LoadDefaultLevels(result.Item2);
            }

            return levelsAmount;
        }

        private async UniTask<int> LoadDefaultLevels(int levelsAmount)
        {
            StringBuilder stringBuilder = new StringBuilder();

            int levelIndex = 0;
            for (; levelIndex < levelsAmount; levelIndex++)
            {
                stringBuilder.Clear();
                stringBuilder.Append("Levels/");
                stringBuilder.Append(GetFileName(levelIndex + 1));

                await StreamingAssetsService.ExtractFromStreamingAssets(stringBuilder.ToString(),
                    _levelsPath + GetFileName(levelIndex + 1));
            }

            return levelIndex;
        }

        private string GetFileName(int id)
        {
            return $"Level{id}.json";
        }

        private async UniTask<(bool, int)> CanLoadNewLevels(int oldLevelsAmount)
        {
            string levelsPath = _levelsPath + "LevelsConfig.json";
            await StreamingAssetsService.ExtractFromStreamingAssets("Levels/LevelsConfig.json", levelsPath);
            int levelsAmount = File.ReadAllText(levelsPath).ToDeserialized<LevelsInfo>().LevelsAmount;
            return (levelsAmount > oldLevelsAmount, levelsAmount);
        }

        protected override string LogTag { get; }
    }

    public class LevelsInfo
    {
        public int LevelsAmount { get; set; }
    }
}