using UnityEngine;
using UnityEngine.UI;

namespace MonsterMaster.UI
{
    [RequireComponent(typeof(Button))]
    public sealed class WardViewSwitch : MonoBehaviour
    {
        [SerializeField] private GameObject currentView;
        [SerializeField] private GameObject targetView;

        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(SwitchView);
        }

        private void OnDestroy()
        {
            if (button != null)
                button.onClick.RemoveListener(SwitchView);
        }

        private void SwitchView()
        {
            if (targetView == null)
            {
                Debug.LogError("Ward view switch has no target view.", this);
                return;
            }

            targetView.SetActive(true);

            if (currentView != null)
                currentView.SetActive(false);
        }
    }
}
