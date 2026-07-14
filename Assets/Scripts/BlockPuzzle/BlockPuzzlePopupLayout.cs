using UnityEngine;
using UnityEngine.UI;

namespace MonsterMaster.BlockPuzzle
{
    public sealed class BlockPuzzlePopupLayout : MonoBehaviour
    {
        [SerializeField] private Canvas popupCanvas;
        [SerializeField] private RectTransform contentRoot;
        [SerializeField] private RectTransform boardHost;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button skipButton;
        [SerializeField] private GameObject victoryPopup;
        [SerializeField] private Button victoryRestartButton;

        public Canvas PopupCanvas => popupCanvas;
        public RectTransform ContentRoot => contentRoot;
        public RectTransform BoardHost => boardHost;
        public Button RestartButton => restartButton;
        public Button CloseButton => closeButton;
        public Button SkipButton => skipButton;
        public GameObject VictoryPopup => victoryPopup;
        public Button VictoryRestartButton => victoryRestartButton;
    }
}
