#pragma once

#include "atlpath.h"
#include "atlstr.h"

// Requires Windows 10 v1803 or higher to compile
#include "ShellScalingApi.h"
#include "WinDef.h"
#include "WinUser.h"

#pragma region Registry path defines

// Defines for the .NET setup registry key used to determine if the current version has the required
// functionality and behaviors needed for Per-Monitor DPI awareness to be enabled.
#define DOTNET_SETUP_KEYNAME L"SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v4\\Full"
#define DOTNET_SETUP_VERSION L"Version"

#pragma endregion Registry path defines

namespace VsUI
{
    // This class contains multi-/mixed-DPI mode helpers for setting the DPI context of the process
    // or the calling thread.
    class CDpiAwareness
    {
    public:
        // Checks whether the appid can turn on Per-Monitor DPI awareness for the process.
        static bool IsPerMonitorDpiAwarenessAvailable()
        {
            if (!s_isAvailableChecked)
            {
                Initialize();

                s_isAvailable        = IsWindowsSupportedVersion() && IsDotNetSupportedVersion();
                s_isAvailableChecked = true;
            }

            return s_isAvailable;
        }

        // Checks whether Per-Monitor DPI awareness is enabled for the process.
        static bool IsPerMonitorDpiAwarenessEnabled()
        {
            if (!s_isPerMonitorAwarenessChecked)
            {
                Initialize();

                PROCESS_DPI_AWARENESS processAwareness;

                // If NULL is passed to GetProcessDpiAwareness, the current process is used
                if (s_pGetPDAC_Win81 && (s_pGetPDAC_Win81(NULL, &processAwareness) == S_OK))
                    s_isPerMonitorAwarenessEnabled = (processAwareness == PROCESS_DPI_AWARENESS::PROCESS_PER_MONITOR_DPI_AWARE);

                s_isPerMonitorAwarenessChecked = true;
            }

            return IsPerMonitorDpiAwarenessAvailable() && s_isPerMonitorAwarenessEnabled;
        }

        #pragma region Get DPI methods

        // Gets the effective DPI for the given monitor. The DPI for a monitor is returned as both
        // vertical and horizontal values as out parameters.
        static HRESULT GetDpiForMonitor(_In_ HMONITOR hMonitor, _Out_ UINT *pDpiX, _Out_ UINT *pDpiY)
        {
            if (hMonitor == nullptr)
                return E_INVALIDARG;

            if ((pDpiX == nullptr) ||
                (pDpiY == nullptr))
                return E_POINTER;

            if (!CanGetMonitorDpi())
            {
                *pDpiX = s_DeviceDpiX;
                *pDpiY = s_DeviceDpiY;
                return S_OK;
            }

            return s_pGetDFM(hMonitor, MONITOR_DPI_TYPE::MDT_EFFECTIVE_DPI, pDpiX, pDpiY);
        }

        // Gets the DPI for the given HWND. The DPI for a window is returned as both vertical and
        // horizontal values as out parameters.
        static HRESULT GetDpiForWindow(_In_ HWND hwnd, _Out_ UINT *pDpiX, _Out_ UINT *pDpiY)
        {
            if (hwnd == nullptr)
                return E_INVALIDARG;

            if ((pDpiX == nullptr) ||
                (pDpiY == nullptr))
                return E_POINTER;

            if (!CanGetWindowDpi())
            {
                *pDpiX = s_DeviceDpiX;
                *pDpiY = s_DeviceDpiY;
                return S_OK;
            }

            UINT dpi = s_pGetDFW(hwnd);
            if (dpi == 0)
                return HRESULT_FROM_WIN32(GetLastError());

            *pDpiX = dpi;
            *pDpiY = dpi;
            return S_OK;
        }

        #pragma endregion Get DPI methods

        #pragma region Device to logical conversion methods

        // Converts a POINT from device units to logical units.
        static HRESULT DeviceToLogicalPoint(_In_ HWND hwnd, _Inout_ POINT *pPoint)
        {
            return ConvertPoint(hwnd, ConversionDirection::DeviceToLogical, pPoint);
        }

