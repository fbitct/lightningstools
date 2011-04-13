#include "StdAfx.h"
/*
PROXY CLASS FOR THE IDIRECTDRAW7 COM INTERFACE
*/
// This one is set up to work with IDirectDraw7 only !!
static BOOL m_bIDDS7HookSet=false;
static BOOL m_bIDDS4HookSet=false;
static BOOL m_bIDDS3HookSet=false;
static BOOL m_bIDDS2HookSet=false;
static BOOL m_bIDDS1HookSet=false;
// ---------------------------------------------------------------------------------------
myIDDraw::myIDDraw(LPDIRECTDRAW7 pOriginal)
{
	//OutputDebugString("DDRAWPROXY: myIDDraw Constructor reached.\r\n");
	m_pIDDraw =NULL;
	if (pOriginal) 
	{
		m_pIDDraw = pOriginal;
		m_pIDDraw->AddRef();
	}
	//OutputDebugString("DDRAWPROXY: myIDDraw Constructor exited.\r\n");
}

// ---------------------------------------------------------------------------------------
myIDDraw::~myIDDraw(void)
{
	if (m_pIDDraw)
	{
		m_pIDDraw->Release();
	}
	//OutputDebugString("DDRAWPROXY: myIDDraw Destructor reached.\r\n");
	//OutputDebugString("DDRAWPROXY: myIDDraw Destructor exited.\r\n");
}

// ---------------------------------------------------------------------------------------
HRESULT WINAPI myIDDraw::QueryInterface (REFIID riid, LPVOID* obp)
{
	//OutputDebugString("DDRAWPROXY: myIDDraw::QueryInterface reached.\r\n");
	*obp = NULL;

	// call this to increase AddRef at original object
	// and to check if such an interface is there
	HRESULT hRes=E_FAIL;
	if (riid == IID_IDirect3D7) 
	{
		LPDIRECT3D7 lpd3d7=NULL;
		hRes = m_pIDDraw->QueryInterface(riid, ( void** ) &lpd3d7);
		if (lpd3d7 && SUCCEEDED(hRes)) {
			m_pID3D7 = static_cast <IDirect3D7 *>( new myID3D7(lpd3d7));
			*obp=m_pID3D7;
		}
	}
	else if (riid == IID_IDirectDraw7) 
	{
		hRes = m_pIDDraw->QueryInterface(riid,obp);
		if (*obp && SUCCEEDED(hRes)) {
			*obp = static_cast<IDirectDraw7 *>(this);
		}
	}
	else 
	{ 
		//OutputDebugString("Unexpected interface requested.\r\n");
		hRes = m_pIDDraw->QueryInterface(riid,obp);
	}
	
	//OutputDebugString("DDRAWPROXY: myIDDraw::QueryInterface exited.\r\n");
	return (hRes);
}

// ---------------------------------------------------------------------------------------
ULONG   WINAPI myIDDraw::AddRef(void)
{
	//OutputDebugString("DDRAWPROXY: myIDDraw::AddRef reached.\r\n");
	return m_pIDDraw->AddRef();
	//OutputDebugString("DDRAWPROXY: myIDDraw::AddRef exited.\r\n");
}

// ---------------------------------------------------------------------------------------
ULONG   WINAPI myIDDraw::Release(void)
{
	//OutputDebugString("DDRAWPROXY: myIDDraw::Release reached.\r\n");
	
	// call original routine
	ULONG count=0;
	
    // in case no further Ref is there, the Original Object has deleted itself
	// so do we here
	if (m_pIDDraw) {
		count = m_pIDDraw->Release();
		if (count == 0) 
		{
			m_pIDDraw = NULL;		
			delete(this); 
		}
	}
	//OutputDebugString("DDRAWPROXY: myIDDraw::Release exited.\r\n");
	return(count);
}

// ---------------------------------------------------------------------------------------
HRESULT  WINAPI myIDDraw::Compact(void)
{
	//OutputDebugString("DDRAWPROXY: myIDDraw::Compact reached.\r\n");
	return m_pIDDraw->Compact();
	//OutputDebugString("DDRAWPROXY: myIDDraw::Compact exited.\r\n");
}

