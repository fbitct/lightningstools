#include "stdafx.h"
/*
DETOURS HOOK CODE FOR THE IDIRECTDRAWSURFACE COM INTERFACE
*/

//function pointers to original methods and new methods

// IUnknown functions
HRESULT WINAPI My_IDDSV3_QueryInterface (LPUNKNOWN lpThis, REFIID riid, LPVOID* obp) 
{
	//OutputDebugString("My_IDDSV3_QueryInterface called.\r\n");
	return ORIG_IDirectDrawSurfaceV3_QueryInterface (lpThis, riid, obp);
}
ULONG   WINAPI My_IDDSV3_AddRef(LPUNKNOWN lpThis) 
{
	//OutputDebugString("My_IDDSV3_AddRef called.\r\n");
	return ORIG_IDirectDrawSurfaceV3_AddRef(lpThis);
}
ULONG   WINAPI My_IDDSV3_Release(LPUNKNOWN lpThis)
{
	//OutputDebugString("My_IDDSV3_Release called.\r\n");
	return ORIG_IDirectDrawSurfaceV3_Release(lpThis);
}

// IDirectDrawSurface (V1) functions
HRESULT WINAPI My_IDDSV3_AddAttachedSurface(
  LPDIRECTDRAWSURFACE3 lpThis,
  LPDIRECTDRAWSURFACE3 lpDDSAttachedSurface  
)
{
	//OutputDebugString("My_IDDSV3_AddAttachedSurface called.\r\n");
	return ORIG_IDirectDrawSurfaceV3_AddAttachedSurface(lpThis, lpDDSAttachedSurface);
}
HRESULT WINAPI My_IDDSV3_AddOverlayDirtyRect(
  LPDIRECTDRAWSURFACE3 lpThis,
  LPRECT lpRect  
)
{
	//OutputDebugString("My_IDDSV3_AddOverlayDirtyRect called.\r\n");
	return ORIG_IDirectDrawSurfaceV3_AddOverlayDirtyRect(lpThis, lpRect);
}
HRESULT WINAPI My_IDDSV3_Blt(
  LPDIRECTDRAWSURFACE3 lpThis,
  LPRECT lpDestRect,                    
  LPDIRECTDRAWSURFACE3 lpDDSrcSurface,  
  LPRECT lpSrcRect,                     
  DWORD dwFlags,                        
  LPDDBLTFX lpDDBltFx                   
)
{
	//OutputDebugString("My_IDDSV3_Blt called.\r\n");
	return ORIG_IDirectDrawSurfaceV3_Blt (lpThis, lpDestRect, lpDDSrcSurface, lpSrcRect, dwFlags, lpDDBltFx);
}
HRESULT WINAPI My_IDDSV3_BltBatch(
  LPDIRECTDRAWSURFACE3 lpThis,
  LPDDBLTBATCH lpDDBltBatch,  
  DWORD dwCount,              
  DWORD dwFlags               
)
{
	//OutputDebugString("My_IDDSV3_BltBatch called.\r\n");
	return ORIG_IDirectDrawSurfaceV3_BltBatch (lpThis, lpDDBltBatch, dwCount, dwFlags);
}
HRESULT WINAPI My_IDDSV3_BltFast(
  LPDIRECTDRAWSURFACE3 lpThis,
  DWORD dwX,                            
  DWORD dwY,                            
  LPDIRECTDRAWSURFACE3 lpDDSrcSurface,  
  LPRECT lpSrcRect,                     
  DWORD dwTrans                         
)
{
	//OutputDebugString("My_IDDSV3_BltFast called.\r\n");
	return ORIG_IDirectDrawSurfaceV3_BltFast (lpThis, dwX, dwY, lpDDSrcSurface, lpSrcRect, dwTrans);
}
HRESULT WINAPI My_IDDSV3_DeleteAttachedSurface(
  LPDIRECTDRAWSURFACE3 lpThis,
  DWORD dwFlags,                             
  LPDIRECTDRAWSURFACE3 lpDDSAttachedSurface  
)
{
	//OutputDebugString("My_IDDSV3_DeleteAttachedSurface called.\r\n");
	return ORIG_IDirectDrawSurfaceV3_DeleteAttachedSurface (lpThis, dwFlags, lpDDSAttachedSurface);
}
HRESULT WINAPI My_IDDSV3_EnumAttachedSurfaces(
  LPDIRECTDRAWSURFACE3 lpThis,
  LPVOID lpContext,                                
  LPDDENUMSURFACESCALLBACK lpEnumSurfacesCallback  
)
{
	//OutputDebugString("My_IDDSV3_EnumAttachedSurfaces called.\r\n");
	return ORIG_IDirectDrawSurfaceV3_EnumAttachedSurfaces (lpThis, lpContext, lpEnumSurfacesCallback);
}
HRESULT WINAPI My_IDDSV3_EnumOverlayZOrders(
  LPDIRECTDRAWSURFACE3 lpThis,
  DWORD dwFlags,                          
  LPVOID lpContext,                       
  LPDDENUMSURFACESCALLBACK lpfnCallback  
)
{
	//OutputDebugString("My_IDDSV3_EnumOverlayZOrders called.\r\n");
	return ORIG_IDirectDrawSurfaceV3_EnumOverlayZOrders (lpThis, dwFlags, lpContext, lpfnCallback);
}
HRESULT WINAPI My_IDDSV3_Flip(
  LPDIRECTDRAWSURFACE3 lpThis,
  LPDIRECTDRAWSURFACE3 lpDDSurfaceTargetOverride,  
  DWORD dwFlags                                    
)
{
	//OutputDebugString("My_IDDSV3_Flip called.\r\n");
	return ORIG_IDirectDrawSurfaceV3_Flip (lpThis, lpDDSurfaceTargetOverride, dwFlags);
}
HRESULT WINAPI My_IDDSV3_GetAttachedSurface(
  LPDIRECTDRAWSURFACE3 lpThis,
  LPDDSCAPS lpDDSCaps, 
  LPDIRECTDRAWSURFACE3 FAR *lplpDDAttachedSurface  
)
{
	//OutputDebugString("My_IDDSV3_GetAttachedSurface called.\r\n");
	return ORIG_IDirectDrawSurfaceV3_GetAttachedSurface(lpThis, lpDDSCaps, lplpDDAttachedSurface);
}
HRESULT WINAPI My_IDDSV3_GetBltStatus(
  LPDIRECTDRAWSURFACE3 lpThis,
  DWORD dwFlags  
)
{
	//OutputDebugString("My_IDDSV3_GetBltStatus called.\r\n");
	return ORIG_IDirectDrawSurfaceV3_GetBltStatus (lpThis, dwFlags);
}
HRESULT WINAPI My_IDDSV3_GetCaps(
  LPDIRECTDRAWSURFACE3 lpThis,
  LPDDSCAPS lpDDSCaps  
)
{
	//OutputDebugString("My_IDDSV3_GetCaps called.\r\n");
	return ORIG_IDirectDrawSurfaceV3_GetCaps(lpThis, lpDDSCaps);
}
HRESULT WINAPI My_IDDSV3_GetClipper(
  LPDIRECTDRAWSURFACE3 lpThis,
  LPDIRECTDRAWCLIPPER FAR *lplpDDClipper  
)
{
	//OutputDebugString("My_IDDSV3_GetClipper called.\r\n");
	return ORIG_IDirectDrawSurfaceV3_GetClipper(lpThis, lplpDDClipper);
}
HRESULT WINAPI My_IDDSV3_GetColorKey(
  LPDIRECTDRAWSURFACE3 lpThis,
  DWORD dwFlags,             
  LPDDCOLORKEY lpDDColorKey  
)
{
	//OutputDebugString("My_IDDSV3_GetColorKey called.\r\n");
	return ORIG_IDirectDrawSurfaceV3_GetColorKey (lpThis, dwFlags, lpDDColorKey);
}
HRESULT WINAPI My_IDDSV3_GetDC(
  LPDIRECTDRAWSURFACE3 lpThis,
  HDC FAR *lphDC  
)
{
	//OutputDebugString("My_IDDSV3_GetDC called.\r\n");
	return ORIG_IDirectDrawSurfaceV3_GetDC (lpThis, lphDC);
}
HRESULT WINAPI My_IDDSV3_GetFlipStatus(
  LPDIRECTDRAWSURFACE3 lpThis,
  DWORD dwFlags  
)
{
	//OutputDebugString("My_IDDSV3_GetFlipStatus called.\r\n");
	return ORIG_IDirectDrawSurfaceV3_GetFlipStatus (lpThis, dwFlags);
}
HRESULT WINAPI My_IDDSV3_GetOverlayPosition(
  LPDIRECTDRAWSURFACE3 lpThis,
  LPLONG lplX, 
  LPLONG lplY  
)
{
	//OutputDebugString("My_IDDSV3_GetOverlayPosition called.\r\n");
	return ORIG_IDirectDrawSurfaceV3_GetOverlayPosition (lpThis, lplX, lplY);
}
HRESULT WINAPI My_IDDSV3_GetPalette(
  LPDIRECTDRAWSURFACE3 lpThis,
  LPDIRECTDRAWPALETTE FAR *lplpDDPalette  
)
{
	//OutputDebugString("My_IDDSV3_GetPalette called.\r\n");
	return ORIG_IDirectDrawSurfaceV3_GetPalette (lpThis, lplpDDPalette);
}

