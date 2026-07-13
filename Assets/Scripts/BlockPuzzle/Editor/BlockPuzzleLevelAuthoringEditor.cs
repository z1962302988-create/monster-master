#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MonsterMaster.BlockPuzzle.Editor
{
    [CustomEditor(typeof(BlockPuzzleBootstrap))]
    public sealed class BlockPuzzleLevelAuthoringEditor : UnityEditor.Editor
    {
        private const float SceneCellSize = 1f;
        private int draggedBlockIndex = -1;
        private Vector2Int dragStartPosition;
        private Vector2Int dragStartMouseCell;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("level"));
            EditorGUILayout.Space(4f);
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("blockVisuals"),
                new GUIContent("棋盘 UI 替换"),
                true);
            if (serializedObject.ApplyModifiedProperties())
                SceneView.RepaintAll();

            BlockPuzzleBootstrap bootstrap = (BlockPuzzleBootstrap)target;
            LevelData level = bootstrap.Level;
            if (level == null)
            {
                EditorGUILayout.HelpBox("Assign a LevelData asset to edit this board.", MessageType.Info);
                return;
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "Scene: drag a colored block to change its grid position. " +
                "Inspector: edit all dimensions and rules below. Changes are saved to this LevelData asset.",
                MessageType.Info);
            DrawInspectorPreview(level);

            SerializedObject levelSerializedObject = new SerializedObject(level);
            levelSerializedObject.Update();
            EditorGUILayout.PropertyField(levelSerializedObject.FindProperty("columns"));
            EditorGUILayout.PropertyField(levelSerializedObject.FindProperty("rows"));
            EditorGUILayout.PropertyField(levelSerializedObject.FindProperty("timeLimitSeconds"));
            EditorGUILayout.PropertyField(levelSerializedObject.FindProperty("obstacles"), true);
            EditorGUILayout.PropertyField(levelSerializedObject.FindProperty("blocks"), true);
            EditorGUILayout.PropertyField(levelSerializedObject.FindProperty("exits"), true);
            if (levelSerializedObject.ApplyModifiedProperties())
                EditorUtility.SetDirty(level);
        }

        private static void DrawInspectorPreview(LevelData level)
        {
            const float CellSize = 30f;
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("棋盘预览（╳ = 障碍）", EditorStyles.boldLabel);
            Rect previewRect = GUILayoutUtility.GetRect(
                level.columns * CellSize,
                level.rows * CellSize,
                GUILayout.ExpandWidth(false));

            for (int y = 0; y < level.rows; y++)
            {
                for (int x = 0; x < level.columns; x++)
                {
                    Rect cellRect = new Rect(
                        previewRect.x + x * CellSize,
                        previewRect.y + (level.rows - 1 - y) * CellSize,
                        CellSize - 1f,
                        CellSize - 1f);
                    bool obstacle = level.obstacles.Contains(new Vector2Int(x, y));
                    EditorGUI.DrawRect(
                        cellRect,
                        obstacle ? new Color(0.13f, 0.13f, 0.15f) : new Color(0.23f, 0.17f, 0.12f));
                    if (obstacle)
                    {
                        GUI.Label(cellRect, "╳", CenteredPreviewLabel);
                        continue;
                    }

                    BlockData block = FindBlockAt(level, new Vector2Int(x, y));
                    if (block != null)
                    {
                        EditorGUI.DrawRect(cellRect, BlockPuzzlePalette.Get(block.color));
                        if (block.position.x == x && block.position.y == y)
                            GUI.Label(cellRect, block.id, CenteredPreviewLabel);
                    }
                }
            }
        }

        private static GUIStyle CenteredPreviewLabel
        {
            get
            {
                GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
                style.alignment = TextAnchor.MiddleCenter;
                style.normal.textColor = Color.white;
                style.fontSize = 9;
                return style;
            }
        }

        private void OnSceneGUI()
        {
            BlockPuzzleBootstrap bootstrap = (BlockPuzzleBootstrap)target;
            LevelData level = bootstrap.Level;
            if (level == null) return;

            HandleInput(bootstrap, level);
            SceneView.RepaintAll();
        }

        private static void DrawConfiguredSpritePreview(BlockPuzzleBootstrap bootstrap, LevelData level)
        {
            if (Event.current == null || Event.current.type != EventType.Repaint) return;
            BlockVisualConfig visuals = bootstrap.BlockVisuals;
            if (visuals == null) return;

            Vector3 origin = GetOrigin(bootstrap.transform, level);
            Handles.BeginGUI();

            if (visuals.boardBackgroundSprite != null)
            {
                Rect boardRect = WorldRectToGuiRect(
                    origin,
                    new Vector2(level.columns * SceneCellSize, level.rows * SceneCellSize));
                DrawSprite(visuals.boardBackgroundSprite, boardRect, new Color(1f, 1f, 1f, 0.55f));
            }

            foreach (Vector2Int obstacle in level.obstacles)
            {
                if (visuals.obstacleSprite == null) break;
                DrawSprite(
                    visuals.obstacleSprite,
                    WorldRectToGuiRect(
                        origin + new Vector3(obstacle.x, obstacle.y),
                        Vector2.one * SceneCellSize),
                    Color.white);
            }

            foreach (BlockData block in level.blocks)
            {
                Sprite shapeSprite = visuals.GetShapeSprite(block.color, block.shape);
                if (shapeSprite != null)
                {
                    DrawSprite(
                        shapeSprite,
                        WorldRectToGuiRect(
                            origin + new Vector3(block.position.x, block.position.y),
                            new Vector2(block.width, block.height) * SceneCellSize),
                        Color.white);
                    continue;
                }

                if (block.shape == BlockShape.Rectangle)
                {
                    Sprite rectangleSprite = visuals.GetRectangleSprite(
                        block.color, block.width, block.height);
                    if (rectangleSprite != null)
                    {
                        DrawSprite(
                            rectangleSprite,
                            WorldRectToGuiRect(
                                origin + new Vector3(block.position.x, block.position.y),
                                new Vector2(block.width, block.height) * SceneCellSize),
                            Color.white);
                    }
                    continue;
                }

                Sprite cellSprite = visuals.GetBlockSprite(block.color);
                if (cellSprite == null) continue;
                foreach (Vector2Int offset in block.CreateOccupiedOffsets())
                {
                    Vector2Int cell = block.position + offset;
                    DrawSprite(
                        cellSprite,
                        WorldRectToGuiRect(
                            origin + new Vector3(cell.x, cell.y),
                            Vector2.one * SceneCellSize),
                        Color.white);
                }
            }

            foreach (ExitData exit in level.exits)
            {
                Sprite exitSprite = visuals.GetExitSprite(exit);
                if (exitSprite == null) continue;
                Vector3 position;
                Vector2 size;
                GetExitRectangle(origin, level, exit, out position, out size);
                Color tint = BlockPuzzlePalette.Get(exit.color);
                tint.a = 0.82f;
                DrawSprite(exitSprite, WorldRectToGuiRect(position - (Vector3)size * 0.5f, size), tint);
            }

            Handles.EndGUI();
        }

        private static Rect WorldRectToGuiRect(Vector3 bottomLeft, Vector2 size)
        {
            Vector2 topLeft = HandleUtility.WorldToGUIPoint(
                bottomLeft + new Vector3(0f, size.y, 0f));
            Vector2 bottomRight = HandleUtility.WorldToGUIPoint(
                bottomLeft + new Vector3(size.x, 0f, 0f));
            return Rect.MinMaxRect(topLeft.x, topLeft.y, bottomRight.x, bottomRight.y);
        }

        private static void DrawSprite(Sprite sprite, Rect rect, Color tint)
        {
            if (sprite == null || sprite.texture == null) return;
            Rect textureRect = sprite.textureRect;
            Rect uv = new Rect(
                textureRect.x / sprite.texture.width,
                textureRect.y / sprite.texture.height,
                textureRect.width / sprite.texture.width,
                textureRect.height / sprite.texture.height);
            Color previousColor = GUI.color;
            GUI.color = tint;
            GUI.DrawTextureWithTexCoords(rect, sprite.texture, uv, true);
            GUI.color = previousColor;
        }

        [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected | GizmoType.Active)]
        private static void DrawBoardGizmo(BlockPuzzleBootstrap bootstrap, GizmoType gizmoType)
        {
            LevelData level = bootstrap.Level;
            if (level == null) return;

            Vector3 origin = GetOrigin(bootstrap.transform, level);
            Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
            for (int x = 0; x < level.columns; x++)
            {
                for (int y = 0; y < level.rows; y++)
                {
                    Vector3 center = GridToWorld(origin, x, y);
                    bool obstacle = level.obstacles.Contains(new Vector2Int(x, y));
                    Color cellColor = obstacle
                        ? new Color(0.14f, 0.14f, 0.16f, 0.92f)
                        : new Color(0.30f, 0.20f, 0.12f, 0.36f);
                    Handles.DrawSolidRectangleWithOutline(
                        GetCellVertices(center),
                        cellColor,
                        new Color(0.52f, 0.38f, 0.25f, 0.8f));
                    if (obstacle)
                    {
                        Vector3[] vertices = GetCellVertices(center);
                        Handles.color = Color.white;
                        Handles.DrawLine(vertices[0], vertices[2], 3f);
                        Handles.DrawLine(vertices[1], vertices[3], 3f);
                        Handles.Label(center, "障碍", EditorStyles.boldLabel);
                    }
                }
            }

            foreach (ExitData exit in level.exits)
                DrawExit(origin, level, exit);

            foreach (BlockData block in level.blocks)
                DrawBlock(origin, block);

            DrawConfiguredSpritePreview(bootstrap, level);
        }

        private void HandleInput(BlockPuzzleBootstrap bootstrap, LevelData level)
        {
            Event current = Event.current;
            if (current.alt) return;

            Vector2Int mouseCell;
            if (!TryGetMouseCell(bootstrap.transform, level, current.mousePosition, out mouseCell))
                return;

            if (current.type == EventType.MouseDown && current.button == 0)
            {
                int index = FindBlockIndexAt(level, mouseCell);
                if (index < 0) return;

                draggedBlockIndex = index;
                dragStartPosition = level.blocks[index].position;
                dragStartMouseCell = mouseCell;
                GUIUtility.hotControl = GUIUtility.GetControlID(FocusType.Passive);
                current.Use();
            }
            else if (current.type == EventType.MouseDrag && draggedBlockIndex >= 0)
            {
                BlockData block = level.blocks[draggedBlockIndex];
                Vector2Int candidate = dragStartPosition + mouseCell - dragStartMouseCell;
                candidate = ClampToBoard(level, block, candidate);
                if (candidate != block.position && IsPlacementAvailable(level, draggedBlockIndex, candidate))
                {
                    Undo.RecordObject(level, "Move Block Puzzle Block");
                    block.position = candidate;
                    EditorUtility.SetDirty(level);
                }
                current.Use();
            }
            else if (current.type == EventType.MouseUp && draggedBlockIndex >= 0)
            {
                draggedBlockIndex = -1;
                GUIUtility.hotControl = 0;
                current.Use();
            }
        }

        private static Vector3 GetOrigin(Transform transform, LevelData level)
        {
            return transform.position - new Vector3(
                level.columns * SceneCellSize * 0.5f,
                level.rows * SceneCellSize * 0.5f,
                0f);
        }

        private static Vector3 GridToWorld(Vector3 origin, int x, int y)
        {
            return origin + new Vector3((x + 0.5f) * SceneCellSize, (y + 0.5f) * SceneCellSize, 0f);
        }

        private static Vector3[] GetCellVertices(Vector3 center)
        {
            float half = SceneCellSize * 0.5f;
            return new[]
            {
                center + new Vector3(-half, -half),
                center + new Vector3(-half, half),
                center + new Vector3(half, half),
                center + new Vector3(half, -half)
            };
        }

        private static void DrawBlock(Vector3 origin, BlockData block)
        {
            Color color = BlockPuzzlePalette.Get(block.color);
            color.a = 0.95f;
            foreach (Vector2Int offset in block.CreateOccupiedOffsets())
            {
                Vector3 center = GridToWorld(origin, block.position.x + offset.x, block.position.y + offset.y);
                Handles.DrawSolidRectangleWithOutline(GetCellVertices(center), color, Color.white);
            }

            Vector3 labelPosition = GridToWorld(
                origin,
                block.position.x + block.width / 2,
                block.position.y + block.height / 2);
            string label = block.id;
            if (block.isKey) label += "\nKEY";
            if (block.isLocked) label += "\nLOCK " + block.requiredKeys;
            Handles.Label(labelPosition, label, EditorStyles.boldLabel);
        }

        private static void DrawExit(Vector3 origin, LevelData level, ExitData exit)
        {
            Color color = BlockPuzzlePalette.Get(exit.color);
            color.a = 0.95f;
            Vector3 center;
            Vector2 size;
            GetExitRectangle(origin, level, exit, out center, out size);
            string arrow;
            switch (exit.edge)
            {
                case BoardEdge.Left:
                    arrow = "←";
                    break;
                case BoardEdge.Right:
                    arrow = "→";
                    break;
                case BoardEdge.Bottom:
                    arrow = "↓";
                    break;
                default:
                    arrow = "↑";
                    break;
            }

            Handles.DrawSolidRectangleWithOutline(GetRectangleVertices(center, size), color, Color.white);
            Handles.Label(center, exit.id + " " + arrow, EditorStyles.whiteBoldLabel);
        }

        private static void GetExitRectangle(
            Vector3 origin,
            LevelData level,
            ExitData exit,
            out Vector3 center,
            out Vector2 size)
        {
            switch (exit.edge)
            {
                case BoardEdge.Left:
                    center = origin + new Vector3(-0.26f, (exit.startIndex + exit.span * 0.5f) * SceneCellSize);
                    size = new Vector2(0.22f, exit.span * SceneCellSize);
                    break;
                case BoardEdge.Right:
                    center = origin + new Vector3(level.columns * SceneCellSize + 0.26f,
                        (exit.startIndex + exit.span * 0.5f) * SceneCellSize);
                    size = new Vector2(0.22f, exit.span * SceneCellSize);
                    break;
                case BoardEdge.Bottom:
                    center = origin + new Vector3((exit.startIndex + exit.span * 0.5f) * SceneCellSize, -0.26f);
                    size = new Vector2(exit.span * SceneCellSize, 0.22f);
                    break;
                default:
                    center = origin + new Vector3((exit.startIndex + exit.span * 0.5f) * SceneCellSize,
                        level.rows * SceneCellSize + 0.26f);
                    size = new Vector2(exit.span * SceneCellSize, 0.22f);
                    break;
            }
        }

        private static Vector3[] GetRectangleVertices(Vector3 center, Vector2 size)
        {
            Vector3 half = new Vector3(size.x * 0.5f, size.y * 0.5f, 0f);
            return new[]
            {
                center + new Vector3(-half.x, -half.y),
                center + new Vector3(-half.x, half.y),
                center + new Vector3(half.x, half.y),
                center + new Vector3(half.x, -half.y)
            };
        }

        private static bool TryGetMouseCell(
            Transform transform,
            LevelData level,
            Vector2 mousePosition,
            out Vector2Int cell)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
            Plane plane = new Plane(Vector3.forward, transform.position);
            float distance;
            if (!plane.Raycast(ray, out distance))
            {
                cell = default;
                return false;
            }

            Vector3 local = ray.GetPoint(distance) - GetOrigin(transform, level);
            cell = new Vector2Int(
                Mathf.FloorToInt(local.x / SceneCellSize),
                Mathf.FloorToInt(local.y / SceneCellSize));
            return cell.x >= 0 && cell.x < level.columns && cell.y >= 0 && cell.y < level.rows;
        }

        private static int FindBlockIndexAt(LevelData level, Vector2Int cell)
        {
            for (int index = level.blocks.Count - 1; index >= 0; index--)
            {
                BlockData block = level.blocks[index];
                foreach (Vector2Int offset in block.CreateOccupiedOffsets())
                {
                    if (block.position + offset == cell)
                        return index;
                }
            }
            return -1;
        }

        private static BlockData FindBlockAt(LevelData level, Vector2Int cell)
        {
            int index = FindBlockIndexAt(level, cell);
            return index >= 0 ? level.blocks[index] : null;
        }

        private static Vector2Int ClampToBoard(LevelData level, BlockData block, Vector2Int position)
        {
            return new Vector2Int(
                Mathf.Clamp(position.x, 0, level.columns - block.width),
                Mathf.Clamp(position.y, 0, level.rows - block.height));
        }

        private static bool IsPlacementAvailable(LevelData level, int movingIndex, Vector2Int position)
        {
            HashSet<Vector2Int> occupied = new HashSet<Vector2Int>(level.obstacles);
            for (int index = 0; index < level.blocks.Count; index++)
            {
                if (index == movingIndex) continue;
                foreach (Vector2Int offset in level.blocks[index].CreateOccupiedOffsets())
                    occupied.Add(level.blocks[index].position + offset);
            }

            foreach (Vector2Int offset in level.blocks[movingIndex].CreateOccupiedOffsets())
            {
                if (occupied.Contains(position + offset))
                    return false;
            }
            return true;
        }
    }
}
#endif
