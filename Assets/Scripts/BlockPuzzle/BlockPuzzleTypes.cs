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
                case BlockColorType.Red: return new Color(0.92f, 0.28f, 0.31f);
                case BlockColorType.Blue: return new Color(0.22f, 0.55f, 0.95f);
                case BlockColorType.DeepBlue: return new Color(0.10f, 0.31f, 0.64f);
                case BlockColorType.Green: return new Color(0.24f, 0.76f, 0.45f);
                case BlockColorType.Yellow: return new Color(0.98f, 0.76f, 0.20f);
                case BlockColorType.Purple: return new Color(0.63f, 0.36f, 0.90f);
                case BlockColorType.Orange: return new Color(0.96f, 0.49f, 0.18f);
                case BlockColorType.Cyan: return new Color(0.17f, 0.72f, 0.70f);
                case BlockColorType.DarkGreen: return new Color(0.08f, 0.43f, 0.24f);
                default: return new Color(0.92f, 0.35f, 0.62f);
            }
        }
    }
}
