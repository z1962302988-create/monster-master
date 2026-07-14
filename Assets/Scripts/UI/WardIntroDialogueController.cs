using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace MonsterMaster.UI
{
    public sealed class WardIntroDialogueController : MonoBehaviour
    {
        private const string FirstDialogueId = "ward_intro_001";
        private const string WaitForRightWallAction = "wait_right_wall";
        private const string WaitForPuzzleAction = "wait_puzzle";
        private const string CloseDialogueAction = "close_dialogue";

        [SerializeField] private DialogueChoicePopup dialoguePrefab;
        [SerializeField] private Button rightWallButton;
        [SerializeField] private Button namePlateButton;
        [SerializeField] private WardBlockPuzzleTrigger puzzleTrigger;

        private DialogueChoicePopup popup;
        private DialogueConfigTable table;
        private bool waitingForRightWall;

        private void Awake()
        {
            table = DialogueConfigTable.LoadFromResources("Data/ward_intro_dialogues");
            popup = Instantiate(dialoguePrefab, transform);
            popup.name = "WardIntroDialoguePopup";
            popup.gameObject.SetActive(false);
            popup.OptionSelected += OnOptionSelected;

            if (rightWallButton != null)
            {
                rightWallButton.interactable = false;
                rightWallButton.onClick.AddListener(OnRightWallClicked);
            }

            if (namePlateButton != null)
                namePlateButton.interactable = false;

            if (puzzleTrigger != null)
                puzzleTrigger.PuzzleCompleted += OnPuzzleCompleted;
        }

        private IEnumerator Start()
        {
            yield return new WaitForSecondsRealtime(1f);
            Show(FirstDialogueId);
        }

        private void OnDestroy()
        {
            if (popup != null)
                popup.OptionSelected -= OnOptionSelected;

            if (rightWallButton != null)
                rightWallButton.onClick.RemoveListener(OnRightWallClicked);

            if (puzzleTrigger != null)
                puzzleTrigger.PuzzleCompleted -= OnPuzzleCompleted;
        }

        private void OnOptionSelected(string action)
        {
            if (action == WaitForRightWallAction)
            {
                waitingForRightWall = true;
                if (rightWallButton != null)
                    rightWallButton.interactable = true;
                return;
            }

            if (action == WaitForPuzzleAction)
            {
                if (namePlateButton != null)
                    namePlateButton.interactable = true;
                return;
            }

            if (action == CloseDialogueAction)
                return;

            Show(action);
        }

        private void OnRightWallClicked()
        {
            if (!waitingForRightWall) return;
            waitingForRightWall = false;
            rightWallButton.interactable = false;
            Show("ward_right_001");
        }

        private void OnPuzzleCompleted()
        {
            if (namePlateButton != null)
                namePlateButton.interactable = false;
            Show("ward_complete_001");
        }

        private void Show(string dialogueId)
        {
            if (table.TryGet(dialogueId, out DialogueConfigTable.Entry entry))
                popup.Show(entry);
            else
                Debug.LogError("Ward intro dialogue ID does not exist: " + dialogueId, this);
        }
    }
}