        // Converts a RECT from device units to logical units.
        static HRESULT DeviceToLogicalRect(_In_ HWND hwnd, _Inout_ RECT *pRect)
        {
            return ConvertRect(hwnd, ConversionDirection::DeviceToLogical, pRect);
        }

        // Converts a SIZE from device units to logical units.
        static HRESULT DeviceToLogicalSize(_In_ HWND hwnd, _Inout_ SIZE *pSize)
        {
            return ConvertSize(hwnd, ConversionDirection::DeviceToLogical, pSize);
        }

        // Converts an x-coordinate (or horizontal) value from device units to logical units.
        static HRESULT DeviceToLogicalUnitsX(_In_ HWND hwnd, _Inout_ int *pValue)
        {
            return ConvertUnits(hwnd, ConversionDirection::DeviceToLogical, Orientation::Horizontal, pValue);
        }

        // Converts a y-coordinate (or vertical) value from device units to logical units.
        static HRESULT DeviceToLogicalUnitsY(_In_ HWND hwnd, _Inout_ int *pValue)
        {
            return ConvertUnits(hwnd, ConversionDirection::DeviceToLogical, Orientation::Vertical, pValue);
        }

        #pragma endregion Device to logical conversion methods

        #pragma region Logical to device conversion methods

        // Converts a POINT from logical units to device units.
        static HRESULT LogicalToDevicePoint(_In_ HWND hwnd, _Inout_ POINT *pPoint)
        {
            return ConvertPoint(hwnd, ConversionDirection::LogicalToDevice, pPoint);
        }

        // Converts a RECT from logical units to device units.
        static HRESULT LogicalToDeviceRect(_In_ HWND hwnd, _Inout_ RECT *pRect)
        {
            return ConvertRect(hwnd, ConversionDirection::LogicalToDevice, pRect);
        }

        // Converts a SIZE from logical units to device units.
        static HRESULT LogicalToDeviceSize(_In_ HWND hwnd, _Inout_ SIZE *pSize)
        {
            return ConvertSize(hwnd, ConversionDirection::LogicalToDevice, pSize);
        }

        // Converts an x-coordinate (or horizontal) value from logical units to device units.
        static HRESULT LogicalToDeviceUnitsX(_In_ HWND hwnd, _Inout_ int *pValue)
        {
            return ConvertUnits(hwnd, ConversionDirection::LogicalToDevice, Orientation::Horizontal, pValue);
        }

        // Converts a y-coordinate (or vertical) value from logical units to device units.
        static HRESULT LogicalToDeviceUnitsY(_In_ HWND hwnd, _Inout_ int *pValue)
        {
            return ConvertUnits(hwnd, ConversionDirection::LogicalToDevice, Orientation::Vertical, pValue);
        }

        #pragma endregion Logical to device conversion methods

        #pragma region DPI awareness methods

        // Gets the DPI awareness context from the given HWND. This method should not be called
        // directly and the CDpiScope class should be used to handle switching the thread DPI
        // context to match the given HWND and to make sure the thread's DPI context is returned to
        // its original state upon leaving the DPI scope.
        //
        // This method will return NULL if GetWindowDpiAwarenessContext is not supported in the
        // current environment, or it will return the DPI context associated with the HWND.
        static DPI_AWARENESS_CONTEXT GetWindowDpiAwarenessContext(_In_ HWND hwnd)
        {
            // If GetWindowDpiAwarenessContext is not supported, mimic the
            // SetThreadDpiAwarenessContext method's error handling and return NULL.
            if (!CanGetWindowDpiContext())
                return NULL;

            return s_pGetWDAC(hwnd);
        }

        // The process DPI context can only be set once and only before the first HWND is created.
        // The appid will do the calculation to determine when and how to call this. All other calls
        // to this method after the process DPI context has been set will fail.
        //
        // This method will return false if SetProcessDpiAwarenessContext is not supported in the
        // current environment, when trying to set Per-Monitor V2 awareness mode when it's not
        // supported, or if it has already been called before.
        static bool SetProcessDpiAwarenessContext(_In_ DPI_AWARENESS_CONTEXT dpiContext)
        {
            if (!CanSetProcessDpiContext())
                return false;

            if (s_pSetPDAC)
            {
                return s_pSetPDAC(dpiContext);
            }
            else
            {
                PROCESS_DPI_AWARENESS dpiContext_Win81 = PROCESS_DPI_AWARENESS::PROCESS_DPI_UNAWARE;

                // Don't convert the PMA values since they aren't supported on Win8.1
                if (dpiContext == DPI_AWARENESS_CONTEXT_SYSTEM_AWARE)
                    dpiContext_Win81 = PROCESS_DPI_AWARENESS::PROCESS_SYSTEM_DPI_AWARE;

                return (s_pSetPDAC_Win81(dpiContext_Win81) == S_OK);
            }
        }

