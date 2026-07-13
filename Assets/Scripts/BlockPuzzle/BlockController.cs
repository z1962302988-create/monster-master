using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MonsterMaster.BlockPuzzle
{
    [RequireComponent(typeof(RectTransform), typeof(Image), typeof(CanvasGroup))]
    public sealed class BlockController : MonoBehaviour
    {
        private RectTransform rectTransform;
        private Image image;
        private CanvasGroup canvasGroup;
        private Color baseColor;
        private Coroutine animationRoutine;
        private readonly List<Image> visualImages = new List<Image>();
        private readonly List<Color> visualBaseColors = new List<Color>();
        private List<Vector2Int> occupiedOffsets;
        private Text idLabel;
        private Text stateLabel;
        private Image stateIcon;
        private List<string> allowedExitIds;
        private BlockVisualConfig visualConfig;

        public string Id { get; private set; }
        public BlockColorType ColorType { get; private set; }
        public Vector2Int GridPosition { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public bool IsTarget { get; private set; }
        public bool IsKey { get; private set; }
        public bool IsLocked { get; private set; }
        public int RequiredKeys { get; private set; }
        public BoardManager Board { get; private set; }
        public IReadOnlyList<Vector2Int> OccupiedOffsets => occupiedOffsets;

        public void Initialize(BlockData data, BoardManager board, BlockVisualConfig visuals = null)
        {
            rectTransform = GetComponent<RectTransform>();
            image = GetComponent<Image>();
            canvasGroup = GetComponent<CanvasGroup>();
            Id = data.id;
            ColorType = data.color;
            GridPosition = data.position;
            Width = data.width;
            Height = data.height;
            IsTarget = data.isTarget;
            IsKey = data.isKey;
            IsLocked = data.isLocked;
            RequiredKeys = data.requiredKeys;
            Board = board;
            visualConfig = visuals;
            occupiedOffsets = data.CreateOccupiedOffsets();
            allowedExitIds = new List<string>(data.allowedExitIds);
            baseColor = BlockPuzzlePalette.Get(data.color);
            canvasGroup.blocksRaycasts = true;
            rectTransform.sizeDelta = board.GetBlockSize(Width, Height);
            rectTransform.anchoredPosition = board.GridToLocal(GridPosition, Width, Height);
            BuildVisuals(data.shape);
            CreateStateLabel();
            RefreshStateLabel(0);

            BlockDragHandler dragHandler = gameObject.AddComponent<BlockDragHandler>();
            dragHandler.Initialize(this);
        }

        private void BuildVisuals(BlockShape shape)
        {
            if (shape == BlockShape.Rectangle)
            {
                StyleBlockCell(image);
                return;
            }

            image.color = Color.clear;
            image.raycastTarget = false;
            Sprite shapeSprite = visualConfig != null
                ? visualConfig.GetShapeSprite(ColorType, shape)
                : null;
            if (shapeSprite != null)
            {
                image.sprite = shapeSprite;
                image.type = Image.Type.Simple;
                image.color = Color.white;
                RegisterVisual(image, Color.white);
                BuildShapeHitTargets();
                return;
            }

            foreach (Vector2Int offset in occupiedOffsets)
            {
                GameObject cell = new GameObject("ShapeCell", typeof(RectTransform), typeof(Image));
                cell.transform.SetParent(transform, false);
                RectTransform cellRect = cell.GetComponent<RectTransform>();
                cellRect.sizeDelta = Vector2.one * Board.GetCellVisualSize();
                cellRect.anchoredPosition = new Vector2(
                    (offset.x + 0.5f - Width * 0.5f) * Board.GetCellStep(),
                    (offset.y + 0.5f - Height * 0.5f) * Board.GetCellStep());
                Image cellImage = cell.GetComponent<Image>();
                StyleBlockCell(cellImage);
            }
        }

        private void BuildShapeHitTargets()
        {
            foreach (Vector2Int offset in occupiedOffsets)
            {
                GameObject hitObject = new GameObject("ShapeHitTarget", typeof(RectTransform), typeof(Image));
                hitObject.transform.SetParent(transform, false);
                RectTransform hitRect = hitObject.GetComponent<RectTransform>();
                hitRect.sizeDelta = Vector2.one * Board.GetCellVisualSize();
                hitRect.anchoredPosition = new Vector2(
                    (offset.x + 0.5f - Width * 0.5f) * Board.GetCellStep(),
                    (offset.y + 0.5f - Height * 0.5f) * Board.GetCellStep());
                Image hitImage = hitObject.GetComponent<Image>();
                hitImage.color = Color.clear;
                hitImage.raycastTarget = true;
            }
        }

        private void StyleBlockCell(Image cellImage)
        {
            Sprite cellSprite = null;
            if (visualConfig != null)
            {
                cellSprite = cellImage == image
                    ? visualConfig.GetRectangleSprite(ColorType, Width, Height)
                    : visualConfig.GetBlockSprite(ColorType);
                cellImage.sprite = cellSprite;
                cellImage.type = cellSprite != null ? Image.Type.Sliced : Image.Type.Simple;
            }
            Color normalColor = cellSprite != null ? Color.white : baseColor;
            cellImage.color = normalColor;
            cellImage.raycastTarget = true;
            RegisterVisual(cellImage, normalColor);

            Shadow shadow = cellImage.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0.01f, 0.02f, 0.04f, 0.48f);
            shadow.effectDistance = new Vector2(0f, -6f);
            shadow.useGraphicAlpha = true;

            Outline outline = cellImage.gameObject.AddComponent<Outline>();
            outline.effectColor = Color.Lerp(baseColor, Color.black, 0.42f);
            outline.effectDistance = new Vector2(3f, -3f);
            outline.useGraphicAlpha = true;

            GameObject glossObject = new GameObject("Gloss", typeof(RectTransform), typeof(Image));
            glossObject.transform.SetParent(cellImage.transform, false);
            RectTransform glossRect = glossObject.GetComponent<RectTransform>();
            glossRect.anchorMin = new Vector2(0.10f, 0.62f);
            glossRect.anchorMax = new Vector2(0.90f, 0.88f);
            glossRect.offsetMin = Vector2.zero;
            glossRect.offsetMax = Vector2.zero;
            Image gloss = glossObject.GetComponent<Image>();
            gloss.color = new Color(1f, 1f, 1f, 0.16f);
            gloss.raycastTarget = false;
        }

        private void RegisterVisual(Image visual, Color normalColor)
        {
            visualImages.Add(visual);
            visualBaseColors.Add(normalColor);
        }

        private void CreateStateLabel()
        {
            GameObject idObject = new GameObject("IdLabel", typeof(RectTransform), typeof(Text));
            idObject.transform.SetParent(transform, false);
            RectTransform idRect = idObject.GetComponent<RectTransform>();
            idRect.anchorMin = new Vector2(0f, 1f);
            idRect.anchorMax = new Vector2(0f, 1f);
            idRect.pivot = new Vector2(0f, 1f);
            idRect.anchoredPosition = new Vector2(10f, -8f);
            idRect.sizeDelta = new Vector2(Mathf.Max(70f, rectTransform.sizeDelta.x - 20f), 30f);
            idLabel = idObject.GetComponent<Text>();
            idLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            idLabel.text = Id;
            idLabel.fontSize = 20;
            idLabel.fontStyle = FontStyle.Bold;
            idLabel.alignment = TextAnchor.UpperLeft;
            idLabel.color = new Color(1f, 1f, 1f, 0.90f);
            idLabel.raycastTarget = false;

            if (!IsKey && !IsLocked) return;

            Sprite iconSprite = IsLocked ? visualConfig?.lockIcon : visualConfig?.keyIcon;
            if (iconSprite != null)
            {
                GameObject iconObject = new GameObject("StateIcon", typeof(RectTransform), typeof(Image));
                iconObject.transform.SetParent(transform, false);
                RectTransform iconRect = iconObject.GetComponent<RectTransform>();
                iconRect.sizeDelta = Vector2.one * Mathf.Min(48f, Board.GetCellVisualSize() * 0.48f);
                stateIcon = iconObject.GetComponent<Image>();
                stateIcon.sprite = iconSprite;
                stateIcon.preserveAspect = true;
                stateIcon.raycastTarget = false;
            }

            GameObject labelObject = new GameObject("StateLabel", typeof(RectTransform), typeof(Text));
            labelObject.transform.SetParent(transform, false);
            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.sizeDelta = rectTransform.sizeDelta;
            stateLabel = labelObject.GetComponent<Text>();
            stateLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            stateLabel.fontSize = 22;
            stateLabel.fontStyle = FontStyle.Bold;
            stateLabel.alignment = TextAnchor.MiddleCenter;
            stateLabel.color = Color.white;
            stateLabel.raycastTarget = false;
        }

        public bool CanExitThrough(string exitId)
        {
            return allowedExitIds.Count == 0 || allowedExitIds.Contains(exitId);
        }

        public void SetLocked(bool locked, int keyCount)
        {
            IsLocked = locked;
            RefreshStateLabel(keyCount);
        }

        public void ShowLockedFeedback()
        {
            if (!IsLocked) return;
            SetValidityVisual(false);
            Invoke(nameof(RestoreBaseVisual), 0.18f);
        }

        private void RefreshStateLabel(int keyCount)
        {
            if (stateLabel == null) return;
            if (IsLocked)
                stateLabel.text = stateIcon != null ? keyCount + "/" + RequiredKeys : "LOCK\n" + keyCount + "/" + RequiredKeys;
            else if (IsKey)
                stateLabel.text = stateIcon != null ? string.Empty : "KEY";
            else
                stateLabel.text = string.Empty;
        }

        private void RestoreBaseVisual()
        {
            SetValidityVisual(true);
        }

        public void SetGridPosition(Vector2Int position) { GridPosition = position; }

        public void SetDragVisual(bool dragging)
        {
            rectTransform.localScale = dragging ? Vector3.one * 1.06f : Vector3.one;
            canvasGroup.alpha = dragging ? 0.92f : 1f;
            if (dragging) transform.SetAsLastSibling();
        }

        public void SetValidityVisual(bool valid)
        {
            Color invalidColor = new Color(1f, 0.18f, 0.18f, 0.58f);
            for (int i = 0; i < visualImages.Count; i++)
                visualImages[i].color = valid ? visualBaseColors[i] : invalidColor;
        }

        public void SetLocalPosition(Vector2 localPosition)
        {
            StopAnimation();
            rectTransform.anchoredPosition = localPosition;
        }

        public void AnimateTo(Vector2 localPosition, float duration, bool removeAfter = false)
        {
            StopAnimation();
            animationRoutine = StartCoroutine(AnimateRoutine(localPosition, duration, removeAfter));
        }

        private IEnumerator AnimateRoutine(Vector2 target, float duration, bool removeAfter)
        {
            Vector2 start = rectTransform.anchoredPosition;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
                rectTransform.anchoredPosition = Vector2.LerpUnclamped(start, target, t);
                yield return null;
            }

            rectTransform.anchoredPosition = target;
            animationRoutine = null;
            if (removeAfter) Destroy(gameObject);
        }

        private void StopAnimation()
        {
            if (animationRoutine == null) return;
            StopCoroutine(animationRoutine);
            animationRoutine = null;
        }
    }
}
