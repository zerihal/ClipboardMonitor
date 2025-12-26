#pragma once

#include <stddef.h>

#ifdef __cplusplus
extern "C" {
#endif

    // Callback types
    typedef void (*ClipboardChangedCallback)();
    typedef void (*ClipboardChangedCallbackWithData)(const char* data, size_t dataSize, int type);

    // Enum for clipboard data types
    typedef enum ClipboardDataType {
		NONE = 0,
        TEXT = 1,
        FILES = 2,
        IMAGE = 3,
        OTHER = 4,
        CLEARED = 5
    } ClipboardDataType;

    // Callback setters
    void SetClipboardChangedCallback(ClipboardChangedCallback callback);
    void SetClipboardChangedCallbackWithData(ClipboardChangedCallbackWithData callback);

#ifdef __cplusplus
}
#endif