#include <windows.h>
#include <iostream>
#include <shlobj.h>   // For HDROP (file paths)
#include <vector>
#include <string>

// Define callback types
typedef void (*ClipboardChangedCallback)();
typedef void (*ClipboardChangedCallbackWithData)(const char* data, int type);

// Global variables for storing callbacks
ClipboardChangedCallback g_clipboardCallback = nullptr;
ClipboardChangedCallbackWithData g_callback = nullptr;

enum ClipboardDataType {
    TEXT = 1,
    FILES = 2,
    IMAGE = 3,
    OTHER = 4
};

// Function to retrieve text from the clipboard
std::string GetClipboardText() {
    if (!OpenClipboard(nullptr)) return "";

    HANDLE hData = GetClipboardData(CF_TEXT);
    if (hData == nullptr) return "";

    char* pszText = static_cast<char*>(GlobalLock(hData));
    if (pszText == nullptr) return "";

    std::string text(pszText);
    GlobalUnlock(hData);
    CloseClipboard();
    return text;
}

// Function to retrieve file paths from clipboard
std::string GetClipboardFiles() {
    if (!OpenClipboard(nullptr)) return "";

    HANDLE hDrop = GetClipboardData(CF_HDROP);
    if (hDrop == nullptr) return "";

    HDROP hDropData = (HDROP)hDrop;
    UINT fileCount = DragQueryFile(hDropData, 0xFFFFFFFF, NULL, 0);
    std::string fileList;

    for (UINT i = 0; i < fileCount; i++) {
        char filePath[MAX_PATH];
        DragQueryFileA(hDropData, i, filePath, MAX_PATH);
        fileList += std::string(filePath) + "\n";
    }

    CloseClipboard();
    return fileList;
}


// Function to check if clipboard contains an image
bool IsClipboardImage() {
    return IsClipboardFormatAvailable(CF_BITMAP) || IsClipboardFormatAvailable(CF_DIB);
}

// Callback function for clipboard changes
LRESULT CALLBACK ClipboardProc(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam) {
    if (uMsg == WM_CLIPBOARDUPDATE) {
        if (g_clipboardCallback != nullptr) {
            // Invoke the callback function when the clipboard changes
            g_clipboardCallback();
        }

        // Ensure the callback with data is valid before invoking it
        // ToDo: Don't bother passing back the image data now - we will handle this on the .NET core side
        if (g_callback != nullptr) {
            if (IsClipboardFormatAvailable(CF_TEXT)) {
                std::string text = GetClipboardText();
                g_callback(text.c_str(), TEXT);
            }
            else if (IsClipboardFormatAvailable(CF_HDROP)) {
                std::string files = GetClipboardFiles();
                g_callback(files.c_str(), FILES);
            }
            else if (IsClipboardImage()) {
                g_callback("ClipboardImage", IMAGE);
            }
            else {
                g_callback("UnsupportedType", OTHER);
            }
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

// Function to set the callback for clipboard changes with data and type
extern "C" __declspec(dllexport) void SetClipboardChangedCallbackWithData(ClipboardChangedCallbackWithData callback) {
    g_callback = callback;
}
