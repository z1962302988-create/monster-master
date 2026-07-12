using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MonsterMaster.UI
{
    /// <summary>
    /// Login screen bootstrap — wires the start and settings buttons.
    /// Attach this to the Canvas or a root GameObject in the Login scene.
    /// </summary>
    public sealed class LoginSceneBootstrap : MonoBehaviour
    {
        [SerializeField] private Button startButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private string mainSceneName = "main";

        private void Awake()
        {
            if (startButton != null)
                startButton.onClick.AddListener(OnStartGame);

            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnSettings);
        }

        private void OnStartGame()
        {
            Debug.Log("[Login] Start Game clicked → loading " + mainSceneName);
            SceneManager.LoadScene(mainSceneName);
        }

        private void OnSettings()
        {
            Debug.Log("[Login] Settings clicked — panel not yet implemented.");
            // TODO: show a settings popup / panel
        }
    }
}
