using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

sealed class FrameClock : IDisposable {
    [DllImport("winmm.dll")] static extern uint timeBeginPeriod(uint period);
    [DllImport("winmm.dll")] static extern uint timeEndPeriod(uint period);

    const double FrameMilliseconds=1000.0/60.0;
    readonly object gate=new object();
    readonly Control owner;
    readonly Action render;
    readonly System.Threading.Timer timer;
    readonly Stopwatch elapsed=new Stopwatch();
    long frame;
    bool running, pending, highResolution, disposed;

    public FrameClock(Control owner,Action render){
        this.owner=owner;
        this.render=render;
        timer=new System.Threading.Timer(Next,null,Timeout.Infinite,Timeout.Infinite);
    }
    public void Start(){
        lock(gate){
            if(disposed||running)return;
            highResolution=timeBeginPeriod(1)==0;
            frame=1;
            elapsed.Restart();
            running=true;
            timer.Change(17,Timeout.Infinite);
        }
    }
    void Next(object state){
        lock(gate){
            if(!running)return;
            double now=elapsed.Elapsed.TotalMilliseconds;
            frame=Math.Max(frame+1,(long)(now/FrameMilliseconds)+1);
            timer.Change(Math.Max(1,(int)Math.Ceiling(frame*FrameMilliseconds-now)),Timeout.Infinite);
            if(pending)return;
            pending=true;
            try{owner.BeginInvoke((Action)Paint);}
            catch(InvalidOperationException){pending=false;}
        }
    }
    void Paint(){
        lock(gate){
            pending=false;
            if(!running)return;
        }
        render();
    }
    public void Stop(){
        lock(gate){
            if(!running)return;
            running=false;
            timer.Change(Timeout.Infinite,Timeout.Infinite);
            if(highResolution){timeEndPeriod(1);highResolution=false;}
        }
    }
    public void Dispose(){
        lock(gate){
            if(disposed)return;
            Stop();
            disposed=true;
            timer.Dispose();
        }
    }
}