// ---------------------------------------------------------------------------------------
HRESULT  WINAPI myIDDraw::CreateClipper(DWORD dwFlags,LPDIRECTDRAWCLIPPER FAR *lplpDDClipper,IUnknown FAR *pUnkOuter)
{
	//OutputDebugString("DDRAWPROXY: myIDDraw::CreateClipper reached.\r\n");
	return m_pIDDraw->CreateClipper(dwFlags, lplpDDClipper, pUnkOuter);
	//OutputDebugString("DDRAWPROXY: myIDDraw::CreateClipper exited.\r\n");
}

// ---------------------------------------------------------------------------------------
HRESULT  WINAPI myIDDraw::CreatePalette(DWORD dwFlags,LPPALETTEENTRY lpDDColorArray,LPDIRECTDRAWPALETTE FAR *lplpDDPalette,IUnknown FAR *pUnkOuter )
{
	//OutputDebugString("DDRAWPROXY: myIDDraw::CreatePalette reached.\r\n");
	return m_pIDDraw->CreatePalette(dwFlags, lpDDColorArray, lplpDDPalette, pUnkOuter);
	//OutputDebugString("DDRAWPROXY: myIDDraw::CreatePalette exited.\r\n");
}

// ---------------------------------------------------------------------------------------
HRESULT  WINAPI myIDDraw::CreateSurface(LPDDSURFACEDESC2 lpDDSurfaceDesc2,LPDIRECTDRAWSURFACE7 FAR *lplpDDSurface,IUnknown FAR *pUnkOuter)
{
	extern LPDIRECTDRAWSURFACE7 g_p3dCockpitRTT;

	//OutputDebugString("DDRAWPROXY: myIDDraw::CreateSurface reached.\r\n");
	LPDIRECTDRAWSURFACE7 lpDDS7 = NULL;
	*lplpDDSurface = NULL;
	HRESULT retVal=E_FAIL;
	if (m_pIDDraw) {
		if (
				((lpDDSurfaceDesc2->ddsCaps.dwCaps & DDSCAPS_VIDEOMEMORY) == DDSCAPS_VIDEOMEMORY) 
				&& 
				((lpDDSurfaceDesc2->ddsCaps.dwCaps & DDSCAPS_TEXTURE) == DDSCAPS_TEXTURE) 
				&& 
				((lpDDSurfaceDesc2->ddsCaps.dwCaps & DDSCAPS_3DDEVICE) == DDSCAPS_3DDEVICE) 
				&& 
				!((lpDDSurfaceDesc2->ddsCaps.dwCaps & DDSCAPS_FRONTBUFFER) == DDSCAPS_FRONTBUFFER) 
				&&
				!((lpDDSurfaceDesc2->ddsCaps.dwCaps & DDSCAPS_BACKBUFFER) == DDSCAPS_BACKBUFFER) 
				&&
				!((lpDDSurfaceDesc2->ddsCaps.dwCaps & DDSCAPS_BACKBUFFER) == DDSCAPS_COMPLEX) 
				&&
				(lpDDSurfaceDesc2->dwHeight >=512)
				&& 
				(lpDDSurfaceDesc2->dwWidth >=512)
				
			)
		{
			//lpDDSurfaceDesc2->dwHeight =1024;
			//lpDDSurfaceDesc2->dwWidth=1024;

			//lpDDSurfaceDesc2->dwFlags |= DDSD_CAPS;
			//lpDDSurfaceDesc2->ddsCaps.dwCaps2 |= DDSCAPS2_HINTDYNAMIC;

			//if a vidmem RTT is being requested and its size is >=512x512, then let's try to create it in non-local vidmem first; if that fails we'll create it
			//in local vidmem per the original request -- but the advantage of creating it in nonlocal vidmem is we can blit it to system memory without incurring a bus
			//penalty
			//lpDDSurfaceDesc2->ddsCaps.dwCaps |= (DDSCAPS_NONLOCALVIDMEM);
			retVal = m_pIDDraw->CreateSurface(lpDDSurfaceDesc2, &lpDDS7, pUnkOuter);
			//if (FAILED(retVal)) 
			//{
			//	lpDDSurfaceDesc2->ddsCaps.dwCaps &= (~DDSCAPS_NONLOCALVIDMEM);
			//	retVal = m_pIDDraw->CreateSurface(lpDDSurfaceDesc2, &lpDDS7, pUnkOuter);
			//}
		}
		else {
			retVal = m_pIDDraw->CreateSurface(lpDDSurfaceDesc2, &lpDDS7, pUnkOuter);
		}
		if (lpDDS7 && SUCCEEDED(retVal)) {
			//*lplpDDSurface = new myIDDS7(lpDDS7);
			
			*lplpDDSurface = lpDDS7;
			if (!m_bIDDS7HookSet) {
				HookDirectDrawSurface7Interface(lpDDS7);
				m_bIDDS7HookSet=true;
			}
		}
	}
	return retVal;
	//OutputDebugString("DDRAWPROXY: myIDDraw::CreateSurface exited.\r\n");
}

