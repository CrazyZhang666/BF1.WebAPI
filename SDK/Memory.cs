using System.Text;

namespace BF1.WebAPI.SDK;

public static class Memory
{
    /// <summary>
    /// 战地1进程类
    /// </summary>
    private static Process Bf1Process { get; set; } = null;
    /// <summary>
    /// 战地1窗口句柄
    /// </summary>
    public static IntPtr Bf1WinHandle { get; private set; } = IntPtr.Zero;
    /// <summary>
    /// 战地1进程Id
    /// </summary>
    public static int Bf1ProId { get; private set; } = 0;
    /// <summary>
    /// 战地1进程句柄
    /// </summary>
    public static IntPtr Bf1ProHandle { get; private set; } = IntPtr.Zero;
    /// <summary>
    /// 战地1主模块基址
    /// </summary>
    public static long Bf1ProBaseAddress { get; private set; } = 0;

    /// <summary>
    /// 初始化战地1内存模块
    /// </summary>
    /// <returns></returns>
    public static bool Initialize()
    {
        try
        {
            var pArray = Process.GetProcessesByName("bf1");
            if (pArray.Length > 0)
            {
                foreach (var item in pArray)
                {
                    if (item.MainWindowTitle.Equals("Battlefield™ 1"))
                    {
                        Bf1Process = item;
                        break;
                    }
                }

                if (Bf1Process == null)
                {
                    return false;
                }

                Bf1WinHandle = Bf1Process.MainWindowHandle;
                Bf1ProId = Bf1Process.Id;
                Bf1ProHandle = Bf1Process.Handle;

                if (Bf1Process.MainModule != null)
                {
                    Bf1ProBaseAddress = Bf1Process.MainModule.BaseAddress.ToInt64();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        catch { return false; }
    }

    /// <summary>
    /// 重置战地1内存模块
    /// </summary>
    public static void Uninitialize()
    {
        Bf1Process = null;
        Bf1WinHandle = IntPtr.Zero;
        Bf1ProId = 0;
        Bf1ProHandle = IntPtr.Zero;
        Bf1ProBaseAddress = 0;
    }

    /// <summary>
    /// 将战地1窗口置顶
    /// </summary>
    public static void SetForegroundWindow()
    {
        Win32.SetForegroundWindow(Bf1WinHandle);
    }

    /// <summary>
    /// 泛型读取内存
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="address"></param>
    /// <returns></returns>
    public static T Read<T>(long address) where T : struct
    {
        var buffer = new byte[Marshal.SizeOf(typeof(T))];
        Win32.ReadProcessMemory(Bf1ProHandle, address, buffer, buffer.Length, out _);
        return ByteArrayToStructure<T>(buffer);
    }

    /// <summary>
    /// 泛型写入内存
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="address"></param>
    /// <param name="value"></param>
    public static void Write<T>(long address, T value) where T : struct
    {
        var buffer = StructureToByteArray(value);
        Win32.WriteProcessMemory(Bf1ProHandle, address, buffer, buffer.Length, out _);
    }

    /// <summary>
    /// 读取字符串
    /// </summary>
    /// <param name="address"></param>
    /// <param name="size"></param>
    /// <returns></returns>
    public static string ReadString(long address, int size)
    {
        var buffer = new byte[size];
        Win32.ReadProcessMemory(Bf1ProHandle, address, buffer, size, out _);

        for (int i = 0; i < buffer.Length; i++)
        {
            if (buffer[i] == 0)
            {
                byte[] _buffer = new byte[i];
                Buffer.BlockCopy(buffer, 0, _buffer, 0, i);
                return Encoding.ASCII.GetString(_buffer);
            }
        }

        return Encoding.ASCII.GetString(buffer);
    }

    /// <summary>
    /// 写入UTF-8格式字符串
    /// </summary>
    /// <param name="address"></param>
    /// <param name="str"></param>
    public static void WriteStringUTF8(long address,  string str)
    {
        var buffer = Encoding.UTF8.GetBytes(str);
        Win32.WriteProcessMemory(Bf1ProHandle, address, buffer, buffer.Length, out _);
    }

    //////////////////////////////////////////////////////////////////

    /// <summary>
    /// 判断内存地址是否有效
    /// </summary>
    /// <param name="Address"></param>
    /// <returns></returns>
    public static bool IsValid(long Address)
    {
        return Address >= 0x10000 && Address < 0x000F000000000000;
    }

    /// <summary>
    /// 字符数组转结构
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="bytes"></param>
    /// <returns></returns>
    private static T ByteArrayToStructure<T>(byte[] bytes) where T : struct
    {
        var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        try
        {
            var obj = Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            if (obj != null)
                return (T)obj;
            else
                return default;
        }
        finally
        {
            handle.Free();
        }
    }

    /// <summary>
    /// 结构转字符数组
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    private static byte[] StructureToByteArray(object obj)
    {
        int length = Marshal.SizeOf(obj);
        byte[] array = new byte[length];
        IntPtr pointer = Marshal.AllocHGlobal(length);
        Marshal.StructureToPtr(obj, pointer, true);
        Marshal.Copy(pointer, array, 0, length);
        Marshal.FreeHGlobal(pointer);
        return array;
    }
}
