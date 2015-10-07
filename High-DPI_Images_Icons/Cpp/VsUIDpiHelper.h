//Copyright (c) Microsoft.  All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//-----------------------------------------------------------------------
// </copyright>
// <summary>Assembly info.</summary>

#pragma once

#include "VsUIGdiplusImage.h"
#include <memory>

namespace VsUI
{
    #define HDPIAPI __stdcall

    // NOTE: The image scaling modes available here for Win32 match the similar scaling modes for WinForms from 
    // Microsoft.VisualStudio.PlatformUI.DpiHelper class
    // If changes are made to algorithms in this native DpiHelper class, matching changes will have to be made to the managed class, too.
    enum class ImageScalingMode
    {
        Default             = 0, // Let the shell pick what looks best depending on the current DPI zoom factor
        BorderOnly          = 1, // Keep the actual image unscaled, add a border around the image
        NearestNeighbor     = 2, // Sharp results, but pixelated, and possibly distorted unless multiple of 100% scaling
        Bilinear            = 3, // Smooth results, without distorsions, but fuzzy (GDI+ InterpolationModeBilinear) 
        Bicubic             = 4, // Smooth results, without distorsions, but fuzzy (GDI+ InterpolationModeBicubic)  
        HighQualityBilinear = 5, // Smooth results, without distorsions, but fuzzy (GDI+ InterpolationModeHighQualityBilinear)
        HighQualityBicubic  = 6, // Smooth results, without distorsions, but fuzzy. Some overshooting/oversharpening-like artifacts may be present (GDI+ InterpolationModeHighQualityBicubic)
    };

    class CDpiHelper
    {
    public:
        // Constructor
        CDpiHelper(int iDeviceDpiX, int iDeviceDpiY, int iLogicalDpiX, int iLogicalDpiY);

        // Get device DPI.
        int HDPIAPI GetDeviceDpiX() const;
        int HDPIAPI GetDeviceDpiY() const;

        // Get logical DPI.
        int HDPIAPI GetLogicalDpiX() const;
        int HDPIAPI GetLogicalDpiY() const;

        // Returns whether scaling is required when converting between logical-device units
        bool HDPIAPI IsScalingRequired() const;
        
        // Return horizontal and vertical scaling factors
        double DeviceToLogicalUnitsScalingFactorX() const;
        double DeviceToLogicalUnitsScalingFactorY() const;
        double LogicalToDeviceUnitsScalingFactorX() const;
        double LogicalToDeviceUnitsScalingFactorY() const;

        // Converts between logical and device units.
        int HDPIAPI LogicalToDeviceUnitsX(int x) const;
        int HDPIAPI LogicalToDeviceUnitsY(int y) const;
    
        // Converts between device and logical units.
        int HDPIAPI DeviceToLogicalUnitsX(int x) const;
        int HDPIAPI DeviceToLogicalUnitsY(int y) const;

        // Converts from logical units to device units.
        void HDPIAPI LogicalToDeviceUnits(_Inout_ POINT * pPoint) const;
        void HDPIAPI LogicalToDeviceUnits(_Inout_ RECT * pRect) const;

        // Converts from device units to logical units.
        void HDPIAPI DeviceToLogicalUnits(_Inout_ POINT * pPoint) const;
        void HDPIAPI DeviceToLogicalUnits(_Inout_ RECT * pRect) const;
        
        // Converts (if necessary) the image from logical to device pixels. By default we use interpolation that gives smoother results when scaling up.
        // The functions will return the original image if no scaling is required due to high DPI modes.
        // Should scaling be necessary, the original image will be destroyed, a new scaled imaged be returned, and the caller will now control the lifetime of the scaled image.
        void HDPIAPI LogicalToDeviceUnits(_Inout_ VsUI::GdiplusImage * pImage, ImageScalingMode scalingMode = ImageScalingMode::Default, Gdiplus::Color clrBackground = TransparentColor);
        void HDPIAPI LogicalToDeviceUnits(_Inout_ HBITMAP * pImage, ImageScalingMode scalingMode = ImageScalingMode::Default, Gdiplus::Color clrBackground = TransparentColor);
        void HDPIAPI LogicalToDeviceUnits(_Inout_ HIMAGELIST * pImageList, ImageScalingMode scalingMode = ImageScalingMode::Default);
        void HDPIAPI LogicalToDeviceUnits(_Inout_ HICON * pIcon, _In_opt_ const SIZE * pLogicalSize = nullptr) const;

