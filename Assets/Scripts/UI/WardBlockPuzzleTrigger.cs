using System.Collections;
using MonsterMaster.BlockPuzzle;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MonsterMaster.UI
{
    [RequireComponent(typeof(Button))]
    public sealed class WardBlockPuzzleTrigger : MonoBehaviour
    {
        [SerializeField] private string puzzleSceneName = "BlockPuzzleTest";

        private Button button;
        private BlockPuzzleBootstrap popup;
        private bool loading;

        private void Awake()
        {
            HideDefaultButtonLabels();
            button = GetComponent<Button>();
            button.onClick.AddListener(Open);
        }

        private void HideDefaultButtonLabels()
        {
            Transform rightView = transform.parent;
            if (rightView == null) return;

            foreach (Text label in rightView.GetComponentsInChildren<Text>(true))
            {
                if (label.text == "Button") label.gameObject.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (button != null) button.onClick.RemoveListener(Open);
            if (popup != null) popup.CloseRequested -= Close;
        }

        private void Update()
        {
            if (popup != null && Input.GetKeyDown(KeyCode.Escape)) Close();
        }

        private void Open()
        {
            if (loading || popup != null) return;
            StartCoroutine(LoadPopup());
        }

        private IEnumerator LoadPopup()
        {
            loading = true;
            AsyncOperation operation = SceneManager.LoadSceneAsync(puzzleSceneName, LoadSceneMode.Additive);
            if (operation == null)
            {
                Debug.LogError("Unable to load Block Puzzle popup scene: " + puzzleSceneName, this);
                loading = false;
                yield break;
            }

            yield return operation;
            Scene popupScene = SceneManager.GetSceneByName(puzzleSceneName);
            foreach (GameObject root in popupScene.GetRootGameObjects())
            {
                Camera sceneCamera = root.GetComponentInChildren<Camera>(true);
                if (sceneCamera != null)
                {
                    AudioListener listener = sceneCamera.GetComponent<AudioListener>();
                    if (listener != null) listener.enabled = false;
                    sceneCamera.enabled = false;
                }

                if (popup == null)
                    popup = root.GetComponentInChildren<BlockPuzzleBootstrap>(true);
            }

            if (popup != null)
                popup.CloseRequested += Close;
            else
                Debug.LogError("BlockPuzzleTest has no BlockPuzzleBootstrap.", this);
            loading = false;
        }

        private void Close()
        {
            if (loading || popup == null) return;
            popup.CloseRequested -= Close;
            popup = null;
            StartCoroutine(UnloadPopup());
        }

        private IEnumerator UnloadPopup()
        {
            loading = true;
            AsyncOperation operation = SceneManager.UnloadSceneAsync(puzzleSceneName);
            if (operation != null) yield return operation;
            loading = false;
        }
    }
}
