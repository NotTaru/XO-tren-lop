using System.Collections;
using System.Reflection;
using Festival;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public sealed class SessionTests
{
    GameFlowController flow;
    [UnitySetUp] public IEnumerator Setup()
    {
        yield return SceneManager.LoadSceneAsync("FestivalMain");
        flow=Object.FindObjectOfType<GameFlowController>();Assert.IsNotNull(flow);
    }
    [UnityTest] public IEnumerator MenuPauseResumeAndRestartAreClean()
    {
        Assert.AreEqual(GameState.Menu,flow.State);flow.StartSong(1);Assert.AreEqual(GameState.Countdown,flow.State);
        yield return null;
        flow.PauseGame();double frozen=flow.Clock.Time;int misses=flow.Score.MissCount;
        yield return new WaitForSecondsRealtime(.25f);
        Assert.AreEqual(frozen,flow.Clock.Time,1e-7);Assert.AreEqual(misses,flow.Score.MissCount);
        flow.ResumeGame();yield return null;
        Assert.AreEqual(GameState.Countdown,flow.State);Assert.AreEqual(frozen,flow.Clock.Time,1e-7);
        for(int i=0;i<4;i++)flow.Restart();
        flow.Judge.Hit(0,2);Assert.AreEqual(100,flow.Score.Score);Assert.AreEqual(1,flow.Score.PerfectCount);
        flow.MenuGame();Assert.AreEqual(GameState.Menu,flow.State);yield return null;
    }
    [UnityTest] public IEnumerator NoInputCompletesWithAllMissesAndZeroAccuracy()
    {
        flow.StartSong(0);
        typeof(SongClock).GetField("startDsp",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(flow.Clock,AudioSettings.dspTime-25);
        typeof(GameFlowController).GetMethod("SetState",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(flow,new object[]{GameState.Playing});
        yield return null;
        Assert.AreEqual(GameState.Results,flow.State);Assert.AreEqual(flow.Score.TotalNotes,flow.Score.MissCount);
        Assert.AreEqual(0,flow.Score.Accuracy);Assert.AreEqual("C",flow.Score.Rank);
    }
    [UnityTest] public IEnumerator ImportedFireworksRemainReusableAcrossSessions()
    {
        flow.StartSong(1);
        foreach(var n in flow.songs[1].chart.notes)flow.Judge.Hit(n.lane,n.hitTimeSeconds);
        Assert.AreEqual(flow.Score.TotalNotes,flow.Score.PerfectCount);Assert.AreEqual(100,flow.Score.Accuracy);
        yield return new WaitForSecondsRealtime(.4f);
        flow.Restart();flow.Judge.Hit(0,2);Assert.AreEqual(1,flow.Score.PerfectCount);
        yield return null;flow.MenuGame();
    }
}
