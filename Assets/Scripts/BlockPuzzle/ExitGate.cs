using UnityEngine;
using UnityEngine.UI;

namespace MonsterMaster.BlockPuzzle
{
    [RequireComponent(typeof(Image))]
    public sealed class ExitGate : MonoBehaviour
    {
        public ExitData Data { get; private set; }

        public void Initialize(ExitData data)
        {
            Data = data;
            Image image = GetComponent<Image>();
            Color color = BlockPuzzlePalette.Get(data.color);
            color.a = 0.82f;
            image.color = color;
            image.raycastTarget = false;
        }

        public bool Accepts(BlockController block, Vector2Int position, int columns, int rows)
        {
            if (block.ColorType != Data.color || !block.CanExitThrough(Data.id)) return false;

            int blockStart;
            int blockEnd;
            bool fullyOutside;
            switch (Data.edge)
            {
                case BoardEdge.Left:
                    fullyOutside = position.x + block.Width <= 0;
                    blockStart = position.y;
                    blockEnd = position.y + block.Height;
                    break;
                case BoardEdge.Right:
                    fullyOutside = position.x >= columns;
                    blockStart = position.y;
                    blockEnd = position.y + block.Height;
                    break;
                case BoardEdge.Bottom:
                    fullyOutside = position.y + block.Height <= 0;
                    blockStart = position.x;
                    blockEnd = position.x + block.Width;
                    break;
                default:
                    fullyOutside = position.y >= rows;
                    blockStart = position.x;
                    blockEnd = position.x + block.Width;
                    break;
            }

            int gateEnd = Data.startIndex + Data.span;
            return fullyOutside && blockStart >= Data.startIndex && blockEnd <= gateEnd;
        }
    }
}
