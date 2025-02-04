using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

internal class Program
{
    // Define a delegate matching the C++ callback function signature
    public delegate void ClipboardChangedCallback();

    // Import SetClipboardChangedCallback function from the DLL
    [DllImport("ClipboardMonitor.Windows.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void SetClipboardChangedCallback(ClipboardChangedCallback callback);

    // Import StartClipboardListener function from the DLL
    [DllImport("ClipboardMonitor.Windows.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void StartClipboardListener();

    // The callback function called when the clipboard changes
    static void OnClipboardChanged()
    {
        Console.WriteLine("Clipboard content changed (from .NET)!");
    }

    static void Main()
    {
        Console.WriteLine("Starting clipboard listener from .NET...");

        // Set the callback function
        SetClipboardChangedCallback(OnClipboardChanged);

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
}