HRESULT WINAPI My_IDDSV3_GetPixelFormat(
  LPDIRECTDRAWSURFACE3 lpThis,
  LPDDPIXELFORMAT lpDDPixelFormat  
)
{
	//OutputDebugString("My_IDDSV3_GetPixelFormat called.\r\n");
	return ORIG_IDirectDrawSurfaceV3_GetPixelFormat(lpThis, lpDDPixelFormat);
}
HRESULT WINAPI My_IDDSV3_GetSurfaceDesc(
  LPDIRECTDRAWSURFACE3 lpThis,
  LPDDSURFACEDESC lpDDSurfaceDesc  
)
{
	//OutputDebugString("My_IDDSV3_GetSurfaceDesc called.\r\n");
	return ORIG_IDirectDrawSurfaceV3_GetSurfaceDesc(lpThis, lpDDSurfaceDesc);
}
HRESULT WINAPI My_IDDSV3_Initialize(
  LPDIRECTDRAWSURFACE3 lpThis,
  LPDIRECTDRAW lpDD,               
  LPDDSURFACEDESC lpDDSurfaceDesc  
)
{
	//OutputDebugString("My_IDDSV3_Initialize called.\r\n");
	return ORIG_IDirectDrawSurfaceV3_Initialize (lpThis, lpDD, lpDDSurfaceDesc);
}
HRESULT WINAPI My_IDDSV3_IsLost(
  LPDIRECTDRAWSURFACE3 lpThis
)
{
	//OutputDebugString("My_IDDSV3_IsLost called.\r\n");
	return ORIG_IDirectDrawSurfaceV3_IsLost (lpThis);
}
HRESULT WINAPI My_IDDSV3_Lock(
  LPDIRECTDRAWSURFACE3 lpThis,
  LPRECT lpDestRect,                
  LPDDSURFACEDESC lpDDSurfaceDesc,  
  DWORD dwFlags,                    
  HANDLE hEvent                     
)
{
	//OutputDebugString("My_IDDSV3_Lock called.\r\n");
	return ORIG_IDirectDrawSurfaceV3_Lock (lpThis, lpDestRect, lpDDSurfaceDesc, dwFlags, hEvent);
}
HRESULT WINAPI My_IDDSV3_ReleaseDC(
  LPDIRECTDRAWSURFACE3 lpThis,
  HDC hDC  
)
{
	//OutputDebugString("My_IDDSV3_ReleaseDC called.\r\n");
	return ORIG_IDirectDrawSurfaceV3_ReleaseDC (lpThis, hDC);
}
HRESULT WINAPI My_IDDSV3_Restore(
  LPDIRECTDRAWSURFACE3 lpThis
)
{
	//OutputDebugString("My_IDDSV3_Restore called.\r\n");
	return ORIG_IDirectDrawSurfaceV3_Restore (lpThis);
}
HRESULT WINAPI My_IDDSV3_SetClipper(
  LPDIRECTDRAWSURFACE3 lpThis,
  LPDIRECTDRAWCLIPPER lpDDClipper  
)
{
	//OutputDebugString("My_IDDSV3_SetClipper called.\r\n");
	return ORIG_IDirectDrawSurfaceV3_SetClipper (lpThis, lpDDClipper);
}
HRESULT WINAPI My_IDDSV3_SetColorKey(
  LPDIRECTDRAWSURFACE3 lpThis,
  DWORD dwFlags,             
  LPDDCOLORKEY lpDDColorKey  
)
{
	//OutputDebugString("My_IDDSV3_SetColorKey called.\r\n");
	return ORIG_IDirectDrawSurfaceV3_SetColorKey (lpThis, dwFlags, lpDDColorKey);
}
HRESULT WINAPI My_IDDSV3_SetOverlayPosition(
  LPDIRECTDRAWSURFACE3 lpThis,
  LONG lX, 
  LONG lY  
)
{
	//OutputDebugString("My_IDDSV3_SetOverlayPosition called.\r\n");
	return ORIG_IDirectDrawSurfaceV3_SetOverlayPosition (lpThis, lX, lY);
}
HRESULT WINAPI My_IDDSV3_SetPalette(
  LPDIRECTDRAWSURFACE3 lpThis,
  LPDIRECTDRAWPALETTE lpDDPalette  
)
{
	//OutputDebugString("My_IDDSV3_SetPalette called.\r\n");
	return ORIG_IDirectDrawSurfaceV3_SetPalette (lpThis, lpDDPalette);
}
HRESULT WINAPI My_IDDSV3_Unlock(
  LPDIRECTDRAWSURFACE3 lpThis,
  LPVOID lpRect 
)
{
	//OutputDebugString("My_IDDSV3_Unlock called.\r\n");
	return ORIG_IDirectDrawSurfaceV3_Unlock (lpThis, lpRect);
}
HRESULT WINAPI My_IDDSV3_UpdateOverlay(
  LPDIRECTDRAWSURFACE3 lpThis,
  LPRECT lpSrcRect,                      
  LPDIRECTDRAWSURFACE3 lpDDDestSurface,  
  LPRECT lpDestRect,                     
  DWORD dwFlags,                         
  LPDDOVERLAYFX lpDDOverlayFx            
)
{
	//OutputDebugString("My_IDDSV3_UpdateOverlay called.\r\n");
	return ORIG_IDirectDrawSurfaceV3_UpdateOverlay (lpThis, lpSrcRect, lpDDDestSurface, lpDestRect,dwFlags,lpDDOverlayFx);
}
HRESULT WINAPI My_IDDSV3_UpdateOverlayDisplay(
  LPDIRECTDRAWSURFACE3 lpThis,
  DWORD dwFlags  
)
{
	//OutputDebugString("My_IDDSV3_UpdateOverlayDisplay called.\r\n");
	return ORIG_IDirectDrawSurfaceV3_UpdateOverlayDisplay (lpThis, dwFlags);
}
HRESULT WINAPI My_IDDSV3_UpdateOverlayZOrder(
  LPDIRECTDRAWSURFACE3 lpThis,
  DWORD dwFlags,                       
  LPDIRECTDRAWSURFACE3 lpDDSReference  
)
{
	//OutputDebugString("My_IDDSV3_UpdateOverlayZOrder called.\r\n");
	return ORIG_IDirectDrawSurfaceV3_UpdateOverlayZOrder (lpThis, dwFlags, lpDDSReference);
}
// IDirectDrawSurface2 functions

