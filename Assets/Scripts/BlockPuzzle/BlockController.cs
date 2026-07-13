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
        private List<Vector2Int> occupiedOffsets;
        private Text stateLabel;
        private List<string> allowedExitIds;

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

        public void Initialize(BlockData data, BoardManager board)
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
                image.color = baseColor;
                image.raycastTarget = true;
                visualImages.Add(image);
                return;
            }

            image.color = Color.clear;
            image.raycastTarget = false;
            foreach (Vector2Int offset in occupiedOffsets)
            {
                GameObject cell = new GameObject("CrossCell", typeof(RectTransform), typeof(Image));
                cell.transform.SetParent(transform, false);
                RectTransform cellRect = cell.GetComponent<RectTransform>();
                cellRect.sizeDelta = Vector2.one * Board.GetCellVisualSize();
                cellRect.anchoredPosition = new Vector2(
                    (offset.x + 0.5f - Width * 0.5f) * Board.GetCellStep(),
                    (offset.y + 0.5f - Height * 0.5f) * Board.GetCellStep());
                Image cellImage = cell.GetComponent<Image>();
                cellImage.color = baseColor;
                cellImage.raycastTarget = true;
                visualImages.Add(cellImage);
            }
        }

        private void CreateStateLabel()
        {
            if (!IsKey && !IsLocked) return;

            GameObject labelObject = new GameObject("StateLabel", typeof(RectTransform), typeof(Text));
            labelObject.transform.SetParent(transform, false);
            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.sizeDelta = rectTransform.sizeDelta;
            stateLabel = labelObject.GetComponent<Text>();
            stateLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            stateLabel.fontSize = 24;
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
                stateLabel.text = "锁\n" + keyCount + "/" + RequiredKeys;
            else if (IsKey)
                stateLabel.text = "钥匙";
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
            Color color = valid ? baseColor : new Color(1f, 0.18f, 0.18f, 0.58f);
            foreach (Image visualImage in visualImages)
                visualImage.color = color;
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