        // The DPI host behavior determines if a piece of parent UI with one DPI context can host a
        // piece of child UI with a different DPI context.
        //
        // This method will return false if SetThreadDpiHostingBehavior is not supported in the
        // current environment, or if the requested hosting behavior is unsupported/invalid.
        static bool SetProcessDpiHostingMode(DPI_HOSTING_BEHAVIOR dpiHostingBehavior)
        {
            if (!CanSetThreadHosting())
                return false;

            return s_pSetTDHB(dpiHostingBehavior) != DPI_HOSTING_BEHAVIOR::DPI_HOSTING_BEHAVIOR_INVALID;
        }

        // The thread DPI context is normally changed by Windows prior to sending te process messages
        // via the message loop. If however some non-message loop code path needs to interact with
        // HWNDs or coordinates that are in different DPI contexts, then this method can be used to
        // change the thread context. This method should not be called directly, and the CDpiScope
        // class should be used to make sure the thread's DPI context is returned to its original
        // state upon leaving the DPI scope.
        //
        // This method will return NULL if SetThreadDpiAwarenessContext is not supported in the
        // current environment, or it will return the previous DPI context.
        static DPI_AWARENESS_CONTEXT SetThreadDpiAwarenessContext(_In_ DPI_AWARENESS_CONTEXT dpiContext)
        {
            // If an invalid context is given to SetThreadDpiAwarenessContext the method returns
            // NULL, so mimic that behavior when that call is unsupported.
            if (!CanSetThreadDpiContext())
                return NULL;

            return s_pSetTDAC(dpiContext);
        }

        #pragma endregion DPI awareness methods

        #pragma region DPI API availability checker methods

        // Used for checking if an HMONITOR's DPI can be queried.
        //
        // This is expected to return true for Windows versions >= 8.1 and false for all others.
        static bool CanGetMonitorDpi()
        {
            Initialize();
            return (s_pGetDFM != nullptr);
        }

        // Used for checking if an HWND's DPI can be queried.
        //
        // This is expected to return true for Windows versions >= 10.1607 and false for all others.
        static bool CanGetWindowDpi()
        {
            Initialize();
            return (s_pGetDFW != nullptr);
        }

        // Used for checking if the window DPI context can be queried.
        //
        // This is expected to return true for Windows versions >= 10.1607 and false for all others.
        static bool CanGetWindowDpiContext()
        {
            Initialize();
            return (s_pGetWDAC != nullptr);
        }

        // Used for checking if the process DPI context can be changed.
        //
        // This is expected to return true for Windows versions >= 8.1 and false for all others.
        static bool CanSetProcessDpiContext()
        {
            Initialize();
            return (s_pSetPDAC != nullptr) ||
                   (s_pSetPDAC_Win81 != nullptr);
        }

        // Used for checking if the thread DPI context can be changed.
        //
        // This is expected to return true for Windows versions >= 10.1607 and false for all others.
        static bool CanSetThreadDpiContext()
        {
            Initialize();
            return (s_pSetTDAC != nullptr);
        }

        // Used for checking if the thread DPI hosting behavior can be changed.
        //
        // This is expected to return true for Windows versions >= 10.1803 and false for all others.
        static bool CanSetThreadHosting()
        {
            Initialize();
            return (s_pSetTDHB != nullptr);
        }

        #pragma endregion DPI API availability checker methods

