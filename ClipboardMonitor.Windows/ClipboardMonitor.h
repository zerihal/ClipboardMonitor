#pragma once

#ifdef _WIN32
#ifdef CLIPBOARD_EXPORTS
#define CLIPBOARD_API extern "C" __declspec(dllexport)
#else
#define CLIPBOARD_API extern "C" __declspec(dllimport)
#endif
#else
#define CLIPBOARD_API
#endif

#ifdef __cplusplus
extern "C" {
#endif

	// Callback types definitions
    typedef void (*ClipboardChangedCallback)();
    typedef void (*ClipboardChangedCallbackWithData)(const char* data, int type);

	// Enum for clipboard data types (clear is currently done from .NET side, but included for completeness in case moved later)
    typedef enum ClipboardDataType {
		NONE = 0,
        TEXT = 1,
        FILES = 2,
        IMAGE = 3,
		OTHER = 4,
		CLEARED = 5
    } ClipboardDataType;

    // Methods and callback setters
    CLIPBOARD_API void StartClipboardListener();
    CLIPBOARD_API void StopClipboardListener();
	CLIPBOARD_API bool ClearClipboard();
    CLIPBOARD_API void SetClipboardChangedCallback(ClipboardChangedCallback callback);
    CLIPBOARD_API void SetClipboardChangedCallbackWithData(ClipboardChangedCallbackWithData callback);

#ifdef __cplusplus
}
#endif