// ---------------------------------------------------------------------------------------
HRESULT  WINAPI myIDDraw::DuplicateSurface(LPDIRECTDRAWSURFACE7 lpDDSurface,LPDIRECTDRAWSURFACE7 FAR *lplpDupDDSurface)
{
	//OutputDebugString("DDRAWPROXY: myIDDraw::DuplicateSurface reached.\r\n");
	return m_pIDDraw->DuplicateSurface(lpDDSurface, lplpDupDDSurface);
	//OutputDebugString("DDRAWPROXY: myIDDraw::DuplicateSurface exited.\r\n");
}

// ---------------------------------------------------------------------------------------
HRESULT  WINAPI myIDDraw::EnumDisplayModes(DWORD dwFlags,LPDDSURFACEDESC2 lpDDSurfaceDesc2,LPVOID lpContext,LPDDENUMMODESCALLBACK2 lpEnumModesCallback)
{
	//OutputDebugString("DDRAWPROXY: myIDDraw::EnumDisplayModes reached.\r\n");
	return m_pIDDraw->EnumDisplayModes(dwFlags, lpDDSurfaceDesc2, lpContext, lpEnumModesCallback);
	//OutputDebugString("DDRAWPROXY: myIDDraw::EnumDisplayModes exited.\r\n");
}

// ---------------------------------------------------------------------------------------
HRESULT  WINAPI myIDDraw::EnumSurfaces(DWORD dwFlags,LPDDSURFACEDESC2 lpDDSD2,LPVOID lpContext,LPDDENUMSURFACESCALLBACK7 lpEnumSurfacesCallback)
{
	//OutputDebugString("DDRAWPROXY: myIDDraw::EnumSurfaces reached.\r\n");
	HRESULT toReturn=m_pIDDraw->EnumSurfaces(dwFlags, lpDDSD2, lpContext, lpEnumSurfacesCallback);
	//OutputDebugString("DDRAWPROXY: myIDDraw::EnumSurfaces exited.\r\n");
	return (toReturn);
}

// ---------------------------------------------------------------------------------------
HRESULT  WINAPI myIDDraw::FlipToGDISurface(void)
{
	//OutputDebugString("DDRAWPROXY: myIDDraw::FlipToGDISurface reached.\r\n");
	return m_pIDDraw->FlipToGDISurface();
	//OutputDebugString("DDRAWPROXY: myIDDraw::FlipToGDISurface exited.\r\n");
}

// ---------------------------------------------------------------------------------------
HRESULT  WINAPI myIDDraw::GetCaps(LPDDCAPS lpDDDriverCaps, LPDDCAPS lpDDHELCaps)
{
	//OutputDebugString("DDRAWPROXY: myIDDraw::GetCaps reached.\r\n");
	return m_pIDDraw->GetCaps(lpDDDriverCaps, lpDDHELCaps);
	//OutputDebugString("DDRAWPROXY: myIDDraw::GetCaps exited.\r\n");
}

// ---------------------------------------------------------------------------------------
HRESULT  WINAPI myIDDraw::GetDisplayMode(LPDDSURFACEDESC2 lpDDSurfaceDesc2)
{
	//OutputDebugString("DDRAWPROXY: myIDDraw::GetDisplayMode reached.\r\n");
	return m_pIDDraw->GetDisplayMode(lpDDSurfaceDesc2);
	//OutputDebugString("DDRAWPROXY: myIDDraw::GetDisplayMode exited.\r\n");
}

// ---------------------------------------------------------------------------------------
HRESULT  WINAPI myIDDraw::GetFourCCCodes(LPDWORD lpNumCodes,LPDWORD lpCodes)
{
	//OutputDebugString("DDRAWPROXY: myIDDraw::GetFourCCCodes reached.\r\n");
	return m_pIDDraw->GetFourCCCodes(lpNumCodes, lpCodes);
	//OutputDebugString("DDRAWPROXY: myIDDraw::GetFourCCCodes exited.\r\n");
}

