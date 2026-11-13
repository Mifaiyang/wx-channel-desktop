using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

sealed class Artwork : Control {
    public event Action OpenFolder, Retry, DragWindow, CloseWindow, MinimizeWindow;
    static readonly Color Base=Color.FromArgb(13,12,17), Panel=Color.FromArgb(22,19,27),
        Ink=Color.FromArgb(237,245,244), Muted=Color.FromArgb(155,175,182),
        Edge=Color.FromArgb(56,49,62), Mint=Color.FromArgb(137,255,210);
    readonly FrameClock motion;
    readonly LinkLabel author, upstream;
    Font creditFont;
    readonly Stopwatch phase = Stopwatch.StartNew();
    readonly Stopwatch wave = new Stopwatch();
    readonly Stopwatch fade = new Stopwatch();
    readonly Bitmap appIcon=LoadMark();
    readonly ConsoleButton retry, folder, close, minimize;
    string state="idle", previous="idle", note="点击下方按钮启动本地服务";
    bool traffic, previousTraffic;

    public Artwork() {
        Dock=DockStyle.Fill;
        BackColor=Base;
        DoubleBuffered=true;
        folder=new ConsoleButton("打开下载文件夹",false);
        retry=new ConsoleButton("启动服务",true);
        minimize=new ConsoleButton("最小化",false,"min");
        close=new ConsoleButton("关闭",false,"close");
        folder.Click+=(s,e)=>{if(OpenFolder!=null)OpenFolder();};
        retry.Click+=(s,e)=>{if(Retry!=null)Retry();};
        minimize.Click+=(s,e)=>{if(MinimizeWindow!=null)MinimizeWindow();};
        close.Click+=(s,e)=>{if(CloseWindow!=null)CloseWindow();};
        author=CreditLink("作者  Mifaiyang · 皓舟","https://github.com/Mifaiyang");
        upstream=CreditLink("致谢  nobiyou/wx_channel","https://github.com/nobiyou/wx_channel");
        Controls.AddRange(new Control[]{retry,folder,minimize,close,author,upstream});
        motion=new FrameClock(this,()=>{
            Invalidate(Scaled(30,98,680,276));
            if(state!="intro" && state!="starting" && !traffic && phase.ElapsedMilliseconds>1250 &&
               (!wave.IsRunning || wave.ElapsedMilliseconds>1600) &&
               (!fade.IsRunning || fade.ElapsedMilliseconds>550))motion.Stop();
        });
    }

    LinkLabel CreditLink(string title,string url){
        var link=new LinkLabel{
            Text=title,AccessibleName=title,LinkColor=Muted,ActiveLinkColor=Mint,
            VisitedLinkColor=Muted,LinkBehavior=LinkBehavior.HoverUnderline,
            BackColor=Base,TextAlign=ContentAlignment.MiddleLeft,TabStop=true
        };
        link.LinkClicked+=(s,e)=>{
            try{Process.Start(new ProcessStartInfo(url){UseShellExecute=true});}
            catch(System.ComponentModel.Win32Exception){MessageBox.Show(url,"GitHub 链接");}
        };
        return link;
    }

    static Bitmap LoadMark(){
        using(var stream=typeof(Artwork).Assembly.GetManifestResourceStream("app-mark"))
        using(var image=Image.FromStream(stream))return new Bitmap(image);
    }

    Rectangle Scaled(int x,int y,int w,int h){
        return Rectangle.Round(new RectangleF(x*Width/740f,y*Height/488f,w*Width/740f,h*Height/488f));
    }
    protected override void OnSizeChanged(EventArgs e){
        base.OnSizeChanged(e);
        if(upstream==null)return;
        retry.Bounds=Scaled(32,390,148,50);
        folder.Bounds=state=="ready"?Scaled(32,390,176,50):Scaled(192,390,176,50);
        minimize.Bounds=Scaled(612,28,44,44);
        close.Bounds=Scaled(664,28,44,44);
        author.Bounds=Scaled(32,451,175,28);
        upstream.Bounds=Scaled(216,451,230,28);
        var previousFont=creditFont;
        creditFont=new Font("Microsoft YaHei UI",12*Math.Max(1,Height)/488f,FontStyle.Regular,GraphicsUnit.Pixel);
        author.Font=creditFont;
        upstream.Font=creditFont;
        if(previousFont!=null)previousFont.Dispose();
        Invalidate();
    }
    protected override void OnMouseDown(MouseEventArgs e){
        base.OnMouseDown(e);
        if(e.Button==MouseButtons.Left && e.Y<Height*96/488 && DragWindow!=null)DragWindow();
    }
    public void SetTraffic(bool active,bool next){
        if(traffic!=active){previousTraffic=traffic;traffic=active;fade.Restart();motion.Start();Invalidate();}
        if(next){wave.Restart();motion.Start();}
    }

