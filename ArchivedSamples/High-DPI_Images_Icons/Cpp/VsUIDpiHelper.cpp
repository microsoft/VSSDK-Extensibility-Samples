//Copyright (c) Microsoft.  All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//-----------------------------------------------------------------------
// </copyright>
// <summary>Assembly info.</summary>

#include "StdAfx.h"
#include "VsUIDpiHelper.h"
#include "vsassert.h"
#include "ScopeGuard.h"
#include "atlgdi.h"
#include <map>
#include <atlstr.h>
#include <atlpath.h>

#define IfFailRetNull(var)          { if (!var) return NULL; }
#define IfNullRetNull(var)          { if (var == NULL) return NULL; }
#define IfNullRet(var)              { if (var == NULL) return; }
#define IfNullRetX(var,retval)      { if (var == NULL) return retval; }
#define IfNullAssertRet(var, msg)   { if (!var) { VSFAIL(msg); return; } }
#define IfNullAssertRetNull(var, msg) { if (!var) { VSFAIL(msg); return nullptr; } }

#define REGKEY_GENERAL L"General"
#define REGKEY_IMAGESCALING L"ImageScaling%d"

using namespace Gdiplus;
using namespace std;

namespace VsUI
{

CDpiHelper::CDpiHelper(int iDeviceDpiX, int iDeviceDpiY, int iLogicalDpiX, int iLogicalDpiY) :
    m_DeviceDpiX(iDeviceDpiX), m_DeviceDpiY(iDeviceDpiY), m_LogicalDpiX(iLogicalDpiX), m_LogicalDpiY(iLogicalDpiY), m_PreferredScalingMode(ImageScalingMode::Default)
{
}

// Get device DPI.
int CDpiHelper::GetDeviceDpiX() const
{ 
    return m_DeviceDpiX; 
}

int CDpiHelper::GetDeviceDpiY() const 
{ 
    return m_DeviceDpiY; 
}

// Get logical DPI.
int CDpiHelper::GetLogicalDpiX() const 
{ 
    return m_LogicalDpiX; 
}

int CDpiHelper::GetLogicalDpiY() const 
{ 
    return m_LogicalDpiY; 
}


bool CDpiHelper::IsScalingRequired() const
{
    return (m_DeviceDpiX != m_LogicalDpiX || m_DeviceDpiY != m_LogicalDpiY); 
}

// Return horizontal and vertical scaling factors
double CDpiHelper::DeviceToLogicalUnitsScalingFactorX() const
{
    return (double)m_LogicalDpiX / m_DeviceDpiX;
}

double CDpiHelper::DeviceToLogicalUnitsScalingFactorY() const
{
    return (double)m_LogicalDpiY / m_DeviceDpiY;
}

double CDpiHelper::LogicalToDeviceUnitsScalingFactorX() const
{
    return (double)m_DeviceDpiX / m_LogicalDpiX;
}

double CDpiHelper::LogicalToDeviceUnitsScalingFactorY() const
{
    return (double)m_DeviceDpiY / m_LogicalDpiY;
}

// Converts between logical and device units.
int CDpiHelper::LogicalToDeviceUnitsX(int x) const 
{ 
    return MulDiv(x, m_DeviceDpiX, m_LogicalDpiX); 
}

int CDpiHelper::LogicalToDeviceUnitsY(int y) const
{ 
    return MulDiv(y, m_DeviceDpiY, m_LogicalDpiY); 
}

// Converts between device and logical units.
int CDpiHelper::DeviceToLogicalUnitsX(int x) const 
{ 
    return MulDiv(x, m_LogicalDpiX, m_DeviceDpiX); 
}

int CDpiHelper::DeviceToLogicalUnitsY(int y) const 
{ 
    return MulDiv(y, m_LogicalDpiY, m_DeviceDpiY); 
}

// Converts from logical units to device units.
void CDpiHelper::LogicalToDeviceUnits(_Inout_ RECT * pRect) const
{
    if (pRect != nullptr)
    {
        pRect->left = LogicalToDeviceUnitsX(pRect->left);
        pRect->right = LogicalToDeviceUnitsX(pRect->right);
        pRect->top = LogicalToDeviceUnitsY(pRect->top);
        pRect->bottom = LogicalToDeviceUnitsY(pRect->bottom);
    }
}

void CDpiHelper::LogicalToDeviceUnits(_Inout_ POINT * pPoint) const
{
    if (pPoint != nullptr)
    {
        pPoint->x = LogicalToDeviceUnitsX(pPoint->x);
        pPoint->y = LogicalToDeviceUnitsY(pPoint->y);
    }
}

// Converts from device units to logical units.
void CDpiHelper::DeviceToLogicalUnits(_Inout_ RECT * pRect) const
{
    if (pRect != nullptr)
    {
        pRect->left = DeviceToLogicalUnitsX(pRect->left);
        pRect->right = DeviceToLogicalUnitsX(pRect->right);
        pRect->top = DeviceToLogicalUnitsY(pRect->top);
        pRect->bottom = DeviceToLogicalUnitsY(pRect->bottom);
    }
}

void CDpiHelper::DeviceToLogicalUnits(_Inout_ POINT * pPoint) const
{
    if (pPoint != nullptr)
    {
        pPoint->x = DeviceToLogicalUnitsX(pPoint->x);
        pPoint->y = DeviceToLogicalUnitsY(pPoint->y);
    }
}

// Convert a point size (1/72 of an inch) to raw pixels.
int CDpiHelper::PointsToDeviceUnits(int pt) const
{ 
    return MulDiv(pt, m_DeviceDpiY, 72); 
}

// Determine the screen dimensions in logical units.
int CDpiHelper::LogicalScreenWidth() const
{ 
    return DeviceToLogicalUnitsX(GetSystemMetrics(SM_CXSCREEN)); 
}

int CDpiHelper::LogicalScreenHeight() const 
{ 
    return DeviceToLogicalUnitsY(GetSystemMetrics(SM_CYSCREEN)); 
}

// Determine if screen resolution meets minimum requirements in logical pixels.
bool CDpiHelper::IsResolutionAtLeast(int cxMin, int cyMin) const
{ 
    return (LogicalScreenWidth() >= cxMin) && (LogicalScreenHeight() >= cyMin); 
}

// Return the monitor information in logical units
BOOL CDpiHelper::GetLogicalMonitorInfo(_In_ HMONITOR hMonitor, _Out_ LPMONITORINFO lpmi) const
{
    if (GetMonitorInfo(hMonitor, lpmi))
    {
        DeviceToLogicalUnits(&lpmi->rcMonitor);
        DeviceToLogicalUnits(&lpmi->rcWork);
        return TRUE;
    }
    
    return FALSE;
}

// Returns the shell preferred scaling mode, depening on the DPI zoom level
ImageScalingMode CDpiHelper::GetDefaultScalingMode(int dpiScalePercent) const
{
    // We'll use NearestNeighbor for 100, 200, 400, etc scaling mode, where we get crisp/pixelated results without image distortions
    // We'll use Bicubic scaling for the rest except when the scale is actually for reducing the image (which we shouldn't have anyway), when Linear produces better results because it uses less neighboring pixels.
    // The algorithm matches GetDefaultBitmapScalingMode from the MPF's DpiHelper class
    if ((dpiScalePercent % 100) == 0)
    {
        return ImageScalingMode::NearestNeighbor;
    }
    else if (dpiScalePercent < 100)
    {
        return ImageScalingMode::HighQualityBilinear;
    }
    else
    {
        return ImageScalingMode::HighQualityBicubic;
    }
}

// Returns the user preference for scaling mode by reading it from registry 
// or returns default scaling mode if the user doesn't want to override
ImageScalingMode CDpiHelper::GetUserScalingMode(int dpiScalePercent, ImageScalingMode defaultScalingMode) const
{
    ImageScalingMode scalingMode = defaultScalingMode;

    CRegKey hKeyGeneral = NULL;
    if (ERROR_SUCCESS == hKeyGeneral.Open(HKEY_CURRENT_USER, LREGKEY_VISUALSTUDIOROOT L"\\" REGKEY_GENERAL, KEY_READ))
    {
        WCHAR szValueName[30];
        _stprintf_s(szValueName, REGKEY_IMAGESCALING, dpiScalePercent);

        DWORD dwType = 0;
        DWORD dwData = 0;
        DWORD cbDataLength = sizeof(dwData);
        if (ERROR_SUCCESS == hKeyGeneral.QueryDWORDValue(szValueName, dwData))
        {
            if (dwData == (DWORD)ImageScalingMode::BorderOnly || 
                dwData == (DWORD)ImageScalingMode::NearestNeighbor || 
                dwData == (DWORD)ImageScalingMode::Bilinear || 
                dwData == (DWORD)ImageScalingMode::Bicubic ||
                dwData == (DWORD)ImageScalingMode::HighQualityBilinear || 
                dwData == (DWORD)ImageScalingMode::HighQualityBicubic)
            {
                scalingMode = (ImageScalingMode)dwData;
            }
            else 
            {
                VSASSERT(dwData == (DWORD)ImageScalingMode::Default, "Invalid override scaling mode value");
            }
        }
    }

    return scalingMode;
}

// Gets the interpolation mode from the specified scaling mode
InterpolationMode CDpiHelper::GetInterpolationMode(_In_ ImageScalingMode scalingMode)
{
    switch (scalingMode)
    {
    case ImageScalingMode::Bilinear:
        {
            // Same as InterpolationModeLowQuality and InterpolationModeDefault
            return InterpolationModeBilinear;
        }
    case ImageScalingMode::Bicubic:
        {
            return InterpolationModeBicubic;
        }
    case ImageScalingMode::HighQualityBilinear:
        {
            return InterpolationModeHighQualityBilinear;
        }
    case ImageScalingMode::HighQualityBicubic: 
        {
            // Same as InterpolationModeHighQuality
            return InterpolationModeHighQualityBicubic;
        }
    case ImageScalingMode::BorderOnly: __fallthrough;
    case ImageScalingMode::NearestNeighbor: 
        {
            return InterpolationModeNearestNeighbor;
        }
    default:
        {
            VSFAIL("Unknown scaling mode, please add an explicit case. Falling back to use default interpolation.");
            __fallthrough;
        }
    case ImageScalingMode::Default:
        {
            return GetInterpolationMode(GetPreferredScalingMode()); 
        }
    }
}

// Gets the actual scaling mode to be used from the suggested scaling mode
ImageScalingMode CDpiHelper::GetActualScalingMode(_In_ ImageScalingMode scalingMode)
{
    // If a scaling mode other than default is specified, use that
    if (scalingMode != ImageScalingMode::Default)
    {
        return scalingMode;
    }

    // Otherwise return the shell preferred scaling mode for the current DPI zoom level or a possible user override
    return GetPreferredScalingMode();
}

// Returns the preferred scaling mode for current DPI zoom level (either shell preferred mode, or a user-override)
ImageScalingMode CDpiHelper::GetPreferredScalingMode()
{
    // If we haven't initialized yet the scaling mode
    if (m_PreferredScalingMode == ImageScalingMode::Default)
    {
        // Get the current zoom level
        int dpiScalePercent = (int)(LogicalToDeviceUnitsScalingFactorX() * 100);
        // Get the shell preferred scaling mode depending on the zoom level
        ImageScalingMode defaultScalingMode = GetDefaultScalingMode(dpiScalePercent);
        // Allow the user to override
        m_PreferredScalingMode = GetUserScalingMode(dpiScalePercent, defaultScalingMode);
    }

    return m_PreferredScalingMode;
}


// Convert GdiplusImage from logical to device units
void CDpiHelper::LogicalToDeviceUnits(_Inout_ VsUI::GdiplusImage * pImage, ImageScalingMode scalingMode, Color clrBackground)
{
    IfNullAssertRet(pImage, "No image given to convert");

    // If no scaling is required, the image can be used in current size
    if (!IsScalingRequired())
        return;

    // Create the new image for the device, cloning the current one if necessary
    unique_ptr<VsUI::GdiplusImage> pDeviceImage = CreateDeviceFromLogicalImage(pImage, scalingMode, clrBackground);
    // If we failed to create the new image, return
    IfNullAssertRet(pDeviceImage.get(), "Failed to create scaled image");
    
    // Finally, replace the original image with the device image. Our pointer will take ownership and will release the original GDI+ Bitmap
    *pImage = *pDeviceImage;
}
 
// Creates new GdiplusImage from logical to device units
unique_ptr<VsUI::GdiplusImage> CDpiHelper::CreateDeviceFromLogicalImage(_In_ VsUI::GdiplusImage* pImage, ImageScalingMode scalingMode, Color clrBackground)
{
    IfNullAssertRetNull(pImage, "No image given to convert");

    // Get the original/logical bitmap
    Bitmap* pBitmap = pImage->GetBitmap();
    IfNullAssertRetNull(pBitmap, "No image given to convert");
    
    // Create a memory image scaled for size
    int deviceWidth = LogicalToDeviceUnitsX(pBitmap->GetWidth());
    int deviceHeight = LogicalToDeviceUnitsY(pBitmap->GetHeight());
    
    unique_ptr<VsUI::GdiplusImage> pDeviceImage(new VsUI::GdiplusImage());
    pDeviceImage->Create( deviceWidth, deviceHeight, pBitmap->GetPixelFormat() );
       
    if (!pDeviceImage->IsLoaded())
    {
        VSFAIL("Failed to create scaled image, out of memory?");
        return nullptr;
    }
    
    // Get a Graphics object for the device image on which we can paint 
    unique_ptr<Graphics> pGraphics(pDeviceImage->GetGraphics());
    if (pGraphics.get() == nullptr)
    {
        VSFAIL("Failed to obtain image Graphics");
        return nullptr;
    }
    
    // Set the interpolation mode. 
    InterpolationMode interpolationMode = GetInterpolationMode(scalingMode);
    pGraphics->SetInterpolationMode(interpolationMode);
    
    // Clear the background (used when scaling mode is not nearest neighbor)
    pGraphics->Clear(clrBackground);
    
    // Calculate the destination rectangle: full available space, except when keeping the image unscaled and just adding a border
    RectF rectD(0, 0, (float)deviceWidth, (float)deviceHeight);
    if (scalingMode == ImageScalingMode::BorderOnly || (scalingMode == ImageScalingMode::Default && m_PreferredScalingMode == ImageScalingMode::BorderOnly))
    {
        rectD = RectF(0, 0, (float)pBitmap->GetWidth(), (float)pBitmap->GetHeight());
        rectD.Offset( (float)((deviceWidth - pBitmap->GetWidth()) / 2),  (float)((deviceHeight - pBitmap->GetHeight())/ 2) );
    }

    // Define the source rectangle
    RectF rectS(0, 0, (float)pBitmap->GetWidth(), (float)pBitmap->GetHeight());
   
    // Specify a source rectangle shifted by half of pixel to account for GDI+ considering the source origin the center of top-left pixel
    // Failing to do so will result in the right and bottom of the bitmap lines being interpolated with the graphics' background color,
    // and will appear black even if we cleared the background with transparent color. 
    // The apparition of these artifacts depends on the interpolation mode, on the dpi scaling factor, etc.
    // E.g. at 150% DPI, Bicubic produces them and NearestNeighbor is fine, but at 200% DPI NearestNeighbor also shows them.
    // Many articles on the web talk about this problem, e.g. http://www.codeproject.com/Articles/14884/BorderBug
    rectS.Offset(-0.5f, -0.5f);
    
    // Draw the scaled bitmap in the device image
    pGraphics->DrawImage(pBitmap, rectD, rectS.X, rectS.Y, rectS.Width, rectS.Height, UnitPixel);
    
    // Return the new image
    return pDeviceImage;
}

void CDpiHelper::LogicalToDeviceUnits(_Inout_ HBITMAP * pImage, ImageScalingMode scalingMode, Color clrBackground)
{
    IfNullAssertRet(pImage, "No image given to convert");

    // If no scaling is required, the image can be used in current size
    if (!IsScalingRequired())
        return;

    // Create a new HBITMAP for the device units
    HBITMAP hDeviceImage = CreateDeviceFromLogicalImage(*pImage, scalingMode, clrBackground);
    // If the device image could not be created, return and keep using the original
    IfNullAssertRet(hDeviceImage, "Failed to create scaled image");

    // Delete the original image and return the converted image
    DeleteObject(*pImage);
    *pImage = hDeviceImage;
}

HBITMAP CDpiHelper::CreateDeviceFromLogicalImage(HBITMAP _In_ hImage, ImageScalingMode scalingMode, Color clrBackground)
{
    IfNullAssertRetNull(hImage, "No image given to convert");

    // Instead of doing HBITMAP resizing with StretchBlt from one memory DC into other memory DC and HALFTONE StretchBltMode
    // which uses nearest neighbor resize algorithm (fast but results in pixelation), we'll use a GdiPlus image to do the resize, 
    // which allows specifying the interpolation mode for the resize resulting in smoother result.
    VsUI::GdiplusImage gdiplusImage;

    // Attaching the bitmap uses Bitmap.FromHBITMAP which does not take ownership of the HBITMAP passed as argument.
    // DeleteObject still needs to be used on the hImage but that should happen after the Bitmap object is deleted or goes out of scope.
    // The caller will have to DeleteObject both the HBITMAP they passed in this function and the new HBITMAP we'll be returning when we detach the GDI+ Bitmap
    gdiplusImage.Attach(hImage);

#ifdef DEBUG
    static bool fDebugDPIHelperScaling = false;
    WCHAR rgTempFolder[MAX_PATH];
    static int imgIndex = 1;
    CStringW strFileName;
    CPath pathTempFile;

    if (fDebugDPIHelperScaling)
    {
        if (!GetTempPath(_countof(rgTempFolder), rgTempFolder))
            *rgTempFolder = '\0';
    
        strFileName.Format(_T("DPIHelper_%05d_Before.png"), imgIndex);

        pathTempFile.Combine(rgTempFolder, strFileName);
        gdiplusImage.Save(pathTempFile);
    }
#endif

    Bitmap* pBitmap = gdiplusImage.GetBitmap();
    PixelFormat format = pBitmap->GetPixelFormat();
    const Color *pclrActualBackground = &clrBackground; 
    InterpolationMode interpolationMode = GetInterpolationMode(scalingMode);
    ImageScalingMode actualScalingMode = GetActualScalingMode(scalingMode);

    if (actualScalingMode != ImageScalingMode::NearestNeighbor)
    {
        // Modify the image. If the image is 24bpp or lower, convert to 32bpp so we can use alpha values
        if (format != PixelFormat32bppARGB)
        {
            pBitmap->ConvertFormat(PixelFormat32bppARGB, DitherTypeNone, PaletteTypeCustom, nullptr/*ColorPalette*/, 0 /*alphaThresholdPercent - all opaque*/);
        }

        // Now that we have 32bpp image, let's play with the pixels
        // Detect magenta or near-green in the image and use that as background
        VsUI::GdiplusImage::ProcessBitmapBits(pBitmap, [&](ARGB * pPixelData) 
        {
            if (clrBackground.GetValue() != TransparentColor.GetValue())
            {
                if (*pPixelData == clrBackground.GetValue())
                {
                    *pPixelData = TransparentHaloColor.GetValue();
                    pclrActualBackground = &clrBackground;
                }
            }
            else
            {
                if (*pPixelData == MagentaColor.GetValue())
                {
                    *pPixelData = TransparentHaloColor.GetValue();
                    pclrActualBackground = &MagentaColor;
                }
                else if (*pPixelData == NearGreenColor.GetValue())
                {
                    *pPixelData = TransparentHaloColor.GetValue();
                    pclrActualBackground = &MagentaColor;
                }
            }
        });
    }

    // Convert the GdiPlus image if necessary
    LogicalToDeviceUnits(&gdiplusImage, scalingMode, TransparentHaloColor);

    // Get again the bitmap, after the resize
    pBitmap = gdiplusImage.GetBitmap();

    if (actualScalingMode != ImageScalingMode::NearestNeighbor)
    {
        // Now that the bitmap is scaled up, convert back the pixels. 
        // Anything that is not fully opaque, make it clrActualBackground
        VsUI::GdiplusImage::ProcessBitmapBits(pBitmap, [&](ARGB * pPixelData) 
        {
            if ((*pPixelData & ALPHA_MASK) != 0xFF000000)
            {
                *pPixelData = pclrActualBackground->GetValue();
            }
        });

        // Convert back to original format
        if (format != PixelFormat32bppARGB)
        {
            pBitmap->ConvertFormat(format, DitherTypeNone, PaletteTypeCustom, nullptr/*ColorPalette*/, 0 /*alphaThresholdPercent - all opaque*/);
        }
    }

#ifdef DEBUG
    if (fDebugDPIHelperScaling)
    {
        strFileName.Format(_T("DPIHelper_%05d_After.png"), imgIndex++);
        pathTempFile.Combine(rgTempFolder, strFileName);
        gdiplusImage.Save(pathTempFile);
    }
#endif
  
    // Get the converted image handle - this returns a new HBITMAP that will need to be deleted when no longer needed
    // Detach using TransparentColor (transparent-black). If the result bitmap is to be used with AlphaBlend, that function 
    // keeps the background if the transparent pixels are black
    HBITMAP hBmpResult = gdiplusImage.Detach( TransparentColor );

    // When the image has 32bpp RGB format, when we call GDI+ to return an HBITMAP for the image, the result is actually
    // an ARGB bitmap (with alpha bytes==0xFF instead of reserved=0x00). Many GDI functions work with it fine, but 
    // adding it to an imagelist with ImageList_AddMasked will produce the wrong result, because the clrTransparent color 
    // won't match any background pixels due to the alpha byte value. So we need to zero-out out those bytes... 
    // If the bitmap was scaled with a bicubic/bilinear interpolation, the colors are interpolated with the clrBackground 
    // which may be transparent, so the resultant image will have alpha channel of interest, and we'll return the image as is.
    if (format == PixelFormat32bppRGB)
    {
        BITMAP bmp = {0};
        if (GetObject(hBmpResult, sizeof(bmp), &bmp) == sizeof(bmp) && bmp.bmBits != nullptr)
        {
            RGBQUAD* pPixels = reinterpret_cast<RGBQUAD*>(bmp.bmBits);

            for (int i=0; i< bmp.bmWidth * bmp.bmHeight; i++)
            {
                pPixels[i].rgbReserved = 0;
            }
        }
    }

    // Return the created image
    return hBmpResult;
}

void CDpiHelper::LogicalToDeviceUnits(_Inout_ HIMAGELIST * pImageList, ImageScalingMode scalingMode)
{
    IfNullAssertRet(pImageList, "No imagelist given to convert");

    // If no scaling is required, the image can be used in current size
    if (!IsScalingRequired())
        return;

    // Create a new HIMAGELIST for the device units
    HIMAGELIST hDeviceImageList = CreateDeviceFromLogicalImage(*pImageList, scalingMode);
    // If the device image could not be created, return and keep using the original
    IfNullAssertRet(hDeviceImageList, "Failed to create scaled imagelist");

    // Delete the original image and return the converted image
    ImageList_Destroy(*pImageList);
    *pImageList = hDeviceImageList;
}

HIMAGELIST CDpiHelper::CreateDeviceFromLogicalImage(HIMAGELIST _In_ hImageList, ImageScalingMode scalingMode)
{
    IfNullAssertRetNull(hImageList, "No imagelist given to convert");

    // If no scaling is required, return an image copy
    if (!IsScalingRequired())
        return ImageList_Duplicate(hImageList);

    int nCount = ImageList_GetImageCount(hImageList);

    int cxImage = 0;
    int cyImage = 0;
    IfFailRetNull( ImageList_GetIconSize(hImageList, &cxImage, &cyImage) );

    int cxImageDevice = LogicalToDeviceUnitsX(cxImage);
    int cyImageDevice = LogicalToDeviceUnitsY(cyImage);

    // Create the new device imagelist. Use ILC_COLOR24 instead of ILC_COLOR32 because the images we're adding with
    // ImageList_AddMasked don't have alpha channel (have 0 bytes), which would otherwise be interpreted by imagelist themeing code
    // imagelist shell theming code as being completely transparent (and losing color information), later resulting in black pixels when the themed imagelist is drawn
    bool fImageListComplete = false;
    HIMAGELIST hImageListDevice = ImageList_Create(cxImageDevice, cyImageDevice, ILC_COLOR24 | ILC_MASK, nCount /*cInitial*/, 0 /*cGrow*/);
    IfNullRetNull(hImageListDevice);
    SCOPE_GUARD({ 
       if (!fImageListComplete) 
           ImageList_Destroy(hImageListDevice);
    });

    ImageList_SetBkColor(hImageListDevice, ImageList_GetBkColor(hImageList));
    
    if (nCount != 0)
    {
        CWinClientDC dcScreen(NULL);
        IfNullRetNull(dcScreen);

        CWinManagedDC dcMemoryLogical(CreateCompatibleDC(dcScreen));
        IfNullRetNull(dcMemoryLogical);

        HIMAGELIST hImageListNoTransparency = NULL;
        SCOPE_GUARD({ 
            if (hImageListNoTransparency)
               ImageList_Destroy(hImageListNoTransparency);
        });
        
        // If the source imagelist uses ILC_COLOR32, the color bitmap may have partial transparent pixels
        // If we were to paint them on a Magenta background for our ILC_COLOR24 output bitmap, those pixels 
        // would get a magenta tint. To get rid of the partial transparency, we'll create first a 24bpp 
        // Imagelist with background of Halo color, and we'll copy the images from the original list.
        // The imagelist background color is used for interpolation of partial transparent pixels.

        IMAGEINFO imageInfo = {0};
        if (ImageList_GetImageInfo(hImageList, 0, &imageInfo))
        {
            BITMAPINFO bi = {0};
            bi.bmiHeader.biSize = sizeof(bi.bmiHeader);
            
            // Call GetDIBits without the underlying array to determine bitmap attributes
            if (GetDIBits(dcScreen, imageInfo.hbmImage, /* uStartScan */ 0, /* cScanLines */ 0, /* lpvBits */ nullptr, &bi, DIB_RGB_COLORS))
            {
                if (bi.bmiHeader.biBitCount == 32)
                {
                    VSASSERT(imageInfo.hbmMask != NULL, "The imagelist contains 32bpp image with no mask, not supported yet. The results will be incorrect.");
                    hImageListNoTransparency = ImageList_Create(cxImage, cyImage, ILC_COLOR24 | ILC_MASK, nCount /*cInitial*/, 0 /*cGrow*/);
                    IfNullRetNull(hImageListNoTransparency);

                    ImageList_SetBkColor(hImageListNoTransparency, HaloColor.ToCOLORREF());
                    
                    for (int iImage = 0; iImage < nCount; iImage++)
                    {
                        // Unfortunately ImageList_Copy can only copy within same imagelist, 
                        // so we have to extract icons one by one and add into the other imagelist
                        HICON hIcon = ImageList_GetIcon(hImageList, iImage, 0);
                        IfNullRetNull(hIcon);
                        SCOPE_GUARD( DestroyIcon(hIcon); );

                        if (ImageList_AddIcon(hImageListNoTransparency, hIcon) == -1) 
                            return NULL;
                    }                    

                    // Set the background color, so further draw operations will use the mask
                    ImageList_SetBkColor(hImageListNoTransparency, CLR_NONE);
                }
            }
        }

        HIMAGELIST hImageListToDraw = hImageListNoTransparency ? hImageListNoTransparency : hImageList;

        // Use Magenta for transparency
        const Color& clrTransparency = MagentaColor;

        CWinManagedBrush brTransparent;
        brTransparent.CreateSolidBrush(clrTransparency.ToCOLORREF());
        IfNullRetNull(brTransparent);

        RECT rcImage = { 0, 0, cxImage, cyImage};

        for (int iImage = 0; iImage < nCount; iImage++)
        {
            CWinManagedBitmap bmpMemory;
            bmpMemory.CreateCompatibleBitmap(dcScreen, cxImage, cyImage);
            IfNullRetNull(bmpMemory);

            // Select the logical bitmap
            dcMemoryLogical.SelectBitmap(bmpMemory);

            // Draw image by image in dcMemoryLogical
            IfFailRetNull( dcMemoryLogical.FillRect(&rcImage, brTransparent) );
            IfFailRetNull( ImageList_Draw(hImageListToDraw, iImage, dcMemoryLogical, 0, 0, ILD_NORMAL) );

            // Restore the original bitmap in the DC
            dcMemoryLogical.SelectBitmap(dcMemoryLogical.m_hOriginalBitmap);

            // Now scale the image according with the current DPI 
            HBITMAP hbmp = bmpMemory.Detach();
            LogicalToDeviceUnits(&hbmp, scalingMode, clrTransparency);
            bmpMemory.Attach(hbmp);

            // Add the device image to the new imagelist
            if (ImageList_AddMasked(hImageListDevice, bmpMemory, clrTransparency.ToCOLORREF()) == -1)
                return NULL;
        }
    }

    // Flag that scop guard should not delete the image we'll be returning
    fImageListComplete = true;
    return hImageListDevice;
}

void CDpiHelper::LogicalToDeviceUnits(_Inout_ HICON * pIcon, _In_opt_ const SIZE * pLogicalSize) const
{
    IfNullAssertRet(pIcon, "No icon given to convert");

    // If no scaling is required, the image can be used in current size
    if (!IsScalingRequired())
        return;

    SIZE iconSize = {0};
    if (!pLogicalSize)
    {
        // First, figure out the image size
        pLogicalSize = &iconSize;
        if (!GetIconSize(*pIcon, &iconSize))
        {
            return;
        }
    }

    *pIcon = CreateDeviceImageOrReuseIcon(*pIcon, false /*fAlwaysCreate*/, pLogicalSize);
}

HICON CDpiHelper::CreateDeviceFromLogicalImage(_In_ HICON hIcon, _In_opt_ const SIZE * pLogicalSize) const
{
    IfNullAssertRetNull(hIcon, "No icon given to convert");

    SIZE iconSize = {0};
    if (!pLogicalSize)
    {
        // First, figure out the image size
        pLogicalSize = &iconSize;
        if (!GetIconSize(hIcon, &iconSize))
        {
            return DuplicateIcon(NULL, hIcon);
        }
    }

    return CreateDeviceImageOrReuseIcon(hIcon, true /*fAlwaysCreate*/, pLogicalSize);
}

bool CDpiHelper::GetIconSize(_In_ HICON hIcon, _Out_ SIZE * pSize) const
{
    bool fGotSize  = false;

    ICONINFO iconInfo = {0};
    if (GetIconInfo(hIcon, &iconInfo))
    {
        BITMAP bmIconsBitmap = {0};
        if ( ::GetObject(iconInfo.hbmColor,sizeof(bmIconsBitmap), &bmIconsBitmap) )
        {
            pSize->cx = bmIconsBitmap.bmWidth;
            pSize->cy = bmIconsBitmap.bmHeight;
            fGotSize = true;
        }

        ::DeleteObject(iconInfo.hbmMask);
        ::DeleteObject(iconInfo.hbmColor);
    }

    return fGotSize;
}

HICON CDpiHelper::CreateDeviceImageOrReuseIcon(_In_ HICON hIcon, bool fAlwaysCreate, const SIZE * pIconSize) const
{
    int cxIcon = LogicalToDeviceUnitsX(pIconSize->cx);
    int cyIcon = LogicalToDeviceUnitsX(pIconSize->cy);

    UINT flags = fAlwaysCreate ? 0 : (LR_COPYDELETEORG | LR_COPYRETURNORG);

    HICON hDeviceIcon = static_cast<HICON>(::CopyImage(hIcon, IMAGE_ICON, cxIcon, cyIcon, flags | LR_COPYFROMRESOURCE));
    if (hDeviceIcon == NULL)
    {
        // Couldn't load from resource (the image was not shared), try stretching the current image
        hDeviceIcon = static_cast<HICON>(::CopyImage(hIcon, IMAGE_ICON, cxIcon, cyIcon, flags));
        if (hDeviceIcon == NULL)
        {
            hDeviceIcon = fAlwaysCreate ? DuplicateIcon(NULL, hIcon) : hIcon;
        }
    }

    return hDeviceIcon;
}

bool DpiHelper::m_fInitialized = false;
int  DpiHelper::m_DeviceDpiX = k_DefaultLogicalDpi;
int  DpiHelper::m_DeviceDpiY = k_DefaultLogicalDpi;

void DpiHelper::Initialize()
{
    if (!m_fInitialized)
    {
        CWinClientDC dcScreen(NULL);
        if (dcScreen)
        {
            m_DeviceDpiX = dcScreen.GetDeviceCaps(LOGPIXELSX);
            m_DeviceDpiY = dcScreen.GetDeviceCaps(LOGPIXELSY);
        }
        else
        {
            m_DeviceDpiX = k_DefaultLogicalDpi;
            m_DeviceDpiY = k_DefaultLogicalDpi;
        }
    
        m_fInitialized = true;
    }
}

CDpiHelper* DpiHelper::GetDefaultHelper()
{
    static CDpiHelper* s_pDefaultHelper = nullptr;
    if (s_pDefaultHelper == nullptr)
    {
        s_pDefaultHelper = GetHelper(100);
        VSASSERT(s_pDefaultHelper != nullptr, "Cannot create DPI scaling helper for default 96dpi");
    }
    return s_pDefaultHelper;
}

// Thread protection on accessing the DPI Helpers map
CComAutoCriticalSection DpiHelper::s_critSection;

// Returns a CDpiHelper that can scale images created for the specified DPI zoom factor, or nullptr if we run out of memory
CDpiHelper* DpiHelper::GetHelper(int zoomPercents)
{
    // Protect multi-threaded access to the helpers map
    CComCritSecLock<CComCriticalSection> lock(s_critSection);

    // Also do the initialization within the critical section
    Initialize();

    static map<int, unique_ptr<CDpiHelper>> mapHelpers;
    auto mapIter = mapHelpers.find(zoomPercents);
    if (mapIter == mapHelpers.end())
    {
        try
        {
            unique_ptr<CDpiHelper> ptrHelper( new CDpiHelper(
                m_DeviceDpiX, m_DeviceDpiY, 
                MulDiv(k_DefaultLogicalDpi, zoomPercents, 100), 
                MulDiv(k_DefaultLogicalDpi, zoomPercents, 100)));

            mapIter = mapHelpers.insert( make_pair(zoomPercents, move(ptrHelper)) ).first;
        }
        catch (const bad_alloc&)
        {
            return nullptr;
        }
    }

    return mapIter->second.get();
}


// Get device DPI.
int DpiHelper::GetDeviceDpiX() 
{ 
    IfNullRetX(GetDefaultHelper(), m_DeviceDpiY);
    return GetDefaultHelper()->GetDeviceDpiX();
}

int DpiHelper::GetDeviceDpiY() 
{ 
    IfNullRetX(GetDefaultHelper(), m_DeviceDpiY);
    return GetDefaultHelper()->GetDeviceDpiY();
}

// Get logical DPI.
int DpiHelper::GetLogicalDpiX() 
{ 
    IfNullRetX(GetDefaultHelper(), m_DeviceDpiX);
    return GetDefaultHelper()->GetLogicalDpiX();
}

int DpiHelper::GetLogicalDpiY() 
{ 
    IfNullRetX(GetDefaultHelper(), m_DeviceDpiY);
    return GetDefaultHelper()->GetLogicalDpiY();
}

// Return whether scaling is required
bool DpiHelper::IsScalingRequired()
{
    IfNullRetX(GetDefaultHelper(), false);
    return GetDefaultHelper()->IsScalingRequired();
}

// Return horizontal and vertical scaling factors
double DpiHelper::DeviceToLogicalUnitsScalingFactorX()
{
    IfNullRetX(GetDefaultHelper(), 1);
    return GetDefaultHelper()->DeviceToLogicalUnitsScalingFactorX();
}

double DpiHelper::DeviceToLogicalUnitsScalingFactorY()
{
    IfNullRetX(GetDefaultHelper(), 1);
    return GetDefaultHelper()->DeviceToLogicalUnitsScalingFactorY();
}

double DpiHelper::LogicalToDeviceUnitsScalingFactorX()
{
    IfNullRetX(GetDefaultHelper(), 1);
    return GetDefaultHelper()->LogicalToDeviceUnitsScalingFactorX();
}

double DpiHelper::LogicalToDeviceUnitsScalingFactorY()
{
    IfNullRetX(GetDefaultHelper(), 1);
    return GetDefaultHelper()->LogicalToDeviceUnitsScalingFactorY();
}

// Converts between logical and device units.
int DpiHelper::LogicalToDeviceUnitsX(int x) 
{ 
    IfNullRetX(GetDefaultHelper(), x);
    return GetDefaultHelper()->LogicalToDeviceUnitsX(x);
}

int DpiHelper::LogicalToDeviceUnitsY(int y) 
{ 
    IfNullRetX(GetDefaultHelper(), y);
    return GetDefaultHelper()->LogicalToDeviceUnitsY(y);
}

// Converts between device and logical units.
int DpiHelper::DeviceToLogicalUnitsX(int x) 
{ 
    IfNullRetX(GetDefaultHelper(), x);
    return GetDefaultHelper()->DeviceToLogicalUnitsX(x);
}

int DpiHelper::DeviceToLogicalUnitsY(int y) 
{ 
    IfNullRetX(GetDefaultHelper(), y);
    return GetDefaultHelper()->DeviceToLogicalUnitsY(y);
}

// Converts from logical units to device units.
void DpiHelper::LogicalToDeviceUnits(_Inout_ RECT * pRect)
{
    IfNullRet(GetDefaultHelper());
    return GetDefaultHelper()->LogicalToDeviceUnits(pRect);
}

void DpiHelper::LogicalToDeviceUnits(_Inout_ POINT * pPoint)
{
    IfNullRet(GetDefaultHelper());
    return GetDefaultHelper()->LogicalToDeviceUnits(pPoint);
}

// Converts from device units to logical units.
void DpiHelper::DeviceToLogicalUnits(_Inout_ RECT * pRect)
{
    IfNullRet(GetDefaultHelper());
    return GetDefaultHelper()->DeviceToLogicalUnits(pRect);
}

void DpiHelper::DeviceToLogicalUnits(_Inout_ POINT * pPoint)
{
    IfNullRet(GetDefaultHelper());
    return GetDefaultHelper()->DeviceToLogicalUnits(pPoint);
}

// Convert a point size (1/72 of an inch) to raw pixels.
int DpiHelper::PointsToDeviceUnits(int pt) 
{ 
    IfNullRetX(GetDefaultHelper(), 0);
    return GetDefaultHelper()->PointsToDeviceUnits(pt);
}

// Determine the screen dimensions in logical units.
int DpiHelper::LogicalScreenWidth() 
{ 
    IfNullRetX(GetDefaultHelper(), 0);
    return GetDefaultHelper()->LogicalScreenWidth();
}

int DpiHelper::LogicalScreenHeight() 
{ 
    IfNullRetX(GetDefaultHelper(), 0);
    return GetDefaultHelper()->LogicalScreenHeight();
}

// Determine if screen resolution meets minimum requirements in logical pixels.
bool DpiHelper::IsResolutionAtLeast(int cxMin, int cyMin)
{ 
    IfNullRetX(GetDefaultHelper(), false);
    return GetDefaultHelper()->IsResolutionAtLeast(cxMin, cyMin);
}

// Return the monitor information in logical units
BOOL DpiHelper::GetLogicalMonitorInfo(_In_ HMONITOR hMonitor, _Out_ LPMONITORINFO lpmi)
{
    IfNullRetX(GetDefaultHelper(), FALSE);
    return GetDefaultHelper()->GetLogicalMonitorInfo(hMonitor, lpmi);
}

// Convert GdiplusImage from logical to device units
void DpiHelper::LogicalToDeviceUnits(_Inout_ VsUI::GdiplusImage * pImage, ImageScalingMode scalingMode, Color clrBackground)
{
    IfNullRet(GetDefaultHelper());
    return GetDefaultHelper()->LogicalToDeviceUnits(pImage, scalingMode, clrBackground);
}
 
// Creates new GdiplusImage from logical to device units
unique_ptr<VsUI::GdiplusImage> DpiHelper::CreateDeviceFromLogicalImage(_In_ VsUI::GdiplusImage* pImage, ImageScalingMode scalingMode, Color clrBackground)
{
    IfNullRetNull(GetDefaultHelper());
    return GetDefaultHelper()->CreateDeviceFromLogicalImage(pImage, scalingMode, clrBackground);
}

void DpiHelper::LogicalToDeviceUnits(_Inout_ HBITMAP * pImage, ImageScalingMode scalingMode, Color clrBackground)
{
    IfNullRet(GetDefaultHelper());
    return GetDefaultHelper()->LogicalToDeviceUnits(pImage, scalingMode, clrBackground);
}

HBITMAP DpiHelper::CreateDeviceFromLogicalImage(HBITMAP _In_ hImage, ImageScalingMode scalingMode, Color clrBackground)
{
    IfNullRetNull(GetDefaultHelper());
    return GetDefaultHelper()->CreateDeviceFromLogicalImage(hImage, scalingMode, clrBackground);
}

void DpiHelper::LogicalToDeviceUnits(_Inout_ HIMAGELIST * pImageList, ImageScalingMode scalingMode)
{
    IfNullRet(GetDefaultHelper());
    return GetDefaultHelper()->LogicalToDeviceUnits(pImageList, scalingMode);
}

HIMAGELIST DpiHelper::CreateDeviceFromLogicalImage(HIMAGELIST _In_ hImageList, ImageScalingMode scalingMode)
{
    IfNullRetNull(GetDefaultHelper());
    return GetDefaultHelper()->CreateDeviceFromLogicalImage(hImageList, scalingMode);
}

void DpiHelper::LogicalToDeviceUnits(_Inout_ HICON * pIcon, _In_opt_ const SIZE * pLogicalSize)
{
    IfNullRet(GetDefaultHelper());
    return GetDefaultHelper()->LogicalToDeviceUnits(pIcon, pLogicalSize);
}

HICON DpiHelper::CreateDeviceFromLogicalImage(_In_ HICON hIcon, _In_opt_ const SIZE * pLogicalSize)
{
    IfNullRetNull(GetDefaultHelper());
    return GetDefaultHelper()->CreateDeviceFromLogicalImage(hIcon, pLogicalSize);
}

} // namespace