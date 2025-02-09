using ClipboardMonitor.Core.ClipboardListenerImp;
using ClipboardMonitor.Core.Enums;
using ClipboardMonitor.Core.EventArguments;
using System;

internal class Program
{
    static void Main()
    {
        Console.WriteLine("Starting clipboard listener from .NET...");

        var clipboardListener = new WindowsClipboardListener();
        clipboardListener.ClipboardChanged += ClipboardListener_ClipboardChanged;
        clipboardListener.Start();


        Console.WriteLine("Press Enter to exit...");
        Console.ReadLine();
    }

    private static void ClipboardListener_ClipboardChanged(object? sender, WinClipboardChangedEventArgs e)
    {
        Console.WriteLine("Clipboard changed!");

        if (e.DataType == ClipboardDataType.IMAGE && e.ClipboardImage != null)
        {
            // Try and convert to a bitmap and save
            try
            {
                Console.WriteLine(e.ClipboardImage.Width);
            }
            catch (Exception ex)
            {

            }
        }
    }
}