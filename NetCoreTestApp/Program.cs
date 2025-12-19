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
        Console.WriteLine("*** Windows .NET Core Test App ***");
        IClipboardListener? clipboardListener = null;

        try
        {
            clipboardListener = ClipboardListenerFactory.CreateClipboardListener();

            // Verify that we have a Windows clipboard listener
            if (clipboardListener is IWindowsClipboardListener)
            {
                Console.WriteLine("Windows Clipboard Listener created. Starting listener ...");
                clipboardListener.ClipboardChanged += ClipboardListener_ClipboardChanged;
                clipboardListener.Start();
            }
            else
            {
                Console.WriteLine("Error: Created clipboard listener is not a Windows clipboard listener.");
                clipboardListener = null;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }

        Console.WriteLine("Press Enter to exit...");
        Console.ReadLine();

        clipboardListener?.Stop();
    }

    [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
    private static void ClipboardListener_ClipboardChanged(object? sender, ClipboardChangedEventArgs e)
    {
        Console.WriteLine("Clipboard changed!");

        if (e.DataType == ClipboardDataType.IMAGE && e.BmpClipboardImage != null)
        {
            try
            {
                // Output width and try and save the file to check that we've got it ok
                Console.WriteLine(e.BmpClipboardImage?.Width);
                e.BmpClipboardImage?.Save(@"C:\Temp\clippic.jpg", ImageFormat.Jpeg);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error saving clipboard image: " + ex.Message);
            }
        }
    }
}