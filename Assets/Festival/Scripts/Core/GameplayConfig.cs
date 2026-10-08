using UnityEngine;
namespace Festival
{
    [CreateAssetMenu(menuName = "Festival/Gameplay Config")]
    public sealed class GameplayConfig : ScriptableObject
    {
        public double perfectWindow = .080, goodWindow = .160, noteTravelSeconds = 2;
        public int perfectScore = 100, goodScore = 60;
        public float countdownSeconds = 3;
        public GameObject[] fireworkPrefabs = new GameObject[4];
        public GameObject purpleFirework, starBurst, sparkle, confetti;
        public int maxWorldEffects = 12, maxHitEffects = 4, maxAudioVoices = 8;
        public AudioClip hitClip, rewardClip, winClip, failClip, uiClip;
    }
}
