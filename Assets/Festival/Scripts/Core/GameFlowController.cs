using System;
using UnityEngine;
namespace Festival
{
    public enum GameState { Menu, Countdown, Playing, Paused, Results }
    public sealed class GameFlowController : MonoBehaviour
    {
        public SongDefinition[] songs;
        public GameplayConfig config;
        public Camera stageCamera;
        public GameState State { get; private set; }
        public float InputOffset { get; private set; }
        public bool ReducedEffects { get; private set; }
        public ScoreManager Score { get; private set; }
        public NoteJudge Judge { get; private set; }
        public SongClock Clock { get; private set; }
        public FestivalView View { get; private set; }
        AudioManager audio;
        FireworkDirector fireworks;
        NoteViewPool notes;
        int selected;
        float judgmentUntil;
        readonly InputRouter input=new InputRouter();
        Action<int,double> judgeInput;
        void Awake()
        {
            Application.targetFrameRate=120;
            InputOffset=PlayerPrefs.GetFloat("festival.offset",0);
            ReducedEffects=PlayerPrefs.GetInt("festival.reduced",0)!=0;
            audio=gameObject.AddComponent<AudioManager>();audio.Initialize(config);
            Clock=new SongClock(audio.Music);
            fireworks=new GameObject("VFX Pool").AddComponent<FireworkDirector>();fireworks.transform.SetParent(transform);
            fireworks.Initialize(config,stageCamera);fireworks.ReducedEffects=ReducedEffects;
            View=new FestivalView(this,audio);notes=new NoteViewPool(View);
            SetState(GameState.Menu);
        }
        void SetState(GameState state){State=state;View.SetState(state);}
        public void SetOffset(float offset){InputOffset=Mathf.Clamp(offset,-.2f,.2f);PlayerPrefs.SetFloat("festival.offset",InputOffset);PlayerPrefs.Save();}
        public void ToggleEffects(){ReducedEffects=!ReducedEffects;fireworks.ReducedEffects=ReducedEffects;PlayerPrefs.SetInt("festival.reduced",ReducedEffects?1:0);PlayerPrefs.Save();}
        public void StartSong(int index)
        {
            if(songs==null||index<0||index>=songs.Length){View.Error.text="Màn chưa sẵn sàng.";return;}
            string error=ChartValidator.Validate(songs[index],config);
            if(error!=null){View.Error.text=error;Debug.LogError("Festival: "+error);return;}
            Cleanup();selected=index;View.Error.text="";
            Judge=new NoteJudge(songs[index].chart,config);Score=new ScoreManager(songs[index].chart.notes.Count,config);
            judgeInput=(lane,time)=>Judge.Hit(lane,time);
            Judge.Judged+=OnJudgment;Score.Milestone+=OnMilestone;
            notes.Begin(songs[index].chart,Judge,config.noteTravelSeconds);
            View.SongTitle.text=songs[index].displayName+" / "+songs[index].chart.difficulty;
            View.JudgmentText.text="SẴN SÀNG";RefreshScore();
            Clock.Start(songs[index],Math.Max(config.countdownSeconds,config.noteTravelSeconds));
            BlockHeldKeys();SetState(GameState.Countdown);
        }
        void Update()
        {
            if(State==GameState.Menu||State==GameState.Results)return;
            if(Input.GetKeyDown(KeyCode.Escape))
            { if(State==GameState.Playing||State==GameState.Countdown)PauseGame();else if(State==GameState.Paused)ResumeGame();return; }
            if(State==GameState.Paused)return;
            double time=Clock.Time;
            if(State==GameState.Countdown)
            {
                View.Countdown.text=Math.Ceiling(Clock.Countdown).ToString();
                input.ObserveCountdown();
                if(Clock.Ready){SetState(GameState.Playing);View.Countdown.text="";fireworks.SetPaused(false);audio.PauseEffects(false);}
            }
            else
            {
                input.Poll(time+InputOffset,judgeInput);
                // Expiration uses the same calibrated clock as input, preserving the full Good window.
                Judge.Expire(time+InputOffset);
                if(time>=songs[selected].chartDurationSeconds && Judge.ResolvedCount==Score.TotalNotes)
                {
                    Finish();return;
                }
                // Detect an audio interruption after the scheduled start, rather than silently running a chart.
                if(time>.25&&!audio.Music.isPlaying&&time<songs[selected].chartDurationSeconds-.05)
                { PauseGame();View.JudgmentText.text="NHẠC BỊ GIÁN ĐOẠN";return; }
            }
            notes.Tick(time);
            float progress=Mathf.Clamp01((float)(time/songs[selected].chartDurationSeconds));
            View.Progress.rectTransform.sizeDelta=new Vector2(progress*1456,4);
            View.ProgressText.text=Math.Max(0,time).ToString("0.0")+" / "+songs[selected].chartDurationSeconds.ToString("0")+" GIÂY";
            for(int lane=0;lane<4;lane++)
            { Color c=View.Flashes[lane].color;c.a=Mathf.MoveTowards(c.a,0,Time.unscaledDeltaTime*1.6f);View.Flashes[lane].color=c; }
            if(Time.unscaledTime>judgmentUntil)View.JudgmentText.text=Score.Combo>=10?"GIỮ NHỊP!":"CHẠM ĐÚNG NHỊP";
        }
        void OnJudgment(NoteData note,Judgment judgment)
        {
            Score.Apply(judgment);RefreshScore();judgmentUntil=Time.unscaledTime+.65f;
            View.JudgmentText.text=judgment==Judgment.BadInput?"BẤM THỪA":judgment.ToString().ToUpperInvariant();
            View.JudgmentText.color=judgment==Judgment.Perfect?new Color(1,.82f,.4f):judgment==Judgment.Good?new Color(.45f,.95f,.8f):new Color(.7f,.73f,.8f);
            Color flash=FestivalView.LaneColors[note.lane];flash.a=.25f;View.Flashes[note.lane].color=flash;
            if(judgment==Judgment.Perfect||judgment==Judgment.Good)
            {
                audio.Hit();Vector3 screenPoint=RectTransformUtility.WorldToScreenPoint(null,View.HitPoints[note.lane].position);
                fireworks.Hit(note.lane,judgment==Judgment.Perfect,Score.Combo,screenPoint);
            }
        }
        void OnMilestone(int combo){fireworks.Milestone(combo);audio.Reward();}
        void RefreshScore()
        { View.Score.text=Score.Score.ToString("D6");View.Combo.text=Score.Combo+"  /  ×"+Score.Multiplier;View.Accuracy.text="Độ chính xác  "+Score.Accuracy.ToString("0.0")+"%"; }
        public void PauseGame()
        {
            if(State!=GameState.Playing&&State!=GameState.Countdown)return;
            Clock.Pause();fireworks.SetPaused(true);audio.PauseEffects(true);BlockHeldKeys();SetState(GameState.Paused);
        }
        public void ResumeGame()
        {
            if(State!=GameState.Paused)return;
            Clock.Resume(3);BlockHeldKeys();SetState(GameState.Countdown);
        }
        void BlockHeldKeys(){input.BlockAll();}
        public void Restart(){StartSong(selected);}
        public void MenuGame(){Cleanup();SetState(GameState.Menu);}
        void Finish()
        {
            Clock.Stop();notes.Clear();fireworks.Clear();audio.StopAll();
            string key="festival.best.v1."+songs[selected].songId+"."+songs[selected].chart.chartId;
            int previous=PlayerPrefs.GetInt(key,0),best=Math.Max(previous,Score.Score);
            PlayerPrefs.SetInt(key,best);PlayerPrefs.Save();
            bool passed=Score.Accuracy>=70;audio.Results(passed);if(passed)fireworks.Celebrate();
            View.ResultTitle.text="HẠNG "+Score.Rank+"  ·  "+(passed?"HOÀN THÀNH!":"THỬ LẠI NHÉ");
            View.ResultStats.text="Điểm  "+Score.Score.ToString("N0")+"     ·     Chính xác  "+Score.Accuracy.ToString("0.0")+"%\nCombo cao nhất  "+Score.MaxCombo+"\n\nPerfect  "+Score.PerfectCount+"     Good  "+Score.GoodCount+"\nMiss  "+Score.MissCount+"     Bấm thừa  "+Score.BadCount;
            View.Best.text=(Score.Score>previous?"KỶ LỤC MỚI  /  ":"ĐIỂM CAO NHẤT  /  ")+best.ToString("N0");
            SetState(GameState.Results);
        }
        void Cleanup()
        {
            if(Judge!=null)Judge.Judged-=OnJudgment;if(Score!=null)Score.Milestone-=OnMilestone;
            Clock.Stop();notes.Clear();fireworks.Clear();audio.StopAll();
            for(int i=0;i<4;i++){var color=View.Flashes[i].color;color.a=0;View.Flashes[i].color=color;}
        }
        void OnApplicationFocus(bool focus){if(!focus&&(State==GameState.Playing||State==GameState.Countdown))PauseGame();}
        void OnDestroy(){if(Clock!=null)Cleanup();}
    }
}
