using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MonsterMaster.UI
{
    [RequireComponent(typeof(Button))]
    public sealed class WardReturnToMain : MonoBehaviour
    {
        [SerializeField] private string mainSceneName = "main";

        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(ReturnToMain);
        }

        private void OnDestroy()
        {
            if (button != null)
                button.onClick.RemoveListener(ReturnToMain);
        }

        private void ReturnToMain()
        {
            SceneManager.LoadScene(mainSceneName);
        }
    }
}
