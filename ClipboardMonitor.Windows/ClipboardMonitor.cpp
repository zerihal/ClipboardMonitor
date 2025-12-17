#include <windows.h>
#include <iostream>
#include <shlobj.h>   // For HDROP (file paths)
#include <vector>
#include <string>
#include "ClipboardMonitor.h"

// Global variables for storing callbacks
ClipboardChangedCallback g_clipboardCallback = nullptr;
ClipboardChangedCallbackWithData g_callback = nullptr;

static HWND g_hwnd = nullptr;
static std::atomic<bool> g_running{ false };

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
        // Update: Don't bother passing back the image data now - we will handle this on the .NET core side
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

// *** Note: Exports for the functions below have been moved to the header file ClipboardMonitor.h ***

// Function to start the clipboard listener
void StartClipboardListener() {
	// Prevent multiple instances
    if (g_running)
        return;

    g_running = true;

    // Register the window class
    WNDCLASS wc = {};
    wc.lpfnWndProc = ClipboardProc;
    wc.lpszClassName = L"ClipboardListenerWindow";
    RegisterClass(&wc);

    // Create a hidden window
    g_hwnd = CreateWindowEx(0, L"ClipboardListenerWindow", L"Clipboard Listener", 0, 0, 0, 0, 0, nullptr, nullptr, nullptr, nullptr);

    // Add the clipboard listener
    AddClipboardFormatListener(g_hwnd);

    // Start the message listener (keep listening for events)
    MSG msg;
    while (GetMessage(&msg, nullptr, 0, 0)) {
        TranslateMessage(&msg);
        DispatchMessage(&msg);
    }

	// Cleanup
    RemoveClipboardFormatListener(g_hwnd);
    DestroyWindow(g_hwnd);
    g_hwnd = nullptr;
}

// Function to stop the clipboard listener
void StopClipboardListener() {
	// Set running to false and post a close message to the window
    g_running = false;

    if (g_hwnd) {
        PostMessage(g_hwnd, WM_CLOSE, 0, 0);
    }
}

// Function to set the callback for clipboard changes
void SetClipboardChangedCallback(ClipboardChangedCallback callback) {
    g_clipboardCallback = callback;
}

// Function to set the callback for clipboard changes with data and type
void SetClipboardChangedCallbackWithData(ClipboardChangedCallbackWithData callback) {
    g_callback = callback;
}
