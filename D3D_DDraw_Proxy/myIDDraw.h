#pragma once

// This one is set up to work with IDirectDraw7 only !!

class myIDDraw : public IDirectDraw7
{
public:
    myIDDraw(LPDIRECTDRAW7 FAR pOriginal);
    virtual ~myIDDraw(void);

	// IUnknown functions
	HRESULT WINAPI IUnknown::QueryInterface (REFIID a, LPVOID FAR * b);
    ULONG   WINAPI IUnknown::AddRef(void);
    ULONG   WINAPI IUnknown::Release(void);

	// v7 functions
	HRESULT  WINAPI IDirectDraw7::Compact(void);
	HRESULT  WINAPI IDirectDraw7::CreateClipper(DWORD dwFlags,LPDIRECTDRAWCLIPPER FAR *lplpDDClipper,IUnknown FAR *pUnkOuter);
	HRESULT  WINAPI IDirectDraw7::CreatePalette(DWORD dwFlags,LPPALETTEENTRY lpDDColorArray,LPDIRECTDRAWPALETTE FAR *lplpDDPalette,IUnknown FAR *pUnkOuter );
	HRESULT  WINAPI IDirectDraw7::CreateSurface(LPDDSURFACEDESC2 lpDDSurfaceDesc2,LPDIRECTDRAWSURFACE7 FAR *lplpDDSurface,IUnknown FAR *pUnkOuter);
	HRESULT  WINAPI IDirectDraw7::DuplicateSurface(LPDIRECTDRAWSURFACE7 lpDDSurface,LPDIRECTDRAWSURFACE7 FAR *lplpDupDDSurface);
	HRESULT  WINAPI IDirectDraw7::EnumDisplayModes(DWORD dwFlags,LPDDSURFACEDESC2 lpDDSurfaceDesc2,LPVOID lpContext,LPDDENUMMODESCALLBACK2 lpEnumModesCallback);
	HRESULT  WINAPI IDirectDraw7::EnumSurfaces(DWORD dwFlags,LPDDSURFACEDESC2 lpDDSD2,LPVOID lpContext,LPDDENUMSURFACESCALLBACK7 lpEnumSurfacesCallback);
	HRESULT  WINAPI IDirectDraw7::FlipToGDISurface(void);
	HRESULT  WINAPI IDirectDraw7::GetCaps(LPDDCAPS lpDDDriverCaps, LPDDCAPS lpDDHELCaps);
    HRESULT  WINAPI IDirectDraw7::GetDisplayMode(LPDDSURFACEDESC2 lpDDSurfaceDesc2);
    HRESULT  WINAPI IDirectDraw7::GetFourCCCodes(LPDWORD lpNumCodes,LPDWORD lpCodes);
    HRESULT  WINAPI IDirectDraw7::GetGDISurface(LPDIRECTDRAWSURFACE7 FAR *lplpGDIDDSSurface);
    HRESULT  WINAPI IDirectDraw7::GetMonitorFrequency(LPDWORD lpdwFrequency);
    HRESULT  WINAPI IDirectDraw7::GetScanLine(LPDWORD lpdwScanLine);
    HRESULT  WINAPI IDirectDraw7::GetVerticalBlankStatus(LPBOOL lpbIsInVB);
    HRESULT  WINAPI IDirectDraw7::Initialize(GUID FAR *lpGUID);
    HRESULT  WINAPI IDirectDraw7::RestoreDisplayMode(void);
	HRESULT  WINAPI IDirectDraw7::SetCooperativeLevel(HWND hWnd,DWORD dwFlags);
	HRESULT  WINAPI IDirectDraw7::SetDisplayMode(DWORD dwWidth,DWORD dwHeight,DWORD dwBPP,DWORD dwRefreshRate,DWORD dwFlags);
	HRESULT  WINAPI IDirectDraw7::WaitForVerticalBlank(DWORD dwFlags, HANDLE hEvent);

	/***  v2 interface ***/
    HRESULT  WINAPI IDirectDraw7::GetAvailableVidMem(LPDDSCAPS2 lpDDSCaps2, LPDWORD lpdwTotal,LPDWORD lpdwFree);

    /***  V4 Interface ***/
    HRESULT  WINAPI IDirectDraw7::GetSurfaceFromDC(HDC hdc,LPDIRECTDRAWSURFACE7 * lpDDS);
    HRESULT  WINAPI IDirectDraw7::RestoreAllSurfaces(void);
    HRESULT  WINAPI IDirectDraw7::TestCooperativeLevel(void);
    HRESULT  WINAPI IDirectDraw7::GetDeviceIdentifier(LPDDDEVICEIDENTIFIER2 lpdddi,DWORD dwFlags);
    HRESULT  WINAPI IDirectDraw7::StartModeTest(LPSIZE lpModesToTest,DWORD dwNumEntries, DWORD dwFlags);
    HRESULT  WINAPI IDirectDraw7::EvaluateMode(DWORD dwFlags,DWORD  *pSecondsUntilTimeout);
    // The original DDraw function definitions END

private:
   	LPDIRECTDRAW7 FAR m_pIDDraw;
	LPDIRECT3D7 FAR m_pID3D7;
};

HRESULT WINAPI EnumSurfacesCallback(LPDIRECTDRAWSURFACE7 a, LPDDSURFACEDESC2 b, LPVOID c);

