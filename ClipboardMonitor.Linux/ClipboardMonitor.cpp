#include <iostream>
#include <X11/Xlib.h>
#include <X11/Xatom.h>
#include <X11/Xutil.h>
#include <X11/keysym.h>
#include <cstring>

// *** NOTE: THIS REQUIRES REMOTE CONNECTION TO LINUX MACHINE TO BE CONFIGURED OR WSL (IN BUILDING IN WINDOWS) TO COMPILE ***

// Define callback types
typedef void (*ClipboardChangedCallback)();
typedef void (*ClipboardChangedCallbackWithData)(const char* data, int type);

// Global variables for storing callbacks
ClipboardChangedCallback g_clipboardCallback = nullptr;
ClipboardChangedCallbackWithData g_callback = nullptr;

enum ClipboardDataType {
    TEXT = 1,
    FILES = 2,
    IMAGE = 3
};

class ClipboardListener {
public:
    ClipboardListener(ClipboardChangedCallback callback) : callback_(callback) {
        // Initialize the X11 display
        display_ = XOpenDisplay(NULL);
        if (!display_) {
            std::cerr << "Cannot open X11 display" << std::endl;
            exit(1);
        }

        // Create a window to listen to events
        window_ = XCreateSimpleWindow(display_, DefaultRootWindow(display_), 0, 0, 1, 1, 0, 0, 0);
        XSelectInput(display_, window_, PropertyChangeMask);

        // Start listening for events
        XMapWindow(display_, window_);
    }

    ~ClipboardListener() {
        if (display_) {
            XCloseDisplay(display_);
        }
    }

    void start() {
        // Enter an infinite loop to listen for events
        while (true) {
            XEvent event;
            XNextEvent(display_, &event);

            if (event.type == PropertyNotify) {
                if (event.xproperty.state == ColormapNotify) {
                    continue;
                }

                // Check if the clipboard has been updated
                if (event.xproperty.atom == XA_PRIMARY || event.xproperty.atom == XA_SECONDARY) {
                    checkClipboard();
                }
            }
        }
    }

private:
    void checkClipboard() {
        // Get clipboard content from the primary selection
        Atom clipboardAtom = XInternAtom(display_, "CLIPBOARD", False);
        Atom utf8StringAtom = XInternAtom(display_, "UTF8_STRING", False);
        Atom type;
        int format;
        unsigned long nItems, bytesAfter;
        unsigned char* data = nullptr;

        int result = XGetWindowProperty(display_, window_, clipboardAtom, 0, (~0L), False, AnyPropertyType,
            &type, &format, &nItems, &bytesAfter, &data);

        if (result == Success && data) {
            if (g_clipboardCallback != nullptr) {
                // Invoke the callback function when the clipboard changes
                g_clipboardCallback();
            }

            if (g_callback != nullptr) {
                // Check if the data is text, files, or image based on the Atom type
                if (type == utf8StringAtom) {
                    // Handle text clipboard data
                    std::string clipboardData(reinterpret_cast<char*>(data), nItems);
                    g_callback(clipboardData.c_str(), TEXT);
                }
                else if (type == XInternAtom(display_, "TEXT/URI_LIST", False)) {
                    // Handle file URIs
                    std::string clipboardData(reinterpret_cast<char*>(data), nItems);
                    g_callback(clipboardData.c_str(), FILES);
                }
                else if (type == XInternAtom(display_, "image/png", False) ||
                    type == XInternAtom(display_, "image/jpeg", False) ||
                    type == XInternAtom(display_, "image/gif", False) ||
                    type == XInternAtom(display_, "image/bmp", False) ||
                    type == XInternAtom(display_, "image/tiff", False)) {
                    // Handle known image clipboard data (PNG, JPEG, GIF, BMP, TIFF)
                    std::string clipboardData(reinterpret_cast<char*>(data), nItems);
                    g_callback(clipboardData.c_str(), IMAGE);
                }
                else if (type == XInternAtom(display_, "image/*", False)) {
                    // Handle other image types (if supported by X11)
                    std::string clipboardData(reinterpret_cast<char*>(data), nItems);
                    g_callback(clipboardData.c_str(), IMAGE);
                }
                else {
                    // If the image type is not recognized, handle it as an unsupported type
                    std::cerr << "Unsupported image type on clipboard" << std::endl;
                }
            }

            XFree(data);
        }
    }

private:
    Display* display_;
    Window window_;
    ClipboardChangedCallback callback_;
};

// Expose a function to start the clipboard listener
extern "C" __attribute__((visibility("default"))) void StartClipboardListener(ClipboardChangedCallback callback) {
    ClipboardListener* listener = new ClipboardListener(callback);
    listener->start(); // This will block, so it will run indefinitely
}

// Function to set the callback for clipboard changes
extern "C" __attribute__((visibility("default"))) void SetClipboardChangedCallback(ClipboardChangedCallback callback) {
    g_clipboardCallback = callback;
}

// Function to set the callback for clipboard changes with data and type
extern "C" __attribute__((visibility("default"))) void SetClipboardChangedCallbackWithData(ClipboardChangedCallbackWithData callback) {
    g_callback = callback;
}

// Expose a function to stop the listener (just to clean up later if needed)
extern "C" __attribute__((visibility("default"))) void StopClipboardListener() {
    // You could add a mechanism here to cleanly stop the listener if needed
    std::cout << "Stopping clipboard listener..." << std::endl;
}