// ---------------------------------------------------------------------------------------
HRESULT  WINAPI myIDDraw::GetGDISurface(LPDIRECTDRAWSURFACE7 FAR *lplpGDIDDSSurface)
{
	//OutputDebugString("DDRAWPROXY: myIDDraw::GetGDISurface reached.\r\n");
	return m_pIDDraw->GetGDISurface(lplpGDIDDSSurface);
	//OutputDebugString("DDRAWPROXY: myIDDraw::GetGDISurface exited.\r\n");
}

// ---------------------------------------------------------------------------------------
HRESULT  WINAPI myIDDraw::GetMonitorFrequency(LPDWORD lpdwFrequency)
{
	//OutputDebugString("DDRAWPROXY: myIDDraw::GetMonitorFrequency reached.\r\n");
	return m_pIDDraw->GetMonitorFrequency(lpdwFrequency);
	//OutputDebugString("DDRAWPROXY: myIDDraw::GetMonitorFrequency exited.\r\n");
}

// ---------------------------------------------------------------------------------------
HRESULT  WINAPI myIDDraw::GetScanLine(LPDWORD lpdwScanLine)
{
	//OutputDebugString("DDRAWPROXY: myIDDraw::GetScanLine reached.\r\n");
	return m_pIDDraw->GetScanLine(lpdwScanLine);
	//OutputDebugString("DDRAWPROXY: myIDDraw::GetScanLine exited.\r\n");
}

// ---------------------------------------------------------------------------------------
HRESULT  WINAPI myIDDraw::GetVerticalBlankStatus(LPBOOL lpbIsInVB)
{
	//OutputDebugString("DDRAWPROXY: myIDDraw::GetVerticalBlankStatus reached.\r\n");
	return m_pIDDraw->GetVerticalBlankStatus(lpbIsInVB);
	//OutputDebugString("DDRAWPROXY: myIDDraw::GetVerticalBlankStatus exited.\r\n");
}

// ---------------------------------------------------------------------------------------
HRESULT  WINAPI myIDDraw::Initialize(GUID FAR *lpGUID)
{
	//OutputDebugString("DDRAWPROXY: myIDDraw::Initialize reached.\r\n");
	return m_pIDDraw->Initialize(lpGUID);
	//OutputDebugString("DDRAWPROXY: myIDDraw::Initialize exited.\r\n");
}

// ---------------------------------------------------------------------------------------
HRESULT  WINAPI myIDDraw::RestoreDisplayMode(void)
{
	//OutputDebugString("DDRAWPROXY: myIDDraw::RestoreDisplayMode reached.\r\n");
	return m_pIDDraw->RestoreDisplayMode();
	//OutputDebugString("DDRAWPROXY: myIDDraw::RestoreDisplayMode exited.\r\n");
}

// ---------------------------------------------------------------------------------------
HRESULT  WINAPI myIDDraw::SetCooperativeLevel(HWND hWnd,DWORD dwFlags)
{
	//OutputDebugString("DDRAWPROXY: myIDDraw::SetCooperativeLevel reached.\r\n");
	if ((dwFlags & (DDSCL_FULLSCREEN |DDSCL_EXCLUSIVE)) !=(DDSCL_FULLSCREEN |DDSCL_EXCLUSIVE))
	{
		if (!hWnd) 
		{
			hWnd = FindWindow("FalconDisplay", NULL);
		}
		SetFalconWindowToFullScreen(hWnd );
	}
	return m_pIDDraw->SetCooperativeLevel(hWnd, dwFlags); 
	//OutputDebugString("DDRAWPROXY: myIDDraw::SetCooperativeLevel exited.\r\n");
}

// ---------------------------------------------------------------------------------------
HRESULT  WINAPI myIDDraw::SetDisplayMode(DWORD dwWidth,DWORD dwHeight,DWORD dwBPP,DWORD dwRefreshRate,DWORD dwFlags)
{
	//OutputDebugString("DDRAWPROXY: myIDDraw::SetDisplayMode reached.\r\n");
	return m_pIDDraw->SetDisplayMode(dwWidth, dwHeight, dwBPP, dwRefreshRate, dwFlags);
	//OutputDebugString("DDRAWPROXY: myIDDraw::SetDisplayMode exited.\r\n");
}

