using System;
using System.Collections;
using System.Collections.Generic;
using MonsterMaster.Characters;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MonsterMaster.UI
{
    public sealed class DialogueChoicePopup : MonoBehaviour
    {
        [SerializeField] private TMP_Text speakerNameText;
        [SerializeField] private TMP_Text dialogueText;
        [SerializeField] private Image portraitImage;
        [SerializeField] private Sprite nursePortrait;
        [SerializeField] private bool showPortrait = true;
        [SerializeField] private SpriteBlink portraitAnimator;
        [SerializeField] private RectTransform optionsPanel;
        [SerializeField] private Button optionButtonTemplate;
        [SerializeField] private Image nextIcon;
        [SerializeField, Min(0.01f)] private float secondsPerCharacter = 0.06f;
        [SerializeField, Min(0.1f)] private float blinkInterval = 2f;

        [SerializeField, Min(0f)] private float buttonPaddingX = 24f;
        [SerializeField, Min(0f)] private float buttonPaddingY = 16f;

        private readonly List<Button> optionButtons = new List<Button>();
        private Coroutine sequence;
        private Coroutine blink;
        private bool isRevealing;
        private bool skipRequested;
        private string tapAdvanceAction;
        private int revealReadyFrame = -1;
        public event Action<string> OptionSelected;
        public event Action DialogueRevealStarted;
        public event Action DialogueRevealEnded;

        public void Show(DialogueConfigTable.Entry entry)
        {
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            if (sequence != null) StopCoroutine(sequence);
            sequence = StartCoroutine(Play(entry));
        }

        public void Close()
        {
            if (sequence != null) StopCoroutine(sequence);
            sequence = null;
            StopBlink();
            if (isRevealing)
            {
                isRevealing = false;
                DialogueRevealEnded?.Invoke();
            }
            if (portraitAnimator != null)
                portraitAnimator.StopTalking();
            skipRequested = false;
            tapAdvanceAction = null;
            gameObject.SetActive(false);
        }

        public void OnOverlayClick()
        {
            if (isRevealing) TrySkipReveal();
            else TryAdvance();
        }

        private void TrySkipReveal()
        {
            if (!isRevealing || Time.frameCount <= revealReadyFrame) return;
            skipRequested = true;
            dialogueText.maxVisibleCharacters = dialogueText.text.Length;
            StopBlink();
        }

        private void TryAdvance()
        {
            if (isRevealing || string.IsNullOrEmpty(tapAdvanceAction)) return;
            string action = tapAdvanceAction;
            tapAdvanceAction = null;
            Select(action);
        }

        private IEnumerator Play(DialogueConfigTable.Entry entry)
        {
            speakerNameText.text = entry.Speaker;
            UpdatePortrait(entry.Speaker);
            dialogueText.text = entry.Text;
            dialogueText.maxVisibleCharacters = 0;
            optionsPanel.gameObject.SetActive(false);
            BuildOptions(entry);

            skipRequested = false;
            isRevealing = true;
            revealReadyFrame = Time.frameCount;
            StartBlink();
            if (showPortrait && entry.Speaker == "兔子护士" && portraitAnimator != null)
                portraitAnimator.StartTalking();
            DialogueRevealStarted?.Invoke();

            for (int i = 1; i <= entry.Text.Length; i++)
            {
                if (skipRequested) break;
                dialogueText.maxVisibleCharacters = i;
                float waited = 0f;
                while (waited < secondsPerCharacter)
                {
                    if (skipRequested) break;
                    waited += Time.unscaledDeltaTime;
                    yield return null;
                }
            }

            dialogueText.maxVisibleCharacters = entry.Text.Length;
            isRevealing = false;
            skipRequested = false;
            StopBlink();
            if (portraitAnimator != null)
                portraitAnimator.StopTalking();
            DialogueRevealEnded?.Invoke();

            if (!string.IsNullOrEmpty(tapAdvanceAction))
            {
                optionsPanel.gameObject.SetActive(false);
                StartBlink();
                sequence = null;
                yield break;
            }

            optionsPanel.gameObject.SetActive(true);
            CanvasGroup group = optionsPanel.GetComponent<CanvasGroup>();
            Vector2 end = optionsPanel.anchoredPosition;
            Vector2 start = end + new Vector2(70f, 0f);
            optionsPanel.anchoredPosition = start;
            group.alpha = 0f;
            float elapsed = 0f;
            while (elapsed < 0.22f)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / 0.22f);
                optionsPanel.anchoredPosition = Vector2.LerpUnclamped(start, end, t);
                group.alpha = t;
                yield return null;
            }
            optionsPanel.anchoredPosition = end;
            group.alpha = 1f;
            sequence = null;
        }

        private void UpdatePortrait(string speaker)
        {
            if (portraitImage == null) return;

            Sprite portrait = showPortrait && speaker == "兔子护士" ? nursePortrait : null;
            portraitImage.sprite = portrait;
            portraitImage.enabled = portrait != null;
        }

        private void StartBlink()
        {
            StopBlink();
            if (nextIcon == null) return;
            nextIcon.gameObject.SetActive(true);
            blink = StartCoroutine(BlinkNextIcon());
        }

        private void StopBlink()
        {
            if (blink != null)
            {
                StopCoroutine(blink);
                blink = null;
            }

            if (nextIcon == null) return;
            Color color = nextIcon.color;
            color.a = 1f;
            nextIcon.color = color;
            nextIcon.gameObject.SetActive(false);
        }

        private IEnumerator BlinkNextIcon()
        {
            Color color = nextIcon.color;
            while (true)
            {
                color.a = 1f;
                nextIcon.color = color;
                yield return new WaitForSecondsRealtime(blinkInterval * 0.5f);
                color.a = 0f;
                nextIcon.color = color;
                yield return new WaitForSecondsRealtime(blinkInterval * 0.5f);
            }
        }

        private void BuildOptions(DialogueConfigTable.Entry entry)
        {
            foreach (Button button in optionButtons) Destroy(button.gameObject);
            optionButtons.Clear();
            optionButtonTemplate.gameObject.SetActive(false);
            tapAdvanceAction = null;

            // A single route is not a decision: advance it by tapping anywhere.
            if (entry.Options.Count == 1)
            {
                tapAdvanceAction = entry.Options[0].Action;
                return;
            }

            // Step 1: create all buttons and collect labels
            var labels = new List<TMP_Text>();
            foreach (DialogueConfigTable.Option option in entry.Options)
            {
                Button button = Instantiate(optionButtonTemplate, optionsPanel);
                button.name = "Option_" + option.Action;
                button.gameObject.SetActive(true);

                TMP_Text label = button.GetComponentInChildren<TMP_Text>();
                label.text = option.Label;
                labels.Add(label);

                string action = option.Action;
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => Select(action));
                optionButtons.Add(button);
            }

            // Step 2: measure the full width of the longest option.
            TMP_Text templateLabel = optionButtonTemplate.GetComponentInChildren<TMP_Text>();
            float maxLineWidth = 0f;
            foreach (TMP_Text label in labels)
            {
                float width = templateLabel.GetPreferredValues(label.text).x;
                if (width > maxLineWidth) maxLineWidth = width;
            }

            if (maxLineWidth <= 0f)
                maxLineWidth = templateLabel.GetPreferredValues("继续").x;

            // Step 3: keep every option on one line and use a uniform button width.
            foreach (TMP_Text label in labels)
            {
                // Change label anchor from stretch to centered fixed-width
                RectTransform textRect = label.rectTransform;
                textRect.anchorMin = new Vector2(0.5f, 0f);
                textRect.anchorMax = new Vector2(0.5f, 1f);
                textRect.pivot = new Vector2(0.5f, 0.5f);
                textRect.sizeDelta = new Vector2(maxLineWidth, 0f);
                textRect.anchoredPosition = Vector2.zero;

                label.enableWordWrapping = false;
                label.overflowMode = TextOverflowModes.Overflow;

                Vector2 preferred = label.GetPreferredValues(label.text);
                LayoutElement layout = label.GetComponentInParent<LayoutElement>();
                if (layout != null)
                    layout.preferredHeight = preferred.y + buttonPaddingY;
            }

            // Step 4: resize panel to fit content
            VerticalLayoutGroup vlg = optionsPanel.GetComponent<VerticalLayoutGroup>();
            vlg.childControlWidth = true;
            vlg.childForceExpandWidth = true;

            float panelWidth = maxLineWidth + buttonPaddingX + vlg.padding.left + vlg.padding.right;
            optionsPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, panelWidth);

            // Force layout to apply immediately (even if panel was inactive)
            LayoutRebuilder.ForceRebuildLayoutImmediate(optionsPanel);
        }

        private void Select(string action)
        {
            Debug.Log("Dialogue option selected: " + action);
            OptionSelected?.Invoke(action);
            Close();
        }
    }
}
