#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MonsterMaster.BlockPuzzle.Editor
{
    public static class BlockPuzzlePopupPrefabBuilder
    {
        private const string PrefabPath = "Assets/Prefabs/UI/BlockPuzzlePopupLayout.prefab";

        [MenuItem("Tools/Block Puzzle/Rebuild Popup Layout Prefab")]
        public static void Build()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            GameObject root = new GameObject("BlockPuzzlePopupLayout", typeof(RectTransform),
                typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(BlockPuzzlePopupLayout));
            root.layer = 5;

            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            GameObject shade = ImageObject("FullscreenShade", root.transform,
                new Color(0f, 0f, 0f, 0.68f), true);
            Stretch(shade.GetComponent<RectTransform>());

            GameObject frame = ImageObject("PopupFrame", root.transform,
                new Color(0.035f, 0.052f, 0.08f, 0.98f), true);
            RectTransform frameRect = frame.GetComponent<RectTransform>();
            frameRect.sizeDelta = new Vector2(780f, 1580f);
            frameRect.localEulerAngles = new Vector3(0f, 0f, 90f);

            GameObject boardHostObject = new GameObject("BoardHost", typeof(RectTransform));
            boardHostObject.layer = 5;
            boardHostObject.transform.SetParent(frame.transform, false);
            RectTransform boardHost = boardHostObject.GetComponent<RectTransform>();
            boardHost.sizeDelta = new Vector2(612f, 816f);

            Button restart = ButtonObject("RestartButton", root.transform, "重新开始", font,
                new Vector2(220f, 76f));
            AnchorTopCorner(restart.GetComponent<RectTransform>(), true, new Vector2(40f, 40f));

            Button close = ButtonObject("CloseButton", root.transform, "×", font,
                new Vector2(64f, 64f));
            AnchorTopCorner(close.GetComponent<RectTransform>(), false, new Vector2(40f, 40f));

            SerializedObject layout = new SerializedObject(root.GetComponent<BlockPuzzlePopupLayout>());
            layout.FindProperty("popupCanvas").objectReferenceValue = canvas;
            layout.FindProperty("contentRoot").objectReferenceValue = frameRect;
            layout.FindProperty("boardHost").objectReferenceValue = boardHost;
            layout.FindProperty("restartButton").objectReferenceValue = restart;
            layout.FindProperty("closeButton").objectReferenceValue = close;
            layout.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Block Puzzle popup layout prefab rebuilt: " + PrefabPath);
        }

        public static void BuildFromCommandLine() { Build(); }

        [MenuItem("Tools/Block Puzzle/Install Popup Layout In Test Scene")]
        public static void InstallInTestScene()
        {
            const string scenePath = "Assets/Scenes/BlockPuzzle/BlockPuzzleTest.unity";
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            BlockPuzzleBootstrap bootstrap = Object.FindObjectOfType<BlockPuzzleBootstrap>();
            if (bootstrap == null)
                throw new MissingReferenceException("BlockPuzzleTest has no BlockPuzzleBootstrap.");

            foreach (BlockPuzzlePopupLayout existing in
                     Object.FindObjectsOfType<BlockPuzzlePopupLayout>(true))
            {
                if (existing.gameObject.scene == scene)
                    Object.DestroyImmediate(existing.gameObject);
            }

            BlockPuzzlePopupLayout prefab = AssetDatabase.LoadAssetAtPath<BlockPuzzlePopupLayout>(PrefabPath);
            if (prefab == null)
                throw new MissingReferenceException("Popup layout prefab is missing: " + PrefabPath);

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab.gameObject, scene);
            instance.name = "BlockPuzzlePopupLayout_EditTemplate";
            BlockPuzzlePopupLayout layout = instance.GetComponent<BlockPuzzlePopupLayout>();
            SerializedObject bootstrapObject = new SerializedObject(bootstrap);
            bootstrapObject.FindProperty("popupLayoutPrefab").objectReferenceValue = layout;
            bootstrapObject.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Selection.activeGameObject = instance;
            Debug.Log("Block Puzzle popup edit template installed in BlockPuzzleTest.");
        }

        public static void InstallInTestSceneFromCommandLine() { InstallInTestScene(); }

        private static GameObject ImageObject(string name, Transform parent, Color color, bool raycast)
        {
            GameObject result = new GameObject(name, typeof(RectTransform), typeof(Image));
            result.layer = 5;
            result.transform.SetParent(parent, false);
            Image image = result.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = raycast;
            return result;
        }

        private static Button ButtonObject(
            string name, Transform parent, string label, Font font, Vector2 size)
        {
            GameObject root = ImageObject(name, parent, new Color(0.18f, 0.32f, 0.51f, 1f), true);
            root.GetComponent<RectTransform>().sizeDelta = size;
            Button button = root.AddComponent<Button>();
            button.targetGraphic = root.GetComponent<Image>();

            GameObject textObject = new GameObject("Label", typeof(RectTransform), typeof(Text));
            textObject.layer = 5;
            textObject.transform.SetParent(root.transform, false);
            Stretch(textObject.GetComponent<RectTransform>());
            Text text = textObject.GetComponent<Text>();
            text.font = font;
            text.text = label;
            text.fontSize = name == "CloseButton" ? 42 : 34;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.raycastTarget = false;
            return button;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
        }

        private static void AnchorTopCorner(RectTransform rect, bool left, Vector2 margin)
        {
            Vector2 anchor = new Vector2(left ? 0f : 1f, 1f);
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.anchoredPosition = new Vector2(left ? margin.x : -margin.x, -margin.y);
        }
    }
}
#endif
