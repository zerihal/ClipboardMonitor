using ClipboardMonitor.Core.ClipboardListenerImp;
using ClipboardMonitor.Core.Enums;
using ClipboardMonitor.Core.EventArguments;
using NetCoreTestApp;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

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

    private static void ClipboardListener_ClipboardChanged(object? sender, ClipboardChangedEventArgs e)
    {
        Console.WriteLine("Clipboard changed!");

        if (e.DataType == ClipboardDataType.IMAGE && e.ClipboardImageData != null)
        {
            // Try and convert to a bitmap and save
            try
            {
                //var clipboardImage = GetBitmapFromClipboard();
                //if (clipboardImage != null)
                //{
                //    clipboardImage.Save(@"C:\Temp\ClipTest.jpg", ImageFormat.Jpeg);
                //    Console.WriteLine("Image saved successfully.");
                //}
                //else
                //{
                //    Console.WriteLine("Failed to extract image from clipboard.");
                //}
            }
            catch (Exception ex)
            {

            }
        }
    }

    private static Bitmap GetBitmapFromClipboard()
    {
        Bitmap bitmap = null;

        if (NativeMethods.IsClipboardFormatAvailable(NativeMethods.CF_BITMAP))
        {
            if (NativeMethods.OpenClipboard(IntPtr.Zero))
            {
                try
                {
                    IntPtr hBitmap = NativeMethods.GetClipboardData(NativeMethods.CF_BITMAP);
                    if (hBitmap != IntPtr.Zero)
                    {
                        // Create .NET Bitmap from GDI bitmap handle
                        bitmap = Image.FromHbitmap(hBitmap);

                        // Ensure the GDI object is released
                        NativeMethods.DeleteObject(hBitmap);
                    }
                }
                finally
                {
                    NativeMethods.CloseClipboard();
                }
            }
        }
        return bitmap;
    }
}