        // Creates and returns a new image suitable for display on device units. A clone image will be created when scaling is not necessary. The caller is reponsible of the lifetime of the returned image.
        std::unique_ptr<VsUI::GdiplusImage> HDPIAPI CreateDeviceFromLogicalImage(_In_ VsUI::GdiplusImage* pImage, ImageScalingMode scalingMode = ImageScalingMode::Default, Gdiplus::Color clrBackground = TransparentColor);
        HBITMAP HDPIAPI CreateDeviceFromLogicalImage(_In_ HBITMAP hImage, ImageScalingMode scalingMode = ImageScalingMode::Default, Gdiplus::Color clrBackground = TransparentColor);
        HIMAGELIST HDPIAPI CreateDeviceFromLogicalImage(_In_ HIMAGELIST hImageList, ImageScalingMode scalingMode = ImageScalingMode::Default);
        HICON HDPIAPI CreateDeviceFromLogicalImage(_In_ HICON hIcon, _In_opt_ const SIZE * pLogicalSize = nullptr) const;

        // Convert a point size (1/72 of an inch) to device units.
        int HDPIAPI PointsToDeviceUnits(int pt) const;

        // Determine the screen dimensions in logical units.
        int HDPIAPI LogicalScreenWidth() const;
        int HDPIAPI LogicalScreenHeight() const;
        
        // Return the monitor information in logical units
        BOOL HDPIAPI GetLogicalMonitorInfo(_In_ HMONITOR hMonitor, _Out_ LPMONITORINFO lpmi) const;

        // Determine if screen resolution meets minimum requirements in logical units.
        bool HDPIAPI IsResolutionAtLeast(int cxMin, int cyMin) const;

    protected:
        bool GetIconSize(_In_ HICON hIcon, _Out_ SIZE * pSize) const;
        HICON CreateDeviceImageOrReuseIcon(_In_ HICON hIcon, bool fAlwaysCreate, _In_ const SIZE * pIconSize) const;

        // Gets the interpolation mode from the specified scaling mode
        Gdiplus::InterpolationMode GetInterpolationMode(_In_ ImageScalingMode scalingMode);
        // Gets the actual scaling mode to be used from the suggested scaling mode
        ImageScalingMode GetActualScalingMode(_In_ ImageScalingMode scalingMode);
        // Get the scaling mode for the specified dpi zom factor
        ImageScalingMode GetDefaultScalingMode(int dpiScalePercent) const;
        // Returns the user preference for scaling mode or default, if the user doesn't want to override
        ImageScalingMode GetUserScalingMode(int dpiScalePercent, ImageScalingMode defaultScalingMode) const;
        // Returns the preferred scaling mode for current DPI zoom level (either shell preferred mode, or a user-override)
        ImageScalingMode GetPreferredScalingMode();

        int m_DeviceDpiX;
        int m_DeviceDpiY;
        int m_LogicalDpiX;
        int m_LogicalDpiY;

        // The shell preferred image scaling mode for current DPI zoom level
        ImageScalingMode m_PreferredScalingMode;
    };

    // The static functions in the DpiHelper class delegate the calls to the default CDpiHelper for 96dpi.
    // Definition: logical pixel = 1 pixel at 96 DPI
    class DpiHelper
    {
    public:

        // Returns a CDpiHelper that can scale images created for the specified DPI zoom factor
        static CDpiHelper* GetHelper(int zoomPercents);

        // Get device DPI.
        static int HDPIAPI GetDeviceDpiX();
        static int HDPIAPI GetDeviceDpiY();

        // Get logical DPI.
        static int HDPIAPI GetLogicalDpiX();
        static int HDPIAPI GetLogicalDpiY();
        
        // Returns whether scaling is required when converting between logical-device units
        static bool HDPIAPI IsScalingRequired();
        
        // Return horizontal and vertical scaling factors
        static double DeviceToLogicalUnitsScalingFactorX();
        static double DeviceToLogicalUnitsScalingFactorY();
        static double LogicalToDeviceUnitsScalingFactorX();
        static double LogicalToDeviceUnitsScalingFactorY();

