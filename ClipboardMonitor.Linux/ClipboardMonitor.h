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
        TEXT = 1,
        FILES = 2,
        IMAGE = 3,
        NONE = 4
    } ClipboardDataType;

    // Callback setters
    void SetClipboardChangedCallback(ClipboardChangedCallback callback);
    void SetClipboardChangedCallbackWithData(ClipboardChangedCallbackWithData callback);

#ifdef __cplusplus
}
#endif