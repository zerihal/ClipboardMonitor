// ClipboardMonitor.Windows.TestApp.cpp - used to test the ClipboardMonitor.Windows assembly

#include <windows.h>
#include <iostream>

// Declare the external function from the DLL
extern "C" __declspec(dllimport) void StartClipboardListener();
extern "C" __declspec(dllimport) void SetClipboardChangedCallback(void(*callback)());

// The callback function to be called when the clipboard changes
void OnClipboardChanged() {
    std::cout << "Clipboard content changed!" << std::endl;
}

int main() {
    SetClipboardChangedCallback(OnClipboardChanged);

    // Start the clipboard listener from the DLL
    std::cout << "Starting clipboard listener..." << std::endl;
    StartClipboardListener();

    return 0;
}