        // Converts between logical and device units.
        static int HDPIAPI LogicalToDeviceUnitsX(int x);
        static int HDPIAPI LogicalToDeviceUnitsY(int y);
    
        // Converts between device and logical units.
        static int HDPIAPI DeviceToLogicalUnitsX(int x);
        static int HDPIAPI DeviceToLogicalUnitsY(int y);

        // Converts from logical units to device units.
        static void HDPIAPI LogicalToDeviceUnits(_Inout_ POINT * pPoint);
        static void HDPIAPI LogicalToDeviceUnits(_Inout_ RECT * pRect);

        // Converts from device units to logical units.
        static void HDPIAPI DeviceToLogicalUnits(_Inout_ POINT * pPoint);
        static void HDPIAPI DeviceToLogicalUnits(_Inout_ RECT * pRect);
        
        // Converts (if necessary) the image from logical to device pixels. By default we use interpolation that gives smoother results when scaling up.
        // The functions will return the original image if no scaling is required due to high DPI modes.
        // Should scaling be necessary, the original image will be destroyed, a new scaled imaged be returned, and the caller will now control the lifetime of the scaled image.
        static void HDPIAPI LogicalToDeviceUnits(_Inout_ VsUI::GdiplusImage * pImage, ImageScalingMode scalingMode = ImageScalingMode::Default, Gdiplus::Color clrBackground = TransparentColor);
        static void HDPIAPI LogicalToDeviceUnits(_Inout_ HBITMAP * pImage, ImageScalingMode scalingMode = ImageScalingMode::Default, Gdiplus::Color clrBackground = TransparentColor);
        static void HDPIAPI LogicalToDeviceUnits(_Inout_ HIMAGELIST * pImageList, ImageScalingMode scalingMode = ImageScalingMode::Default);
        // Note: Currently, icon scaling supports either loading a different frame from the icon resource or NearestNeighbor resizing
        static void HDPIAPI LogicalToDeviceUnits(_Inout_ HICON * pIcon, _In_opt_ const SIZE * pLogicalSize = nullptr);

        // Creates and returns a new image suitable for display on device units. A clone image will be created when scaling is not necessary. The caller is reponsible of the lifetime of the returned image.
        static std::unique_ptr<VsUI::GdiplusImage> HDPIAPI CreateDeviceFromLogicalImage(_In_ VsUI::GdiplusImage* pImage, ImageScalingMode scalingMode = ImageScalingMode::Default, Gdiplus::Color clrBackground = TransparentColor);
        static HBITMAP HDPIAPI CreateDeviceFromLogicalImage(_In_ HBITMAP hImage, ImageScalingMode scalingMode = ImageScalingMode::Default, Gdiplus::Color clrBackground = TransparentColor);
        static HIMAGELIST HDPIAPI CreateDeviceFromLogicalImage(_In_ HIMAGELIST hImageList, ImageScalingMode scalingMode = ImageScalingMode::Default);
        static HICON HDPIAPI CreateDeviceFromLogicalImage(_In_ HICON hIcon, _In_opt_ const SIZE * pLogicalSize = nullptr);

        // Convert a point size (1/72 of an inch) to device units.
        static int HDPIAPI PointsToDeviceUnits(int pt);

        // Determine the screen dimensions in logical units.
        static int HDPIAPI LogicalScreenWidth();
        static int HDPIAPI LogicalScreenHeight();
        
        // Return the monitor information in logical units
        static BOOL HDPIAPI GetLogicalMonitorInfo(_In_ HMONITOR hMonitor, _Out_ LPMONITORINFO lpmi);

        // Determine if screen resolution meets minimum requirements in logical units.
        static bool HDPIAPI IsResolutionAtLeast(int cxMin, int cyMin);

    private:
        // Returns the helper for 100% zoom factor, aka 96dpi
        static CDpiHelper* GetDefaultHelper();
        static CComAutoCriticalSection s_critSection;

        static void Initialize();

        static bool m_fInitialized;
        static int m_DeviceDpiX;
        static int m_DeviceDpiY;

        static const int k_DefaultLogicalDpi = 96;
    };

} // namespace