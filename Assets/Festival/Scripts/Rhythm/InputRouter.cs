using System;
using UnityEngine;
namespace Festival
{
    public sealed class InputRouter
    {
        readonly KeyCode[] keys={KeyCode.D,KeyCode.F,KeyCode.J,KeyCode.K};
        readonly bool[] blocked=new bool[4];
        public void BlockAll(){for(int i=0;i<4;i++)blocked[i]=true;}
        public void ObserveCountdown(){for(int i=0;i<4;i++)blocked[i]=Input.GetKey(keys[i]);}
        public bool AcceptEdge(int lane,bool held,bool down)
        {
            if(!held)blocked[lane]=false;
            return !blocked[lane]&&down;
        }
        public void Poll(double calibratedTime,Action<int,double> hit)
        {
            for(int lane=0;lane<4;lane++)
                if(AcceptEdge(lane,Input.GetKey(keys[lane]),Input.GetKeyDown(keys[lane])))hit(lane,calibratedTime);
        }
    }
}
