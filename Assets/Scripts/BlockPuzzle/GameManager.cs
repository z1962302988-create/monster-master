using System;
using UnityEngine;
using UnityEngine.UI;

namespace MonsterMaster.BlockPuzzle
{
    public sealed class GameManager : MonoBehaviour
    {
        private BoardManager board;
        private Text pauseButtonText;
        private GameObject pauseOverlay;
        private GameObject victoryPopup;
        private Action restartAction;
        private bool paused;
        private bool finished;

        public void Initialize(
            BoardManager boardManager,
            LevelManager levelManager,
            Text pauseText,
            GameObject pausePanel,
            GameObject winPanel,
            Action onRestart)
        {
            board = boardManager;
            pauseButtonText = pauseText;
            pauseOverlay = pausePanel;
            victoryPopup = winPanel;
            restartAction = onRestart;
            levelManager.LevelCompleted += Win;
        }

        public void TogglePause()
        {
            if (finished) return;
            paused = !paused;
            Time.timeScale = paused ? 0f : 1f;
            board.CanInteract = !paused;
            if (pauseOverlay != null) pauseOverlay.SetActive(paused);
            if (pauseButtonText != null) pauseButtonText.text = paused ? "继续" : "暂停";
        }

        public void Restart()
        {
            Time.timeScale = 1f;
            restartAction?.Invoke();
        }

        private void Win()
        {
            if (finished) return;
            finished = true;
            board.CanInteract = false;
            victoryPopup.SetActive(true);
        }

        private void OnDestroy() { Time.timeScale = 1f; }
    }
}
