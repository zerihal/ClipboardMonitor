#pragma once
#include <windows.h>

// Define the function signature for the callback
typedef void (*ClipboardChangedCallback)();

#ifdef __cplusplus
extern "C" {
#endif

    // Export the SetClipboardChangedCallback function
    __declspec(dllexport) void SetClipboardChangedCallback(ClipboardChangedCallback callback);

    // Export the StartClipboardMonitoring function
    __declspec(dllexport) void StartClipboardMonitoring();

#ifdef __cplusplus
}
#endif
