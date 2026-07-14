using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MonsterMaster.BlockPuzzle
{
    [RequireComponent(typeof(RectTransform), typeof(Image))]
    public sealed class BoardManager : MonoBehaviour
    {
        private const float CellGap = 8f;
        private const float DesignCellSize = 102f;
        private readonly List<BlockController> blocks = new List<BlockController>();
        private readonly List<ExitGate> exits = new List<ExitGate>();
        private readonly List<RectTransform> obstacleVisuals = new List<RectTransform>();
        private RectTransform boardRect;
        private GridCell[,] cells;
        private LevelData level;
        private BlockVisualConfig visualConfig;
        private float visualCounterRotation;
        private bool showBlockLabels;
        private float cellSize;

        public event Action<BlockController> BlockExited;
        public bool CanInteract { get; set; } = true;
        public int Columns => level.columns;
        public int Rows => level.rows;
        public IReadOnlyList<BlockController> Blocks => blocks;
        public float VisualCounterRotation => visualCounterRotation;
        public bool ShowBlockLabels => showBlockLabels;

        public void Initialize(
            LevelData levelData,
            BlockVisualConfig visualConfig = null,
            float contentCounterRotation = 0f,
            bool showRuntimeBlockLabels = false,
            float maximumWidth = 820f,
            float maximumHeight = 1120f)
        {
            level = levelData;
            this.visualConfig = visualConfig;
            visualCounterRotation = contentCounterRotation;
            showBlockLabels = showRuntimeBlockLabels;
            boardRect = GetComponent<RectTransform>();
            cellSize = DesignCellSize;
            boardRect.sizeDelta = new Vector2(level.columns * cellSize, level.rows * cellSize);

            Image background = GetComponent<Image>();
            background.sprite = visualConfig != null ? visualConfig.boardBackgroundSprite : null;
            background.type = background.sprite != null ? Image.Type.Sliced : Image.Type.Simple;
            background.color = background.sprite != null
                ? Color.white
                : new Color(0.36f, 0.23f, 0.14f, 1f);
            background.raycastTarget = false;

            BuildCells();
            BuildGridVisuals();
            BuildExits();
            BuildBlocks();
            BringObstaclesToFront();
        }

        private void BuildCells()
        {
            cells = new GridCell[level.columns, level.rows];
            for (int x = 0; x < level.columns; x++)
            {
                for (int y = 0; y < level.rows; y++)
                {
                    Vector2Int position = new Vector2Int(x, y);
                    cells[x, y] = new GridCell(position, level.obstacles.Contains(position));
                }
            }
        }

        private void BuildGridVisuals()
        {
            for (int x = 0; x < level.columns; x++)
            {
                for (int y = 0; y < level.rows; y++)
                {
                    GridCell cell = cells[x, y];
                    GameObject visual = CreateImageObject(cell.IsObstacle ? "Obstacle" : "Cell", transform);
                    RectTransform rect = visual.GetComponent<RectTransform>();
                    rect.sizeDelta = Vector2.one * (cellSize - CellGap);
                    rect.anchoredPosition = GridToLocal(cell.Position, 1, 1);
                    Image image = visual.GetComponent<Image>();
                    bool hasObstacleSprite = cell.IsObstacle && visualConfig != null &&
                                             visualConfig.obstacleSprite != null;
                    if (hasObstacleSprite)
                    {
                        CreateCounterRotatedImage(rect, image, visualConfig.obstacleSprite);
                    }
                    else
                    {
                        image.color = cell.IsObstacle
                            ? new Color(0.22f, 0.15f, 0.10f, 1f)
                            : new Color(0.43f, 0.29f, 0.19f, 1f);
                    }
                    image.raycastTarget = false;
                    if (cell.IsObstacle && !hasObstacleSprite)
                        AddObstacleMarker(rect);
                    if (cell.IsObstacle)
                        obstacleVisuals.Add(rect);
                }
            }
        }

        private void BringObstaclesToFront()
        {
            foreach (RectTransform obstacle in obstacleVisuals)
            {
                if (obstacle != null) obstacle.SetAsLastSibling();
            }
        }

        private void CreateCounterRotatedImage(RectTransform parent, Image hitImage, Sprite sprite)
        {
            if (Mathf.Approximately(visualCounterRotation, 0f))
            {
                hitImage.sprite = sprite;
                hitImage.type = Image.Type.Simple;
                hitImage.color = Color.white;
                return;
            }

            hitImage.color = Color.clear;
            GameObject visualObject = CreateImageObject("Visual", parent);
            RectTransform visualRect = visualObject.GetComponent<RectTransform>();
            visualRect.sizeDelta = new Vector2(parent.sizeDelta.y, parent.sizeDelta.x);
            visualRect.localEulerAngles = new Vector3(0f, 0f, visualCounterRotation);
            Image visual = visualObject.GetComponent<Image>();
            visual.sprite = sprite;
            visual.type = Image.Type.Simple;
            visual.color = Color.white;
            visual.raycastTarget = false;
        }

        private void AddObstacleMarker(RectTransform parent)
        {
            GameObject marker = new GameObject("ObstacleMarker", typeof(RectTransform), typeof(Text));
            marker.transform.SetParent(parent, false);
            RectTransform markerRect = marker.GetComponent<RectTransform>();
            markerRect.anchorMin = Vector2.zero;
            markerRect.anchorMax = Vector2.one;
            markerRect.sizeDelta = Vector2.zero;

            Text text = marker.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = "╳";
            text.fontSize = 72;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = new Color(1f, 1f, 1f, 0.82f);
            text.raycastTarget = false;
            if (!Mathf.Approximately(visualCounterRotation, 0f))
                markerRect.localEulerAngles = new Vector3(0f, 0f, visualCounterRotation);
        }

        private void BuildExits()
        {
            foreach (ExitData exitData in level.exits)
            {
                GameObject gateObject = CreateImageObject("Exit_" + exitData.id, transform);
                ExitGate gate = gateObject.AddComponent<ExitGate>();
                gate.Initialize(exitData, visualConfig);
                PositionExit(gateObject.GetComponent<RectTransform>(), exitData);
                exits.Add(gate);
            }
        }

        private void PositionExit(RectTransform rect, ExitData exit)
        {
            float thickness = cellSize * 0.34f;
            float length = exit.span * cellSize - CellGap;
            float start = exit.startIndex * cellSize;
            Vector2 origin = -boardRect.sizeDelta * 0.5f;

            if (exit.edge == BoardEdge.Left || exit.edge == BoardEdge.Right)
            {
                rect.sizeDelta = new Vector2(thickness, length);
                float x = exit.edge == BoardEdge.Left
                    ? origin.x - thickness * 0.5f
                    : -origin.x + thickness * 0.5f;
                rect.anchoredPosition = new Vector2(x, origin.y + start + exit.span * cellSize * 0.5f);
            }
            else
            {
                rect.sizeDelta = new Vector2(length, thickness);
                float y = exit.edge == BoardEdge.Bottom
                    ? origin.y - thickness * 0.5f
                    : -origin.y + thickness * 0.5f;
                rect.anchoredPosition = new Vector2(origin.x + start + exit.span * cellSize * 0.5f, y);
            }
        }

        private void BuildBlocks()
        {
            foreach (BlockData sourceData in level.blocks)
            {
                BlockData data = sourceData.Clone();
                GameObject blockObject = CreateImageObject("Block_" + data.id, transform);
                blockObject.AddComponent<CanvasGroup>();
                BlockController block = blockObject.AddComponent<BlockController>();
                block.Initialize(data, this, visualConfig);
                blocks.Add(block);

                if (!IsInsideBoard(data.position, data.width, data.height) ||
                    !CanOccupy(block, data.position))
                {
                    Debug.LogError("Invalid block placement in level: " + data.id, level);
                    continue;
                }
                SetOccupancy(block, data.position, true);
            }
        }

        public Vector2 GetBlockSize(int width, int height)
        {
            return new Vector2(width * cellSize - CellGap, height * cellSize - CellGap);
        }

        public float GetCellStep() => cellSize;
        public float GetCellVisualSize() => cellSize - CellGap;

        public Vector2 GridToLocal(Vector2Int position, int width, int height)
        {
            Vector2 origin = -boardRect.sizeDelta * 0.5f;
            return origin + new Vector2(
                (position.x + width * 0.5f) * cellSize,
                (position.y + height * 0.5f) * cellSize);
        }

        public Vector2Int LocalToGrid(Vector2 localPoint, int width, int height)
        {
            Vector2 origin = -boardRect.sizeDelta * 0.5f;
            Vector2 grid = (localPoint - origin) / cellSize - new Vector2(width * 0.5f, height * 0.5f);
            return new Vector2Int(Mathf.RoundToInt(grid.x), Mathf.RoundToInt(grid.y));
        }

        public bool TryScreenToBoardLocal(Vector2 screenPoint, Camera eventCamera, out Vector2 localPoint)
        {
            return RectTransformUtility.ScreenPointToLocalPointInRectangle(
                boardRect, screenPoint, eventCamera, out localPoint);
        }

        public void BeginDrag(BlockController block)
        {
            SetOccupancy(block, block.GridPosition, false);
        }

        public bool IsMoveValid(BlockController block, Vector2Int start, Vector2Int target)
        {
            if (target.x != start.x && target.y != start.y)
                return false;

            int steps = Mathf.Max(Mathf.Abs(target.x - start.x), Mathf.Abs(target.y - start.y)) * 4;
            steps = Mathf.Max(1, steps);
            Vector2 previous = new Vector2(float.NaN, float.NaN);
            for (int i = 1; i <= steps; i++)
            {
                Vector2 point = Vector2.Lerp(start, target, i / (float)steps);
                Vector2Int sample = new Vector2Int(Mathf.RoundToInt(point.x), Mathf.RoundToInt(point.y));
                if (sample == previous) continue;
                previous = sample;
                if (!IsPlacementAllowed(block, sample)) return false;
            }
            return true;
        }

        public void EndDrag(
            BlockController block,
            Vector2Int start,
            Vector2Int target,
            bool valid)
        {
            if (!valid)
            {
                RestoreBlock(block, start);
                return;
            }

            ExitGate acceptingExit = GetAcceptingExit(block, target);
            if (acceptingExit != null)
            {
                blocks.Remove(block);
                block.SetGridPosition(target);
                block.AnimateTo(GridToLocal(target, block.Width, block.Height), 0.16f, true);
                BlockExited?.Invoke(block);
                return;
            }

            block.SetGridPosition(target);
            SetOccupancy(block, target, true);
            block.AnimateTo(GridToLocal(target, block.Width, block.Height), 0.12f);
        }

        private void RestoreBlock(BlockController block, Vector2Int start)
        {
            block.SetGridPosition(start);
            SetOccupancy(block, start, true);
            block.AnimateTo(GridToLocal(start, block.Width, block.Height), 0.18f);
        }

        private bool IsPlacementAllowed(BlockController block, Vector2Int position)
        {
            if (IsInsideBoard(position, block.Width, block.Height))
                return CanOccupy(block, position);

            ExitGate corridor = GetExitCorridor(block, position);
            return corridor != null && CanOccupyVisibleCells(block, position);
        }

        private bool CanOccupy(BlockController block, Vector2Int position)
        {
            foreach (Vector2Int offset in block.OccupiedOffsets)
            {
                int x = position.x + offset.x;
                int y = position.y + offset.y;
                GridCell cell = cells[x, y];
                if (cell.IsObstacle || (cell.Occupant != null && cell.Occupant != block))
                    return false;
            }
            return true;
        }

        private bool CanOccupyVisibleCells(BlockController block, Vector2Int position)
        {
            foreach (Vector2Int offset in block.OccupiedOffsets)
            {
                int x = position.x + offset.x;
                int y = position.y + offset.y;
                if (!IsCellInside(x, y)) continue;
                GridCell cell = cells[x, y];
                if (cell.IsObstacle || (cell.Occupant != null && cell.Occupant != block))
                    return false;
            }
            return true;
        }

        private ExitGate GetExitCorridor(BlockController block, Vector2Int position)
        {
            foreach (ExitGate gate in exits)
            {
                ExitData data = gate.Data;
                if (data.color != block.ColorType || !block.CanExitThrough(data.id)) continue;

                int start;
                int end;
                bool crossesEdge;
                switch (data.edge)
                {
                    case BoardEdge.Left:
                        crossesEdge = position.x < 0;
                        start = position.y;
                        end = position.y + block.Height;
                        break;
                    case BoardEdge.Right:
                        crossesEdge = position.x + block.Width > level.columns;
                        start = position.y;
                        end = position.y + block.Height;
                        break;
                    case BoardEdge.Bottom:
                        crossesEdge = position.y < 0;
                        start = position.x;
                        end = position.x + block.Width;
                        break;
                    default:
                        crossesEdge = position.y + block.Height > level.rows;
                        start = position.x;
                        end = position.x + block.Width;
                        break;
                }

                if (crossesEdge && start >= data.startIndex && end <= data.startIndex + data.span)
                    return gate;
            }
            return null;
        }

        private ExitGate GetAcceptingExit(BlockController block, Vector2Int position)
        {
            foreach (ExitGate gate in exits)
            {
                if (gate.Accepts(block, position, level.columns, level.rows))
                    return gate;
            }
            return null;
        }

        private void SetOccupancy(BlockController block, Vector2Int position, bool occupied)
        {
            foreach (Vector2Int offset in block.OccupiedOffsets)
            {
                int x = position.x + offset.x;
                int y = position.y + offset.y;
                if (!IsCellInside(x, y)) continue;
                if (occupied)
                    cells[x, y].Occupant = block;
                else if (cells[x, y].Occupant == block)
                    cells[x, y].Occupant = null;
            }
        }

        private bool IsInsideBoard(Vector2Int position, int width, int height)
        {
            return position.x >= 0 && position.y >= 0 &&
                   position.x + width <= level.columns &&
                   position.y + height <= level.rows;
        }

        private bool IsCellInside(int x, int y)
        {
            return x >= 0 && y >= 0 && x < level.columns && y < level.rows;
        }

        private static GameObject CreateImageObject(string objectName, Transform parent)
        {
            GameObject result = new GameObject(objectName, typeof(RectTransform), typeof(Image));
            result.transform.SetParent(parent, false);
            return result;
        }
    }
}
