using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

internal class Program
{
    // Define a delegate matching the C++ callback function signature
    public delegate void ClipboardChangedCallback();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void ClipboardChangedCallbackWithData([MarshalAs(UnmanagedType.LPStr)] string data, int type);

    // Import SetClipboardChangedCallback function from the DLL
    [DllImport("ClipboardMonitor.Windows.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void SetClipboardChangedCallback(ClipboardChangedCallback callback);

    // Import StartClipboardListener function from the DLL
    [DllImport("ClipboardMonitor.Windows.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void StartClipboardListener();

    [DllImport("ClipboardMonitor.Windows.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void SetClipboardChangedCallbackWithData(ClipboardChangedCallbackWithData callback);

    // The callback function called when the clipboard changes
    static void OnClipboardChanged()
    {
        Console.WriteLine("Clipboard content changed (from .NET)!");
    }

    static void OnClipboardChangedWithData(string data, int type)
    {
        var dataType = (ClipboardDataType)type;

        switch (dataType)
        {
            case ClipboardDataType.TEXT:
                Console.WriteLine("Text copied: " + data);
                break;
            case ClipboardDataType.FILES:
                Console.WriteLine("Files copied: " + data);
                break;
            case ClipboardDataType.IMG_BITMAP:
            case ClipboardDataType.IMG_DIB:
                Console.WriteLine("Image copied");
                break;
            default:
                Console.WriteLine("Unknown clipboard event");
                break;
        }
    }

    static void Main()
    {
        Console.WriteLine("Starting clipboard listener from .NET...");

        // Set the callback function
        SetClipboardChangedCallback(OnClipboardChanged);

        // Register the callback
        SetClipboardChangedCallbackWithData(OnClipboardChangedWithData);

        // Start clipboard monitoring thread in the background so not to block the main thread
        //var clipboardMonitorThread = new Thread(StartClipboardListener) { IsBackground = true };
        //clipboardMonitorThread.Start();
        var cts = new CancellationTokenSource();
        var t = Task.Run(Start, cts.Token);
        

        Console.WriteLine("Press Enter to exit...");
        Console.ReadLine();

        cts.Cancel();
        cts = null;
        t = null;

        Console.WriteLine("Resources released");
    }

    static async void Start()
    {
        StartClipboardListener();
        await Task.CompletedTask;
    }

    private enum ClipboardDataType
    {
        TEXT = 1,
        FILES = 2,
        IMG_BITMAP = 3,
        IMG_DIB = 4,
    }
}