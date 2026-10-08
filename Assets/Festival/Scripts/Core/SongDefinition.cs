using UnityEngine;
namespace Festival
{
    [CreateAssetMenu(menuName = "Festival/Song")]
    public sealed class SongDefinition : ScriptableObject
    {
        public string songId, displayName;
        public AudioClip clip;
        public ChartDefinition chart;
        public double audioStartOffsetSeconds, chartDurationSeconds, previewStartSeconds;
    }
}
