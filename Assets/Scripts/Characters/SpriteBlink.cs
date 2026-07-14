using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace MonsterMaster.Characters
{
    /// <summary>
    /// Idle face animation: blink / headwave, plus a mouth talk loop driven by dialogue.
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

        [Header("Talking")]
        [SerializeField] private Sprite mouthOpen;
        [SerializeField, Min(0.02f)] private float talkSwapInterval = 0.14f;

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
        private Coroutine talkLoop;
        private bool lastPreviewHeadwave;
        private bool isTalking;

        private Sprite MouthClosed => eyesOpen;
        private Sprite MouthOpenSprite => mouthOpen != null ? mouthOpen : eyesOpen;

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

            ApplyPose(MouthClosed, idleAnchoredPosition);
            RestartIdleLoop();
        }

        private void OnDisable()
        {
            StopTalkInternal(restoreIdle: false);
            StopIdleLoop();

            if (Application.isPlaying)
                ApplyPose(MouthClosed, idleAnchoredPosition);
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

        /// <summary>Start mouth talk loop (call while dialogue text is revealing).</summary>
        public void StartTalking()
        {
            if (!Application.isPlaying || isTalking) return;
            isTalking = true;
            StopIdleLoop();
            ApplyPose(MouthClosed, idleAnchoredPosition);

            if (talkLoop != null) StopCoroutine(talkLoop);
            talkLoop = StartCoroutine(TalkLoop());
        }

        /// <summary>Moves the animated portrait while preserving its headwave offset.</summary>
        public void SetIdleAnchoredPosition(Vector2 position)
        {
            Vector2 headwaveOffset = headwaveAnchoredPosition - idleAnchoredPosition;
            idleAnchoredPosition = position;
            headwaveAnchoredPosition = position + headwaveOffset;
            if (!isTalking && targetImage != null)
                targetImage.rectTransform.anchoredPosition = position;
        }

        /// <summary>Stop mouth talk loop and resume idle blink / headwave.</summary>
        public void StopTalking()
        {
            if (!Application.isPlaying) return;
            if (!isActiveAndEnabled)
            {
                StopTalkInternal(restoreIdle: false);
                return;
            }
            StopTalkInternal(restoreIdle: true);
            RestartIdleLoop();
        }

        private void StopTalkInternal(bool restoreIdle)
        {
            isTalking = false;
            if (talkLoop != null)
            {
                StopCoroutine(talkLoop);
                talkLoop = null;
            }

            if (restoreIdle)
                ApplyPose(MouthClosed, idleAnchoredPosition);
        }

        private void RestartIdleLoop()
        {
            StopIdleLoop();
            if (!isActiveAndEnabled || isTalking) return;
            blinkLoop = StartCoroutine(IdleLoop());
        }

        private void StopIdleLoop()
        {
            if (blinkLoop == null) return;
            StopCoroutine(blinkLoop);
            blinkLoop = null;
        }

        private void ApplyEditorPreview(bool force)
        {
            if (Application.isPlaying || targetImage == null) return;

            if (previewHeadwave && CanHeadwave)
                ApplyPose(headwaveEyesOpen, headwaveAnchoredPosition);
            else
                ApplyPose(MouthClosed, idleAnchoredPosition);

            lastPreviewHeadwave = previewHeadwave;
        }

        private IEnumerator TalkLoop()
        {
            bool open = false;
            while (isTalking)
            {
                open = !open;
                ApplyPose(open ? MouthOpenSprite : MouthClosed, idleAnchoredPosition);
                yield return new WaitForSeconds(talkSwapInterval);
            }
        }

        private IEnumerator IdleLoop()
        {
            while (true)
            {
                float wait = Random.Range(minInterval, Mathf.Max(minInterval, maxInterval));
                yield return new WaitForSeconds(wait);
                if (isTalking) yield break;

                if (CanHeadwave && Random.value < headwaveChance)
                    yield return PlayHeadwave();
                else
                    yield return PlayBlink();
            }
        }

        private IEnumerator PlayBlink()
        {
            yield return BlinkPair(eyesClosed, MouthClosed, idleAnchoredPosition);

            if (Random.value < doubleBlinkChance)
            {
                yield return new WaitForSeconds(0.08f);
                yield return BlinkPair(eyesClosed, MouthClosed, idleAnchoredPosition);
            }
        }

        private IEnumerator PlayHeadwave()
        {
            ApplyPose(headwaveEyesOpen, headwaveAnchoredPosition);
            yield return new WaitForSeconds(headwaveHoldDuration);
            if (isTalking) yield break;

            yield return BlinkPair(headwaveEyesClosed, headwaveEyesOpen, headwaveAnchoredPosition);
            if (isTalking) yield break;

            if (Random.value < doubleBlinkChance)
            {
                yield return new WaitForSeconds(0.08f);
                yield return BlinkPair(headwaveEyesClosed, headwaveEyesOpen, headwaveAnchoredPosition);
            }

            yield return new WaitForSeconds(headwaveHoldDuration * 0.75f);
            if (!isTalking)
                ApplyPose(MouthClosed, idleAnchoredPosition);
        }

        private IEnumerator BlinkPair(Sprite closed, Sprite open, Vector2 anchoredPosition)
        {
            if (targetImage == null || closed == null || open == null)
                yield break;

            ApplyPose(closed, anchoredPosition);
            yield return new WaitForSeconds(closedDuration);
            if (!isTalking)
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
