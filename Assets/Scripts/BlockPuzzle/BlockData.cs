using System;
using System.Collections.Generic;
using UnityEngine;

namespace MonsterMaster.BlockPuzzle
{
    [Serializable]
    public sealed class BlockData
    {
        public string id;
        public BlockColorType color;
        public Vector2Int position;
        public BlockShape shape;
        [Range(1, 3)] public int width = 1;
        [Range(1, 3)] public int height = 1;
        public bool isTarget;
        public bool isKey;
        public bool isLocked;
        [Min(1)] public int requiredKeys = 1;
        public List<string> allowedExitIds = new List<string>();

        public List<Vector2Int> CreateOccupiedOffsets()
        {
            if (shape == BlockShape.Cross)
            {
                return new List<Vector2Int>
                {
                    new Vector2Int(1, 0),
                    new Vector2Int(0, 1),
                    new Vector2Int(1, 1),
                    new Vector2Int(2, 1),
                    new Vector2Int(1, 2)
                };
            }

            if (shape == BlockShape.LShape)
            {
                return new List<Vector2Int>
                {
                    new Vector2Int(0, 0),
                    new Vector2Int(1, 0),
                    new Vector2Int(0, 1)
                };
            }

            if (shape == BlockShape.SevenShape)
            {
                return new List<Vector2Int>
                {
                    new Vector2Int(0, 1),
                    new Vector2Int(1, 1),
                    new Vector2Int(1, 0)
                };
            }

            List<Vector2Int> offsets = new List<Vector2Int>(width * height);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                    offsets.Add(new Vector2Int(x, y));
            }
            return offsets;
        }

        public BlockData Clone()
        {
            return new BlockData
            {
                id = id,
                color = color,
                position = position,
                shape = shape,
                width = width,
                height = height,
                isTarget = isTarget,
                isKey = isKey,
                isLocked = isLocked,
                requiredKeys = requiredKeys,
                allowedExitIds = new List<string>(allowedExitIds)
            };
        }
    }
}
