//Copyright (c) Microsoft.  All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//-----------------------------------------------------------------------
// </copyright>
// <summary>Assembly info.</summary>

//-----------------------------------------------------------------------------
// Wrapper around Gdiplus::Bitmap
// Uses GDI+ so we can work entirely in 32bpp ARGB mode
// Note: Some of this is copied from ATL::CImage (particularly the CInitGDIPlus
// helper). The key difference is that we store the image as Gdiplus::Bitmap
// internally.
//-----------------------------------------------------------------------------
#pragma once

#include <shlwapi.h> // For PathFileExists
#pragma comment(lib, "shlwapi.lib")

// we want the lastest API supported
#ifndef GDIPVER 
#define GDIPVER 0x0110
#endif

#pragma push_macro("new")
#undef new
#pragma push_macro("delete")
#undef delete
#include <gdiplus.h>
#pragma pop_macro("delete")
#pragma pop_macro("new")

#pragma comment(lib, "gdiplus.lib")
#pragma comment(lib, "msimg32.lib")

#include <intrin.h>
#include <atlbase.h>
#include <algorithm>
#include <functional>

namespace VsUI
{
    extern const Gdiplus::Color TransparentColor;
    extern const Gdiplus::Color MagentaColor;
    extern const Gdiplus::Color NearGreenColor;
    extern const Gdiplus::Color HaloColor;
    extern const Gdiplus::Color TransparentHaloColor;

    class GdiplusImage
    {
    private:
        class CInitGDIPlus
        {
        public:
            CInitGDIPlus() : m_GdiplusToken(0), m_GdiplusImageObjects(0)
            {
            }

            ~CInitGDIPlus()
            {
                ReleaseGDIPlus();
            }

            bool Init();
            void ReleaseGDIPlus();
            void IncreaseImageCount();
            void DecreaseImageCount();

        private:
            ULONG_PTR InterlockedExchangeToken( ULONG_PTR token );
            volatile ULONG_PTR m_GdiplusToken;
            LONG m_GdiplusImageObjects;
        };

    public:
        class ImageDC
        {
            ATL::CAutoPtr<Gdiplus::Graphics> m_pGraphics;
            HDC m_hDC;

        public:
            ImageDC(GdiplusImage& img);
            ~ImageDC();

            operator HDC() const
            {
                return m_hDC;
            }
        };

        GdiplusImage();
        ~GdiplusImage();
        
        // Exchange the content of the 2 images
        GdiplusImage& operator=(GdiplusImage&& rhs);

        bool IsLoaded() const
        {
            return m_pBitmap.m_p != NULL;
        }

        operator Gdiplus::Bitmap* () const
        {
            return GetBitmap();
        }
        
        Gdiplus::Bitmap* GetBitmap() const
        {
            return m_pBitmap;
        }

        // Releases a loaded image
        void Release();
        
        // Return loaded image dimensions
        int GetWidth() const;
        int GetHeight() const;

        // Create an image with the specified size and format
        void Create( int width, int height, const Gdiplus::PixelFormat format = PixelFormat32bppARGB );
        
        // Attach to an existing HBITMAP. If the HBITMAP is a 32bpp DIB,then we'll create an ARGB Gdiplus image.
        void Attach( HBITMAP hBmp );

        // Convert the image to an HBITMAP and detach ownership.
        HBITMAP Detach( const Gdiplus::Color& backgroundColor = TransparentColor );

        // Attach to an existing HICON
        void AttachIcon( HICON hIcon );

        // Convert the image to an HICON and detach ownership.
        HICON DetachIcon();

        // Get a Gdiplus graphics surface for drawing onto the loaded image
        Gdiplus::Graphics* GetGraphics();

        // Load the image from a file with formats that Gdiplus supports (BMP, PNG, JPG etc)
        HRESULT Load( _In_z_ LPCWSTR wszFilename );

        // Load the image from resources with formats that Gdiplus supports (BMP, PNG, JPG etc)
        HRESULT LoadFromResource( HINSTANCE hInstance, UINT nIDResource, _In_z_ LPCWSTR wszResourceType );

        // Load the image from resources. Try PNG first and then BMP format
        HRESULT LoadFromPngOrBmp( HINSTANCE hInstance, UINT nIDResource );

        // Save to the given stream in the specified format
        HRESULT Save( _In_ IStream* pStream, const GUID& format = Gdiplus::ImageFormatPNG );

        // Save to the given file in the specified format
        HRESULT Save( _In_z_ LPCWSTR wszFilename, const GUID& format = Gdiplus::ImageFormatPNG );

        // Converts the bitmap to 32bpp ARGB if necessary and converts all pixels of clrTransparency color to be fully transparent.
        HRESULT MakeTransparent(const Gdiplus::Color& clrTransparency = MagentaColor);
        
        // Apply a processor function to all bitmap pixels 
        static void ProcessBitmapBits(_In_ Gdiplus::Bitmap * pBitmap, std::function<void (_Inout_ Gdiplus::ARGB* pPixelData)> pixelProcessor);

    private:

        // Create an in-memory stream over a resource. The resource must have been found via FindResource
        static HRESULT CreateStreamOnResource( HINSTANCE hInst, HRSRC hResource, _Out_ IStream** ppStream );

        // Create a 32bpp ARGB Gdiplus::Bitmap from a DIBSECTION
        static Gdiplus::Bitmap* CreateARGBBitmapFromDIB( const DIBSECTION& dib );
        
        // Release the current bitmap and attaches to the new one
        void SetBitmap(Gdiplus::Bitmap* pBitmap);

        // Locates the codec for the specified format and calls the save function to save the bitmap
        HRESULT SaveBitmap(const GUID& format, std::function< Gdiplus::Status (_In_ const CLSID * clsidEncoder) > saveFunction );

    private:
        static CInitGDIPlus s_initGDIPlus;
        ATL::CAutoPtr<Gdiplus::Bitmap> m_pBitmap;
    };

};  // namespace VsUI