    public void SetStatus(string next,string message) {
        if(state==next && note==message)return;
        if(state!=next){previous=state;state=next;phase.Restart();motion.Start();}
        note=message;
        retry.Text=next=="intro"?"正在唤醒…":next=="starting"?"启动中…":next=="error"?"重新启动":"启动服务";
        retry.AccessibleName=retry.Text;
        retry.Visible=next!="ready";
        retry.Enabled=next!="intro" && next!="starting";
        folder.Bounds=next=="ready"?Scaled(32,390,176,50):Scaled(192,390,176,50);
        Invalidate();
    }

    static GraphicsPath Rounded(RectangleF r,float radius) {
        var p=new GraphicsPath();
        float d=radius*2;
        p.AddArc(r.X,r.Y,d,d,180,90);
        p.AddArc(r.Right-d,r.Y,d,d,270,90);
        p.AddArc(r.Right-d,r.Bottom-d,d,d,0,90);
        p.AddArc(r.X,r.Bottom-d,d,d,90,90);
        p.CloseFigure();
        return p;
    }

    static void Surface(Graphics g,RectangleF r,float radius,Color fill,Color border) {
        using(var path=Rounded(r,radius)) {
            using(var b=new SolidBrush(fill))g.FillPath(b,path);
            using(var p=new Pen(border))g.DrawPath(p,path);
        }
    }

    static void Label(Graphics g,string value,int x,int y,float size,Color color,bool bold=false,bool mono=false) {
        using(var f=new Font(mono?"Consolas":"Microsoft YaHei UI",size,bold?FontStyle.Bold:FontStyle.Regular,GraphicsUnit.Pixel))
        using(var brush=new SolidBrush(color))g.DrawString(value,f,brush,x,y,StringFormat.GenericTypographic);
    }

    static readonly Color[] Spectrum = {
        Color.FromArgb(255,200,112), Color.FromArgb(133,244,202), Color.FromArgb(134,196,255),
        Color.FromArgb(190,145,255), Color.FromArgb(250,126,196)
    };
    static readonly Dictionary<char,string> Pixels=new Dictionary<char,string> {
        {'R',"11110/10001/10001/11110/10100/10010/10001"},
        {'E',"11111/10000/10000/11110/10000/10000/11111"},
        {'A',"01110/10001/10001/11111/10001/10001/10001"},
        {'D',"11110/10001/10001/10001/10001/10001/11110"},
        {'Y',"10001/10001/01010/00100/00100/00100/00100"},
        {'B',"11110/10001/10001/11110/10001/10001/11110"},
        {'O',"01110/10001/10001/10001/10001/10001/01110"},
        {'T',"11111/00100/00100/00100/00100/00100/00100"},
        {'S',"01111/10000/10000/01110/00001/00001/11110"},
        {'P',"11110/10001/10001/11110/10000/10000/10000"},
        {'I',"11111/00100/00100/00100/00100/00100/11111"},
        {'L',"10000/10000/10000/10000/10000/10000/11111"}
    };

    protected override void OnPaint(PaintEventArgs e) {
        base.OnPaint(e);
        var g=e.Graphics;
        g.SmoothingMode=SmoothingMode.AntiAlias;
        g.TextRenderingHint=TextRenderingHint.AntiAliasGridFit;
        g.InterpolationMode=InterpolationMode.HighQualityBicubic;
        g.Clear(Base);
        g.ScaleTransform(Width/740f,Height/488f);
        g.DrawImage(appIcon,new Rectangle(32,28,44,44));
        Label(g,"视频号下载助手",92,29,25,Ink,true);
        Label(g,"v5.7.16",530,44,13,Muted,false,true);
        PaintPanel(g);
        Surface(g,new RectangleF(32,390,676,50),12,Panel,Edge);
        Color indicator=state=="ready"?Mint:state=="error"?Spectrum[0]:Spectrum[3];
        using(var b=new SolidBrush(indicator))g.FillEllipse(b,595,410,6,6);
        Label(g,state=="ready"?(traffic?"下载活动中":"监听端口 2025"):state=="error"?"服务已停止":state=="idle"?"等待启动":state=="intro"?"正在唤醒":"连接中",610,404,13,Ink);
        Label(g,"使用说明见同目录 PDF",572,458,12,Muted);
    }