// ---------------------------------------------------------------------------------------
HRESULT  WINAPI myIDDraw::WaitForVerticalBlank(DWORD dwFlags, HANDLE hEvent)
{
	//OutputDebugString("DDRAWPROXY: myIDDraw::WaitForVerticalBlank reached.\r\n");
	return m_pIDDraw->WaitForVerticalBlank(dwFlags, hEvent);
	//OutputDebugString("DDRAWPROXY: myIDDraw::WaitForVerticalBlank exited.\r\n");
}

// ---------------------------------------------------------------------------------------
HRESULT  WINAPI myIDDraw::GetAvailableVidMem(LPDDSCAPS2 lpDDSCaps2, LPDWORD lpdwTotal,LPDWORD lpdwFree)
{
	//OutputDebugString("DDRAWPROXY: myIDDraw::GetAvailableVidMem reached.\r\n");
	return m_pIDDraw->GetAvailableVidMem(lpDDSCaps2, lpdwTotal, lpdwFree);
	//OutputDebugString("DDRAWPROXY: myIDDraw::GetAvailableVidMem exited.\r\n");
}

// ---------------------------------------------------------------------------------------
HRESULT  WINAPI myIDDraw::GetSurfaceFromDC(HDC hdc,LPDIRECTDRAWSURFACE7 * lpDDS)
{
	//OutputDebugString("DDRAWPROXY: myIDDraw::GetSurfaceFromDC reached.\r\n");
	return m_pIDDraw->GetSurfaceFromDC(hdc, lpDDS);
	//OutputDebugString("DDRAWPROXY: myIDDraw::GetSurfaceFromDC exited.\r\n");
}

// ---------------------------------------------------------------------------------------
HRESULT  WINAPI myIDDraw::RestoreAllSurfaces(void)
{
	//OutputDebugString("DDRAWPROXY: myIDDraw::RestoreAllSurfaces reached.\r\n");
	return m_pIDDraw->RestoreAllSurfaces();
	//OutputDebugString("DDRAWPROXY: myIDDraw::RestoreAllSurfaces exited.\r\n");
}

// ---------------------------------------------------------------------------------------
HRESULT  WINAPI myIDDraw::TestCooperativeLevel(void)
{
	//OutputDebugString("DDRAWPROXY: myIDDraw::TestCooperativeLevel reached.\r\n");
	return m_pIDDraw->TestCooperativeLevel();
	//OutputDebugString("DDRAWPROXY: myIDDraw::TestCooperativeLevel exited.\r\n");
}

// ---------------------------------------------------------------------------------------
HRESULT  WINAPI myIDDraw::GetDeviceIdentifier(LPDDDEVICEIDENTIFIER2 lpdddi,DWORD dwFlags)
{
	//OutputDebugString("DDRAWPROXY: myIDDraw::GetDeviceIdentifier reached.\r\n");
	return m_pIDDraw->GetDeviceIdentifier(lpdddi, dwFlags);
	//OutputDebugString("DDRAWPROXY: myIDDraw::GetDeviceIdentifier exited.\r\n");
}

// ---------------------------------------------------------------------------------------
HRESULT  WINAPI myIDDraw::StartModeTest(LPSIZE lpModesToTest,DWORD dwNumEntries, DWORD dwFlags)
{
	//OutputDebugString("DDRAWPROXY: myIDDraw::StartModeTest reached.\r\n");
	return m_pIDDraw->StartModeTest(lpModesToTest, dwNumEntries, dwFlags);
	//OutputDebugString("DDRAWPROXY: myIDDraw::StartModeTest exited.\r\n");
}

// ---------------------------------------------------------------------------------------
HRESULT  WINAPI myIDDraw::EvaluateMode(DWORD dwFlags,DWORD  *pSecondsUntilTimeout)
{
	//OutputDebugString("DDRAWPROXY: myIDDraw::EvaluateMode reached.\r\n");
	return m_pIDDraw->EvaluateMode(dwFlags, pSecondsUntilTimeout);
	//OutputDebugString("DDRAWPROXY: myIDDraw::EvaluateMode exited.\r\n");
}