    private:
        // Initializes the backing state.
        static void Initialize()
        {
            if (s_isInitialized)
                return;

            // Get the required method addresses from User32.dll.
            HMODULE hModUser32 = LoadLibrary(GetSystemPath(L"user32.dll"));
            if (hModUser32)
            {
                s_pGetDFW  = (GetDpiForWindowProc)GetProcAddress(hModUser32, "GetDpiForWindow");
                s_pGetWDAC = (GetWindowDpiAwarenessContextProc)GetProcAddress(hModUser32, "GetWindowDpiAwarenessContext");
                s_pSetPDAC = (SetProcessDpiAwarenessContextProc)GetProcAddress(hModUser32, "SetProcessDpiAwarenessContext");
                s_pSetTDAC = (SetThreadDpiAwarenessContextProc)GetProcAddress(hModUser32, "SetThreadDpiAwarenessContext");
                s_pSetTDHB = (SetThreadDpiHostingBehaviorProc)GetProcAddress(hModUser32, "SetThreadDpiHostingBehavior");
            }

            // Get the required methods addresses from ShCore.dll.
            HMODULE hModShCore = LoadLibrary(GetSystemPath(L"shcore.dll"));
            if (hModShCore)
            {
                s_pGetDFM        = (GetDpiForMonitorProc)GetProcAddress(hModShCore, "GetDpiForMonitor");
                s_pGetPDAC_Win81 = (GetProcessDpiAwarenessProc)GetProcAddress(hModShCore, "GetProcessDpiAwareness");
                s_pSetPDAC_Win81 = (SetProcessDpiAwarenessProc)GetProcAddress(hModShCore, "SetProcessDpiAwareness");
            }

            // Set up the system device DPI values
            HDC hdc = ::GetDC(NULL);
            if (hdc)
            {
                s_DeviceDpiX = ::GetDeviceCaps(hdc, LOGPIXELSX);
                s_DeviceDpiY = ::GetDeviceCaps(hdc, LOGPIXELSY);
                ::ReleaseDC(NULL, hdc);
            }

            s_isInitialized = true;
        }

        #pragma region Private enums

        enum class ConversionDirection
        {
            DeviceToLogical,
            LogicalToDevice,
        };

        enum class Orientation
        {
            Horizontal,
            Vertical,
        };

        #pragma endregion Private enums

        #pragma region Private helper methods

        typedef CStrBufT<wchar_t, false> CAtlStrBufW;
        typedef CPathT<CAtlStringW> CAtlPathW;

        // Prepends the System32 Windows path to the given DLL name.
        static CStringW GetSystemPath(_In_ LPCWSTR dllName)
        {
            CAtlPathW fullPath;

            GetSystemDirectoryW(CAtlStrBufW(fullPath, MAX_PATH), MAX_PATH);

            fullPath.Append(dllName);
            return fullPath.m_strPath;
        }

        #pragma region Unit conversion helpers

        // Converts the given POINT between device and logical units based on the DPI of the given
        // HWND.
        static HRESULT ConvertPoint(_In_ HWND hwnd, ConversionDirection conversion, _Inout_ POINT *pPoint)
        {
            if (pPoint == nullptr)
                return E_POINTER;

            HRESULT hr = S_OK;

            UINT dpiX, dpiY;
            if (SUCCEEDED(hr = GetDpiForWindow(hwnd, &dpiX, &dpiY)))
            {
                pPoint->x = ScaleValue(pPoint->x, dpiX, conversion);
                pPoint->y = ScaleValue(pPoint->y, dpiY, conversion);
            }

            return hr;
        }

        // Converts the given RECT between device and logical units based on the DPI of the given
        // HWND.
        static HRESULT ConvertRect(_In_ HWND hwnd, ConversionDirection conversion, _Inout_ RECT *pRect)
        {
            if (pRect == nullptr)
                return E_POINTER;

            HRESULT hr = S_OK;

            UINT dpiX, dpiY;
            if (SUCCEEDED(hr = GetDpiForWindow(hwnd, &dpiX, &dpiY)))
            {
                pRect->left   = ScaleValue(pRect->left,   dpiX, conversion);
                pRect->top    = ScaleValue(pRect->top,    dpiY, conversion);
                pRect->right  = ScaleValue(pRect->right,  dpiX, conversion);
                pRect->bottom = ScaleValue(pRect->bottom, dpiY, conversion);
            }

            return hr;
        }

