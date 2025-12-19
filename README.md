<h1>Clipboard Monitor</h1>
<p>Notification based library for Windows and Linux clipboard monitoring. This monitors the system clipboard for new items being added on a background subscription to fire an callback whenever data is added, optionally (default) providing a reference to the data type and data itself or source.</p>
<p>Note: The package currently only includes Windows and Linux implementation, however Mac implementations will also be included in later releases.</p>
<br/>
<h3>Library Contents and Usage</h3>
<h4>Properties</h4>
<ul>
  <li>IsMonitoring - Flag to indicate whether clipboard is listener is currently active</li>
</ul>
<h4>Methods:</h4>
<ul>
  <li>SetNotificationType(NotificationType) - Sets notification type for clipboard changed with or without data</li>
  <li>Start() - Starts the clipboard monitor for notification type selected</li>
  <li>Stop() - Stops the clipboard monitor, cleaning up callbacks and resources</li>
</ul>
<h4>Events</h4>
<ul>
  <li>ClipboardChanged - Occurs when clipboard monitor has been started and clipboard data is added (event arguments can incldue data)</li>
</ul>
<h4>Sample use:</h4>

```
// Create an instance of Clipboard Listener (relevant implementation for the OS will be created by the factory method)
var clipboardListener = ClipboardListenerFactory.CreateClipboardListener();

// Add callback event handler
clipboardListener.ClipboardChanged += ClipboardListener_OnClipboardChanged;

// Start the listener
clipboardListener.Start();

[...]
```
```
// Event handler for clipboard changed
private void ClipboardListener_ClipboardChanged(object? sender, WinClipboardChangedEventArgs e)
{
  if (e.DataType == ClipboardDataType.TEXT)
  {
    // Do something with clipboard text
  }
  if (e.DataType == ClipboardDataType.FILES)
  {
    // Do something with clipboard files (these include file path)
  }
  if (e.DataType == ClipboardDataType.IMAGE)
  {
    if (e.ClipboardImage != null)
    {
      // Do something with clipboard image (Bitmap - Windows)
    }
    else if (e.ClipboardImage is IClipboardImage clipboardImage)
    {
      // Linux clipboard image - can get data (image byte array) or format (e.g. png), or save such as below
      clipboardImage.Save(Path.Combine("/tmp/ClipboardMonitor/someImage.png");
    }
  }
  else
  {
    // Handle unknown / null type
  }
}
```
