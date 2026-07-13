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

        public Canvas PopupCanvas => popupCanvas;
        public RectTransform ContentRoot => contentRoot;
        public RectTransform BoardHost => boardHost;
        public Button RestartButton => restartButton;
        public Button CloseButton => closeButton;
    }
}
