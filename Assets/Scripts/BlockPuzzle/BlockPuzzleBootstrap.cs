using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MonsterMaster.BlockPuzzle
{
    public sealed class BlockPuzzleBootstrap : MonoBehaviour
    {
        [SerializeField] private LevelData level;
        [SerializeField] private BlockVisualConfig blockVisuals = new BlockVisualConfig();
        [SerializeField] private bool popupMode;
        [SerializeField] private float popupRotationDegrees = 90f;
        [SerializeField] private BlockPuzzlePopupLayout popupLayoutPrefab;
        private GameObject runtimeRoot;
        private Font font;

        public LevelData Level => level;
        public BlockVisualConfig BlockVisuals => blockVisuals;
        public bool PopupMode => popupMode;
        public float PopupRotationDegrees => popupRotationDegrees;
        public BlockPuzzlePopupLayout PopupLayout => popupLayoutPrefab;
        public event Action CloseRequested;

        private void Awake()
        {
            if (level == null)
            {
                Debug.LogError("BlockPuzzleBootstrap requires a LevelData asset.", this);
                enabled = false;
                return;
            }
            Build();
        }

        public void RequestRestart()
        {
            StartCoroutine(RestartRoutine());
        }

        private IEnumerator RestartRoutine()
        {
            Time.timeScale = 1f;
            runtimeRoot.SetActive(false);
            Destroy(runtimeRoot);
            yield return null;
            Build();
        }

        private void Build()
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            runtimeRoot = new GameObject("BlockPuzzleRuntime");
            runtimeRoot.transform.SetParent(transform, false);

            EnsureCamera();
            CreateEventSystem();
            BlockPuzzlePopupLayout popupLayout = null;
            Canvas canvas;
            if (popupMode && popupLayoutPrefab != null)
            {
                bool sceneTemplate = popupLayoutPrefab.gameObject.scene.IsValid();
                if (sceneTemplate) popupLayoutPrefab.gameObject.SetActive(false);
                popupLayout = Instantiate(popupLayoutPrefab, runtimeRoot.transform, false);
                popupLayout.gameObject.SetActive(true);
                canvas = popupLayout.PopupCanvas;
            }
            else
            {
                canvas = CreateCanvas();
            }
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            RectTransform contentRect = popupLayout != null ? popupLayout.ContentRoot : canvasRect;
            if (popupLayout != null)
            {
                popupLayout.CloseButton.onClick.AddListener(RequestClose);
            }
            else if (popupMode)
            {
                GameObject shade = CreatePanel("ModalShade", canvasRect, Vector2.zero, Vector2.zero,
                    new Color(0f, 0f, 0f, 0.68f), true);
                StretchToParent(shade.GetComponent<RectTransform>());
                GameObject popup = CreatePanel("BlockPuzzlePopup", canvasRect, Vector2.zero,
                    new Vector2(780f, 1580f), new Color(0.035f, 0.052f, 0.08f, 0.98f), true);
                contentRect = popup.GetComponent<RectTransform>();
                contentRect.localEulerAngles = new Vector3(0f, 0f, popupRotationDegrees);
                Button closeButton = CreateButton("CloseButton", canvasRect, "×",
                    Vector2.zero, new Vector2(64f, 64f));
                AnchorTopCorner(closeButton.GetComponent<RectTransform>(), false, new Vector2(40f, 40f));
                closeButton.onClick.AddListener(RequestClose);
            }
            else
            {
                CreatePanel("Background", canvasRect, Vector2.zero, new Vector2(1080f, 1920f),
                    new Color(0.035f, 0.052f, 0.08f, 1f), false);
            }

            Text keyCounter = null;
            if (!popupMode)
            {
                CreateText("Title", contentRect, "BLOCK ESCAPE", 56, TextAnchor.MiddleCenter,
                    new Vector2(0f, 790f), new Vector2(680f, 90f), Color.white);
                keyCounter = CreateText("KeyCounter", contentRect, "钥匙 0/5", 36,
                    TextAnchor.MiddleCenter, new Vector2(0f, 690f), new Vector2(360f, 56f),
                    new Color(1f, 0.86f, 0.37f));
            }

            RectTransform boardParent = popupLayout != null ? popupLayout.BoardHost : contentRect;
            GameObject boardObject = CreatePanel("Board", boardParent,
                new Vector2(0f, popupMode ? 0f : -50f),
                new Vector2(820f, 1120f), Color.white, false);
            BoardManager board = boardObject.AddComponent<BoardManager>();
            board.Initialize(
                level,
                blockVisuals,
                popupMode ? -popupRotationDegrees : 0f,
                false);

            LevelManager levelManager = runtimeRoot.AddComponent<LevelManager>();
            levelManager.Initialize(level, board);
            if (keyCounter != null)
            {
                levelManager.KeyCountChanged += (count, required) =>
                    keyCounter.text = "钥匙 " + count + "/" + required;
                keyCounter.text = "钥匙 " + levelManager.KeyCount + "/" + levelManager.RequiredKeyCount;
            }
            GameManager gameManager = runtimeRoot.AddComponent<GameManager>();

            Button restartButton;
            Button pauseButton = null;
            Text pauseButtonText = null;
            GameObject pauseOverlay = null;
            if (popupMode)
            {
                if (popupLayout != null)
                {
                    restartButton = popupLayout.RestartButton;
                }
                else
                {
                    restartButton = CreateButton("RestartButton", canvasRect, "重新开始",
                        Vector2.zero, new Vector2(220f, 76f));
                    AnchorTopCorner(restartButton.GetComponent<RectTransform>(), true, new Vector2(40f, 40f));
                }
            }
            else
            {
                restartButton = CreateButton("RestartButton", contentRect, "重新开始",
                    new Vector2(-225f, -770f), new Vector2(360f, 100f));
                pauseButton = CreateButton("PauseButton", contentRect, "暂停",
                    new Vector2(225f, -770f), new Vector2(360f, 100f));
                pauseButtonText = pauseButton.GetComponentInChildren<Text>();
                pauseOverlay = CreateOverlay(contentRect, "PauseOverlay", "已暂停",
                    new Color(0f, 0f, 0f, 0.55f), false);
            }
            GameObject victoryPopup = CreateResultPopup(contentRect, "VictoryPopup", "关卡完成！", gameManager);

            gameManager.Initialize(board, levelManager, pauseButtonText,
                pauseOverlay, victoryPopup, RequestRestart);
            restartButton.onClick.AddListener(gameManager.Restart);
            if (pauseButton != null) pauseButton.onClick.AddListener(gameManager.TogglePause);
        }

        private static void AnchorTopCorner(
            RectTransform rect,
            bool left,
            Vector2 margin)
        {
            Vector2 anchor = new Vector2(left ? 0f : 1f, 1f);
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.anchoredPosition = new Vector2(
                left ? margin.x : -margin.x,
                -margin.y);
        }

        private void RequestClose()
        {
            Time.timeScale = 1f;
            CloseRequested?.Invoke();
        }

        private Canvas CreateCanvas()
        {
            GameObject canvasObject = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(runtimeRoot.transform, false);
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = popupMode ? 100 : 0;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = popupMode
                ? new Vector2(1920f, 1080f)
                : new Vector2(1080f, 1920f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        private static void StretchToParent(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
        }

        private void EnsureCamera()
        {
            if (FindObjectOfType<Camera>() != null) return;

            GameObject cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.transform.SetParent(runtimeRoot.transform, false);
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            cameraObject.tag = "MainCamera";

            Camera camera = cameraObject.GetComponent<Camera>();
            camera.orthographic = true;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.035f, 0.052f, 0.08f, 1f);
        }

        private void CreateEventSystem()
        {
            if (FindObjectOfType<EventSystem>() != null) return;
            GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            eventSystem.transform.SetParent(runtimeRoot.transform, false);
        }

        private GameObject CreateOverlay(
            RectTransform parent,
            string objectName,
            string message,
            Color color,
            bool active)
        {
            GameObject overlay = CreatePanel(objectName, parent, Vector2.zero,
                new Vector2(1080f, 1920f), color, false);
            overlay.GetComponent<Image>().raycastTarget = false;
            CreateText("Message", overlay.GetComponent<RectTransform>(), message, 72,
                TextAnchor.MiddleCenter, Vector2.zero, new Vector2(700f, 180f), Color.white);
            overlay.SetActive(active);
            return overlay;
        }

        private GameObject CreateResultPopup(
            RectTransform parent,
            string objectName,
            string message,
            GameManager gameManager)
        {
            GameObject shade = CreatePanel(objectName, parent, Vector2.zero,
                new Vector2(1080f, 1920f), new Color(0f, 0f, 0f, 0.72f), true);
            GameObject card = CreatePanel("Card", shade.GetComponent<RectTransform>(), Vector2.zero,
                new Vector2(760f, 480f), new Color(0.10f, 0.15f, 0.23f, 1f), true);
            CreateText("Message", card.GetComponent<RectTransform>(), message, 70,
                TextAnchor.MiddleCenter, new Vector2(0f, 90f), new Vector2(650f, 150f), Color.white);
            Button button = CreateButton("PopupRestart", card.GetComponent<RectTransform>(), "再来一次",
                new Vector2(0f, -100f), new Vector2(360f, 100f));
            button.onClick.AddListener(gameManager.Restart);
            shade.SetActive(false);
            return shade;
        }

        private Button CreateButton(
            string objectName,
            RectTransform parent,
            string label,
            Vector2 position,
            Vector2 size)
        {
            GameObject buttonObject = CreatePanel(objectName, parent, position, size,
                new Color(0.18f, 0.32f, 0.51f, 1f), true);
            Button button = buttonObject.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.highlightedColor = new Color(0.24f, 0.42f, 0.66f, 1f);
            colors.pressedColor = new Color(0.12f, 0.24f, 0.39f, 1f);
            button.colors = colors;
            CreateText("Label", buttonObject.GetComponent<RectTransform>(), label, 40,
                TextAnchor.MiddleCenter, Vector2.zero, size, Color.white);
            return button;
        }

        private GameObject CreatePanel(
            string objectName,
            RectTransform parent,
            Vector2 position,
            Vector2 size,
            Color color,
            bool raycastTarget)
        {
            GameObject panel = new GameObject(objectName, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
            Image image = panel.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = raycastTarget;
            return panel;
        }

        private Text CreateText(
            string objectName,
            RectTransform parent,
            string value,
            int fontSize,
            TextAnchor alignment,
            Vector2 position,
            Vector2 size,
            Color color)
        {
            GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);
            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
            Text text = textObject.GetComponent<Text>();
            text.font = font;
            text.text = value;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = color;
            text.raycastTarget = false;
            return text;
        }
    }
}
