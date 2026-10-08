using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Festival
{
    public sealed class FestivalView
    {
        public static readonly Color[] LaneColors = { new Color(.27f,.73f,1), new Color(.42f,.94f,.69f), new Color(1,.39f,.55f), new Color(1,.81f,.35f) };
        public static readonly string[] Keys = { "D", "F", "J", "K" };
        public readonly RectTransform Root;
        public GameObject Menu, Play, Pause, Results, Settings;
        public Text Score, Combo, Accuracy, SongTitle, JudgmentText, Countdown, ResultTitle, ResultStats, Best, Error, OffsetLabel, ProgressText;
        public Image Progress;
        public RectTransform[] HitPoints = new RectTransform[4];
        public Image[] Flashes = new Image[4];
        public RectTransform NoteContainer;
        Font font;
        readonly Color ink = new Color(.92f,.94f,1), muted = new Color(.55f,.62f,.76f), gold = new Color(1,.8f,.41f);
        AudioManager audio;
        Text effectsLabel;
        public FestivalView(GameFlowController flow, AudioManager audio)
        {
            this.audio = audio;
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var canvasObject = new GameObject("Festival Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(flow.transform);
            canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1600, 900); scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            Root = Rect("Layout 1600 × 900", canvasObject.transform, 0,0,1600,900);
            Root.anchorMin = Root.anchorMax = new Vector2(.5f,.5f); Root.pivot = new Vector2(.5f,.5f); Root.anchoredPosition = Vector2.zero;
            // Static stars and waterfront silhouette frame the 3D performance without covering it.
            var random = new System.Random(74);
            for (int i=0;i<65;i++) Box(Root, "Star", random.Next(40,1560),random.Next(35,390), i%4==0?3:1, i%4==0?3:1,new Color(.7f,.8f,1,.45f));
            for (int i=0;i<34;i++)
            {
                float height = random.Next(18,100);
                Box(Root,"Skyline",i*49,860-height,45,height,new Color(.025f,.04f,.075f));
                Box(Root,"Window",i*49+13,865-height,3,5,new Color(1,.76f,.35f,.35f));
            }
            Box(Root,"Waterfront",0,860,1600,40,new Color(.02f,.035f,.065f));
            Label(Root,"FESTIVAL / RHYTHM EXPERIENCE",70,38,640,26,16,muted);
            Label(Root,"LỄ HỘI PHÁO HOA",68,70,1000,64,46,ink,true);
            Label(Root,"Một nhịp chạm. Một bầu trời rực rỡ.",72,137,900,30,20,muted);
            var es = new GameObject("Festival EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            es.transform.SetParent(flow.transform);
            BuildMenu(flow); BuildPlay(flow); BuildPause(flow); BuildResults(flow); BuildSettings(flow);
        }
        void BuildMenu(GameFlowController flow)
        {
            Menu = Panel("Menu");
            Label(Menu.transform,"CHỌN MÀN TRÌNH DIỄN",72,265,900,30,17,gold,true);
            Card(flow,0,72,"01 / LÀM QUEN","Luyện tập","24 giây • 4 làn • Nhịp thưa", "Thử từng phím, làm quen vạch đánh.");
            Card(flow,1,820,"02 / ĐÊM HỘI","Đêm rực rỡ","72 giây • Nhịp tăng dần • Combo 50", "Giữ nhịp để mở màn pháo hoa nhiều lớp.");
            Label(Menu.transform,"CÁCH CHƠI",72,664,200,24,16,gold,true);
            Label(Menu.transform,"Bấm D  F  J  K khi nốt chạm vạch sáng. Perfect ≤80ms · Good ≤160ms.\nGiữ phím không tự đánh nốt. Esc để tạm dừng. Đạt 70% để hoàn thành màn.",72,700,1440,70,21,ink);
            Button(Menu.transform,"ÂM THANH & HIỆU CHỈNH",72,804,345,48,()=>Settings.SetActive(true),false);
            Error = Label(Menu.transform,"",450,806,1050,50,18,new Color(1,.5f,.5f));
        }
        void Card(GameFlowController flow,int index,float x,string indexText,string name,string detail,string description)
        {
            var card = Box(Menu.transform,"Song card",x,318,708,300,new Color(.055f,.085f,.15f,.95f));
            Box(card.transform,"Accent",0,0,708,3,index==0?LaneColors[0]:gold);
            Label(card.transform,indexText,28,26,650,24,15,muted);
            Label(card.transform,name,26,66,650,58,37,ink,true);
            Label(card.transform,detail,28,134,650,30,18,gold);
            Label(card.transform,description,28,178,650,35,18,muted);
            Button(card.transform,"BẮT ĐẦU  →",28,234,250,46,()=>flow.StartSong(index),true);
        }
        void BuildPlay(GameFlowController flow)
        {
            Play = Panel("Gameplay");
            SongTitle=Label(Play.transform,"",72,224,1000,35,22,gold,true);
            Button(Play.transform,"TẠM DỪNG / ESC",1255,50,270,46,flow.PauseGame,false);
            Box(Play.transform,"Progress track",72,280,1456,4,new Color(.15f,.2f,.3f));
            Progress=Box(Play.transform,"Progress",72,280,0,4,gold);
            ProgressText=Label(Play.transform,"",1240,231,285,28,17,muted,false,TextAnchor.MiddleRight);
            Label(Play.transform,"ĐIỂM SỐ",80,395,330,25,15,muted);
            Score=Label(Play.transform,"000000",76,434,370,72,48,ink,true);
            Label(Play.transform,"COMBO / HỆ SỐ",80,538,350,25,15,muted);
            Combo=Label(Play.transform,"0  /  ×1",76,575,375,65,40,gold,true);
            Accuracy=Label(Play.transform,"Độ chính xác  0.0%",80,670,380,32,20,ink);
            Label(Play.transform,"10  →  ×2\n25  →  ×3\n50  →  ×4 + Grand Finale",80,724,370,92,19,muted);
            var lanes = Box(Play.transform,"Lanes",505,326,590,522,new Color(.02f,.035f,.065f,.5f));
            for(int lane=0;lane<4;lane++)
            {
                float x=12+lane*144;
                Box(lanes.transform,"Lane "+Keys[lane],x,0,134,476,new Color(.06f,.085f,.13f,.65f));
                Box(lanes.transform,"Lane divider",x+134,0,1,476,new Color(.13f,.17f,.25f));
                Flashes[lane]=Box(lanes.transform,"Key flash",x,370,134,106,new Color(LaneColors[lane].r,LaneColors[lane].g,LaneColors[lane].b,0));
                var line=Box(lanes.transform,"Hit line "+Keys[lane],x,427,134,4,LaneColors[lane]);
                HitPoints[lane]=line.rectTransform;
                Label(lanes.transform,Keys[lane],x,464,134,53,28,LaneColors[lane],true,TextAnchor.MiddleCenter);
            }
            NoteContainer = Rect("Pooled note views",lanes.transform,0,0,590,522);
            var maskObject = new GameObject("Note clip",typeof(RectTransform),typeof(RectMask2D));
            maskObject.transform.SetParent(lanes.transform,false);
            var maskRect=(RectTransform)maskObject.transform;
            maskRect.anchorMin=maskRect.anchorMax=new Vector2(0,1);maskRect.pivot=new Vector2(0,1);
            maskRect.sizeDelta=new Vector2(590,460);maskRect.anchoredPosition=Vector2.zero;
            NoteContainer.SetParent(maskRect,false);
            Label(Play.transform,"CHẠM ĐÚNG NHỊP",1160,405,365,28,16,muted);
            JudgmentText=Label(Play.transform,"SẴN SÀNG",1155,450,370,65,31,gold,true);
            Label(Play.transform,"D  /  XANH DƯƠNG\nF  /  XANH LÁ\nJ  /  ĐỎ\nK  /  VÀNG",1160,560,370,146,21,ink);
            Label(Play.transform,"Nốt đến vạch sáng → bấm phím\nAlt-tab sẽ tự động tạm dừng",1160,745,370,64,17,muted);
            Countdown=Label(Play.transform,"",505,470,590,140,78,ink,true,TextAnchor.MiddleCenter);
        }
        void BuildPause(GameFlowController flow)
        {
            Pause=Modal("Pause","TẠM DỪNG","Nhạc và nốt đã được giữ nguyên.");
            Button(Pause.transform,"TIẾP TỤC",535,433,530,56,flow.ResumeGame,true);
            Button(Pause.transform,"CHƠI LẠI",535,509,530,56,flow.Restart,false);
            Button(Pause.transform,"VỀ MENU",535,585,530,56,flow.MenuGame,false);
        }
        void BuildResults(GameFlowController flow)
        {
            Results=Modal("Results","MÀN TRÌNH DIỄN KẾT THÚC","");
            ResultTitle=Label(Results.transform,"",420,335,760,65,44,gold,true,TextAnchor.MiddleCenter);
            ResultStats=Label(Results.transform,"",470,420,660,164,23,ink,false,TextAnchor.MiddleCenter);
            Best=Label(Results.transform,"",470,593,660,35,18,muted,false,TextAnchor.MiddleCenter);
            Button(Results.transform,"CHƠI LẠI",460,660,325,56,flow.Restart,true);
            Button(Results.transform,"VỀ MENU",815,660,325,56,flow.MenuGame,false);
        }
        void BuildSettings(GameFlowController flow)
        {
            Settings=Modal("Settings","ÂM THANH & HIỆU CHỈNH","");
            SliderRow(Settings.transform,"Nhạc",365,0,1,audio.MusicVolume,v=>audio.SetVolumes(v,audio.SfxVolume,audio.UiVolume));
            SliderRow(Settings.transform,"Hiệu ứng game",425,0,1,audio.SfxVolume,v=>audio.SetVolumes(audio.MusicVolume,v,audio.UiVolume));
            SliderRow(Settings.transform,"Thao tác UI",485,0,1,audio.UiVolume,v=>audio.SetVolumes(audio.MusicVolume,audio.SfxVolume,v));
            OffsetLabel=Label(Settings.transform,"",460,554,680,28,18,gold);
            SliderRow(Settings.transform,"",580,-200,200,flow.InputOffset*1000,v=>{flow.SetOffset(v/1000);UpdateOffset(flow.InputOffset);});
            UpdateOffset(flow.InputOffset);
            Label(Settings.transform,"Offset dương: lần bấm được đánh giá muộn hơn.\nBấm sớm đều đặn: thử tăng offset. Bấm muộn: thử giảm.",460,638,720,65,17,muted);
            var effectsButton=Button(Settings.transform,flow.ReducedEffects?"HIỆU ỨNG: GIẢM":"HIỆU ỨNG: ĐẦY ĐỦ",460,724,325,50,()=>
            { flow.ToggleEffects();effectsLabel.text=flow.ReducedEffects?"HIỆU ỨNG: GIẢM":"HIỆU ỨNG: ĐẦY ĐỦ"; },false);
            effectsLabel=effectsButton.GetComponentInChildren<Text>();
            Button(Settings.transform,"LƯU & ĐÓNG",815,724,325,50,()=>Settings.SetActive(false),true);
        }
        void UpdateOffset(float offset) { OffsetLabel.text="Độ lệch input: "+(offset*1000).ToString("+0;-0;0")+" ms"; }
        void SliderRow(Transform parent,string title,float y,float min,float max,float value,Action<float> changed)
        {
            Label(parent,title,460,y-4,220,28,19,ink);
            var go = Rect("Slider "+title,parent,700,y,440,28).gameObject;
            var slider=go.AddComponent<Slider>();slider.minValue=min;slider.maxValue=max;
            var track=Box(go.transform,"Track",0,10,440,6,new Color(.2f,.26f,.37f));
            var handle=Box(go.transform,"Handle",0,1,24,26,gold);
            slider.targetGraphic=handle;slider.handleRect=handle.rectTransform;
            slider.direction=Slider.Direction.LeftToRight;slider.value=value;
            slider.onValueChanged.AddListener(v=>changed(v));
        }
        GameObject Modal(string name,string title,string sub)
        {
            var go=Panel(name);Box(go.transform,"Scrim",0,0,1600,900,new Color(.008f,.014f,.033f,.96f),true);
            Box(go.transform,"Modal",400,238,800,578,new Color(.055f,.08f,.14f));
            Box(go.transform,"Gold edge",400,238,800,3,gold);
            Label(go.transform,title,420,270,760,52,30,ink,true,TextAnchor.MiddleCenter);
            Label(go.transform,sub,420,345,760,48,20,muted,false,TextAnchor.MiddleCenter);
            return go;
        }
        GameObject Panel(string name) { return Rect(name,Root,0,0,1600,900).gameObject; }
        public void SetState(GameState state)
        {
            Menu.SetActive(state==GameState.Menu);Play.SetActive(state==GameState.Playing||state==GameState.Countdown||state==GameState.Paused);
            Pause.SetActive(state==GameState.Paused);Results.SetActive(state==GameState.Results);
            Settings.SetActive(false);
        }
        public RectTransform Rect(string name,Transform parent,float x,float y,float w,float h)
        {
            var rt=(RectTransform)new GameObject(name,typeof(RectTransform)).transform;rt.SetParent(parent,false);
            rt.anchorMin=rt.anchorMax=new Vector2(0,1);rt.pivot=new Vector2(0,1);
            rt.anchoredPosition=new Vector2(x,-y);rt.sizeDelta=new Vector2(w,h);return rt;
        }
        public Image Box(Transform parent,string name,float x,float y,float w,float h,Color color,bool raycast=false)
        { var image=Rect(name,parent,x,y,w,h).gameObject.AddComponent<Image>();image.color=color;image.raycastTarget=raycast;return image; }
        public Text Label(Transform parent,string value,float x,float y,float w,float h,int size,Color color,bool bold=false,TextAnchor alignment=TextAnchor.UpperLeft)
        {
            var text=Rect(value,parent,x,y,w,h).gameObject.AddComponent<Text>();text.font=font;text.text=value;text.fontSize=size;
            text.color=color;text.fontStyle=bold?FontStyle.Bold:FontStyle.Normal;text.alignment=alignment;text.raycastTarget=false;
            text.horizontalOverflow=HorizontalWrapMode.Wrap;text.verticalOverflow=VerticalWrapMode.Overflow;return text;
        }
        public Button Button(Transform parent,string title,float x,float y,float w,float h,Action action,bool primary)
        {
            var image=Box(parent,title,x,y,w,h,primary?gold:new Color(.12f,.17f,.26f),true);
            var button=image.gameObject.AddComponent<Button>();button.targetGraphic=image;
            var colors=button.colors;colors.highlightedColor=new Color(1,1,1,.8f);colors.pressedColor=new Color(.6f,.7f,.8f);button.colors=colors;
            Label(image.transform,title,0,0,w,h,18,primary?new Color(.05f,.07f,.11f):ink,true,TextAnchor.MiddleCenter);
            button.onClick.AddListener(()=>{audio.Click();action();});
            var navigation=button.navigation;navigation.mode=Navigation.Mode.None;button.navigation=navigation;
            return button;
        }
    }
}
