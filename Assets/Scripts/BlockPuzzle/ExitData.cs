using System;

namespace MonsterMaster.BlockPuzzle
{
    [Serializable]
    public sealed class ExitData
    {
        public string id;
        public BlockColorType color;
        public BoardEdge edge;
        public int startIndex;
        public int span = 1;
    }
}