        // Converts the given SIZE between device and logical units based on the DPI of the given
        // HWND.
        static HRESULT ConvertSize(_In_ HWND hwnd, ConversionDirection conversion, _Inout_ SIZE *pSize)
        {
            if (pSize == nullptr)
                return E_POINTER;

            HRESULT hr = S_OK;

            UINT dpiX, dpiY;
            if (SUCCEEDED(hr = GetDpiForWindow(hwnd, &dpiX, &dpiY)))
            {
                pSize->cx = ScaleValue(pSize->cx, dpiX, conversion);
                pSize->cy = ScaleValue(pSize->cy, dpiY, conversion);
            }

            return hr;
        }

        // Converts the given value between device and logical units based on the DPI of the given
        // HWND.
        static HRESULT ConvertUnits(_In_ HWND hwnd, ConversionDirection conversion, Orientation orientation, _Inout_ int *pValue)
        {
            if (pValue == nullptr)
                return E_POINTER;

            HRESULT hr = S_OK;

            UINT dpiX, dpiY;
            if (SUCCEEDED(hr = GetDpiForWindow(hwnd, &dpiX, &dpiY)))
            {
                UINT dpi;
                if (orientation == Orientation::Horizontal)
                    dpi = dpiX;
                else
                    dpi = dpiY;

                *pValue = ScaleValue(*pValue, dpi, conversion);
            }

            return hr;
        }

        // This method implicitly casts both input values to doubles. (Normally they'd be an int
        // and UINT respectively.) This is done to prevent truncating the intermediate value that
        // results from the multiplication operation if it would normally exceed the max value of
        // an int.
        static int ScaleValue(double originalValue, double dpi, ConversionDirection conversion)
        {
            if (conversion == ConversionDirection::DeviceToLogical)
                return Round((originalValue * k_DefaultLogicalDpi) / dpi);
            else
                return Round((originalValue * dpi) / k_DefaultLogicalDpi);
        }

        static int Round(double value)
        {
            if (value >= 0)
                return (int)(value + 0.5);
            else
                return (int)(value - 0.5);
        }

        #pragma endregion Unit conversion helpers

        #pragma endregion Private helper methods

        #pragma region Per-Monitor DPI availability checker methods

        // The version of .NET that offers the required multi-/mixed-DPI functionality and behaviors
        // sets a regkey when present. So check for that instead of checking for dll version numbers
        // or the presence of certain APIs.
        static bool IsDotNetSupportedVersion()
        {
            CRegKey dotNetSetupKey;

            if (dotNetSetupKey.Open(HKEY_LOCAL_MACHINE, DOTNET_SETUP_KEYNAME, KEY_READ) == ERROR_SUCCESS)
            {
                CStringW versionValue;
                ULONG charsRead = MAX_PATH;

                if (dotNetSetupKey.QueryStringValue(DOTNET_SETUP_VERSION, CStrBufW(versionValue, MAX_PATH), &charsRead) == ERROR_SUCCESS)
                    return versionValue.Find(L"4.8") == 0;

                dotNetSetupKey.Close();
            }

            return false;
        }

        // Windows has had incremental rollout of all of the required multi-/mixed-DPI APIs needed
        // to fully support Per-Monitor DPI awareness and enable mixed DPI hosting behavior. This
        // will check for the oldest version of Windows that has all the required APIs and behaviors.
        static bool IsWindowsSupportedVersion()
        {
            // Currently being able to set the DPI hosting behavior is the
            // low bar for enabling Per-Monitor DPI awareness mode.
            return CanSetThreadHosting();
        }

        #pragma endregion Per-Monitor DPI availability checker methods

        #pragma region Statics declarations

        static const UINT k_DefaultLogicalDpi = 96;

        static int  s_DeviceDpiX;
        static int  s_DeviceDpiY;
        static bool s_isInitialized;
        static bool s_isAvailable;
        static bool s_isAvailableChecked;
        static bool s_isPerMonitorAwarenessChecked;
        static bool s_isPerMonitorAwarenessEnabled;
        static GetDpiForMonitorProc              s_pGetDFM;
        static GetDpiForWindowProc               s_pGetDFW;
        static GetProcessDpiAwarenessProc        s_pGetPDAC_Win81;
        static GetWindowDpiAwarenessContextProc  s_pGetWDAC;
        static SetProcessDpiAwarenessContextProc s_pSetPDAC;
        static SetProcessDpiAwarenessProc        s_pSetPDAC_Win81;
        static SetThreadDpiAwarenessContextProc  s_pSetTDAC;
        static SetThreadDpiHostingBehaviorProc   s_pSetTDHB;