    void PaintPanel(Graphics g) {
        var frame=new RectangleF(32,100,676,272);
        Surface(g,frame,24,Panel,Edge);
        var saved=g.Save();
        using(var path=Rounded(frame,24))g.SetClip(path,CombineMode.Intersect);
        bool ready=state=="ready", intro=state=="intro", starting=state=="starting";
        float t=(float)phase.Elapsed.TotalSeconds;
        float duration=intro?Launcher.IntroDurationMs/1000f:ready?1.15f:.55f;
        float progress=Math.Min(1,t/duration);
        float settle=progress*progress*(3-2*progress);
        float activityMix=fade.IsRunning?Math.Min(1,(float)fade.Elapsed.TotalMilliseconds/500):1;
        float activityLevel=(previousTraffic?1:0)*(1-activityMix)+(traffic?1:0)*activityMix;
        float pulse=intro?progress:starting?(t+.3f)/2.2f%1:wave.IsRunning?(float)wave.Elapsed.TotalSeconds/1.6f:1;
        float clock=(float)phase.Elapsed.TotalSeconds;
        int glow=ready?112:intro||starting?65:24;
        int oldGlow=previous=="ready"?112:previous=="intro"||previous=="starting"?65:24;
        PaintGlow(g,(int)(oldGlow+(glow-oldGlow)*settle+activityLevel*24));
        using(var dot=new SolidBrush(Color.FromArgb(80,111,94,127))) {
            for(int y=128;y<335;y+=17)
                for(int x=302;x<687;x+=17)g.FillEllipse(dot,x,y,1.25f,1.25f);
        }
        for(int i=4;i>=0;i--) {
            float target=ready?1:intro||starting?.54f:.22f;
            float origin=previous=="ready"?1:previous=="intro"||previous=="starting"?.54f:.22f;
            float amplitude=(148-i*26)*(origin+(target-origin)*settle);
            float colorLevel=intro?settle:ready||starting?1:0;
            PaintWave(g,i,amplitude,clock,activityLevel,pulse,colorLevel);
        }
        g.Restore(saved);
        Label(g,"下载服务",56,128,14,Muted);
        string before=Word(previous), current=Word(state);
        if(before!=current && settle<1)PaintWord(g,before,57,167,WordColor(previous),1-settle);
        PaintWord(g,current,57,167,WordColor(state),before==current?1:settle);
        Label(g,ready?(traffic?"检测到下载活动":"已就绪，可以下载"):intro?"正在唤醒":starting?"正在启动服务":state=="idle"?"等待启动":"服务暂未启动",56,242,20,Ink,true);
        Label(g,note.TrimEnd('。'),56,276,13,Muted);
    }

    static string Word(string value){return value=="ready"?"READY":value=="intro"||value=="starting"?"BOOT":value=="idle"?"IDLE":"STOP";}
    static Color WordColor(string value){return value=="error"?Color.FromArgb(255,193,152):value=="intro"||value=="starting"?Spectrum[3]:Ink;}

    static void PaintGlow(Graphics g,int alpha) {
        using(var halo=new GraphicsPath()){
            halo.AddEllipse(128,210,665,258);
            using(var b=new PathGradientBrush(halo)){
                b.CenterPoint=new PointF(488,346);
                b.CenterColor=Color.FromArgb(alpha,202,61,159);
                b.SurroundColors=new[]{Color.Transparent};
                g.FillPath(b,halo);
            }
        }
    }

    static void PaintWave(Graphics g,int index,float amplitude,float time,float activity,float pulse,float colorLevel) {
        var points=new PointF[97];
        for(int i=0;i<points.Length;i++){
            float x=i/(float)(points.Length-1);
            float envelope=(float)Math.Pow(Math.Sin(Math.PI*x),2.4);
            float ripple=activity*10*(float)Math.Sin(x*15-time*3.2+index*.38);
            float sweep=0;
            if(pulse<1){
                double d=(x-(-.2+1.4*pulse))/.10;
                sweep=(float)(24*Math.Exp(-d*d));
            }
            points[i]=new PointF(224+456*x,322-envelope*(amplitude+ripple+sweep));
        }
        Color dim=Color.FromArgb(151,128,169), lit=Spectrum[index];
        Color hue=Color.FromArgb((int)(dim.R+(lit.R-dim.R)*colorLevel),(int)(dim.G+(lit.G-dim.G)*colorLevel),(int)(dim.B+(lit.B-dim.B)*colorLevel));
        using(var shade=new LinearGradientBrush(new RectangleF(223,130,458,226),
            Color.Transparent,Color.Transparent,LinearGradientMode.Horizontal)){
            foreach(var layer in new[]{new[]{18,9},new[]{7,31},new[]{2,100+(int)(155*colorLevel)}}){
                shade.InterpolationColors=new ColorBlend {
                    Positions=new[]{0f,.17f,.83f,1f},
                    Colors=new[]{Color.FromArgb(0,hue),Color.FromArgb(layer[1],hue),Color.FromArgb(layer[1],hue),Color.FromArgb(0,hue)}
                };
                using(var pen=new Pen(shade,layer[0]))g.DrawLines(pen,points);
            }
        }
    }

