using ClipboardMonitor.Core.Enums;
using ClipboardMonitor.Core.EventArguments;
using ClipboardMonitor.Core.Factories;
using ClipboardMonitor.Core.Interfaces;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing.Imaging;

internal class Program
{
    static void Main()
    {
        Console.WriteLine("Starting clipboard listener ...");

        var clipboardListener = ClipboardListenerFactory.CreateClipboardListener<IWindowsClipboardListener>();
        clipboardListener.ClipboardChanged += ClipboardListener_ClipboardChanged;
        clipboardListener.Start();


        Console.WriteLine("Press Enter to exit...");
        Console.ReadLine();

        clipboardListener.Stop();
    }

    [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
    private static void ClipboardListener_ClipboardChanged(object? sender, WinClipboardChangedEventArgs e)
    {
        Console.WriteLine("Clipboard changed!");

        if (e.DataType == ClipboardDataType.IMAGE && e.ClipboardImage != null)
        {
            try
            {
                // Output width and try and save the file to check that we've got it ok
                Console.WriteLine(e.ClipboardImage?.Width);
                e.ClipboardImage?.Save(@"C:\Temp\clippic.jpg", ImageFormat.Jpeg);
            }
            catch (Exception ex)
            {

            }
        }
    }
}