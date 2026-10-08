using System;
using System.IO;
using System.Collections.Generic;
using Festival;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class FestivalProjectSetup
{
    const string Root="Assets/Festival/";
    const string ScenePath=Root+"Scenes/FestivalMain.unity";
    [InitializeOnLoadMethod]
    static void Initialize()
    {
        // Bootstrap only once. Future edits to scene and data remain under the designer's control.
        EditorApplication.delayCall+=()=>
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isCompiling||File.Exists(ScenePath))return;
            CreateProject();
        };
    }
    [MenuItem("Festival/Create missing game assets")]
    public static void CreateProject()
    {
        string[] folders={"Scenes","Data/Songs","Data/Charts","Data/Config","Audio/BG","Audio/SFX","Prefabs/VFXWrappers","Materials"};
        foreach(var folder in folders)Directory.CreateDirectory(Root+folder);
        string musicPath=Root+"Audio/BG/FestivalDemo.wav";
        if(!File.Exists(musicPath))WriteMusic(musicPath,72);
        WriteToneIfMissing("Hit",.065,1100);
        WriteToneIfMissing("Click",.05,750);
        WriteToneIfMissing("Reward",.45,660);
        WriteToneIfMissing("Win",.7,880);
        WriteToneIfMissing("Fail",.4,220);
        AssetDatabase.Refresh();
        var musicImporter=(AudioImporter)AssetImporter.GetAtPath(musicPath);
        var samples=musicImporter.defaultSampleSettings;samples.loadType=AudioClipLoadType.DecompressOnLoad;
        musicImporter.defaultSampleSettings=samples;musicImporter.SaveAndReimport();
        var config=LoadOrCreate<GameplayConfig>(Root+"Data/Config/Gameplay.asset");
        string fx="Assets/Epic Toon FX/";
        string[] colors={"Blue","Green","Red","Yellow"};
        for(int i=0;i<4;i++)config.fireworkPrefabs[i]=CompatiblePrefab(fx+"Prefabs/Environment/Firework/Firework"+colors[i]+".prefab");
        config.purpleFirework=CompatiblePrefab(fx+"Prefabs/Environment/Firework/FireworkPurple.prefab");
        config.confetti=CompatiblePrefab(fx+"Prefabs/Environment/Confetti/Blast/ConfettiBlastRainbow.prefab");
        config.starBurst=CompatiblePrefab(fx+"Prefabs 2D/Explosions/StarBurst2D.prefab");
        config.sparkle=CompatiblePrefab(fx+"Prefabs/Interactive/Sparkle/TinySparkle.prefab");
        config.hitClip=Tone("Hit");config.uiClip=Tone("Click");config.rewardClip=Tone("Reward");config.winClip=Tone("Win");config.failClip=Tone("Fail");
        EditorUtility.SetDirty(config);
        var practice=CreateSong("Practice","Luyện tập / Festival Demo",24,config);
        var main=CreateSong("Main","Đêm rực rỡ / Festival Demo",72,config);
        AssetDatabase.SaveAssets();
        if(!File.Exists(ScenePath))
        {
            Scene previous=SceneManager.GetActiveScene();
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,Application.isBatchMode?NewSceneMode.Single:NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            var cameraObject=new GameObject("Main Camera",typeof(Camera),typeof(AudioListener));
            cameraObject.tag="MainCamera";
            var camera=cameraObject.GetComponent<Camera>();camera.orthographic=true;camera.orthographicSize=8;
            camera.transform.position=new Vector3(0,0,-20);
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.018f,.028f,.065f);
            camera.nearClipPlane=.1f;camera.farClipPlane=100;
            var game=new GameObject("GameFlow").AddComponent<GameFlowController>();
            game.config=config;game.songs=new[]{practice,main};game.stageCamera=camera;
            EditorSceneManager.SaveScene(scene,ScenePath);
            if(!Application.isBatchMode)
            {
                EditorSceneManager.CloseScene(scene,true);
                if(previous.IsValid())SceneManager.SetActiveScene(previous);
            }
            // The requested game is the launch scene. The old sample remains on disk.
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};
            if(!Application.isBatchMode&&previous.IsValid()&&previous.name=="SampleScene"&&!previous.isDirty)
                EditorSceneManager.OpenScene(ScenePath);
        }
        Debug.Log("Festival setup complete: open Assets/Festival/Scenes/FestivalMain.unity and press Play. Demo soundtrack: 120 BPM; authored charts, not the unprovided MP3 tracks.");
    }
    static T LoadOrCreate<T>(string path) where T:ScriptableObject
    {
        var asset=AssetDatabase.LoadAssetAtPath<T>(path);
        if(asset==null){asset=ScriptableObject.CreateInstance<T>();AssetDatabase.CreateAsset(asset,path);}return asset;
    }
    static GameObject CompatiblePrefab(string sourcePath)
    {
        var source=AssetDatabase.LoadAssetAtPath<GameObject>(sourcePath);if(source==null)return null;
        string destination=Root+"Prefabs/VFXWrappers/"+source.name+".prefab";
        var existing=AssetDatabase.LoadAssetAtPath<GameObject>(destination);if(existing!=null)return existing;
        var instance=PrefabUtility.LoadPrefabContents(sourcePath);
        try
        {
            foreach(var renderer in instance.GetComponentsInChildren<Renderer>(true))
            {
                var materials=renderer.sharedMaterials;
                for(int i=0;i<materials.Length;i++)if(materials[i]!=null)materials[i]=CompatibleMaterial(materials[i]);
                renderer.sharedMaterials=materials;
            }
            return PrefabUtility.SaveAsPrefabAsset(instance,destination);
        }
        finally{PrefabUtility.UnloadPrefabContents(instance);}
    }
    static Material CompatibleMaterial(Material source)
    {
        string sourceGuid=AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(source));
        string path=Root+"Materials/"+source.name+"_"+sourceGuid.Substring(0,8)+".mat";
        var existing=AssetDatabase.LoadAssetAtPath<Material>(path);if(existing!=null)return existing;
        var shader=Shader.Find("Festival/Toon Particles");if(shader==null)throw new Exception("Missing Festival particle shader.");
        var material=new Material(shader){name=source.name+" (Festival Built-in)"};
        // Read serialized values even when the original material's shader GUID cannot be resolved.
        var serialized=new SerializedObject(source);
        var textures=serialized.FindProperty("m_SavedProperties.m_TexEnvs");
        for(int i=0;i<textures.arraySize;i++)
        {
            var property=textures.GetArrayElementAtIndex(i);
            if(property.FindPropertyRelative("first").stringValue!="_MainTex")continue;
            var value=property.FindPropertyRelative("second");
            material.SetTexture("_MainTex",value.FindPropertyRelative("m_Texture").objectReferenceValue as Texture);
            material.SetTextureScale("_MainTex",value.FindPropertyRelative("m_Scale").vector2Value);
            material.SetTextureOffset("_MainTex",value.FindPropertyRelative("m_Offset").vector2Value);
        }
        var colors=serialized.FindProperty("m_SavedProperties.m_Colors");
        for(int i=0;i<colors.arraySize;i++)
        {
            var property=colors.GetArrayElementAtIndex(i);
            if(property.FindPropertyRelative("first").stringValue=="_TintColor")
                material.SetColor("_TintColor",property.FindPropertyRelative("second").colorValue);
        }
        material.SetFloat("_SrcBlend",(float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetFloat("_DstBlend",(float)(source.name.Contains("_ADD")?UnityEngine.Rendering.BlendMode.One:UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha));
        AssetDatabase.CreateAsset(material,path);return material;
    }
    static SongDefinition CreateSong(string id,string title,int duration,GameplayConfig config)
    {
        string chartPath=Root+"Data/Charts/"+id+".asset";
        bool missing=AssetDatabase.LoadAssetAtPath<ChartDefinition>(chartPath)==null;
        var chart=LoadOrCreate<ChartDefinition>(chartPath);
        if(missing)
        {
            chart.chartId=id.ToLowerInvariant()+"-demo-v1";chart.difficulty=id=="Practice"?"Luyện tập":"Tiêu chuẩn";
            chart.notes=AuthoredChart(id=="Practice");EditorUtility.SetDirty(chart);
        }
        var song=LoadOrCreate<SongDefinition>(Root+"Data/Songs/"+id+".asset");
        song.songId="festival-demo";song.displayName=title; song.chart=chart;
        song.clip=AssetDatabase.LoadAssetAtPath<AudioClip>(Root+"Audio/BG/FestivalDemo.wav");
        song.chartDurationSeconds=duration;song.audioStartOffsetSeconds=0;EditorUtility.SetDirty(song);
        string error=ChartValidator.Validate(song,config);if(error!=null)throw new InvalidOperationException(error);
        return song;
    }
    static List<NoteData> AuthoredChart(bool practice)
    {
        var notes=new List<NoteData>();
        // Explicit lane phrases authored for the original 120 BPM demo. No runtime auto-charting.
        int[] intro={0,1,2,3,0,1,2,3,0,2,1,3,3,2,1,0};
        if(practice)
        {
            int[] lanes={0,0,1,1,2,2,3,3,0,1,2,3,0,2,1,3,3,2,1,0};
            for(int i=0;i<lanes.Length;i++)notes.Add(new NoteData(i,lanes[i],2+i));
            return notes;
        }
        for(int i=0;i<intro.Length;i++)notes.Add(new NoteData(notes.Count,intro[i],2+i));
        int[] middle={0,1,2,3,2,1,0,3,0,2,1,3,1,2,0,3,3,1,2,0,1,0,3,2,0,1,3,2,1,3,0,2};
        for(int i=0;i<64;i++)notes.Add(new NoteData(notes.Count,middle[i%middle.Length],18+i*.5));
        int[] finale={0,1,2,3,0,2,1,3,2,0,3,1,0,3,2,1};
        for(int i=0;i<48;i++)notes.Add(new NoteData(notes.Count,finale[i%16],50+i*.375));
        int[] ending={0,2,1,3,0,1,2,3};
        for(int i=0;i<ending.Length;i++)notes.Add(new NoteData(notes.Count,ending[i],68+i*.375));
        return notes;
    }
    static AudioClip Tone(string name)=>AssetDatabase.LoadAssetAtPath<AudioClip>(Root+"Audio/SFX/"+name+".wav");
    static void WriteToneIfMissing(string name,double duration,double frequency)
    {
        string path=Root+"Audio/SFX/"+name+".wav";if(File.Exists(path))return;
        int rate=44100;var data=new float[(int)(duration*rate)];
        for(int i=0;i<data.Length;i++)
        {
            double t=i/(double)rate;
            data[i]=(float)(Math.Sin(2*Math.PI*frequency*t)*Math.Exp(-t*9)*Math.Min(1,t/.004)*.25);
        }
        WriteWav(path,data,rate);
    }
    static void WriteMusic(string path,int duration)
    {
        const int rate=44100;var data=new float[rate*duration];
        double[] melody={523.251,659.255,783.991,659.255,587.330,698.456,880,698.456,440,523.251,659.255,523.251,391.995,493.883,587.330,493.883};
        for(int i=0;i<data.Length;i++)
        {
            double t=i/(double)rate;int beat=(int)(t/.5);double phase=t-beat*.5;
            double barPhase=t-Math.Floor(t/2)*2;
            double melodyFrequency=melody[(beat/2)%melody.Length];
            double envelope=Math.Exp(-phase*7)*Math.Min(1,phase/.008);
            double synth=(Math.Sin(t*2*Math.PI*melodyFrequency)+.2*Math.Sin(t*4*Math.PI*melodyFrequency))*.10*envelope;
            double kick=Math.Sin(2*Math.PI*(52*phase+30*(1-Math.Exp(-phase*25))/25))*Math.Exp(-phase*22)*.22;
            double hat=(Math.Sin(t*18437)+Math.Sin(t*23891))*.018*Math.Exp(-phase*65);
            double bass=Math.Sin(2*Math.PI*t*melody[(beat/4*4)%16]/4)*.07*Math.Exp(-barPhase*1.6);
            // The finale's authored eighth-triplet pulse is also audible in the percussion.
            double triplet=t-50-Math.Floor((t-50)/.375)*.375;
            double finale=t>=50?Math.Sin(t*27113)*.018*Math.Exp(-triplet*65):0;
            double fade=Math.Min(1,t/.5)*Math.Min(1,(duration-t)/1.25);
            data[i]=(float)((synth+kick+hat+bass+finale)*fade);
        }
        WriteWav(path,data,rate);
    }
    static void WriteWav(string path,float[] data,int rate)
    {
        using(var writer=new BinaryWriter(File.Create(path)))
        {
            writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));writer.Write(36+data.Length*2);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));writer.Write(16);writer.Write((short)1);writer.Write((short)1);
            writer.Write(rate);writer.Write(rate*2);writer.Write((short)2);writer.Write((short)16);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));writer.Write(data.Length*2);
            foreach(float value in data)writer.Write((short)(Mathf.Clamp(value,-1,1)*32767));
        }
    }
    [MenuItem("Festival/Open game scene")]
    public static void OpenGame(){CreateProject();if(EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())EditorSceneManager.OpenScene(ScenePath);}
    public static void BuildWindows()
    {
        CreateProject();Directory.CreateDirectory("Build/Festival");
        var report=BuildPipeline.BuildPlayer(new[]{ScenePath},"Build/Festival/LeHoiPhaoHoa.exe",BuildTarget.StandaloneWindows64,BuildOptions.None);
        if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Festival build failed: "+report.summary.result);
    }
}
