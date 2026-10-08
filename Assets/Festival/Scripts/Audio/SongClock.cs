using UnityEngine;

namespace Festival
{
    public sealed class SongClock
    {
        readonly AudioSource source;
        double startDsp, anchorTime, frozenTime;
        int frozenSample;
        bool frozen, resumeCountdown;
        public double Time => frozen ? frozenTime : resumeCountdown && AudioSettings.dspTime < startDsp ? anchorTime : anchorTime + AudioSettings.dspTime - startDsp;
        public bool Ready => !frozen && AudioSettings.dspTime >= startDsp;
        public double Countdown => System.Math.Max(0, startDsp - AudioSettings.dspTime);
        public SongClock(AudioSource source) { this.source = source; }
        public void Start(SongDefinition song, double delay)
        {
            source.Stop(); source.clip = song.clip;
            source.timeSamples = (int)(song.audioStartOffsetSeconds * song.clip.frequency);
            anchorTime = 0; frozen = resumeCountdown = false;
            startDsp = AudioSettings.dspTime + delay; source.PlayScheduled(startDsp);
        }
        public void Pause()
        {
            frozenTime = Time; frozenSample = source.timeSamples;
            frozen = true; source.Stop();
        }
        public void Resume(double delay)
        {
            source.timeSamples = frozenSample; anchorTime = frozenTime;
            startDsp = AudioSettings.dspTime + delay;
            frozen = false; resumeCountdown = true;
            source.PlayScheduled(startDsp + System.Math.Max(0, -frozenTime));
        }
        public void Stop() { source.Stop(); frozen = true; frozenTime = 0; }
    }
}
