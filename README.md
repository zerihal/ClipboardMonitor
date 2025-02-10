<h1>Clipboard Monitor</h1>
<p>Notification based library for Windows clipboard monitoring. This monitors the system clipboard for new items being added on a background subscription to fire an callback whenever data is added, optionally (default) providing a reference to the data type and data itself or source.</p>
<p>Note: The package currently only include Windows implementation, however Linux and Mac implementations will also be included in later releases.</p>
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
<h4>Events (Windows implementation)</h4>
<ul>
  <li>ClipboardChanged - Occurs when clipboard monitor has been started and clipboard data is added (event arguments can incldue data)</li>
</ul>
<h4>Sample use:</h4>

```
// Create an instance of Clipboard Listener
var clipboardListener = ClipboardListenerFactory.CreateClipboardListener<IWindowsClipboardListener>();

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
  if (e.DataType == ClipboardDataType.IMAGE && e.ClipboardImage != null)
  {
    // Do something with clipboard image (Bitmap)
  }
  else
  {
    // Handle unknown / null type
  }
}
```
