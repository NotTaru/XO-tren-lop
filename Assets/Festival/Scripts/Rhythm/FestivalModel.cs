using System;
using System.Collections.Generic;
using UnityEngine;

namespace Festival
{
    [Serializable] public struct NoteData
    {
        public int id, lane;
        public double hitTimeSeconds;
        public NoteData(int id, int lane, double time) { this.id = id; this.lane = lane; hitTimeSeconds = time; }
    }
    public enum Judgment { Pending, Perfect, Good, Miss, BadInput }
    public static class ChartValidator
    {
        public static string Validate(SongDefinition song, GameplayConfig config)
        {
            if (config == null || !Finite(config.perfectWindow) || !Finite(config.goodWindow) ||
                config.perfectWindow <= 0 || config.goodWindow < config.perfectWindow ||
                !Finite(config.noteTravelSeconds) || config.noteTravelSeconds <= 0)
                return "Cấu hình cửa sổ đánh hoặc thời gian chạy nốt không hợp lệ.";
            if (song == null || song.clip == null || song.chart == null || song.chart.notes.Count == 0)
                return "Thiếu nhạc hoặc chart. Màn chưa sẵn sàng.";
            if (!Finite(song.audioStartOffsetSeconds) || !Finite(song.chartDurationSeconds) ||
                song.audioStartOffsetSeconds < 0 || song.chartDurationSeconds <= 0 ||
                song.audioStartOffsetSeconds + song.chartDurationSeconds > song.clip.length + .001)
                return "Đoạn chơi vượt quá độ dài nhạc.";
            var ids = new HashSet<int>();
            double previous = -1;
            var laneTimes = new double[] { -100, -100, -100, -100 };
            foreach (var note in song.chart.notes)
            {
                if (!ids.Add(note.id)) return "ID nốt bị trùng.";
                if (note.lane < 0 || note.lane > 3) return "Làn phải nằm trong 0–3.";
                if (!Finite(note.hitTimeSeconds) || note.hitTimeSeconds < 0 || note.hitTimeSeconds <= previous)
                    return "Timestamp không hợp lệ, không tăng dần hoặc có chord.";
                if (note.hitTimeSeconds + config.goodWindow > song.chartDurationSeconds)
                    return "Nốt cuối không đủ thời gian đánh giá.";
                if (note.hitTimeSeconds - laneTimes[note.lane] <= config.goodWindow * 2 + 1e-9)
                    return "Cửa sổ nốt cùng làn chồng nhau.";
                previous = laneTimes[note.lane] = note.hitTimeSeconds;
            }
            return null;
        }
        static bool Finite(double v) { return !double.IsNaN(v) && !double.IsInfinity(v); }
    }
    public sealed class NoteJudge
    {
        readonly GameplayConfig config;
        readonly List<NoteData>[] lanes = { new List<NoteData>(), new List<NoteData>(), new List<NoteData>(), new List<NoteData>() };
        readonly int[] cursor = new int[4];
        public readonly Dictionary<int, Judgment> states = new Dictionary<int, Judgment>();
        public event Action<NoteData, Judgment> Judged;
        public int ResolvedCount { get; private set; }
        public NoteJudge(ChartDefinition chart, GameplayConfig config)
        {
            this.config = config;
            foreach (var n in chart.notes) { lanes[n.lane].Add(n); states.Add(n.id, Judgment.Pending); }
        }
        public Judgment Hit(int lane, double time)
        {
            if (cursor[lane] < lanes[lane].Count)
            {
                var n = lanes[lane][cursor[lane]];
                double delta = Math.Abs(time - n.hitTimeSeconds);
                // Tiny numerical tolerance preserves inclusive decimal boundaries in double arithmetic.
                if (delta <= config.goodWindow + 1e-9)
                {
                    var result = delta <= config.perfectWindow + 1e-9 ? Judgment.Perfect : Judgment.Good;
                    Resolve(n, result); return result;
                }
            }
            Judged?.Invoke(new NoteData(-1, lane, time), Judgment.BadInput);
            return Judgment.BadInput;
        }
        public void Expire(double time)
        {
            for (int lane = 0; lane < 4; lane++)
                while (cursor[lane] < lanes[lane].Count && time - lanes[lane][cursor[lane]].hitTimeSeconds > config.goodWindow + 1e-9)
                    Resolve(lanes[lane][cursor[lane]], Judgment.Miss);
        }
        void Resolve(NoteData note, Judgment result)
        {
            states[note.id] = result; cursor[note.lane]++; ResolvedCount++; Judged?.Invoke(note, result);
        }
    }
    public sealed class ScoreManager
    {
        readonly GameplayConfig config;
        public int Score, Combo, MaxCombo, PerfectCount, GoodCount, MissCount, BadCount;
        public readonly int TotalNotes;
        public event Action<int> Milestone;
        public int Multiplier => Combo >= 50 ? 4 : Combo >= 25 ? 3 : Combo >= 10 ? 2 : 1;
        public double Accuracy => TotalNotes == 0 ? 0 : (PerfectCount + .6 * GoodCount) / TotalNotes * 100;
        public string Rank => Accuracy >= 95 ? "S" : Accuracy >= 85 ? "A" : Accuracy >= 70 ? "B" : "C";
        public ScoreManager(int total, GameplayConfig config) { TotalNotes = total; this.config = config; }
        public void Apply(Judgment judgment)
        {
            if (judgment == Judgment.Perfect || judgment == Judgment.Good)
            {
                if (judgment == Judgment.Perfect) PerfectCount++; else GoodCount++;
                Combo++; MaxCombo = Math.Max(MaxCombo, Combo);
                Score += (judgment == Judgment.Perfect ? config.perfectScore : config.goodScore) * Multiplier;
                if (Combo == 10 || Combo == 25 || Combo == 50) Milestone?.Invoke(Combo);
            }
            else if (judgment == Judgment.Miss || judgment == Judgment.BadInput)
            { Combo = 0; if (judgment == Judgment.Miss) MissCount++; else BadCount++; }
        }
    }
}
