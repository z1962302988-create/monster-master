using UnityEngine;
using UnityEngine.EventSystems;

namespace MonsterMaster.BlockPuzzle
{
    public sealed class BlockDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private BlockController block;
        private Vector2Int startPosition;
        private Vector2Int candidatePosition;
        private Vector2 dragOffset;
        private bool candidateIsValid;
        private bool dragging;

        public void Initialize(BlockController blockController) { block = blockController; }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (block == null || !block.Board.CanInteract) return;
            if (block.IsLocked)
            {
                block.ShowLockedFeedback();
                return;
            }
            dragging = true;
            startPosition = block.GridPosition;
            candidatePosition = startPosition;
            candidateIsValid = true;

            Vector2 pointerLocal;
            if (block.Board.TryScreenToBoardLocal(
                    eventData.position, eventData.pressEventCamera, out pointerLocal))
            {
                dragOffset = block.Board.GridToLocal(startPosition, block.Width, block.Height) - pointerLocal;
            }
            else
            {
                dragOffset = Vector2.zero;
            }

            block.Board.BeginDrag(block);
            block.SetDragVisual(true);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!dragging) return;
            Vector2 localPoint;
            if (!block.Board.TryScreenToBoardLocal(eventData.position, eventData.pressEventCamera, out localPoint))
                return;

            localPoint += dragOffset;
            candidatePosition = block.Board.LocalToGrid(localPoint, block.Width, block.Height);
            candidateIsValid = block.Board.IsMoveValid(block, startPosition, candidatePosition);
            block.SetValidityVisual(candidateIsValid);
            block.SetLocalPosition(block.Board.GridToLocal(candidatePosition, block.Width, block.Height));
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!dragging) return;
            dragging = false;
            block.SetDragVisual(false);
            block.SetValidityVisual(true);
            block.Board.EndDrag(block, startPosition, candidatePosition, candidateIsValid);
        }
    }
}
