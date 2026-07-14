using System;
using UnityEngine;

namespace MonsterMaster.BlockPuzzle
{
    public sealed class LevelManager : MonoBehaviour
    {
        private int remainingTargets;
        private int keyCount;
        private readonly System.Collections.Generic.List<BlockController> lockedBlocks =
            new System.Collections.Generic.List<BlockController>();

        public event Action LevelCompleted;
        public event Action<int, int> KeyCountChanged;
        public int RemainingTargets => remainingTargets;
        public int KeyCount => keyCount;
        public int RequiredKeyCount { get; private set; }

        public void Initialize(LevelData level, BoardManager board)
        {
            remainingTargets = 0;
            foreach (BlockData block in level.blocks)
            {
                if (block == null) continue;
                if (block.isTarget) remainingTargets++;
                if (block.isLocked) RequiredKeyCount = Mathf.Max(RequiredKeyCount, block.requiredKeys);
            }

            foreach (BlockController block in board.Blocks)
            {
                if (!block.IsLocked) continue;
                lockedBlocks.Add(block);
                block.SetLocked(true, keyCount);
            }
            board.BlockExited += OnBlockExited;
            KeyCountChanged?.Invoke(keyCount, RequiredKeyCount);
        }

        private void OnBlockExited(BlockController block)
        {
            if (block.IsKey)
            {
                keyCount++;
                UnlockEligibleBlocks();
                KeyCountChanged?.Invoke(keyCount, RequiredKeyCount);
            }
            if (!block.IsTarget) return;
            remainingTargets = Mathf.Max(0, remainingTargets - 1);
            if (remainingTargets == 0) LevelCompleted?.Invoke();
        }

        public void CompleteLevel()
        {
            if (remainingTargets == 0) return;
            remainingTargets = 0;
            LevelCompleted?.Invoke();
        }

        private void UnlockEligibleBlocks()
        {
            foreach (BlockController block in lockedBlocks)
            {
                if (!block.IsLocked) continue;
                block.SetLocked(keyCount < block.RequiredKeys, keyCount);
            }
        }
    }
}
