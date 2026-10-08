using UnityEngine;
namespace Festival
{
    public sealed class AudioManager : MonoBehaviour
    {
        public AudioSource Music { get; private set; }
        AudioSource ui;
        AudioSource[] voices;
        GameplayConfig config;
        public float MusicVolume { get; private set; }
        public float SfxVolume { get; private set; }
        public float UiVolume { get; private set; }
        public void Initialize(GameplayConfig config)
        {
            this.config = config;
            Music = Source("Music"); ui = Source("UI SFX");
            voices = new AudioSource[Mathf.Max(1, config.maxAudioVoices)];
            for (int i = 0; i < voices.Length; i++) voices[i] = Source("Gameplay voice " + i);
            MusicVolume = PlayerPrefs.GetFloat("festival.music", .65f);
            SfxVolume = PlayerPrefs.GetFloat("festival.sfx", .35f);
            UiVolume = PlayerPrefs.GetFloat("festival.ui", .45f);
            ApplyVolumes();
        }
        AudioSource Source(string label)
        {
            var go = new GameObject(label); go.transform.SetParent(transform);
            var source = go.AddComponent<AudioSource>(); source.playOnAwake = false;
            source.spatialBlend = 0; return source;
        }
        public void SetVolumes(float music, float sfx, float menu)
        {
            MusicVolume = music; SfxVolume = sfx; UiVolume = menu;
            PlayerPrefs.SetFloat("festival.music", music); PlayerPrefs.SetFloat("festival.sfx", sfx);
            PlayerPrefs.SetFloat("festival.ui", menu); PlayerPrefs.Save(); ApplyVolumes();
        }
        void ApplyVolumes()
        {
            Music.volume = MusicVolume; ui.volume = UiVolume;
            foreach (var voice in voices) voice.volume = SfxVolume;
        }
        public void Click() { if (config.uiClip != null) ui.PlayOneShot(config.uiClip); }
        public void Hit() { Play(config.hitClip, false, .35f); }
        public void Reward() { Play(config.rewardClip, true, .6f); }
        public void Results(bool passed) { Play(passed ? config.winClip : config.failClip, true, .65f); }
        void Play(AudioClip clip, bool priority, float gain)
        {
            if (clip == null) return;
            foreach (var voice in voices)
                if (!voice.isPlaying) { voice.clip = clip; voice.volume = SfxVolume * gain; voice.Play(); return; }
            if (priority) { voices[0].Stop(); voices[0].clip = clip; voices[0].volume = SfxVolume * gain; voices[0].Play(); }
        }
        public void PauseEffects(bool paused)
        { foreach (var voice in voices) { if (paused) voice.Pause(); else voice.UnPause(); } }
        public void StopAll()
        { Music.Stop(); ui.Stop(); foreach (var voice in voices) voice.Stop(); }
    }
}
