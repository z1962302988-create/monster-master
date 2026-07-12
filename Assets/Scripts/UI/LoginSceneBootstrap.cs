using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MonsterMaster.UI
{
    /// <summary>
    /// Login screen bootstrap — wires buttons and plays the clinic door-open
    /// transition before entering the main scene.
    /// </summary>
    public sealed class LoginSceneBootstrap : MonoBehaviour
    {
        [SerializeField] private Button startButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private string mainSceneName = "main";

        [Header("Door Open Transition")]
        [SerializeField] private Image clinicImage;
        [SerializeField] private Image clinicOpenImage;
        [SerializeField] private Sprite clinicOpenSprite;
        [SerializeField] private GameObject titleObject;
        [SerializeField, Min(0.01f)] private float uiFadeDuration = 0.35f;
        [SerializeField, Min(0.01f)] private float doorOpenDuration = 1.1f;
        [SerializeField, Min(0f)] private float holdAfterOpen = 0.45f;

        private bool starting;
        private CanvasGroup titleGroup;
        private CanvasGroup startGroup;
        private CanvasGroup settingsGroup;
        private LoginAnimation loginAnimation;

        private void Awake()
        {
            ResolveReferences();

            if (startButton != null)
                startButton.onClick.AddListener(OnStartGame);

            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnSettings);

            // Start with door closed.
            if (clinicOpenImage != null)
            {
                SetImageAlpha(clinicOpenImage, 0f);
                clinicOpenImage.gameObject.SetActive(true);
            }
        }

        private void ResolveReferences()
        {
            if (clinicImage == null)
            {
                Transform clinic = transform.Find("Clinic");
                if (clinic != null) clinicImage = clinic.GetComponent<Image>();
            }

            if (clinicOpenImage == null)
            {
                Transform open = transform.Find("ClinicOpen");
                if (open != null) clinicOpenImage = open.GetComponent<Image>();
            }

            if (clinicOpenImage != null && clinicOpenSprite != null)
                clinicOpenImage.sprite = clinicOpenSprite;

            if (titleObject == null)
            {
                Transform title = transform.Find("Title");
                if (title != null) titleObject = title.gameObject;
            }

            if (startButton == null)
            {
                Transform start = transform.Find("StartButton");
                if (start != null) startButton = start.GetComponent<Button>();
            }

            if (settingsButton == null)
            {
                Transform settings = transform.Find("SettingsButton");
                if (settings != null) settingsButton = settings.GetComponent<Button>();
            }

            if (loginAnimation == null)
                loginAnimation = GetComponent<LoginAnimation>();
        }

        private void OnStartGame()
        {
            if (starting) return;
            starting = true;
            StartCoroutine(PlayStartSequence());
        }

        private void OnSettings()
        {
            Debug.Log("[Login] Settings clicked — panel not yet implemented.");
        }

        private IEnumerator PlayStartSequence()
        {
            ResolveReferences();

            if (startButton != null) startButton.interactable = false;
            if (settingsButton != null) settingsButton.interactable = false;

            CacheFadeGroups();

            // Fade out title and buttons so the clinic becomes the focus.
            float elapsed = 0f;
            while (elapsed < uiFadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / uiFadeDuration));
                SetGroupAlpha(titleGroup, t);
                SetGroupAlpha(startGroup, t);
                SetGroupAlpha(settingsGroup, t);
                yield return null;
            }

            SetGroupAlpha(titleGroup, 0f);
            SetGroupAlpha(startGroup, 0f);
            SetGroupAlpha(settingsGroup, 0f);
            if (titleGroup != null) titleGroup.blocksRaycasts = false;
            if (startGroup != null) startGroup.blocksRaycasts = false;
            if (settingsGroup != null) settingsGroup.blocksRaycasts = false;

            Image openImage = clinicOpenImage;
            if (openImage == null)
                openImage = CreateClinicOpenOverlay();

            if (openImage != null)
            {
                if (clinicOpenSprite != null)
                    openImage.sprite = clinicOpenSprite;

                openImage.gameObject.SetActive(true);
                SetImageAlpha(openImage, 0f);

                float blurStart = loginAnimation != null ? loginAnimation.GetBackgroundBlur() : 0f;

                elapsed = 0f;
                while (elapsed < doorOpenDuration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float linear = Mathf.Clamp01(elapsed / doorOpenDuration);
                    float t = linear * linear * (3f - 2f * linear);
                    SetImageAlpha(openImage, t);

                    // Clear background blur as the door opens.
                    if (loginAnimation != null)
                        loginAnimation.SetBackgroundBlur(Mathf.Lerp(blurStart, 0f, t));

                    yield return null;
                }

                SetImageAlpha(openImage, 1f);
                if (loginAnimation != null)
                    loginAnimation.SetBackgroundBlur(0f);
                if (clinicImage != null && openImage.sprite != null)
                    clinicImage.sprite = openImage.sprite;
            }
            else if (clinicImage != null && clinicOpenSprite != null)
            {
                // Last-resort hard swap if overlay could not be created.
                clinicImage.sprite = clinicOpenSprite;
                if (loginAnimation != null)
                    loginAnimation.SetBackgroundBlur(0f);
            }
            else
            {
                Debug.LogWarning("[Login] Door-open sprites are missing — loading main scene without transition.", this);
                if (loginAnimation != null)
                    loginAnimation.SetBackgroundBlur(0f);
            }

            if (holdAfterOpen > 0f)
                yield return new WaitForSecondsRealtime(holdAfterOpen);

            Debug.Log("[Login] Start Game → loading " + mainSceneName);
            SceneManager.LoadScene(mainSceneName);
        }

        private void CacheFadeGroups()
        {
            if (titleObject != null)
                titleGroup = GetOrAddCanvasGroup(titleObject);

            if (startButton != null)
                startGroup = GetOrAddCanvasGroup(startButton.gameObject);

            if (settingsButton != null)
                settingsGroup = GetOrAddCanvasGroup(settingsButton.gameObject);
        }

        private Image CreateClinicOpenOverlay()
        {
            if (clinicImage == null) return null;

            Sprite sprite = clinicOpenSprite;
            if (sprite == null && clinicOpenImage != null)
                sprite = clinicOpenImage.sprite;
            if (sprite == null) return null;

            RectTransform clinicRect = clinicImage.rectTransform;
            GameObject go = new GameObject("ClinicOpen", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.layer = clinicImage.gameObject.layer;

            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(clinicRect.parent, false);
            rect.SetSiblingIndex(clinicRect.GetSiblingIndex() + 1);
            rect.anchorMin = clinicRect.anchorMin;
            rect.anchorMax = clinicRect.anchorMax;
            rect.pivot = clinicRect.pivot;
            rect.anchoredPosition = clinicRect.anchoredPosition;
            rect.sizeDelta = clinicRect.sizeDelta;
            rect.localRotation = clinicRect.localRotation;
            rect.localScale = clinicRect.localScale;

            Image image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = clinicImage.preserveAspect;
            image.raycastTarget = false;
            SetImageAlpha(image, 0f);
            clinicOpenImage = image;
            return image;
        }

        private static CanvasGroup GetOrAddCanvasGroup(GameObject go)
        {
            CanvasGroup group = go.GetComponent<CanvasGroup>();
            return group != null ? group : go.AddComponent<CanvasGroup>();
        }

        private static void SetGroupAlpha(CanvasGroup group, float alpha)
        {
            if (group != null) group.alpha = alpha;
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