        #pragma endregion Statics declarations
    };

    #pragma region CDpiScope

    // This class is used for changing the calling thread's DPI context for the duration of the
    // lifetime of this scope object. A DPI scope should not be created and stored indefinitely.
    // Proper use of a DPI scope limits its lifetime to just the code that needs the different
    // DPI context.
    class CDpiScope
    {
    public:
        CDpiScope(DPI_AWARENESS_CONTEXT dpiContext)
        {
            // If the process is not in Per-Monitor awareness mode, then don't allow switching the
            // thread's DPI context.
            if (CDpiAwareness::IsPerMonitorDpiAwarenessEnabled() && dpiContext)
                oldDpi = CDpiAwareness::SetThreadDpiAwarenessContext(dpiContext);
            else
                oldDpi = NULL;
        }

        CDpiScope(HWND hwnd)
        {
            // If the process is not in Per-Monitor awareness mode, then don't allow switching the
            // thread's DPI context.
            if (CDpiAwareness::IsPerMonitorDpiAwarenessEnabled() && IsWindow(hwnd))
            {
                // Get the message HWND's DPI context and temporarily set the thread context to that
                // DPI awareness so that coordinate lookup and translation works correctly.
                DPI_AWARENESS_CONTEXT sourceDpi = CDpiAwareness::GetWindowDpiAwarenessContext(hwnd);
                oldDpi = CDpiAwareness::SetThreadDpiAwarenessContext(sourceDpi);
            }
            else
            {
                oldDpi = NULL;
            }
        }

        ~CDpiScope()
        {
            // Reset the thread DPI awareness to its original value.
            if (oldDpi != NULL)
                CDpiAwareness::SetThreadDpiAwarenessContext(oldDpi);
        }

    private:
        DPI_AWARENESS_CONTEXT oldDpi;
    };

    #pragma endregion CDpiScope

    #pragma region CDpiAwareness declspec statics

    __declspec(selectany) int  CDpiAwareness::s_DeviceDpiX                   = CDpiAwareness::k_DefaultLogicalDpi;
    __declspec(selectany) int  CDpiAwareness::s_DeviceDpiY                   = CDpiAwareness::k_DefaultLogicalDpi;
    __declspec(selectany) bool CDpiAwareness::s_isInitialized                = false;
    __declspec(selectany) bool CDpiAwareness::s_isAvailable                  = false;
    __declspec(selectany) bool CDpiAwareness::s_isAvailableChecked           = false;
    __declspec(selectany) bool CDpiAwareness::s_isPerMonitorAwarenessChecked = false;
    __declspec(selectany) bool CDpiAwareness::s_isPerMonitorAwarenessEnabled = false;
    __declspec(selectany) GetDpiForMonitorProc              CDpiAwareness::s_pGetDFM        = nullptr;
    __declspec(selectany) GetDpiForWindowProc               CDpiAwareness::s_pGetDFW        = nullptr;
    __declspec(selectany) GetProcessDpiAwarenessProc        CDpiAwareness::s_pGetPDAC_Win81 = nullptr;
    __declspec(selectany) GetWindowDpiAwarenessContextProc  CDpiAwareness::s_pGetWDAC       = nullptr;
    __declspec(selectany) SetProcessDpiAwarenessContextProc CDpiAwareness::s_pSetPDAC       = nullptr;
    __declspec(selectany) SetProcessDpiAwarenessProc        CDpiAwareness::s_pSetPDAC_Win81 = nullptr;
    __declspec(selectany) SetThreadDpiAwarenessContextProc  CDpiAwareness::s_pSetTDAC       = nullptr;
    __declspec(selectany) SetThreadDpiHostingBehaviorProc   CDpiAwareness::s_pSetTDHB       = nullptr;

    #pragma endregion CDpiAwareness declspec statics
}
