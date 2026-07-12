using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MonsterMaster.UI
{
    /// <summary>
    /// Drives the login screen intro animation sequence:
    ///   1. Background and Clinic appear immediately (already visible).
    ///   2. Title fades in while the background gradually blurs.
    ///   3. After the title is fully shown, the Start and Settings
    ///      buttons fade in smoothly.
    /// </summary>
    public sealed class LoginAnimation : MonoBehaviour
    {
        [Header("Title Fade-in")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] [Min(0.01f)] private float titleFadeDuration = 1.2f;
        [SerializeField] [Min(0f)] private float titleStartDelay = 0.4f;

        [Header("Background Blur")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Material backgroundBlurMaterial;
        [SerializeField] [Min(0f)] private float maxBlurSize = 4.5f;

        [Header("Button Fade-in")]
        [SerializeField] private GameObject startButton;
        [SerializeField] private GameObject settingsButton;
        [SerializeField] [Min(0f)] private float buttonAppearDelay = 0.3f;
        [SerializeField] [Min(0.01f)] private float buttonFadeDuration = 0.8f;

        private static readonly int BlurSizeId = Shader.PropertyToID("_BlurSize");

        private CanvasGroup  titleGroup;
        private CanvasGroup  startButtonGroup;
        private CanvasGroup  settingsButtonGroup;
        private Button       startBtn;
        private Button       settingsBtn;
        private Material     blurMaterialInstance;

        private void Start()
        {
            PrepareComponents();
            StartCoroutine(PlayAnimationSequence());
        }

        private void OnDestroy()
        {
            if (blurMaterialInstance != null)
                Destroy(blurMaterialInstance);
        }

        /// <summary>
        /// Ensure CanvasGroup components exist, cache Button references,
        /// and hide everything initially (Background and Clinic are already visible).
        /// </summary>
        private void PrepareComponents()
        {
            if (titleText != null)
            {
                titleGroup = titleText.gameObject.GetOrAddComponent<CanvasGroup>();
                titleGroup.alpha = 0f;
            }

            PrepareBackgroundBlur();

            // Start button
            if (startButton != null)
            {
                startButtonGroup = startButton.GetOrAddComponent<CanvasGroup>();
                startButtonGroup.alpha = 0f;
                startButtonGroup.blocksRaycasts = false;

                startBtn = startButton.GetComponent<Button>();
                if (startBtn != null) startBtn.interactable = false;
            }

            // Settings button
            if (settingsButton != null)
            {
                settingsButtonGroup = settingsButton.GetOrAddComponent<CanvasGroup>();
                settingsButtonGroup.alpha = 0f;
                settingsButtonGroup.blocksRaycasts = false;

                settingsBtn = settingsButton.GetComponent<Button>();
                if (settingsBtn != null) settingsBtn.interactable = false;
            }
        }

        private void PrepareBackgroundBlur()
        {
            if (backgroundImage == null)
            {
                Transform background = transform.Find("Background");
                if (background != null)
                    backgroundImage = background.GetComponent<Image>();
            }

            if (backgroundImage == null || backgroundBlurMaterial == null)
                return;

            blurMaterialInstance = new Material(backgroundBlurMaterial);
            blurMaterialInstance.SetFloat(BlurSizeId, 0f);
            backgroundImage.material = blurMaterialInstance;
        }

        /// <summary>
        /// Sets background blur amount in the 0–1 range (0 = sharp, 1 = maxBlurSize).
        /// </summary>
        public void SetBackgroundBlur(float normalized)
        {
            if (blurMaterialInstance == null)
                return;

            blurMaterialInstance.SetFloat(BlurSizeId, Mathf.Lerp(0f, maxBlurSize, Mathf.Clamp01(normalized)));
        }

        /// <summary>Current blur amount in the 0–1 range.</summary>
        public float GetBackgroundBlur()
        {
            if (blurMaterialInstance == null)
                return 0f;

            float size = blurMaterialInstance.GetFloat(BlurSizeId);
            return maxBlurSize > 0f ? Mathf.Clamp01(size / maxBlurSize) : 0f;
        }

        private IEnumerator PlayAnimationSequence()
        {
            // ---- Phase 1: initial delay (background & clinic are already visible) ----
            yield return new WaitForSeconds(titleStartDelay);

            // ---- Phase 2: title fade-in + background blur ----
            float elapsed = 0f;
            while (elapsed < titleFadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / titleFadeDuration));

                if (titleGroup != null)
                    titleGroup.alpha = t;

                SetBackgroundBlur(t);
                yield return null;
            }

            if (titleGroup != null)
                titleGroup.alpha = 1f;
            SetBackgroundBlur(1f);

            // ---- Phase 3: brief pause then buttons fade in ----
            yield return new WaitForSeconds(buttonAppearDelay);

            float buttonElapsed = 0f;
            while (buttonElapsed < buttonFadeDuration)
            {
                buttonElapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(buttonElapsed / buttonFadeDuration));

                if (startButtonGroup != null) startButtonGroup.alpha = t;
                if (settingsButtonGroup != null) settingsButtonGroup.alpha = t;

                yield return null;
            }

            // ---- Finalise: ensure full opacity & enable interaction ----
            if (startButtonGroup != null)
            {
                startButtonGroup.alpha = 1f;
                startButtonGroup.blocksRaycasts = true;
                if (startBtn != null) startBtn.interactable = true;
            }

            if (settingsButtonGroup != null)
            {
                settingsButtonGroup.alpha = 1f;
                settingsButtonGroup.blocksRaycasts = true;
                if (settingsBtn != null) settingsBtn.interactable = true;
            }
        }
    }

    /// <summary>
    /// Tiny extension so we don't need to write GetComponent /
    /// AddComponent boilerplate everywhere.
    /// </summary>
    internal static class ComponentExtensions
    {
        public static T GetOrAddComponent<T>(this GameObject go) where T : Component
        {
            T comp = go.GetComponent<T>();
            return comp != null ? comp : go.AddComponent<T>();
        }
    }
}
