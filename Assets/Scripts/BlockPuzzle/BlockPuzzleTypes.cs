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
                case BlockColorType.Red: return new Color(0.92f, 0.66f, 0.64f);
                case BlockColorType.Blue: return new Color(0.62f, 0.75f, 0.84f);
                case BlockColorType.DeepBlue: return new Color(0.55f, 0.65f, 0.76f);
                case BlockColorType.Green: return new Color(0.67f, 0.79f, 0.67f);
                case BlockColorType.Yellow: return new Color(0.88f, 0.79f, 0.59f);
                case BlockColorType.Purple: return new Color(0.74f, 0.66f, 0.80f);
                case BlockColorType.Orange: return new Color(0.90f, 0.73f, 0.57f);
                case BlockColorType.Cyan: return new Color(0.61f, 0.79f, 0.78f);
                case BlockColorType.DarkGreen: return new Color(0.56f, 0.70f, 0.62f);
                default: return new Color(0.86f, 0.67f, 0.75f);
            }
        }
    }
}
