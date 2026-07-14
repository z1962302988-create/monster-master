using UnityEngine;

namespace MonsterMaster.BlockPuzzle
{
    public enum BlockColorType { Red, Blue, Green, Yellow, Purple, Orange, Cyan, Pink, DarkGreen, DeepBlue }
    public enum BoardEdge { Left, Right, Bottom, Top }
    public enum BlockShape { Rectangle, Cross, LShape, SevenShape }

    public static class BlockPuzzlePalette
    {
        public static Color Get(BlockColorType type)
        {
            switch (type)
            {
                case BlockColorType.Red: return new Color(0.90f, 0.25f, 0.23f);
                case BlockColorType.Blue: return new Color(0.24f, 0.58f, 0.94f);
                case BlockColorType.DeepBlue: return new Color(0.12f, 0.25f, 0.58f);
                case BlockColorType.Green: return new Color(0.29f, 0.75f, 0.36f);
                case BlockColorType.Yellow: return new Color(0.98f, 0.79f, 0.16f);
                case BlockColorType.Purple: return new Color(0.61f, 0.31f, 0.86f);
                case BlockColorType.Orange: return new Color(0.96f, 0.45f, 0.12f);
                case BlockColorType.Cyan: return new Color(0.12f, 0.78f, 0.82f);
                case BlockColorType.DarkGreen: return new Color(0.10f, 0.43f, 0.25f);
                default: return new Color(0.95f, 0.35f, 0.64f);
            }
        }
    }
}
