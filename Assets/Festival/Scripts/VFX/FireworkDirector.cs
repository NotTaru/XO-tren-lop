using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Festival
{
    // Imported assets remain untouched. The wrapper disables destructive vendor scripts on instances.
    public sealed class PooledEffect
    {
        public GameObject Root;
        public GameObject Prefab;
        public bool HitEffect, Active;
        ParticleSystem[] particles;
        Transform[] transforms;
        Quaternion[] rotations;
        Vector3[] scales;
        Light[] lights;
        float[] intensities;
        float age;
        bool looping;
        public PooledEffect(GameObject prefab, Transform parent, bool hit)
        {
            Prefab = prefab; HitEffect = hit;
            Root = Object.Instantiate(prefab, parent); Root.name = "Pooled " + prefab.name;
            foreach (var script in Root.GetComponentsInChildren<MonoBehaviour>(true))
                if (script.GetType().Name == "ETFXLightFade" || script.GetType().Name == "ETFXRotation") script.enabled = false;
            foreach (var audio in Root.GetComponentsInChildren<AudioSource>(true))
            { audio.Stop(); audio.enabled = false; }
            particles = Root.GetComponentsInChildren<ParticleSystem>(true);
            foreach (var ps in particles)
            {
                var main = ps.main; main.stopAction = ParticleSystemStopAction.None;
                main.scalingMode = ParticleSystemScalingMode.Hierarchy;
                looping |= main.loop;
            }
            transforms = Root.GetComponentsInChildren<Transform>(true);
            rotations = new Quaternion[transforms.Length]; scales = new Vector3[transforms.Length];
            for (int i = 0; i < transforms.Length; i++) { rotations[i] = transforms[i].localRotation; scales[i] = transforms[i].localScale; }
            lights = Root.GetComponentsInChildren<Light>(true); intensities = new float[lights.Length];
            for (int i = 0; i < lights.Length; i++) intensities[i] = lights[i].intensity;
            Release();
        }
        public void Play(Vector3 position, float scale)
        {
            for (int i = 0; i < transforms.Length; i++) { transforms[i].localRotation = rotations[i]; transforms[i].localScale = scales[i]; }
            Root.transform.position = position; Root.transform.localScale *= scale;
            for (int i = 0; i < lights.Length; i++) lights[i].intensity = intensities[i];
            Root.SetActive(true); age = 0; Active = true;
            foreach (var ps in particles) ps.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
            foreach (var ps in particles) ps.Play(false);
        }
        public void Tick(float delta)
        {
            if (!Active) return; age += delta;
            for (int i = 0; i < lights.Length; i++) lights[i].intensity = intensities[i] * Mathf.Clamp01(1 - age / .3f);
            if (looping && age > 3) foreach (var ps in particles) ps.Stop(false, ParticleSystemStopBehavior.StopEmitting);
            bool alive = false; foreach (var ps in particles) alive |= ps.IsAlive(false);
            if (!alive || age > 12) Release();
        }
        public void Pause(bool pause)
        { if (Active) foreach (var ps in particles) { if (pause) ps.Pause(false); else if (ps.isPaused) ps.Play(false); } }
        public void Release()
        {
            foreach (var ps in particles) ps.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
            Root.SetActive(false); Active = false;
        }
    }
    public sealed class FireworkDirector : MonoBehaviour
    {
        readonly List<PooledEffect> pool = new List<PooledEffect>();
        GameplayConfig config;
        Camera stageCamera;
        bool paused, warned;
        public bool ReducedEffects;
        [Header("Effect sizes")]
        [Min(.01f)] public float perfectScale = .9f;
        [Min(.01f)] public float goodScale = .65f;
        [Min(.01f)] public float hitScale = .3f;
        [Min(.01f)] public float milestoneScale = 1.1f;
        [Min(.01f)] public float finaleScale = 1.4f;
        [Min(.01f)] public float confettiScale = 1f;
        public void Initialize(GameplayConfig settings, Camera camera)
        {
            config = settings; stageCamera = camera;
            foreach (var prefab in config.fireworkPrefabs) Prewarm(prefab, 4, false);
            Prewarm(config.purpleFirework, 3, false); Prewarm(config.confetti, 2, false);
            Prewarm(config.starBurst, 4, true); Prewarm(config.sparkle, 4, true);
        }
        void Prewarm(GameObject prefab, int count, bool hit)
        {
            if (prefab == null) return;
            for (int i = 0; i < count; i++) pool.Add(new PooledEffect(prefab, transform, hit));
        }
        void Update() { if (!paused) foreach (var fx in pool) fx.Tick(Time.unscaledDeltaTime); }
        public void Hit(int lane, bool perfect, int combo, Vector3 screenHitPoint)
        {
            float size = (combo >= 50 ? 1.6f : combo >= 25 ? 1.4f : combo >= 10 ? 1.2f : 1) * (perfect ? perfectScale : goodScale);
            Launch(config.fireworkPrefabs[lane], SkyPosition(lane), size, false);
            // UI screen position -> camera-aligned world plane, never parent particles under Canvas.
            Vector3 world = stageCamera.ScreenToWorldPoint(new Vector3(screenHitPoint.x, screenHitPoint.y, 20));
            Launch(perfect ? config.starBurst : config.sparkle, world, hitScale, true);
        }
        Vector3 SkyPosition(int lane)
        {
            // Spread bursts around the lane panel and below the title area.
            float x = lane < 2 ? .12f + .20f * lane : .68f + .20f * (lane - 2);
            return stageCamera.ViewportToWorldPoint(new Vector3(x, .67f + Random.Range(0, .09f), 20));
        }
        void Launch(GameObject prefab, Vector3 position, float scale, bool hit)
        {
            if (prefab == null)
            { if (!warned) { Debug.LogWarning("Festival: thiếu VFX, vẫn dùng phản hồi UI."); warned = true; } return; }
            int active = 0; foreach (var fx in pool) if (fx.Active && fx.HitEffect == hit) active++;
            int cap = hit ? config.maxHitEffects : ReducedEffects ? 4 : config.maxWorldEffects;
            if (active >= cap) return;
            foreach (var fx in pool) if (!fx.Active && fx.Prefab == prefab) { fx.Play(position, scale); return; }
        }
        public void Milestone(int combo) { StartCoroutine(Salute(combo)); }
        IEnumerator Salute(int combo)
        {
            int count = ReducedEffects ? 2 : combo == 10 ? 3 : combo == 25 ? 5 : 8;
            for (int i = 0; i < count; i++)
            {
                while (paused) yield return null;
                Launch(i % 3 == 2 ? config.purpleFirework : config.fireworkPrefabs[i % 4], SkyPosition(i % 4), combo >= 50 ? finaleScale : milestoneScale, false);
                float elapsed = 0;
                while (elapsed < .12f) { yield return null; if (!paused) elapsed += Time.unscaledDeltaTime; }
            }
        }
        public void Celebrate() { Milestone(50); if (!ReducedEffects) Launch(config.confetti, SkyPosition(1), confettiScale, false); }
        public void SetPaused(bool pause) { paused = pause; foreach (var fx in pool) fx.Pause(pause); }
        public void Clear()
        { StopAllCoroutines(); paused = false; foreach (var fx in pool) fx.Release(); }
        void OnDestroy() { Clear(); }
    }
}
