using System.Collections.Generic;
using UnityEngine;
namespace Festival
{
    [CreateAssetMenu(menuName = "Festival/Chart")]
    public sealed class ChartDefinition : ScriptableObject
    {
        public string chartId, difficulty;
        public List<NoteData> notes = new List<NoteData>();
    }
}
