using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MonsterMaster.UI
{
    public sealed class DialogueChoicePopup : MonoBehaviour
    {
        [SerializeField] private TMP_Text speakerNameText;
        [SerializeField] private TMP_Text dialogueText;
        [SerializeField] private RectTransform optionsPanel;
        [SerializeField] private Button optionButtonTemplate;
        [SerializeField, Min(0.01f)] private float secondsPerCharacter = 0.06f;

        private readonly List<Button> optionButtons = new List<Button>();
        private Coroutine sequence;
        public event Action<string> OptionSelected;

        public void Show(DialogueConfigTable.Entry entry)
        {
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            if (sequence != null) StopCoroutine(sequence);
            sequence = StartCoroutine(Play(entry));
        }

        public void OnOverlayClick()
        {
            Close();
        }

        public void Close()
        {
            if (sequence != null) StopCoroutine(sequence);
            sequence = null;
            gameObject.SetActive(false);
        }

        private IEnumerator Play(DialogueConfigTable.Entry entry)
        {
            speakerNameText.text = entry.Speaker;
            dialogueText.text = entry.Text;
            dialogueText.maxVisibleCharacters = 0;
            optionsPanel.gameObject.SetActive(false);
            BuildOptions(entry);

            for (int i = 1; i <= entry.Text.Length; i++)
            {
                dialogueText.maxVisibleCharacters = i;
                yield return new WaitForSecondsRealtime(secondsPerCharacter);
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

        private void BuildOptions(DialogueConfigTable.Entry entry)
        {
            foreach (Button button in optionButtons) Destroy(button.gameObject);
            optionButtons.Clear();
            optionButtonTemplate.gameObject.SetActive(false);
            foreach (DialogueConfigTable.Option option in entry.Options)
            {
                Button button = Instantiate(optionButtonTemplate, optionsPanel);
                button.name = "Option_" + option.Action;
                button.gameObject.SetActive(true);
                button.GetComponentInChildren<TMP_Text>().text = option.Label;
                string action = option.Action;
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => Select(action));
                optionButtons.Add(button);
            }
        }

        private void Select(string action)
        {
            Debug.Log("Dialogue option selected: " + action);
            OptionSelected?.Invoke(action);
            Close();
        }
    }
}
