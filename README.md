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

// Stop the listener and cleanup
clipboardListener.Stop();
clipboardListener.ClipboardChanged -= ClipboardListener_OnClipboardChanged;
```
```
// Event handler for clipboard changed
private void ClipboardListener_ClipboardChanged(object? sender, ClipboardChangedEventArgs e)
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
### Linux Prerequisites

The Linux native binary (`libClipboardMonitor.Linux.so`) depends on X11 for clipboard access. 

Before using the package on Linux, ensure the following libraries are installed:

- Debian / Ubuntu:
  ```bash
  sudo apt-get install libx11-dev
- Fedora / RHEL:
  ```bash
  sudo dnf install libX11-devel

To use the ```ClearClipboardContent()``` method, also ensure that xclip is installed:

- Debian / Ubuntu:
  ```bash
  sudo apt install xclip
- Fedora / RHEL:
  ```bash
  sudo dnf install xclip

<h4>Building the Linux native assembly (ClipboardMonitor.Linux):</h4>

Prerequisites

* Linux environment (remote Linux machine or WSL2 on Windows)
* C++ compiler supporting C++11 or higher (g++)
* X11 development libraries

If building from Windows (Visual Studio), in Project Properties > Configuration Properties > General for ClipboardMonitor.Linux, set Remote Build Machine details for the Linux environment to build the .so file. If using the pre-built .so file from this repo then exclude this project from build when building the solution.
