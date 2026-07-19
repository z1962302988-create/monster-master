using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MonsterMaster.Systems
{
    /// <summary>Persistent scene-aware BGM player with gentle crossfades.</summary>
    public sealed class SceneBgmPlayer : MonoBehaviour
    {
        private static SceneBgmPlayer instance;
        private AudioSource source;
        private Coroutine transition;
        private const float TargetVolume = 0.1f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (instance != null) return;
            var go = new GameObject("Scene BGM Player");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<SceneBgmPlayer>();
        }

        private void Awake()
        {
            source = gameObject.AddComponent<AudioSource>();
            source.loop = true;
            source.playOnAwake = false;
            source.volume = 0f;
            source.spatialBlend = 0f;
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            PlayForScene(SceneManager.GetActiveScene().name);
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            PlayForScene(scene.name);
        }

        private void OnSceneUnloaded(Scene scene)
        {
            if (scene.name == "BlockPuzzleTest")
                PlayForScene(SceneManager.GetActiveScene().name);
        }

        private void PlayForScene(string sceneName)
        {
            string clipName = sceneName switch
            {
                "Login" => "BGM_Login_MidnightClinic",
                "main" => "BGM_Main_GentleReception",
                "ward" => "BGM_Ward_QuietCompanionship",
                "BlockPuzzleTest" => "BGM_Puzzle_MistAndName",
                _ => "BGM_Main_GentleReception"
            };
            AudioClip next = Resources.Load<AudioClip>("Audio/Music/" + clipName);
            if (next == null || source.clip == next) return;
            if (transition != null) StopCoroutine(transition);
            transition = StartCoroutine(Crossfade(next));
        }

        private IEnumerator Crossfade(AudioClip next)
        {
            while (source.volume > 0.01f)
            {
                source.volume -= TargetVolume * Time.unscaledDeltaTime / 0.8f;
                yield return null;
            }
            source.Stop(); source.clip = next; source.Play();
            while (source.volume < TargetVolume)
            {
                source.volume += TargetVolume * Time.unscaledDeltaTime / 1.2f;
                yield return null;
            }
            source.volume = TargetVolume;
        }
    }
}
