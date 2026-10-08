using System;
using System.Collections.Generic;
using Festival;
using NUnit.Framework;
using UnityEngine;

public sealed class RhythmTests
{
    GameplayConfig config;
    ChartDefinition chart;
    SongDefinition song;
    [SetUp] public void Setup()
    {
        config=ScriptableObject.CreateInstance<GameplayConfig>();chart=ScriptableObject.CreateInstance<ChartDefinition>();
        chart.notes.Add(new NoteData(0,0,1));chart.notes.Add(new NoteData(1,1,2));
        song=ScriptableObject.CreateInstance<SongDefinition>();song.chart=chart;
        song.clip=AudioClip.Create("Test song",44100*5,1,44100,false);song.chartDurationSeconds=5;
    }
    [TearDown] public void Cleanup()
    { UnityEngine.Object.DestroyImmediate(song.clip);UnityEngine.Object.DestroyImmediate(song);UnityEngine.Object.DestroyImmediate(chart);UnityEngine.Object.DestroyImmediate(config); }
    [TestCase(.080,Judgment.Perfect)] [TestCase(-.080,Judgment.Perfect)]
    [TestCase(.08001,Judgment.Good)] [TestCase(.160,Judgment.Good)]
    [TestCase(-.160,Judgment.Good)] [TestCase(.16001,Judgment.BadInput)]
    public void InclusiveTimingBoundaries(double delta,Judgment expected)
    { Assert.AreEqual(expected,new NoteJudge(chart,config).Hit(0,1+delta)); }
    [Test] public void WrongLaneDoesNotConsumeOtherNote()
    {
        var judge=new NoteJudge(chart,config);Assert.AreEqual(Judgment.BadInput,judge.Hit(2,1));
        Assert.AreEqual(Judgment.Pending,judge.states[0]);Assert.AreEqual(Judgment.Perfect,judge.Hit(0,1));
    }
    [Test] public void InputBeforeTimeoutOnlyResolvesOnce()
    {
        var judge=new NoteJudge(chart,config);int results=0;judge.Judged+=(n,j)=>results++;
        judge.Hit(0,1.160);judge.Expire(1.16001);Assert.AreEqual(1,results);Assert.AreEqual(1,judge.ResolvedCount);
        judge.Hit(0,1.16);Assert.AreEqual(Judgment.Good,judge.states[0]);
    }
    [Test] public void TimeoutAtBoundaryRemainsPending()
    { var judge=new NoteJudge(chart,config);judge.Expire(1.16);Assert.AreEqual(Judgment.Pending,judge.states[0]);judge.Expire(1.16001);Assert.AreEqual(Judgment.Miss,judge.states[0]); }
    [Test] public void MilestonesAndMultipliersAreAppliedAfterIncrement()
    {
        var score=new ScoreManager(100,config);var milestones=new List<int>();score.Milestone+=milestones.Add;
        for(int i=0;i<9;i++)score.Apply(Judgment.Perfect);
        Assert.AreEqual(900,score.Score);score.Apply(Judgment.Perfect);Assert.AreEqual(1100,score.Score);
        for(int i=10;i<51;i++)score.Apply(Judgment.Perfect);
        CollectionAssert.AreEqual(new[]{10,25,50},milestones);Assert.AreEqual(2*400+25*300+15*200+9*100,score.Score);
        score.Apply(Judgment.BadInput);Assert.AreEqual(0,score.Combo);Assert.AreEqual(1,score.BadCount);
        for(int i=0;i<10;i++)score.Apply(Judgment.Good);
        CollectionAssert.AreEqual(new[]{10,25,50,10},milestones);Assert.AreEqual(51,score.MaxCombo);
    }
    [Test] public void AccuracyUsesTotalNotesAndBadInputsDoNotChangeDenominator()
    {
        var score=new ScoreManager(4,config);score.Apply(Judgment.Perfect);score.Apply(Judgment.Perfect);
        score.Apply(Judgment.Good);score.Apply(Judgment.Miss);score.Apply(Judgment.BadInput);
        Assert.AreEqual(65,score.Accuracy,1e-9);Assert.AreEqual("C",score.Rank);Assert.AreEqual(260,score.Score);Assert.AreEqual(3,score.MaxCombo);
    }
    [TestCase(95,"S")] [TestCase(85,"A")] [TestCase(70,"B")] [TestCase(69,"C")]
    public void RankThresholds(int perfect,string expected)
    { var score=new ScoreManager(100,config);for(int i=0;i<perfect;i++)score.Apply(Judgment.Perfect);Assert.AreEqual(expected,score.Rank); }
    [Test] public void ValidChartIsAccepted(){Assert.IsNull(ChartValidator.Validate(song,config));}
    [Test] public void InvalidLaneIsRejected(){chart.notes[0]=new NoteData(0,4,1);Assert.IsNotNull(ChartValidator.Validate(song,config));}
    [Test] public void DuplicateIdIsRejected(){chart.notes[1]=new NoteData(0,1,2);Assert.IsNotNull(ChartValidator.Validate(song,config));}
    [Test] public void ChordIsRejected(){chart.notes[1]=new NoteData(1,1,1);Assert.IsNotNull(ChartValidator.Validate(song,config));}
    [Test] public void OverlappingLaneIsRejected(){chart.notes[1]=new NoteData(1,0,1.32);Assert.IsNotNull(ChartValidator.Validate(song,config));}
    [Test] public void NonFiniteTimeIsRejected(){chart.notes[0]=new NoteData(0,0,double.NaN);Assert.IsNotNull(ChartValidator.Validate(song,config));}
    [Test] public void OutOfClipIsRejected(){song.chartDurationSeconds=6;Assert.IsNotNull(ChartValidator.Validate(song,config));}
    [Test] public void InsufficientFinalWindowIsRejected(){chart.notes[1]=new NoteData(1,1,4.9);Assert.IsNotNull(ChartValidator.Validate(song,config));}
    [Test] public void HoldingDuringCountdownRequiresRelease()
    {
        var router=new InputRouter();router.BlockAll();
        Assert.IsFalse(router.AcceptEdge(0,true,true));Assert.IsFalse(router.AcceptEdge(0,true,false));
        Assert.IsFalse(router.AcceptEdge(0,false,false));Assert.IsTrue(router.AcceptEdge(0,true,true));
        Assert.IsFalse(router.AcceptEdge(0,true,false));
    }
    [Test] public void ImportedVfxHaveResolvedShaderAndTextureReferences()
    {
        var asset=UnityEditor.AssetDatabase.LoadAssetAtPath<GameplayConfig>("Assets/Festival/Data/Config/Gameplay.asset");
        Assert.IsNotNull(asset);
        var prefabs=new List<GameObject>(asset.fireworkPrefabs){asset.purpleFirework,asset.confetti,asset.starBurst,asset.sparkle};
        foreach(var prefab in prefabs)
        {
            Assert.IsNotNull(prefab,"A required imported effect is missing.");
            foreach(var renderer in prefab.GetComponentsInChildren<Renderer>(true))
            {
                Assert.IsNotNull(renderer.sharedMaterial,"Primary material missing on "+prefab.name);
                foreach(var material in renderer.sharedMaterials)
                {
                    // Imported particle renderers have unused, intentionally empty trail slots.
                    if(material==null)continue;
                    Assert.AreEqual("Festival/Toon Particles",material.shader.name,material.name);
                    Assert.IsNotNull(material.mainTexture,"Texture missing on "+material.name);
                }
            }
        }
    }
}
