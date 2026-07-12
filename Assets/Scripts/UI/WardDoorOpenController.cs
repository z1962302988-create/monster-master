using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MonsterMaster.UI
{
    /// <summary>
    /// Plays the ward-door open crossfade when the nurse dialogue option
    /// "去病房看看" (action: visit_ward) is selected, then loads the ward scene.
    /// </summary>
    public sealed class WardDoorOpenController : MonoBehaviour
    {
        public const string VisitWardAction = "visit_ward";

        [Header("References")]
        [SerializeField] private DialogueChoicePopup dialoguePopup;
        [SerializeField] private Image closedBackground;
        [SerializeField] private Image openBackground;
        [SerializeField] private Sprite wardOpenSprite;

        [Header("Timing")]
        [SerializeField, Min(0.01f)] private float doorOpenDuration = 1.1f;
        [SerializeField, Min(0f)] private float holdAfterOpen = 0.35f;

        [Header("Scene")]
        [SerializeField] private string wardSceneName = "ward";

        private bool playing;
        private bool subscribed;

        private void Awake()
        {
            ResolveReferences();
            PrepareOpenBackground();
            Subscribe();
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void ResolveReferences()
        {
            if (closedBackground == null)
            {
                Transform closed = transform.Find("bg_back");
                if (closed != null) closedBackground = closed.GetComponent<Image>();
            }

            if (openBackground == null)
            {
                Transform open = transform.Find("bg_wardopen");
                if (open != null) openBackground = open.GetComponent<Image>();
            }

            if (dialoguePopup == null)
                dialoguePopup = GetComponentInChildren<DialogueChoicePopup>(true);
        }

        private void PrepareOpenBackground()
        {
            if (openBackground == null) return;

            if (wardOpenSprite != null)
                openBackground.sprite = wardOpenSprite;

            openBackground.gameObject.SetActive(true);
            SetImageAlpha(openBackground, 0f);
            openBackground.raycastTarget = false;
        }

        private void Subscribe()
        {
            if (subscribed || dialoguePopup == null) return;
            dialoguePopup.OptionSelected += OnOptionSelected;
            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed || dialoguePopup == null) return;
            dialoguePopup.OptionSelected -= OnOptionSelected;
            subscribed = false;
        }

        private void OnOptionSelected(string action)
        {
            if (action != VisitWardAction) return;
            if (playing) return;
            StartCoroutine(PlayDoorOpen());
        }

        private IEnumerator PlayDoorOpen()
        {
            playing = true;
            ResolveReferences();

            if (openBackground == null && closedBackground != null)
                openBackground = CreateOpenOverlay();

            if (openBackground == null ||
                (wardOpenSprite == null && openBackground.sprite == null))
            {
                Debug.LogWarning("[WardDoor] Missing open-door background — cannot play animation.", this);
                playing = false;
                yield break;
            }

            if (wardOpenSprite != null)
                openBackground.sprite = wardOpenSprite;

            openBackground.gameObject.SetActive(true);
            SetImageAlpha(openBackground, 0f);

            // Keep open image just above the closed background.
            if (closedBackground != null)
            {
                int closedIndex = closedBackground.transform.GetSiblingIndex();
                openBackground.transform.SetSiblingIndex(closedIndex + 1);
            }

            float elapsed = 0f;
            while (elapsed < doorOpenDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float linear = Mathf.Clamp01(elapsed / doorOpenDuration);
                float t = linear * linear * (3f - 2f * linear);
                SetImageAlpha(openBackground, t);
                yield return null;
            }

            SetImageAlpha(openBackground, 1f);

            // Bake the open state into the base background so we don't keep two full-screen layers.
            if (closedBackground != null && openBackground.sprite != null)
            {
                closedBackground.sprite = openBackground.sprite;
                SetImageAlpha(openBackground, 0f);
                openBackground.gameObject.SetActive(false);
            }

            if (holdAfterOpen > 0f)
                yield return new WaitForSecondsRealtime(holdAfterOpen);

            Debug.Log("[WardDoor] Door open complete → loading " + wardSceneName);
            SceneManager.LoadScene(wardSceneName);
        }

        private Image CreateOpenOverlay()
        {
            if (closedBackground == null) return null;

            Sprite sprite = wardOpenSprite;
            if (sprite == null) return null;

            RectTransform closedRect = closedBackground.rectTransform;
            GameObject go = new GameObject("bg_wardopen", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.layer = closedBackground.gameObject.layer;

            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(closedRect.parent, false);
            rect.SetSiblingIndex(closedRect.GetSiblingIndex() + 1);
            rect.anchorMin = closedRect.anchorMin;
            rect.anchorMax = closedRect.anchorMax;
            rect.pivot = closedRect.pivot;
            rect.anchoredPosition = closedRect.anchoredPosition;
            rect.sizeDelta = closedRect.sizeDelta;
            rect.localRotation = closedRect.localRotation;
            rect.localScale = closedRect.localScale;

            Image image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = closedBackground.preserveAspect;
            image.raycastTarget = false;
            SetImageAlpha(image, 0f);
            openBackground = image;
            return image;
        }

        private static void SetImageAlpha(Image image, float alpha)
        {
            if (image == null) return;
            Color color = image.color;
            color.a = alpha;
            image.color = color;
        }
    }
}
