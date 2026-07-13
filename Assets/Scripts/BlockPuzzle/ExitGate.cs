using UnityEngine;
using UnityEngine.UI;

namespace MonsterMaster.BlockPuzzle
{
    [RequireComponent(typeof(Image))]
    public sealed class ExitGate : MonoBehaviour
    {
        public ExitData Data { get; private set; }

        public void Initialize(ExitData data, BlockVisualConfig visualConfig = null)
        {
            Data = data;
            Image image = GetComponent<Image>();
            image.sprite = visualConfig != null ? visualConfig.GetExitSprite(data) : null;
            image.type = image.sprite != null ? Image.Type.Sliced : Image.Type.Simple;
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
            bool reachedExit;
            switch (Data.edge)
            {
                case BoardEdge.Left:
                    reachedExit = position.x <= 0;
                    blockStart = position.y;
                    blockEnd = position.y + block.Height;
                    break;
                case BoardEdge.Right:
                    reachedExit = position.x + block.Width >= columns;
                    blockStart = position.y;
                    blockEnd = position.y + block.Height;
                    break;
                case BoardEdge.Bottom:
                    reachedExit = position.y <= 0;
                    blockStart = position.x;
                    blockEnd = position.x + block.Width;
                    break;
                default:
                    reachedExit = position.y + block.Height >= rows;
                    blockStart = position.x;
                    blockEnd = position.x + block.Width;
                    break;
            }

            int gateEnd = Data.startIndex + Data.span;
            return reachedExit && blockStart >= Data.startIndex && blockEnd <= gateEnd;
        }
    }
}