    static void PaintWord(Graphics g,string word,float x,float y,Color color,float brightness) {
        using(var active=new SolidBrush(Color.FromArgb((int)(255*brightness),color)))
        using(var idle=new SolidBrush(Color.FromArgb(37,165,143,174))) {
            foreach(char glyph in word){
                var rows=Pixels[glyph].Split('/');
                for(int row=0;row<7;row++)for(int col=0;col<5;col++){
                    bool lit=rows[row][col]=='1';
                    float size=lit?4.8f:2f;
                    g.FillEllipse(lit?active:idle,x+col*7-size/2,y+row*7-size/2,size,size);
                }
                x+=42;
            }
        }
    }
    protected override void Dispose(bool disposing){
        if(disposing){if(motion!=null)motion.Dispose();if(appIcon!=null)appIcon.Dispose();}
        base.Dispose(disposing);
        if(disposing && creditFont!=null){creditFont.Dispose();creditFont=null;}
    }

    sealed class ConsoleButton : Button {
        readonly bool primary;
        readonly string glyph;
        readonly FrameClock timer;
        readonly Stopwatch clock=new Stopwatch();
        float from, target, hover;
        public ConsoleButton(string title,bool accent,string icon=null){
            Text=title;AccessibleName=title;primary=accent;glyph=icon;FlatStyle=FlatStyle.Flat;FlatAppearance.BorderSize=0;
            Cursor=Cursors.Hand;Font=new Font("Microsoft YaHei UI",14,FontStyle.Bold,GraphicsUnit.Pixel);
            SetStyle(ControlStyles.OptimizedDoubleBuffer|ControlStyles.AllPaintingInWmPaint,true);
            timer=new FrameClock(this,()=>{
                float t=Math.Min(1,(float)clock.Elapsed.TotalMilliseconds/135);
                hover=from+(target-from)*(1-(float)Math.Pow(1-t,3));
                Invalidate();
                if(t>=1)timer.Stop();
            });
        }
        void Animate(float next){from=hover;target=next;clock.Restart();timer.Start();}
        protected override void OnMouseEnter(EventArgs e){base.OnMouseEnter(e);Animate(1);}
        protected override void OnMouseLeave(EventArgs e){base.OnMouseLeave(e);Animate(0);}
        protected override void OnPaint(PaintEventArgs e){
            var g=e.Graphics;g.Clear(Base);g.SmoothingMode=SmoothingMode.AntiAlias;
            float scale=Height/(glyph==null?50f:44f);
            Color fill=!Enabled?Panel:primary?Color.FromArgb(235+(int)(18*hover),230+(int)(18*hover),240+(int)(12*hover)):Color.FromArgb(23+(int)(16*hover),20+(int)(13*hover),30+(int)(16*hover));
            if(glyph!=null && hover<.01f)fill=Base;
            Surface(g,new RectangleF(.5f,.5f,Width-1,Height-1),8*scale,fill,primary||glyph!=null?fill:Edge);
            if(glyph==null){
                using(var scaled=new Font("Microsoft YaHei UI",14*scale,FontStyle.Bold,GraphicsUnit.Pixel))
                    TextRenderer.DrawText(g,Text,scaled,ClientRectangle,!Enabled?Muted:primary?Base:Ink,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter);
            }else{
                float x=Width/2f,y=Height/2f,r=5.2f*scale;
                using(var pen=new Pen(Ink,1.4f*scale)){
                    pen.StartCap=LineCap.Round;pen.EndCap=LineCap.Round;
                    if(glyph=="min")g.DrawLine(pen,x-r,y,x+r,y);
                    else {g.DrawLine(pen,x-r,y-r,x+r,y+r);g.DrawLine(pen,x+r,y-r,x-r,y+r);}
                }
            }
            if(Focused && ShowFocusCues)ControlPaint.DrawFocusRectangle(g,Rectangle.Inflate(ClientRectangle,-5,-5),primary?Base:Ink,fill);
        }
        protected override void Dispose(bool disposing){if(disposing)timer.Dispose();base.Dispose(disposing);}
    }
}
