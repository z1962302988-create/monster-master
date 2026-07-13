#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MonsterMaster.BlockPuzzle.Editor
{
    public static class BlockPuzzleSceneBuilder
    {
        private const string LevelPath = "Assets/Data/BlockPuzzle/Levels/Level_001.asset";
        private const string ScenePath = "Assets/Scenes/BlockPuzzle/BlockPuzzleTest.unity";

        [MenuItem("Tools/Block Puzzle/Rebuild Test Scene")]
        public static void Build()
        {
            LevelData level = CreateOrUpdateLevel();
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.orthographic = true;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.035f, 0.052f, 0.08f, 1f);
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);

            GameObject bootstrapObject = new GameObject("BlockPuzzleBootstrap");
            BlockPuzzleBootstrap bootstrap = bootstrapObject.AddComponent<BlockPuzzleBootstrap>();
            SerializedObject serializedBootstrap = new SerializedObject(bootstrap);
            serializedBootstrap.FindProperty("level").objectReferenceValue = level;
            serializedBootstrap.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Block Puzzle test content rebuilt.");
        }

        public static void BuildFromCommandLine()
        {
            Build();
        }

        private static LevelData CreateOrUpdateLevel()
        {
            LevelData level = AssetDatabase.LoadAssetAtPath<LevelData>(LevelPath);
            if (level == null)
            {
                level = ScriptableObject.CreateInstance<LevelData>();
                AssetDatabase.CreateAsset(level, LevelPath);
            }

            level.columns = 6;
            level.rows = 8;
            level.timeLimitSeconds = 240f;
            level.obstacles = new List<Vector2Int>
            {
                new Vector2Int(2, 5),
                new Vector2Int(3, 4),
                new Vector2Int(0, 0),
                new Vector2Int(5, 0),
                new Vector2Int(0, 7)
            };
            level.blocks = new List<BlockData>
            {
                Block("B01", BlockColorType.Yellow, 2, 7, 2, 1, "E5"),
                Block("B02", BlockColorType.Green, 1, 6, 1, 1, "E2"),
                Block("K1", BlockColorType.Green, 2, 5, 2, 2, "E2", true, false, BlockShape.SevenShape),
                Block("B03", BlockColorType.DeepBlue, 4, 6, 1, 1, "E1"),
                Block("B04", BlockColorType.DeepBlue, 0, 4, 1, 2, "E9"),
                Block("B05", BlockColorType.Red, 1, 5, 1, 1, "E6"),
                Block("B06", BlockColorType.Purple, 4, 5, 1, 1, "E3"),
                Block("B07", BlockColorType.DeepBlue, 5, 4, 1, 2, "E9"),
                Block("B08", BlockColorType.DarkGreen, 1, 4, 1, 1, "E8"),
                Block("L1", BlockColorType.Orange, 2, 3, 2, 2, "E10", false, true, BlockShape.LShape),
                Block("B09", BlockColorType.Red, 4, 4, 1, 1, "E6"),
                Block("B10", BlockColorType.Red, 0, 2, 1, 2, "E6"),
                Block("B11", BlockColorType.DeepBlue, 1, 3, 1, 1, "E9"),
                Block("B12", BlockColorType.Purple, 5, 2, 1, 2, "E3"),
                Block("B13", BlockColorType.Pink, 1, 2, 1, 1, "E7"),
                Block("B14", BlockColorType.DarkGreen, 2, 2, 1, 1, "E8"),
                Block("B15", BlockColorType.Yellow, 3, 2, 1, 1, "E5"),
                Block("B16", BlockColorType.Cyan, 4, 2, 1, 1, "E4"),
                Block("K2", BlockColorType.Purple, 0, 1, 2, 1, "E3", true),
                Block("B19", BlockColorType.Red, 2, 1, 2, 1, "E6"),
                Block("K3", BlockColorType.Green, 4, 1, 2, 1, "E2", true),
                Block("K4", BlockColorType.Cyan, 1, 0, 2, 1, "E4", true),
                Block("K5", BlockColorType.Blue, 3, 0, 2, 1, "E1", true)
            };
            level.exits = new List<ExitData>
            {
                Exit("E1", BlockColorType.Blue, BoardEdge.Top, 1, 2),
                Exit("E2", BlockColorType.Green, BoardEdge.Top, 3, 2),
                Exit("E3", BlockColorType.Purple, BoardEdge.Left, 4, 2),
                Exit("E4", BlockColorType.Cyan, BoardEdge.Left, 3, 1),
                Exit("E5", BlockColorType.Yellow, BoardEdge.Left, 1, 1),
                Exit("E6", BlockColorType.Red, BoardEdge.Right, 5, 2),
                Exit("E7", BlockColorType.Pink, BoardEdge.Right, 4, 1),
                Exit("E8", BlockColorType.DarkGreen, BoardEdge.Right, 1, 1),
                Exit("E9", BlockColorType.DeepBlue, BoardEdge.Bottom, 1, 2),
                Exit("E10", BlockColorType.Orange, BoardEdge.Bottom, 3, 2)
            };
            EditorUtility.SetDirty(level);
            return level;
        }

        private static BlockData Block(
            string id,
            BlockColorType color,
            int x,
            int y,
            int width,
            int height,
            string exitId,
            bool isKey = false,
            bool isLocked = false,
            BlockShape shape = BlockShape.Rectangle)
        {
            return new BlockData
            {
                id = id,
                color = color,
                position = new Vector2Int(x, y),
                shape = shape,
                width = width,
                height = height,
                isTarget = true,
                isKey = isKey,
                isLocked = isLocked,
                requiredKeys = isLocked ? 5 : 1,
                allowedExitIds = new List<string> { exitId }
            };
        }

        private static ExitData Exit(
            string id,
            BlockColorType color,
            BoardEdge edge,
            int startIndex,
            int span)
        {
            return new ExitData
            {
                id = id,
                color = color,
                edge = edge,
                startIndex = startIndex,
                span = span
            };
        }
    }
}
#endif