HRESULT WINAPI My_IDDSV3_GetDDInterface(
  LPDIRECTDRAWSURFACE3 lpThis,
  LPVOID FAR *lplpDD  
)
{
	//OutputDebugString("My_IDDSV3_GetDDInterface called.\r\n");
	return ORIG_IDirectDrawSurfaceV3_GetDDInterface(lpThis, lplpDD);
}
HRESULT WINAPI My_IDDSV3_PageLock(
  LPDIRECTDRAWSURFACE3 lpThis,
  DWORD dwFlags  
)
{
	//OutputDebugString("My_IDDSV3_PageLock called.\r\n");
	return ORIG_IDirectDrawSurfaceV3_PageLock(lpThis, dwFlags);
}
HRESULT WINAPI My_IDDSV3_PageUnlock(
  LPDIRECTDRAWSURFACE3 lpThis,
  DWORD dwFlags  
)
{
	//OutputDebugString("My_IDDSV3_PageUnlock called.\r\n");
	return ORIG_IDirectDrawSurfaceV3_PageUnlock(lpThis, dwFlags);
}
// IDirectDrawSurface3 functions
HRESULT WINAPI My_IDDSV3_SetSurfaceDesc(
  LPDIRECTDRAWSURFACE3 lpThis,
  LPDDSURFACEDESC lpddsd,  
  DWORD dwFlags            
)
{
	//OutputDebugString("My_IDDSV3_SetSurfaceDesc called.\r\n");
	return ORIG_IDirectDrawSurfaceV3_SetSurfaceDesc(lpThis, lpddsd, dwFlags);
}


