using ClipboardMonitor.Core.Factories;
using ClipboardMonitor.Core.Interfaces;
using System;

internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("Linux Clipboard Listener Test App");

        var clipboardListener = ClipboardListenerFactory.CreateClipboardListener<ILinuxClipboardListener>();
        clipboardListener.Start();

        if (clipboardListener.IsMonitoring)
            Console.WriteLine("Clipboard monitor started");

        Console.WriteLine("Press Enter to exit...");
        Console.ReadLine();

        clipboardListener.Stop();
    }
}