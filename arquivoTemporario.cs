public class Inicio {
    /*public Inicio(){
        MoveToMonitor(2);
    }

    private List<MonitorInfo> GetAllMonitors()
    {
        List<MonitorInfo> monitors = new List<MonitorInfo>();

        NativeMethods.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, (IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData) =>
        {
            var monitorInfo = new NativeMethods.MONITORINFO();
            monitorInfo.cbSize = Marshal.SizeOf(monitorInfo);
            NativeMethods.GetMonitorInfo(hMonitor, ref monitorInfo);

            monitors.Add(new MonitorInfo{
                MonitorHandle = hMonitor,
                MonitorArea = monitorInfo.rcMonitor
            });
            return true;
        } , IntPtr.Zero);
    }

    private void MoveToMonitor(int numMonitor)
    {
        List<MonitorInfo> monitors = GetAllMonitors();
        MonitorInfo monitor = default;

        if(monitors.Count >= numMonitor){
            numMonitor--; //Torna o numero do monitor dentro dos indices da lista
            monitor = monitors[numMonitor];
        } else {
            return;
        }

        var hwnd = new WindowInteropHelper(this).Handle;

        NativeMethods.SetWindowPos(
            hwnd, 
            IntPtr.Zero, 
            monitor.MonitorArea.left, 
            monitor.MonitorArea.top,
            monitor.MonitorArea.right - monitor.MonitorArea.left,
            monitor.MonitorArea.bottom - monitor.MonitorArea.top,
            NativeMethods.SWP_NOZORDER | NativeMethods.SWP_NOACTIVE);
    }
}

public struct MonitorInfo{
    public IntPtr MonitorHandle;
    public RECT MonitorArea;
}

public static class NativeMethods{
    public const int SWP_NOZORDER = 0x0004;
    public const int SWP_NOACTIVE = 0x0010;

    [DLLIMPORT("user32.dll")]
    public static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumProc lpfEnum, IntPtr dwData);

    [DLLIMPORT("user32.dll")]
    public static extern bool GetMonitorInfo(IntPtr hMonitor,ref MONITORINFO lpmi);

    [DLLIMPORT("user32.dll", SetLastError = true)]
    public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, int uFlags);

    public delegate bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData);

    [StructLayout(LayoutKind.Sequential)]
    private struct MONITORINFO
    {
        public int cbSize;
        public RECT rcMonitor;
        public RECT rcWork;
        public uint dwFlags;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
         
        public override string ToString(){
            return $"[Left = {left}, Top = {top}, Right = {right}, Bottom = {bottom}]";
        }
    }*/
}