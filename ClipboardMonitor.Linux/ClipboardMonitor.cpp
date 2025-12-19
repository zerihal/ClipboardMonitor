#include <X11/Xlib.h>
#include <X11/Xatom.h>
#include <cstring>
#include <iostream>
#include <fstream>
#include <thread>
#include <chrono>
#include <vector>
//#include <openssl/sha.h>  // For SHA256 hashing
#include <iomanip>
#include <sstream>
#include <atomic>
#include "ClipboardMonitor.h"
#include "sha256.h"

// Global variables for storing callbacks
ClipboardChangedCallback g_clipboardCallback = nullptr;
ClipboardChangedCallbackWithData g_callback = nullptr;

class ClipboardListener {
public:
    ClipboardListener() : running_(true) {}

    void monitorClipboard() {
        Display* display = XOpenDisplay(nullptr);
        if (!display) {
            std::cerr << "Failed to open X display." << std::endl;
            return;
        }

        Atom utf8 = XInternAtom(display, "UTF8_STRING", False);
        Atom uriList = XInternAtom(display, "text/uri-list", False);
        Atom property = XInternAtom(display, "XSEL_DATA", False);

        std::vector<Atom> imageTargets = {
            XInternAtom(display, "image/png", False),
            XInternAtom(display, "image/bmp", False),
            XInternAtom(display, "image/jpeg", False),
            XInternAtom(display, "image/x-png", False)
        };

        std::string lastGlobalHash;

        while (running_) {
            std::vector<unsigned char> imageData;
            std::string textData;
            std::string uriData;
            std::string content; // content passed to callback
            size_t dataSize = 0;
            ClipboardDataType dataType = NONE;

            // --- Try image formats first ---
            for (Atom target : imageTargets) {
                imageData.clear();
                getClipboardContent(display, target, property, imageData);
                if (!imageData.empty()) {
                    content.assign(reinterpret_cast<const char*>(imageData.data()), imageData.size());
                    dataSize = imageData.size();
                    dataType = IMAGE;
                    break; // stop at first available image format
                }
            }

            // --- Try URI if no image ---
            if (dataType == NONE) {
                std::vector<unsigned char> dummy;
                uriData = getClipboardContent(display, uriList, property, dummy);
                if (!uriData.empty()) {
                    content = uriData;
                    dataSize = content.size();
                    dataType = FILES;
                }
            }

            // --- Try UTF-8 text if no image or URI ---
            if (dataType == NONE) {
                std::vector<unsigned char> dummy;
                textData = getClipboardContent(display, utf8, property, dummy);
                if (!textData.empty()) {
                    content = textData;
                    dataSize = content.size();
                    dataType = TEXT;
                }
            }

            // --- Compute global hash to deduplicate across formats ---
            std::string combinedHash;
            combinedHash += std::string(imageData.begin(), imageData.end());
            combinedHash += uriData;
            combinedHash += textData;
            std::string currentGlobalHash = SHA256::hash(combinedHash);

            if (dataType != NONE && !currentGlobalHash.empty() && currentGlobalHash != lastGlobalHash) {
                lastGlobalHash = currentGlobalHash;

                // --- Trigger callback only once per logical copy ---
                if (g_clipboardCallback != nullptr) {
                    g_clipboardCallback();
                }

                if (g_callback != nullptr) {
                    g_callback(content.c_str(), dataSize, dataType);
                }
            }

            std::this_thread::sleep_for(std::chrono::milliseconds(100));
        }

        XCloseDisplay(display);
    }


    void stop() {
        running_ = false;
    }

private:
    std::string getClipboardContent(Display* display, Atom targetAtom, Atom propertyAtom, std::vector<unsigned char>& outBinary) {
        Window window = XCreateSimpleWindow(display, DefaultRootWindow(display), 0, 0, 1, 1, 0, 0, 0);
        Atom clipboard = XInternAtom(display, "CLIPBOARD", False);
        Atom imagePng = XInternAtom(display, "image/png", False);
        Atom imageBmp = XInternAtom(display, "image/bmp", False);
        Atom imageJpeg = XInternAtom(display, "image/jpeg", False);
        Atom imageXPng = XInternAtom(display, "image/x-png", False);

        XConvertSelection(display, clipboard, targetAtom, propertyAtom, window, CurrentTime);
        XFlush(display);

        XEvent event;
        auto start = std::chrono::steady_clock::now();
        const int timeoutMs = 100; // 100 ms timeout

        while (true) {
            // Check if there is a pending event
            if (XPending(display)) {
                XNextEvent(display, &event);
                if (event.type == SelectionNotify && event.xselection.selection == clipboard) {
                    Atom actual_type;
                    int actual_format;
                    unsigned long nitems, bytes_after;
                    unsigned char* prop = nullptr;

                    int result = XGetWindowProperty(display, window, propertyAtom, 0, (~0L), False,
                        AnyPropertyType, &actual_type, &actual_format,
                        &nitems, &bytes_after, &prop);

                    if (result == Success && prop) {
                        std::string str;

                        bool isImage =
                            actual_type == imagePng ||
                            actual_type == imageBmp ||
                            actual_type == imageJpeg ||
                            actual_type == imageXPng;

                        if (isImage) {
                            outBinary.assign(prop, prop + nitems);
                        }
                        else {
                            str.assign(reinterpret_cast<char*>(prop), nitems);
                        }

                        //if (targetAtom == XInternAtom(display, "image/png", False)) {
                        //    outBinary.assign(prop, prop + nitems);
                        //}
                        //else {
                        //    str.assign(reinterpret_cast<char*>(prop), nitems);
                        //}
                        XFree(prop);
                        XDestroyWindow(display, window);
                        return str;
                    }
                    break;
                }
            }

            // Check timeout
            auto now = std::chrono::steady_clock::now();
            if (std::chrono::duration_cast<std::chrono::milliseconds>(now - start).count() > timeoutMs) {
                break; // exit loop on timeout
            }

            std::this_thread::sleep_for(std::chrono::milliseconds(1));
        }

        XDestroyWindow(display, window);
        return "";
    }


private:
    ClipboardChangedCallback callback_;
    std::atomic<bool> running_;
};

// Clipboard listener
ClipboardListener* g_listener = nullptr;

// Expose a function to start the clipboard listener.
// This call blocks and runs indefinitely until StopClipboardListener() is called externally.
extern "C" __attribute__((visibility("default"))) void StartClipboardListener() {
    if (!g_listener) {
        g_listener = new ClipboardListener();
        g_listener->monitorClipboard(); // Blocking call
    }
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

    if (g_listener) {
        // Ensure callbacks are removed
        g_clipboardCallback = nullptr;
        g_callback = nullptr;

        // Stop listener and clean up
        g_listener->stop();
        delete g_listener;
        g_listener = nullptr;
    }
}