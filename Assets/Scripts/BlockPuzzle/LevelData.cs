using System.Collections.Generic;
using UnityEngine;

namespace MonsterMaster.BlockPuzzle
{
    [CreateAssetMenu(fileName = "Level_New", menuName = "Monster Master/Block Puzzle Level")]
    public sealed class LevelData : ScriptableObject
    {
        [Min(1)] public int columns = 6;
        [Min(1)] public int rows = 8;
        [Min(1f)] public float timeLimitSeconds = 120f;
        public List<Vector2Int> obstacles = new List<Vector2Int>();
        public List<BlockData> blocks = new List<BlockData>();
        public List<ExitData> exits = new List<ExitData>();

        private void OnValidate()
        {
            columns = Mathf.Max(1, columns);
            rows = Mathf.Max(1, rows);
            timeLimitSeconds = Mathf.Max(1f, timeLimitSeconds);

            foreach (BlockData block in blocks)
            {
                if (block == null) continue;
                if (block.shape == BlockShape.Cross)
                {
                    block.width = 3;
                    block.height = 3;
                }
                else if (block.shape == BlockShape.LShape || block.shape == BlockShape.SevenShape)
                {
                    block.width = 2;
                    block.height = 2;
                }
                else
                {
                    block.width = Mathf.Clamp(block.width, 1, 2);
                    block.height = Mathf.Clamp(block.height, 1, 2);
                }
                block.requiredKeys = Mathf.Max(1, block.requiredKeys);
                if (block.allowedExitIds == null)
                    block.allowedExitIds = new List<string>();
            }

            foreach (ExitData exit in exits)
            {
                if (exit == null) continue;
                exit.span = Mathf.Max(1, exit.span);
            }
        }
    }
}
