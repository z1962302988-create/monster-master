using UnityEngine;

namespace MonsterMaster.BlockPuzzle
{
    public sealed class GridCell
    {
        public Vector2Int Position { get; }
        public bool IsObstacle { get; }
        public BlockController Occupant { get; set; }

        public GridCell(Vector2Int position, bool isObstacle)
        {
            Position = position;
            IsObstacle = isObstacle;
        }
    }
}
