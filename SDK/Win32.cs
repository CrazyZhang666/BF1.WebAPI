namespace BF1.WebAPI.SDK;

public static class Win32
{
    [DllImport("user32.dll")]
    public static extern int SetForegroundWindow(IntPtr hWnd);

    [DllImport("kernel32.dll")]
    public static extern bool ReadProcessMemory(IntPtr hProcess, long lpBaseAddress, [In, Out] byte[] lpBuffer, int nsize, out IntPtr lpNumberOfBytesRead);

    [DllImport("kernel32.dll")]
    public static extern bool WriteProcessMemory(IntPtr hProcess, long lpBaseAddress, [In, Out] byte[] lpBuffer, int nsize, out IntPtr lpNumberOfBytesWritten);
}
