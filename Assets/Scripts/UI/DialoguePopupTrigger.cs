using UnityEngine;
using UnityEngine.UI;

namespace MonsterMaster.UI
{
    [RequireComponent(typeof(Button))]
    public sealed class DialoguePopupTrigger : MonoBehaviour
    {
        [SerializeField] private DialogueChoicePopup popup;
        [SerializeField] private string dialogueId;
        private DialogueConfigTable table;

        private void Awake()
        {
            table = DialogueConfigTable.LoadFromResources("Data/dialogues");
            GetComponent<Button>().onClick.AddListener(Open);
        }

        private void Open()
        {
            if (table.TryGet(dialogueId, out DialogueConfigTable.Entry entry)) popup.Show(entry);
            else Debug.LogError("Dialogue ID does not exist in dialogues.csv: " + dialogueId, this);
        }
    }
}
