#include <windows.h>
#include <iostream>

// Defines a function pointer type for the callback
typedef void (*ClipboardChangedCallback)();

// Declares a global variable to store the callback function
ClipboardChangedCallback g_clipboardCallback = nullptr;

// Callback function for clipboard changes
LRESULT CALLBACK ClipboardProc(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam) {
    if (uMsg == WM_CLIPBOARDUPDATE) {
        if (g_clipboardCallback != nullptr) {
            // Invoke the callback function when the clipboard changes
            g_clipboardCallback();
        }
    }
    return DefWindowProc(hwnd, uMsg, wParam, lParam);
}

// Function to start the clipboard listener
extern "C" __declspec(dllexport) void StartClipboardListener() {
    // Register the window class
    WNDCLASS wc = { 0 };
    wc.lpfnWndProc = ClipboardProc;
    wc.lpszClassName = L"ClipboardListenerWindow";
    RegisterClass(&wc);

    // Create a hidden window
    HWND hwnd = CreateWindowEx(0, L"ClipboardListenerWindow", L"Clipboard Listener", 0, 0, 0, 0, 0, NULL, NULL, NULL, NULL);

    // Add the clipboard listener
    AddClipboardFormatListener(hwnd);

    // Start the message listener (keep listening for events)
    MSG msg;
    while (GetMessage(&msg, NULL, 0, 0)) {
        TranslateMessage(&msg);
        DispatchMessage(&msg);
    }
}

// Function to set the callback for clipboard changes
extern "C" __declspec(dllexport) void SetClipboardChangedCallback(ClipboardChangedCallback callback) {
    g_clipboardCallback = callback;
}


