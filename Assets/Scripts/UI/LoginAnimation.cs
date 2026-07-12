using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MonsterMaster.UI
{
    /// <summary>
    /// Drives the login screen intro animation sequence:
    ///   1. Background and Clinic appear immediately (already visible).
    ///   2. Title text reveals character-by-character (typewriter effect).
    ///   3. After the title is fully shown, the Start and Settings
    ///      buttons fade in smoothly.
    /// </summary>
    public sealed class LoginAnimation : MonoBehaviour
    {
        [Header("Title Typewriter")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] [Min(1f)] private float charsPerSecond = 18f;
        [SerializeField] [Min(0f)] private float titleStartDelay = 0.4f;

        [Header("Button Fade-in")]
        [SerializeField] private GameObject startButton;
        [SerializeField] private GameObject settingsButton;
        [SerializeField] [Min(0f)] private float buttonAppearDelay = 0.3f;
        [SerializeField] [Min(0.01f)] private float buttonFadeDuration = 0.8f;

        private CanvasGroup  startButtonGroup;
        private CanvasGroup  settingsButtonGroup;
        private Button       startBtn;
        private Button       settingsBtn;

        private void Start()
        {
            PrepareComponents();
            StartCoroutine(PlayAnimationSequence());
        }

        /// <summary>
        /// Ensure CanvasGroup components exist, cache Button references,
        /// and hide everything initially (Background and Clinic are already visible).
        /// </summary>
        private void PrepareComponents()
        {
            // Title: start with no visible characters
            if (titleText != null)
            {
                titleText.ForceMeshUpdate();
                titleText.maxVisibleCharacters = 0;
            }

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

        private IEnumerator PlayAnimationSequence()
        {
            // ---- Phase 1: initial delay (background & clinic are already visible) ----
            yield return new WaitForSeconds(titleStartDelay);

            // ---- Phase 2: title typewriter ----
            if (titleText != null)
            {
                titleText.ForceMeshUpdate();
                int totalChars = titleText.textInfo.characterCount;

                if (totalChars > 0)
                {
                    float interval = 1f / charsPerSecond;

                    for (int i = 1; i <= totalChars; i++)
                    {
                        titleText.maxVisibleCharacters = i;
                        yield return new WaitForSeconds(interval);
                    }
                }
            }

            // ---- Phase 3: brief pause then buttons fade in ----
            yield return new WaitForSeconds(buttonAppearDelay);

            float elapsed = 0f;
            while (elapsed < buttonFadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / buttonFadeDuration);

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
