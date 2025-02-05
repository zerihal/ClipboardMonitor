#include <windows.h>
#include <iostream>

// Declare the callback type for the callback with data (takes a const char* parameter)
typedef void(__cdecl* ClipboardChangedCallbackWithData)(const char*, int type);

// Declare the external functions from the DLLs
extern "C" __declspec(dllimport) void StartClipboardListener();
extern "C" __declspec(dllimport) void SetClipboardChangedCallback(void(*callback)());
extern "C" __declspec(dllimport) void SetClipboardChangedCallbackWithData(ClipboardChangedCallbackWithData callback);

// The callback function to be called when the clipboard changes
void OnClipboardChanged() {
    std::cout << "Clipboard content changed (notification only)" << std::endl;
}

// The callback function with clipboard data (now accepts const char* data)
void OnClipboardChangedWithData(const char* clipboardData, int type) {
    if (clipboardData != nullptr) {
        std::cout << "Clipboard content changed with data" << std::endl;

        if (type == 3 || type == 4) {
            std::cout << "Data: Image" << std::endl;
        }
        else {
            std::cout << "Data: " << clipboardData << std::endl;
        }        
    }
    else {
        std::cout << "Clipboard content changed, but no data available!" << std::endl;
    }
}

int main() {
    // Set the callbacks for clipboard changes
    std::cout << "Setting callback for clipboard change notification..." << std::endl;
    SetClipboardChangedCallback(OnClipboardChanged);

    // Set the callback for clipboard changes with data
    std::cout << "Setting callback for clipboard change with data..." << std::endl;
    SetClipboardChangedCallbackWithData(OnClipboardChangedWithData);

    // Check if callbacks are set correctly (for debugging)
    std::cout << "Callbacks set. Starting clipboard listener..." << std::endl;

    // Start the clipboard listener from the DLL
    StartClipboardListener();

    return 0;
}
