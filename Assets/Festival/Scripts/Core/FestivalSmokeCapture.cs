using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace Festival
{
    // Opt-in standalone verification only; ordinary launches never create this component.
    public sealed class FestivalSmokeCapture : MonoBehaviour
    {
        GameFlowController session;
        string bestKey;
        int previousBest;
        bool hadBest, restore;
        float previousOffset;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Initialize()
        {
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"--festival-smoke")<0 &&
                Array.IndexOf(Environment.GetCommandLineArgs(),"--festival-smoke-preview")<0)return;
            new GameObject("Festival automated verification").AddComponent<FestivalSmokeCapture>();
        }
        IEnumerator Start()
        {
            Application.runInBackground=true;
            var flow=FindObjectOfType<GameFlowController>();
            if(flow==null){Debug.LogError("FESTIVAL_SMOKE: missing game scene");Application.Quit(2);yield break;}
            string folder=Path.GetFullPath(Path.Combine(Application.dataPath,"../QA"));Directory.CreateDirectory(folder);
            bool preview=Array.IndexOf(Environment.GetCommandLineArgs(),"--festival-smoke-preview")>=0;
            session=flow;previousOffset=flow.InputOffset;
            bestKey="festival.best.v1."+flow.songs[1].songId+"."+flow.songs[1].chart.chartId;
            hadBest=PlayerPrefs.HasKey(bestKey);previousBest=PlayerPrefs.GetInt(bestKey,0);restore=true;
            Screen.SetResolution(1600,900,false);
            yield return new WaitForSecondsRealtime(1);
            Capture(flow,Path.Combine(folder,"01-menu.png"));
            yield return new WaitForSecondsRealtime(.3f);
            flow.SetOffset(0);flow.StartSong(1);
            int next=0;bool captured=false,pausedOnce=false;double deadline=Time.realtimeSinceStartupAsDouble+100;
            while(flow.State!=GameState.Results&&Time.realtimeSinceStartupAsDouble<deadline)
            {
                if(flow.State==GameState.Paused)flow.ResumeGame();
                if(flow.State==GameState.Playing)
                {
                    double time=flow.Clock.Time;
                    var chart=flow.songs[1].chart;
                    while(next<chart.notes.Count&&chart.notes[next].hitTimeSeconds<=time)
                    { flow.Judge.Hit(chart.notes[next].lane,time);next++; }
                    if(!captured&&time>18.2)
                    { Capture(flow,Path.Combine(folder,"02-gameplay.png"));captured=true; }
                    if(!pausedOnce&&time>20.2)
                    {
                        flow.PauseGame();double frozen=flow.Clock.Time;int misses=flow.Score.MissCount;
                        Capture(flow,Path.Combine(folder,"03-pause.png"));
                        yield return new WaitForSecondsRealtime(.5f);
                        if(Math.Abs(flow.Clock.Time-frozen)>1e-7||flow.Score.MissCount!=misses)
                            Debug.LogError("FESTIVAL_SMOKE: pause advanced timeline or recorded misses");
                        if(preview)
                        {
                            RestorePreferences();Debug.Log("FESTIVAL_SMOKE: visual preview complete");Application.Quit(0);yield break;
                        }
                        flow.ResumeGame();pausedOnce=true;
                    }
                }
                yield return null;
            }
            if(flow.State!=GameState.Results){Debug.LogError("FESTIVAL_SMOKE: timed out");Application.Quit(3);yield break;}
            Capture(flow,Path.Combine(folder,"04-results.png"));
            string report="Notes="+flow.Score.TotalNotes+" Perfect="+flow.Score.PerfectCount+" Good="+flow.Score.GoodCount+" Miss="+flow.Score.MissCount+" Bad="+flow.Score.BadCount+" Score="+flow.Score.Score+" Accuracy="+flow.Score.Accuracy+" MaxCombo="+flow.Score.MaxCombo+" Rank="+flow.Score.Rank;
            File.WriteAllText(Path.Combine(folder,"session.txt"),report);Debug.Log("FESTIVAL_SMOKE: "+report);
            yield return new WaitForSecondsRealtime(1);
            flow.Restart();yield return null;
            flow.MenuGame();yield return null;
            RestorePreferences();Debug.Log("FESTIVAL_SMOKE: complete; replay/menu clean");Application.Quit(0);
        }
        void RestorePreferences()
        {
            if(!restore)return;restore=false;
            if(hadBest)PlayerPrefs.SetInt(bestKey,previousBest);else PlayerPrefs.DeleteKey(bestKey);
            if(session!=null)session.SetOffset(previousOffset);PlayerPrefs.Save();
        }
        static void Capture(GameFlowController flow,string path)
        {
            // Hidden windows may have no presented backbuffer. Render the real UI/stage to a GPU target instead.
            var canvas=flow.View.Root.GetComponentInParent<Canvas>();var camera=flow.stageCamera;
            var previousMode=canvas.renderMode;var previousCamera=canvas.worldCamera;float distance=canvas.planeDistance;
            var previousTarget=camera.targetTexture;var previousActive=RenderTexture.active;
            var target=RenderTexture.GetTemporary(1600,900,24,RenderTextureFormat.ARGB32);
            canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;
            camera.targetTexture=target;Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=target;
            var texture=new Texture2D(1600,900,TextureFormat.RGB24,false);
            texture.ReadPixels(new Rect(0,0,1600,900),0,0);texture.Apply();File.WriteAllBytes(path,texture.EncodeToPNG());
            camera.targetTexture=previousTarget;canvas.renderMode=previousMode;canvas.worldCamera=previousCamera;canvas.planeDistance=distance;
            RenderTexture.active=previousActive;RenderTexture.ReleaseTemporary(target);Destroy(texture);Canvas.ForceUpdateCanvases();
        }
        void OnApplicationQuit(){RestorePreferences();}
    }
}
