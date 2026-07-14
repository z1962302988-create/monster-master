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
        [Serializable]
        private sealed class PortraitLayout
        {
            public int id;
            public string speaker;
            public Sprite portrait;
            public Vector2 anchoredPosition;
            [Min(0.01f)] public float scale = 1f;
        }

        [SerializeField] private TMP_Text speakerNameText;
        [SerializeField] private TMP_Text dialogueText;
        [SerializeField] private Image portraitImage;
        [SerializeField] private Sprite nursePortrait;
        [SerializeField] private Sprite wuwuPortrait;
        [SerializeField] private List<PortraitLayout> portraitLayouts = new List<PortraitLayout>();
        [SerializeField] private bool showPortrait = true;
        [SerializeField] private SpriteBlink portraitAnimator;
        [SerializeField] private RectTransform optionsPanel;
        [SerializeField] private Button optionButtonTemplate;
        [SerializeField] private Image nextIcon;
        [SerializeField, Min(0.01f)] private float secondsPerCharacter = 0.14f;
        [SerializeField, Min(0.1f)] private float blinkInterval = 2f;
        [SerializeField] private DialogueAnimaleseVoice animaleseVoice;
        [SerializeField, Min(0.04f)] private float characterFadeDuration = 0.2f;
        [SerializeField, Min(0f)] private float characterRiseDistance = 0f;

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

        private void Awake()
        {
            if (portraitImage != null)
                portraitImage.gameObject.SetActive(false);
            if (portraitAnimator != null)
                portraitAnimator.enabled = false;

            if (animaleseVoice == null)
                animaleseVoice = GetComponent<DialogueAnimaleseVoice>();
            if (animaleseVoice == null)
                animaleseVoice = gameObject.AddComponent<DialogueAnimaleseVoice>();
        }

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
            if (animaleseVoice != null)
                animaleseVoice.Stop();
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
            ShowCompleteDialogue();
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
            speakerNameText.text = entry.Speaker == "玩家" && entry.Options.Count <= 1
                ? "我"
                : entry.Speaker;
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
            if (animaleseVoice != null)
                animaleseVoice.BeginLine(entry.Text, entry.Speaker);

            yield return RevealDialogue(entry.Text);

            ShowCompleteDialogue();
            isRevealing = false;
            skipRequested = false;
            StopBlink();
            if (portraitAnimator != null)
                portraitAnimator.StopTalking();
            if (animaleseVoice != null)
                animaleseVoice.Stop();
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

        private IEnumerator RevealDialogue(string text)
        {
            dialogueText.maxVisibleCharacters = text.Length;
            dialogueText.ForceMeshUpdate();
            TMP_TextInfo textInfo = dialogueText.textInfo;
            TMP_MeshInfo[] originalMesh = textInfo.CopyMeshInfoVertexData();
            bool[] voiced = new bool[text.Length];
            float elapsed = 0f;
            float totalDuration = Mathf.Max(0f, (text.Length - 1) * secondsPerCharacter) + characterFadeDuration;

            while (!skipRequested && elapsed < totalDuration)
            {
                for (int i = 0; i < textInfo.characterCount; i++)
                {
                    TMP_CharacterInfo characterInfo = textInfo.characterInfo[i];
                    float startTime = i * secondsPerCharacter;
                    float progress = Mathf.SmoothStep(0f, 1f,
                        Mathf.Clamp01((elapsed - startTime) / characterFadeDuration));

                    if (!voiced[i] && elapsed >= startTime)
                    {
                        voiced[i] = true;
                        if (animaleseVoice != null)
                            animaleseVoice.Speak(text[i], i, text.Length);
                    }

                    if (!characterInfo.isVisible) continue;
                    int meshIndex = characterInfo.materialReferenceIndex;
                    int vertexIndex = characterInfo.vertexIndex;
                    Vector3[] vertices = textInfo.meshInfo[meshIndex].vertices;
                    Color32[] colors = textInfo.meshInfo[meshIndex].colors32;
                    Vector3[] originalVertices = originalMesh[meshIndex].vertices;
                    float offsetY = -characterRiseDistance * (1f - progress);
                    byte alpha = (byte)Mathf.RoundToInt(255f * progress);

                    for (int vertex = 0; vertex < 4; vertex++)
                    {
                        vertices[vertexIndex + vertex] = originalVertices[vertexIndex + vertex]
                            + new Vector3(0f, offsetY, 0f);
                        Color32 color = colors[vertexIndex + vertex];
                        color.a = alpha;
                        colors[vertexIndex + vertex] = color;
                    }
                }

                for (int i = 0; i < textInfo.meshInfo.Length; i++)
                {
                    textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
                    textInfo.meshInfo[i].mesh.colors32 = textInfo.meshInfo[i].colors32;
                    dialogueText.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
                }

                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        private void ShowCompleteDialogue()
        {
            dialogueText.maxVisibleCharacters = dialogueText.text.Length;
            // Rebuilding restores the original vertices and full opacity after the
            // per-character flow animation, including when the player skips it.
            dialogueText.ForceMeshUpdate();
        }

        private void UpdatePortrait(string speaker)
        {
            if (portraitImage == null) return;

            Sprite portrait = null;
            Vector2 position = new Vector2(0f, -81f);
            float scale = 1f;
            if (showPortrait)
            {
                int portraitId = speaker == "兔子护士" ? 1 : speaker == "雾雾" ? 2 : 0;
                for (int i = 0; portraitId > 0 && i < portraitLayouts.Count; i++)
                {
                    PortraitLayout layout = portraitLayouts[i];
                    if (layout == null || layout.id != portraitId) continue;
                    portrait = layout.portrait;
                    position = layout.anchoredPosition;
                    scale = Mathf.Max(0.01f, layout.scale);
                    break;
                }

                // Backwards-compatible fallback for prefabs made before the layout tool.
                if (portrait == null && speaker == "兔子护士") portrait = nursePortrait;
                else if (portrait == null && speaker == "雾雾") portrait = wuwuPortrait;
            }

            bool animateNurse = showPortrait && speaker == "兔子护士";
            if (portraitAnimator != null && portraitAnimator.enabled != animateNurse)
                portraitAnimator.enabled = animateNurse;

            portraitImage.sprite = portrait;
            portraitImage.enabled = portrait != null;
            portraitImage.preserveAspect = true;
            if (portrait == null)
            {
                portraitImage.gameObject.SetActive(false);
                return;
            }

            portraitImage.rectTransform.anchoredPosition = position;
            portraitImage.rectTransform.localScale = Vector3.one * scale;
            if (animateNurse && portraitAnimator != null)
                portraitAnimator.SetIdleAnchoredPosition(position);
            portraitImage.gameObject.SetActive(true);
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
            Close();
            OptionSelected?.Invoke(action);
        }
    }
}
