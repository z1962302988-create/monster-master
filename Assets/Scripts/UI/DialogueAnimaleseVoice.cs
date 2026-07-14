using System.Collections.Generic;
using UnityEngine;

namespace MonsterMaster.UI
{
    /// <summary>
    /// Produces short, speech-like chirps in sync with typewriter dialogue.
    /// The sound follows the rhythm of the text without forming intelligible speech.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(AudioSource))]
    public sealed class DialogueAnimaleseVoice : MonoBehaviour
    {
        private const int SampleRate = 22050;

        [SerializeField, Range(0f, 1f)] private float volume = 0.22f;
        [SerializeField, Range(0.6f, 2.5f)] private float voicePitch = 1.82f;
        [SerializeField, Range(0.025f, 0.12f)] private float syllableLength = 0.075f;

        private readonly Dictionary<int, AudioClip> clips = new Dictionary<int, AudioClip>();
        private AudioSource source;
        private int lineSeed;
        private bool isWuwu;

        private void Awake()
        {
            source = GetComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 0f;
            source.ignoreListenerPause = true;
        }

        public void BeginLine(string text, string speaker)
        {
            Stop();
            lineSeed = StableHash(text + "|" + speaker);
            isWuwu = speaker == "雾雾";
        }

        public void Speak(char character, int index, int characterCount)
        {
            if (source == null || char.IsWhiteSpace(character) || char.IsPunctuation(character)) return;

            int key = StableHash(character.ToString()) ^ (lineSeed & 7) ^ (isWuwu ? 0x40000000 : 0);
            if (!clips.TryGetValue(key, out AudioClip clip))
            {
                clip = CreateSyllable(key, isWuwu);
                clips.Add(key, clip);
            }

            float position = characterCount > 1 ? index / (float)(characterCount - 1) : 0f;
            if (isWuwu)
            {
                // Her phrases gently fall away, and alternating syllables waver as if
                // she is unsure whether to continue speaking.
                float fallingTone = Mathf.Lerp(1f, 0.88f, position);
                float hesitation = index % 4 == 0 ? 0.93f : 1f;
                source.pitch = 0.96f * fallingTone * hesitation * (0.98f + Mathf.Abs(key % 3) * 0.012f);
                source.PlayOneShot(clip, volume * (index % 4 == 0 ? 0.48f : 0.62f));
            }
            else
            {
                float questionLift = position > 0.65f ? Mathf.Lerp(1f, 1.11f, (position - 0.65f) / 0.35f) : 1f;
                source.pitch = voicePitch * questionLift * (0.98f + Mathf.Abs(key % 5) * 0.01f);
                source.PlayOneShot(clip, volume);
            }
        }

        public void Stop()
        {
            if (source != null) source.Stop();
        }

        private void OnDestroy()
        {
            foreach (AudioClip clip in clips.Values)
                if (clip != null) Destroy(clip);
            clips.Clear();
        }

        private AudioClip CreateSyllable(int seed, bool softVoice)
        {
            float length = softVoice ? syllableLength * 1.28f : syllableLength;
            int sampleCount = Mathf.CeilToInt(SampleRate * length);
            float[] samples = new float[sampleCount];
            int positiveSeed = seed & 0x7fffffff;
            float fundamental = softVoice ? 175f + positiveSeed % 55 : 245f + positiveSeed % 105;
            float vowel = softVoice ? 560f + (positiveSeed / 7) % 360 : 1050f + (positiveSeed / 7) % 720;
            float phase = 0f;

            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)SampleRate;
                float normalized = i / (float)Mathf.Max(1, sampleCount - 1);
                float attack = softVoice ? 0.42f : 0.28f;
                float envelope = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(normalized / attack));
                envelope *= Mathf.Pow(1f - normalized, softVoice ? 0.7f : 0.42f);
                float wobbleAmount = softVoice ? 0.022f : 0.055f;
                float wobble = 1f + wobbleAmount * Mathf.Sin(2f * Mathf.PI * (softVoice ? 8f : 15f) * t);
                if (softVoice) wobble *= Mathf.Lerp(1.015f, 0.94f, normalized);
                phase += 2f * Mathf.PI * fundamental * wobble / SampleRate;

                float voiced = Mathf.Sin(phase) * (softVoice ? 0.62f : 0.42f);
                voiced += Mathf.Sin(phase * 2.01f) * (softVoice ? 0.18f : 0.30f);
                if (!softVoice) voiced += Mathf.Sin(phase * 3.02f) * 0.10f;
                voiced += Mathf.Sin(2f * Mathf.PI * vowel * t + Mathf.Sin(phase) * 0.85f)
                    * (softVoice ? 0.12f : 0.18f);
                samples[i] = voiced * envelope;
            }

            AudioClip clip = AudioClip.Create("Animalese_" + positiveSeed, sampleCount, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static int StableHash(string value)
        {
            unchecked
            {
                int hash = 23;
                for (int i = 0; i < value.Length; i++) hash = hash * 31 + value[i];
                return hash;
            }
        }
    }
}
