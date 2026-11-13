using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

static class PortOwner {
    [DllImport("iphlpapi.dll")]
    static extern uint GetExtendedTcpTable(IntPtr buffer, ref int size, bool sort, int family, int table, uint reserved);

    public static bool IsListening(int port, int pid) {
        return HasListener(port,pid,2,24,8,20) || HasListener(port,pid,23,56,20,52);
    }

    static bool HasListener(int port,int pid,int family,int rowSize,int portOffset,int pidOffset) {
        int size=0;
        const int ownerPidListener=3;
        if(GetExtendedTcpTable(IntPtr.Zero,ref size,false,family,ownerPidListener,0)!=122 || size<4)return false;
        int capacity=size;
        IntPtr table=Marshal.AllocHGlobal(capacity);
        try {
            if(GetExtendedTcpTable(table,ref size,false,family,ownerPidListener,0)!=0)return false;
            int count=Math.Min(Marshal.ReadInt32(table),(capacity-4)/rowSize);
            for(int i=0;i<count;i++){
                int row=4+i*rowSize;
                int actual=(Marshal.ReadByte(table,row+portOffset)<<8)|Marshal.ReadByte(table,row+portOffset+1);
                if(actual==port && Marshal.ReadInt32(table,row+pidOffset)==pid)return true;
            }
            return false;
        } finally {Marshal.FreeHGlobal(table);}
    }
}

sealed class DownloadActivity {
    string path, pending="";
    long offset;
    DateTime activeUntil;
    public bool IsActive {get {return DateTime.UtcNow<activeUntil;}}
    public void Reset(string logPath){
        path=logPath; pending=""; offset=0; activeUntil=DateTime.MinValue;
        try{if(File.Exists(path))offset=new FileInfo(path).Length;}catch(IOException){}
    }
    public int Read(){
        if(!File.Exists(path))return 0;
        try{
            using(var stream=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.ReadWrite|FileShare.Delete)){
                if(stream.Length<offset){offset=0;pending="";}
                if(stream.Length-offset>262144){offset=stream.Length-262144;pending="";}
                stream.Position=offset;
                byte[] bytes=new byte[Math.Min(65536,(int)(stream.Length-offset))];
                int count=stream.Read(bytes,0,bytes.Length);
                if(count==0)return 0;
                offset+=count;
                string[] rows=(pending+Encoding.UTF8.GetString(bytes,0,count)).Split('\n');
                pending=rows[rows.Length-1];
                if(pending.Length>16384)pending="";
                int starts=0;
                for(int i=0;i<rows.Length-1;i++){
                    var row=rows[i];
                    bool single=row.Contains("[视频下载]");
                    bool worker=row.Contains("[Worker ");
                    bool start=(single && row.Contains("使用 Gopeed 引擎:")) || (worker && row.Contains("开始下载:"));
                    bool progress=(single || worker || row.Contains("[批量下载]")) && row.Contains("进度:");
                    if(start)starts++;
                    if(start || progress)activeUntil=DateTime.UtcNow.AddSeconds(7);
                    else if(single && (row.Contains("视频已保存") || row.Contains("下载失败")))activeUntil=DateTime.UtcNow;
                }
                return starts;
            }
        }catch(IOException){return 0;}catch(UnauthorizedAccessException){return 0;}
    }
}
