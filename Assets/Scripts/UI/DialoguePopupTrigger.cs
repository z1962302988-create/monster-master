using MonsterMaster.Characters;
using UnityEngine;
using UnityEngine.UI;

namespace MonsterMaster.UI
{
    [RequireComponent(typeof(Button))]
    public sealed class DialoguePopupTrigger : MonoBehaviour
    {
        [SerializeField] private DialogueChoicePopup popup;
        [SerializeField] private string dialogueId;
        [SerializeField] private SpriteBlink speakerFace;

        private DialogueConfigTable table;
        private bool subscribed;

        private void Awake()
        {
            table = DialogueConfigTable.LoadFromResources("Data/dialogues");
            if (speakerFace == null)
                speakerFace = GetComponent<SpriteBlink>();

            GetComponent<Button>().onClick.AddListener(Open);
            Subscribe();
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void Subscribe()
        {
            if (subscribed || popup == null) return;
            popup.DialogueRevealStarted += OnRevealStarted;
            popup.DialogueRevealEnded += OnRevealEnded;
            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed || popup == null) return;
            popup.DialogueRevealStarted -= OnRevealStarted;
            popup.DialogueRevealEnded -= OnRevealEnded;
            subscribed = false;
        }

        private void Open()
        {
            if (table.TryGet(dialogueId, out DialogueConfigTable.Entry entry)) popup.Show(entry);
            else Debug.LogError("Dialogue ID does not exist in dialogues.csv: " + dialogueId, this);
        }

        private void OnRevealStarted()
        {
            if (speakerFace != null) speakerFace.StartTalking();
        }

        private void OnRevealEnded()
        {
            if (speakerFace != null) speakerFace.StopTalking();
        }
    }
}
