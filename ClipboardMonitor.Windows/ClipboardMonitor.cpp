#include <windows.h>
#include <iostream>
#include <shlobj.h>   // For HDROP (file paths)
#include <vector>
#include <string>

// Define callback types
typedef void (*ClipboardChangedCallback)();
typedef void (*ClipboardChangedCallbackWithData)(const char* data, int type);
typedef void (*ClipboardChangedCallbackWithImage)(const BYTE* data, size_t length, int type);

// Global variables for storing callbacks
ClipboardChangedCallback g_clipboardCallback = nullptr;
ClipboardChangedCallbackWithData g_text_callback = nullptr;
ClipboardChangedCallbackWithImage g_image_callback = nullptr;

enum ClipboardDataType {
    TEXT = 1,
    FILES = 2,
    IMG_BITMAP = 3,
    IMG_DIB = 4
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

std::vector<BYTE> GetClipboardBitmapData() {
    std::vector<BYTE> imageData;

    if (!OpenClipboard(nullptr)) return imageData;

    HANDLE hData = GetClipboardData(CF_BITMAP);  // Get the CF_BITMAP format
    if (hData == nullptr) return imageData;

    HBITMAP hBitmap = (HBITMAP)hData;
    if (hBitmap == nullptr) return imageData;

    // Create a device context to work with the bitmap
    HDC hdc = GetDC(nullptr);
    if (hdc == nullptr) return imageData;

    // Get the bitmap information
    BITMAP bmp;
    if (GetObject(hBitmap, sizeof(bmp), &bmp) == 0) {
        ReleaseDC(nullptr, hdc);
        return imageData;
    }

    // Prepare a buffer for the bitmap data
    BITMAPINFO bmi;
    memset(&bmi, 0, sizeof(bmi));
    bmi.bmiHeader.biSize = sizeof(BITMAPINFOHEADER);
    bmi.bmiHeader.biWidth = bmp.bmWidth;
    bmi.bmiHeader.biHeight = bmp.bmHeight;
    bmi.bmiHeader.biPlanes = 1;
    bmi.bmiHeader.biBitCount = 24;  // Assuming 24 bits per pixel
    bmi.bmiHeader.biCompression = BI_RGB;

    // Allocate memory to hold the bitmap bits
    int size = bmp.bmWidthBytes * bmp.bmHeight;
    imageData.resize(size);

    // Get the bitmap data into the imageData vector
    if (GetDIBits(hdc, hBitmap, 0, bmp.bmHeight, imageData.data(), &bmi, DIB_RGB_COLORS) == 0) {
        imageData.clear();  // If getting the data failed
    }

    // Clean up
    ReleaseDC(nullptr, hdc);
    CloseClipboard();

    return imageData;
}

// Function to extract image (CF_DIB) from clipboard and return it as binary data
std::vector<BYTE> GetClipboardImageData() {
    std::vector<BYTE> imageData;

    if (!OpenClipboard(nullptr)) return imageData;

    HANDLE hData = GetClipboardData(CF_DIB);  // Get the DIB format data
    if (hData == nullptr) return imageData;

    // Lock the data and get the raw bytes
    void* pData = GlobalLock(hData);
    if (pData == nullptr) return imageData;

    // Copy data into vector (binary)
    DWORD size = GlobalSize(hData);
    imageData.resize(size);
    memcpy(imageData.data(), pData, size);

    GlobalUnlock(hData);
    CloseClipboard();
    return imageData;
}

// Function to handle clipboard image (CF_BITMAP) format
void HandleClipboardImage(ClipboardDataType type) {
    if (g_image_callback != nullptr) {
        std::vector<BYTE> imgData = type == IMG_DIB ? GetClipboardImageData() : GetClipboardBitmapData();

        if (!imgData.empty()) {
            g_image_callback(imgData.data(), imgData.size(), type);
        }
        else {
            g_text_callback("No image data", type);
        }
    }
}

// Callback function for clipboard changes
LRESULT CALLBACK ClipboardProc(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam) {
    if (uMsg == WM_CLIPBOARDUPDATE) {
        if (g_clipboardCallback != nullptr) {
            // Invoke the callback function when the clipboard changes
            g_clipboardCallback();
        }

        // Ensure the callback with data is valid before invoking it
        if (g_text_callback != nullptr) {
            if (IsClipboardFormatAvailable(CF_TEXT)) {
                std::string text = GetClipboardText();
                g_text_callback(text.c_str(), TEXT);
            }
            else if (IsClipboardFormatAvailable(CF_HDROP)) {
                std::string files = GetClipboardFiles();
                g_text_callback(files.c_str(), FILES);
            }
            else if (IsClipboardFormatAvailable(CF_BITMAP)) {
                //g_callback("Clipboard contains an image", IMAGE);
                HandleClipboardImage(IMG_BITMAP);
            }
            else if (IsClipboardFormatAvailable(CF_DIB)) {
                HandleClipboardImage(IMG_DIB);
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
    g_text_callback = callback;
}

// Register the image callback
extern "C" __declspec(dllexport) void SetClipboardChangedCallbackWithImage(ClipboardChangedCallbackWithImage callback) {
    g_image_callback = callback;
}
