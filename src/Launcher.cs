using System;
using System.IO;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Reflection;

class Launcher : Form {
    internal const int IntroDurationMs = 3000;
    readonly Timer introTimer=new Timer { Interval=IntroDurationMs };
    [DllImport("user32.dll")] static extern uint GetDpiForWindow(IntPtr window);
    [DllImport("user32.dll")] static extern bool ReleaseCapture();
    [DllImport("user32.dll")] static extern IntPtr SendMessage(IntPtr window,int message,IntPtr w,IntPtr l);
    [DllImport("dwmapi.dll")] static extern int DwmSetWindowAttribute(IntPtr window,int attribute,ref int value,int size);
    readonly DownloadActivity activity=new DownloadActivity();
    protected override void OnHandleCreated(EventArgs e){
        base.OnHandleCreated(e);
        try {
            uint dpi=GetDpiForWindow(Handle);
            if(dpi>0)SetDpi((int)dpi);
            int round=2;
            DwmSetWindowAttribute(Handle,33,ref round,4);
        }catch(EntryPointNotFoundException){}
    }
    void SetDpi(int dpi){ClientSize=new Size((740*dpi+48)/96,(488*dpi+48)/96);}
    protected override void WndProc(ref Message m){
        if(m.Msg==0x02E0){
            var rect=(NativeRect)Marshal.PtrToStructure(m.LParam,typeof(NativeRect));
            SetDpi((int)(m.WParam.ToInt64()&0xffff));
            Location=new Point(rect.Left,rect.Top);
            return;
        }
        base.WndProc(ref m);
    }
    [StructLayout(LayoutKind.Sequential)] struct NativeRect {public int Left,Top,Right,Bottom;}
    void DragWindow(){ReleaseCapture();SendMessage(Handle,0xA1,new IntPtr(2),IntPtr.Zero);}
    [DllImport("kernel32.dll", SetLastError=true)] static extern bool AttachConsole(uint id);
    [DllImport("kernel32.dll", SetLastError=true)] static extern bool FreeConsole();
    [DllImport("kernel32.dll", SetLastError=true)] static extern bool SetConsoleCtrlHandler(IntPtr handler, bool add);
    [DllImport("kernel32.dll", SetLastError=true)] static extern bool GenerateConsoleCtrlEvent(uint ctrlEvent,uint groupId);
    readonly string dir=AppDomain.CurrentDomain.BaseDirectory, folder=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos),"视频号下载");
    Process backend; Artwork art; Timer poll;
    Launcher(){Text="视频号下载助手";ClientSize=new Size(740,488);FormBorderStyle=FormBorderStyle.None;Icon=Icon.ExtractAssociatedIcon(Application.ExecutablePath);MaximizeBox=false;StartPosition=FormStartPosition.CenterScreen;AutoScaleMode=AutoScaleMode.None;
        art=new Artwork();Controls.Add(art);art.OpenFolder+=OpenFolder;art.Retry+=BeginStartup;art.DragWindow+=DragWindow;art.CloseWindow+=Close;art.MinimizeWindow+=()=>WindowState=FormWindowState.Minimized;poll=new Timer();poll.Interval=300;poll.Tick+=(s,e)=>RefreshStatus();poll.Start();
        introTimer.Tick+=(s,e)=>{introTimer.Stop();StartService();};
        FormClosing+=(s,e)=>{introTimer.Stop();StopService(e);};
        Disposed+=(s,e)=>{introTimer.Dispose();poll.Dispose();};
    }
    bool PortBusy(){try{foreach(var ep in IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners())if(ep.Port==2025)return true;}catch{}return false;}
    void Update(string state,string note){art.SetStatus(state,note);}
    internal static string BackendPath(string directory){return Path.Combine(directory,"程序文件","wx_channel.exe");}
    void BeginStartup(){
        if(introTimer.Enabled)return;
        if(backend!=null&&!backend.HasExited){RefreshStatus();return;}
        if(PortBusy()){Update("error","端口被占用，请先退出其他下载服务。");return;}
        Update("intro","流光正在点亮，片刻后启动本地服务");
        introTimer.Start();
    }
    void StartService(){if(backend!=null&&!backend.HasExited){RefreshStatus();return;} if(PortBusy()){Update("error","端口被占用，请先退出其他下载服务。");return;}
        try {Directory.CreateDirectory(folder);string cfg=Path.Combine(dir,"config.yaml");if(!File.Exists(cfg))File.WriteAllText(cfg,"port: 2025\ndownload_dir: '"+folder.Replace("'","''")+"'\ncloud_enabled: false\nradar_enabled: false\n",new System.Text.UTF8Encoding(false));if(!File.Exists(BackendPath(dir)))throw new FileNotFoundException("下载组件不完整，请重新完整解压 ZIP。");
            activity.Reset(Path.Combine(dir,"logs","wx_channel.log"));
            backend=Process.Start(new ProcessStartInfo{FileName=BackendPath(dir),Arguments="-p 2025",WorkingDirectory=dir,UseShellExecute=false,CreateNoWindow=true});
            Update("starting","首次运行请留意证书提示");
        }catch(Exception ex){Update("error","启动失败，请检查文件或权限。");MessageBox.Show(ex.Message,"启动失败");}}
    void RefreshStatus(){
        if(introTimer.Enabled||backend==null)return;
        try {
            if(backend.HasExited){art.SetTraffic(false,false);Update("error","服务已停止，可点击下方按钮重试");}
            else if(PortOwner.IsListening(2025,backend.Id)){
                int starts=activity.Read();
                art.SetTraffic(activity.IsActive,starts>0);
                Update("ready","打开电脑微信，进入视频号");
            }else {art.SetTraffic(false,false);Update("starting","首次运行请留意证书提示");}
        }catch(Exception){Update("error","无法读取服务状态，请重新启动。");}
    }
    void StopService(FormClosingEventArgs e){if(backend==null||backend.HasExited)return;bool sent=false;try{if(AttachConsole((uint)backend.Id)){SetConsoleCtrlHandler(IntPtr.Zero,true);sent=GenerateConsoleCtrlEvent(0,0);FreeConsole();SetConsoleCtrlHandler(IntPtr.Zero,false);}if(sent&&backend.WaitForExit(8000))return;}catch{}e.Cancel=true;MessageBox.Show("后台服务暂时无法安全退出，请稍等几秒后再次关闭。","关闭服务",MessageBoxButtons.OK,MessageBoxIcon.Warning);}
    void OpenFolder(){try{Directory.CreateDirectory(folder);Process.Start(new ProcessStartInfo(folder){UseShellExecute=true});}catch(Exception e){MessageBox.Show(e.Message,"无法打开文件夹");}}
    [STAThread] static void Main(){Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);Application.Run(new Launcher());}
}
