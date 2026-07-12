using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace MonsterMaster.Characters
{
    /// <summary>
    /// Idle face animation: randomly plays a normal blink or a head-tilt blink.
    /// Each sprite uses its native pixel size.
    /// Enable "Preview Headwave" in the Inspector (edit mode) to pose the tilt in Scene view.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(Image))]
    public sealed class SpriteBlink : MonoBehaviour
    {
        [Header("Idle (straight)")]
        [SerializeField] private Image targetImage;
        [SerializeField] private Sprite eyesOpen;
        [SerializeField] private Sprite eyesClosed;
        [SerializeField] private Vector2 idleAnchoredPosition = new Vector2(-16f, 0f);

        [Header("Headwave (tilt)")]
        [SerializeField] private Sprite headwaveEyesOpen;
        [SerializeField] private Sprite headwaveEyesClosed;
        [SerializeField] private Vector2 headwaveAnchoredPosition = new Vector2(-16f, 0f);
        [SerializeField, Range(0f, 1f)] private float headwaveChance = 0.35f;
        [SerializeField, Min(0.05f)] private float headwaveHoldDuration = 0.4f;
        [Tooltip("Edit mode only: show the headwave pose so you can drag its position in the Scene view.")]
        [SerializeField] private bool previewHeadwave;

        [Header("Timing")]
        [SerializeField, Min(0.1f)] private float minInterval = 2.5f;
        [SerializeField, Min(0.1f)] private float maxInterval = 4.5f;
        [SerializeField, Min(0.02f)] private float closedDuration = 0.12f;
        [SerializeField, Range(0f, 1f)] private float doubleBlinkChance = 0.25f;

        private Coroutine blinkLoop;
        private bool lastPreviewHeadwave;

        private bool CanHeadwave =>
            headwaveEyesOpen != null && headwaveEyesClosed != null;

        private void Awake()
        {
            if (targetImage == null)
                targetImage = GetComponent<Image>();

            Transform leftover = transform.Find("PoseBlend");
            if (leftover != null)
            {
                if (Application.isPlaying) Destroy(leftover.gameObject);
                else DestroyImmediate(leftover.gameObject);
            }
        }

        private void OnEnable()
        {
            if (!Application.isPlaying)
            {
                ApplyEditorPreview(force: true);
                return;
            }

            ApplyPose(eyesOpen, idleAnchoredPosition);
            if (blinkLoop != null) StopCoroutine(blinkLoop);
            blinkLoop = StartCoroutine(IdleLoop());
        }

        private void OnDisable()
        {
            if (blinkLoop != null)
            {
                StopCoroutine(blinkLoop);
                blinkLoop = null;
            }

            if (Application.isPlaying)
                ApplyPose(eyesOpen, idleAnchoredPosition);
        }

        private void OnValidate()
        {
            if (targetImage == null)
                targetImage = GetComponent<Image>();

            if (!Application.isPlaying)
                ApplyEditorPreview(force: true);
        }

        private void Update()
        {
            if (Application.isPlaying || targetImage == null) return;

            // While previewing, keep saving the dragged Scene position.
            if (previewHeadwave)
            {
                headwaveAnchoredPosition = targetImage.rectTransform.anchoredPosition;
                if (!lastPreviewHeadwave)
                    ApplyEditorPreview(force: true);
            }
            else if (lastPreviewHeadwave)
            {
                ApplyEditorPreview(force: true);
            }

            lastPreviewHeadwave = previewHeadwave;
        }

        private void ApplyEditorPreview(bool force)
        {
            if (Application.isPlaying || targetImage == null) return;

            if (previewHeadwave && CanHeadwave)
                ApplyPose(headwaveEyesOpen, headwaveAnchoredPosition);
            else
                ApplyPose(eyesOpen, idleAnchoredPosition);

            lastPreviewHeadwave = previewHeadwave;
        }

        private IEnumerator IdleLoop()
        {
            while (true)
            {
                float wait = Random.Range(minInterval, Mathf.Max(minInterval, maxInterval));
                yield return new WaitForSeconds(wait);

                if (CanHeadwave && Random.value < headwaveChance)
                    yield return PlayHeadwave();
                else
                    yield return PlayBlink();
            }
        }

        private IEnumerator PlayBlink()
        {
            yield return BlinkPair(eyesClosed, eyesOpen, idleAnchoredPosition);

            if (Random.value < doubleBlinkChance)
            {
                yield return new WaitForSeconds(0.08f);
                yield return BlinkPair(eyesClosed, eyesOpen, idleAnchoredPosition);
            }
        }

        private IEnumerator PlayHeadwave()
        {
            ApplyPose(headwaveEyesOpen, headwaveAnchoredPosition);
            yield return new WaitForSeconds(headwaveHoldDuration);

            yield return BlinkPair(headwaveEyesClosed, headwaveEyesOpen, headwaveAnchoredPosition);

            if (Random.value < doubleBlinkChance)
            {
                yield return new WaitForSeconds(0.08f);
                yield return BlinkPair(headwaveEyesClosed, headwaveEyesOpen, headwaveAnchoredPosition);
            }

            yield return new WaitForSeconds(headwaveHoldDuration * 0.75f);
            ApplyPose(eyesOpen, idleAnchoredPosition);
        }

        private IEnumerator BlinkPair(Sprite closed, Sprite open, Vector2 anchoredPosition)
        {
            if (targetImage == null || closed == null || open == null)
                yield break;

            ApplyPose(closed, anchoredPosition);
            yield return new WaitForSeconds(closedDuration);
            ApplyPose(open, anchoredPosition);
        }

        private void ApplyPose(Sprite sprite, Vector2 anchoredPosition)
        {
            if (targetImage == null || sprite == null) return;
            targetImage.sprite = sprite;
            Rect rect = sprite.rect;
            targetImage.rectTransform.sizeDelta = new Vector2(rect.width, rect.height);
            targetImage.rectTransform.anchoredPosition = anchoredPosition;
        }
    }
}