void HookDirectDrawSurface3Interface  (LPDIRECTDRAWSURFACE3 lpInterface) 
{
	//OutputDebugString("HookInterface for IDirectDrawSurface3 reached.\r\n");
	LONG result = DetourTransactionBegin();
	if (result != NO_ERROR) {
		//OutputDebugString("DetourTransactionBegin failed.\r\n");
	}
	result = DetourUpdateThread(GetCurrentThread());
	if (result != NO_ERROR) {
		//OutputDebugString("DetourUpdateThread failed.\r\n");
	}
	PDWORD pInterface = (PDWORD)lpInterface;
	PDWORD* pVtable=(PDWORD*)*pInterface;
	ORIG_IDirectDrawSurfaceV3_QueryInterface =(TD_IDirectDrawSurfaceV3_QueryInterface)pVtable[0];
	NEW_IDirectDrawSurfaceV3_QueryInterface =My_IDDSV3_QueryInterface;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV3_QueryInterface, NEW_IDirectDrawSurfaceV3_QueryInterface);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface3::QueryInterface) failed.\r\n");
	}

	ORIG_IDirectDrawSurfaceV3_AddRef=(TD_IDirectDrawSurfaceV3_AddRef)pVtable[1];
	NEW_IDirectDrawSurfaceV3_AddRef=My_IDDSV3_AddRef;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV3_AddRef, NEW_IDirectDrawSurfaceV3_AddRef);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface3::AddRef) failed.\r\n");
	}

	ORIG_IDirectDrawSurfaceV3_Release=(TD_IDirectDrawSurfaceV3_Release)pVtable[2];
	NEW_IDirectDrawSurfaceV3_Release=My_IDDSV3_Release;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV3_Release, NEW_IDirectDrawSurfaceV3_Release);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface3::Release) failed.\r\n");
	}

	ORIG_IDirectDrawSurfaceV3_AddAttachedSurface=(TD_IDirectDrawSurfaceV3_AddAttachedSurface)pVtable[3];
	NEW_IDirectDrawSurfaceV3_AddAttachedSurface=My_IDDSV3_AddAttachedSurface;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV3_AddAttachedSurface, NEW_IDirectDrawSurfaceV3_AddAttachedSurface);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface3::AddAttachedSurface) failed.\r\n");
	}

	ORIG_IDirectDrawSurfaceV3_AddOverlayDirtyRect =(TD_IDirectDrawSurfaceV3_AddOverlayDirtyRect)pVtable[4];
	NEW_IDirectDrawSurfaceV3_AddOverlayDirtyRect =My_IDDSV3_AddOverlayDirtyRect ;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV3_AddOverlayDirtyRect , NEW_IDirectDrawSurfaceV3_AddOverlayDirtyRect);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface3::AddOverlayDirtyRect) failed.\r\n");
	}

	ORIG_IDirectDrawSurfaceV3_Blt =(TD_IDirectDrawSurfaceV3_Blt)pVtable[5];
	NEW_IDirectDrawSurfaceV3_Blt =My_IDDSV3_Blt ;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV3_Blt, NEW_IDirectDrawSurfaceV3_Blt);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface3::Blt) failed.\r\n");
	}

	ORIG_IDirectDrawSurfaceV3_BltBatch =(TD_IDirectDrawSurfaceV3_BltBatch)pVtable[6];
	NEW_IDirectDrawSurfaceV3_BltBatch =My_IDDSV3_BltBatch ;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV3_BltBatch, NEW_IDirectDrawSurfaceV3_BltBatch);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface3::BltBatch) failed.\r\n");
	}

	ORIG_IDirectDrawSurfaceV3_BltFast =(TD_IDirectDrawSurfaceV3_BltFast)pVtable[7];
	NEW_IDirectDrawSurfaceV3_BltFast =My_IDDSV3_BltFast ;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV3_BltFast, NEW_IDirectDrawSurfaceV3_BltFast);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface3::BltFast) failed.\r\n");
	}
	
	ORIG_IDirectDrawSurfaceV3_DeleteAttachedSurface=(TD_IDirectDrawSurfaceV3_DeleteAttachedSurface)pVtable[8];
	NEW_IDirectDrawSurfaceV3_DeleteAttachedSurface =My_IDDSV3_DeleteAttachedSurface;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV3_DeleteAttachedSurface, NEW_IDirectDrawSurfaceV3_DeleteAttachedSurface);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface3::DeleteAttachedSurface) failed.\r\n");
	}

	ORIG_IDirectDrawSurfaceV3_EnumAttachedSurfaces=(TD_IDirectDrawSurfaceV3_EnumAttachedSurfaces)pVtable[9];
	NEW_IDirectDrawSurfaceV3_EnumAttachedSurfaces=My_IDDSV3_EnumAttachedSurfaces;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV3_EnumAttachedSurfaces, NEW_IDirectDrawSurfaceV3_EnumAttachedSurfaces);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface3::EnumAttachedSurfaces) failed.\r\n");
	}

	ORIG_IDirectDrawSurfaceV3_EnumOverlayZOrders=(TD_IDirectDrawSurfaceV3_EnumOverlayZOrders)pVtable[10];
	NEW_IDirectDrawSurfaceV3_EnumOverlayZOrders=My_IDDSV3_EnumOverlayZOrders;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV3_EnumOverlayZOrders, NEW_IDirectDrawSurfaceV3_EnumOverlayZOrders);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface3::EnumOverlayZOrders) failed.\r\n");
	}

	ORIG_IDirectDrawSurfaceV3_Flip=(TD_IDirectDrawSurfaceV3_Flip)pVtable[11];
	NEW_IDirectDrawSurfaceV3_Flip=My_IDDSV3_Flip;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV3_Flip, NEW_IDirectDrawSurfaceV3_Flip);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface3::Flip) failed.\r\n");
	}

	ORIG_IDirectDrawSurfaceV3_GetAttachedSurface=(TD_IDirectDrawSurfaceV3_GetAttachedSurface)pVtable[12];
	NEW_IDirectDrawSurfaceV3_GetAttachedSurface=My_IDDSV3_GetAttachedSurface;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV3_GetAttachedSurface, NEW_IDirectDrawSurfaceV3_GetAttachedSurface);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface3::GetAttachedSurface) failed.\r\n");
	}
	
	ORIG_IDirectDrawSurfaceV3_GetBltStatus=(TD_IDirectDrawSurfaceV3_GetBltStatus)pVtable[13];
	NEW_IDirectDrawSurfaceV3_GetBltStatus=My_IDDSV3_GetBltStatus;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV3_GetBltStatus, NEW_IDirectDrawSurfaceV3_GetBltStatus);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface3::GetBltStatus) failed.\r\n");
	}

	ORIG_IDirectDrawSurfaceV3_GetCaps=(TD_IDirectDrawSurfaceV3_GetCaps)pVtable[14];
	NEW_IDirectDrawSurfaceV3_GetCaps=My_IDDSV3_GetCaps;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV3_GetCaps, NEW_IDirectDrawSurfaceV3_GetCaps);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface3::GetCaps) failed.\r\n");
	}

	ORIG_IDirectDrawSurfaceV3_GetClipper=(TD_IDirectDrawSurfaceV3_GetClipper)pVtable[15];
	NEW_IDirectDrawSurfaceV3_GetClipper=My_IDDSV3_GetClipper;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV3_GetClipper, NEW_IDirectDrawSurfaceV3_GetClipper);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface3::GetClipper) failed.\r\n");
	}
	
	ORIG_IDirectDrawSurfaceV3_GetColorKey=(TD_IDirectDrawSurfaceV3_GetColorKey)pVtable[16];
	NEW_IDirectDrawSurfaceV3_GetColorKey=My_IDDSV3_GetColorKey;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV3_GetColorKey, NEW_IDirectDrawSurfaceV3_GetColorKey);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface3::GetColorKey) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV3_GetDC=(TD_IDirectDrawSurfaceV3_GetDC)pVtable[17];
	NEW_IDirectDrawSurfaceV3_GetDC=My_IDDSV3_GetDC;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV3_GetDC, NEW_IDirectDrawSurfaceV3_GetDC);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface3::GetDC) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV3_GetFlipStatus=(TD_IDirectDrawSurfaceV3_GetFlipStatus)pVtable[18];
	NEW_IDirectDrawSurfaceV3_GetFlipStatus=My_IDDSV3_GetFlipStatus;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV3_GetFlipStatus, NEW_IDirectDrawSurfaceV3_GetFlipStatus);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface3::GetFlipStatus) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV3_GetOverlayPosition=(TD_IDirectDrawSurfaceV3_GetOverlayPosition)pVtable[19];
	NEW_IDirectDrawSurfaceV3_GetOverlayPosition=My_IDDSV3_GetOverlayPosition;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV3_GetOverlayPosition, NEW_IDirectDrawSurfaceV3_GetOverlayPosition);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface3::GetOverlayPosition) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV3_GetPalette=(TD_IDirectDrawSurfaceV3_GetPalette)pVtable[20];
	NEW_IDirectDrawSurfaceV3_GetPalette=My_IDDSV3_GetPalette;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV3_GetPalette, NEW_IDirectDrawSurfaceV3_GetPalette);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface3::GetPalette) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV3_GetPixelFormat=(TD_IDirectDrawSurfaceV3_GetPixelFormat)pVtable[21];
	NEW_IDirectDrawSurfaceV3_GetPixelFormat=My_IDDSV3_GetPixelFormat;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV3_GetPixelFormat, NEW_IDirectDrawSurfaceV3_GetPixelFormat);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface3::GetPixelFormat) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV3_GetSurfaceDesc=(TD_IDirectDrawSurfaceV3_GetSurfaceDesc)pVtable[22];
	NEW_IDirectDrawSurfaceV3_GetSurfaceDesc=My_IDDSV3_GetSurfaceDesc;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV3_GetSurfaceDesc, NEW_IDirectDrawSurfaceV3_GetSurfaceDesc);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface3::GetSurfaceDesc) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV3_Initialize=(TD_IDirectDrawSurfaceV3_Initialize)pVtable[23];
	NEW_IDirectDrawSurfaceV3_Initialize=My_IDDSV3_Initialize;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV3_Initialize, NEW_IDirectDrawSurfaceV3_Initialize);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface3::Initialize) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV3_IsLost=(TD_IDirectDrawSurfaceV3_IsLost)pVtable[24];
	NEW_IDirectDrawSurfaceV3_IsLost=My_IDDSV3_IsLost;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV3_IsLost, NEW_IDirectDrawSurfaceV3_IsLost);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface3::IsLost) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV3_Lock=(TD_IDirectDrawSurfaceV3_Lock)pVtable[25];
	NEW_IDirectDrawSurfaceV3_Lock=My_IDDSV3_Lock;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV3_Lock, NEW_IDirectDrawSurfaceV3_Lock);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface3::Lock) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV3_ReleaseDC=(TD_IDirectDrawSurfaceV3_ReleaseDC)pVtable[26];
	NEW_IDirectDrawSurfaceV3_ReleaseDC=My_IDDSV3_ReleaseDC;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV3_ReleaseDC, NEW_IDirectDrawSurfaceV3_ReleaseDC);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface3::ReleaseDC) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV3_Restore=(TD_IDirectDrawSurfaceV3_Restore)pVtable[27];
	NEW_IDirectDrawSurfaceV3_Restore=My_IDDSV3_Restore;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV3_Restore, NEW_IDirectDrawSurfaceV3_Restore);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface3::Restore) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV3_SetClipper=(TD_IDirectDrawSurfaceV3_SetClipper)pVtable[28];
	NEW_IDirectDrawSurfaceV3_SetClipper=My_IDDSV3_SetClipper;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV3_SetClipper, NEW_IDirectDrawSurfaceV3_SetClipper);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface3::SetClipper) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV3_SetColorKey=(TD_IDirectDrawSurfaceV3_SetColorKey)pVtable[29];
	NEW_IDirectDrawSurfaceV3_SetColorKey=My_IDDSV3_SetColorKey;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV3_SetColorKey, NEW_IDirectDrawSurfaceV3_SetColorKey);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface3::SetColorKey) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV3_SetOverlayPosition=(TD_IDirectDrawSurfaceV3_SetOverlayPosition)pVtable[30];
	NEW_IDirectDrawSurfaceV3_SetOverlayPosition=My_IDDSV3_SetOverlayPosition;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV3_SetOverlayPosition, NEW_IDirectDrawSurfaceV3_SetOverlayPosition);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface3::SetOverlayPosition) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV3_SetPalette=(TD_IDirectDrawSurfaceV3_SetPalette)pVtable[31];
	NEW_IDirectDrawSurfaceV3_SetPalette=My_IDDSV3_SetPalette;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV3_SetPalette, NEW_IDirectDrawSurfaceV3_SetPalette);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface3::SetPalette) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV3_Unlock=(TD_IDirectDrawSurfaceV3_Unlock)pVtable[32];
	NEW_IDirectDrawSurfaceV3_Unlock=My_IDDSV3_Unlock;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV3_Unlock, NEW_IDirectDrawSurfaceV3_Unlock);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface3::Unlock) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV3_UpdateOverlay=(TD_IDirectDrawSurfaceV3_UpdateOverlay)pVtable[33];
	NEW_IDirectDrawSurfaceV3_UpdateOverlay=My_IDDSV3_UpdateOverlay;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV3_UpdateOverlay, NEW_IDirectDrawSurfaceV3_UpdateOverlay);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface3::UpdateOverlay) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV3_UpdateOverlayDisplay=(TD_IDirectDrawSurfaceV3_UpdateOverlayDisplay)pVtable[34];
	NEW_IDirectDrawSurfaceV3_UpdateOverlayDisplay=My_IDDSV3_UpdateOverlayDisplay;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV3_UpdateOverlayDisplay, NEW_IDirectDrawSurfaceV3_UpdateOverlayDisplay);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface3::UpdateOverlayDisplay) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV3_UpdateOverlayZOrder=(TD_IDirectDrawSurfaceV3_UpdateOverlayZOrder)pVtable[35];
	NEW_IDirectDrawSurfaceV3_UpdateOverlayZOrder=My_IDDSV3_UpdateOverlayZOrder;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV3_UpdateOverlayZOrder, NEW_IDirectDrawSurfaceV3_UpdateOverlayZOrder);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface3::UpdateOverlayZOrder) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV3_GetDDInterface=(TD_IDirectDrawSurfaceV3_GetDDInterface)pVtable[36];
	NEW_IDirectDrawSurfaceV3_GetDDInterface=My_IDDSV3_GetDDInterface;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV3_GetDDInterface, NEW_IDirectDrawSurfaceV3_GetDDInterface);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface3::GetDDInterface) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV3_PageLock=(TD_IDirectDrawSurfaceV3_PageLock)pVtable[37];
	NEW_IDirectDrawSurfaceV3_PageLock=My_IDDSV3_PageLock;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV3_PageLock, NEW_IDirectDrawSurfaceV3_PageLock);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface3::PageLock) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV3_PageUnlock=(TD_IDirectDrawSurfaceV3_PageUnlock)pVtable[38];
	NEW_IDirectDrawSurfaceV3_PageUnlock=My_IDDSV3_PageUnlock;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV3_PageUnlock, NEW_IDirectDrawSurfaceV3_PageUnlock);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface3::PageUnlock) failed.\r\n");
	}

	ORIG_IDirectDrawSurfaceV3_SetSurfaceDesc=(TD_IDirectDrawSurfaceV3_SetSurfaceDesc)pVtable[39];
	NEW_IDirectDrawSurfaceV3_SetSurfaceDesc=My_IDDSV3_SetSurfaceDesc;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV3_SetSurfaceDesc, NEW_IDirectDrawSurfaceV3_SetSurfaceDesc);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface3::SetSurfaceDesc) failed.\r\n");
	}

	result = DetourTransactionCommit();
	if (result != NO_ERROR) {
		//OutputDebugString("DetourTransactionCommit failed.\r\n");
	}
	//OutputDebugString("HookInterface for IDirectDrawSurface3 exited.\r\n");
	
}
