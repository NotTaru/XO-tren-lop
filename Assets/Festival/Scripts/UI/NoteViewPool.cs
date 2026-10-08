using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace Festival
{
    public sealed class NoteViewPool
    {
        sealed class View { public Image Image; public NoteData Note; public bool Active; }
        readonly List<View> pool=new List<View>();
        readonly FestivalView ui;
        ChartDefinition chart;
        NoteJudge judge;
        double travel;
        int next;
        public NoteViewPool(FestivalView view)
        {
            ui=view;
            for(int i=0;i<32;i++)
            {
                var image=ui.Box(ui.NoteContainer,"Pooled note "+i,0,0,112,20,Color.white);
                ui.Box(image.transform,"Note highlight",9,4,94,3,new Color(1,1,1,.6f));
                image.gameObject.SetActive(false);pool.Add(new View{Image=image});
            }
        }
        public void Begin(ChartDefinition chart,NoteJudge judge,double travel)
        { Clear();this.chart=chart;this.judge=judge;this.travel=travel;next=0; }
        public void Tick(double time)
        {
            while(next<chart.notes.Count && chart.notes[next].hitTimeSeconds-travel<=time)
            {
                var note=chart.notes[next++];
                if(judge.states[note.id]!=Judgment.Pending)continue;
                foreach(var v in pool)if(!v.Active)
                {
                    v.Note=note;v.Active=true;v.Image.color=FestivalView.LaneColors[note.lane];v.Image.gameObject.SetActive(true);break;
                }
            }
            foreach(var v in pool)if(v.Active)
            {
                if(judge.states[v.Note.id]!=Judgment.Pending){v.Active=false;v.Image.gameObject.SetActive(false);continue;}
                float progress=(float)(1-(v.Note.hitTimeSeconds-time)/travel);
                v.Image.rectTransform.anchoredPosition=new Vector2(23+144*v.Note.lane,-(progress*429-10));
            }
        }
        public void Clear(){foreach(var v in pool){v.Active=false;v.Image.gameObject.SetActive(false);}}
    }
}
