#include "stdafx.h"
/*
DETOURS HOOK CODE FOR THE IDIRECTDRAWSURFACE COM INTERFACE
*/

//function pointers to original methods and new methods

// IUnknown functions
HRESULT WINAPI My_IDDSV7_QueryInterface (LPUNKNOWN lpThis, REFIID riid, LPVOID* obp) 
{
	//OutputDebugString("My_IDDSV7_QueryInterface called.\r\n");
	return ORIG_IDirectDrawSurfaceV7_QueryInterface (lpThis, riid, obp);
}
ULONG   WINAPI My_IDDSV7_AddRef(LPUNKNOWN lpThis) 
{
	//OutputDebugString("My_IDDSV7_AddRef called.\r\n");
	return ORIG_IDirectDrawSurfaceV7_AddRef(lpThis);
}
ULONG   WINAPI My_IDDSV7_Release(LPUNKNOWN lpThis)
{
	//OutputDebugString("My_IDDSV7_Release called.\r\n");
	return ORIG_IDirectDrawSurfaceV7_Release(lpThis);
}

// IDirectDrawSurface (V1) functions
HRESULT WINAPI My_IDDSV7_AddAttachedSurface(
  LPDIRECTDRAWSURFACE7 lpThis,
  LPDIRECTDRAWSURFACE7 lpDDSAttachedSurface  
)
{
	//OutputDebugString("My_IDDSV7_AddAttachedSurface called.\r\n");
	return ORIG_IDirectDrawSurfaceV7_AddAttachedSurface(lpThis, lpDDSAttachedSurface);
}
HRESULT WINAPI My_IDDSV7_AddOverlayDirtyRect(
  LPDIRECTDRAWSURFACE7 lpThis,
  LPRECT lpRect  
)
{
	//OutputDebugString("My_IDDSV7_AddOverlayDirtyRect called.\r\n");
	return ORIG_IDirectDrawSurfaceV7_AddOverlayDirtyRect(lpThis, lpRect);
}
HRESULT WINAPI My_IDDSV7_Blt(
  LPDIRECTDRAWSURFACE7 lpThis,
  LPRECT lpDestRect,                    
  LPDIRECTDRAWSURFACE7 lpDDSrcSurface,  
  LPRECT lpSrcRect,                     
  DWORD dwFlags,                        
  LPDDBLTFX lpDDBltFx                   
)
{
	//OutputDebugString("My_IDDSV7_Blt called.\r\n");
	return ORIG_IDirectDrawSurfaceV7_Blt (lpThis, lpDestRect, lpDDSrcSurface, lpSrcRect, dwFlags, lpDDBltFx);
}
HRESULT WINAPI My_IDDSV7_BltBatch(
  LPDIRECTDRAWSURFACE7 lpThis,
  LPDDBLTBATCH lpDDBltBatch,  
  DWORD dwCount,              
  DWORD dwFlags               
)
{
	//OutputDebugString("My_IDDSV7_BltBatch called.\r\n");
	return ORIG_IDirectDrawSurfaceV7_BltBatch (lpThis, lpDDBltBatch, dwCount, dwFlags);
}
HRESULT WINAPI My_IDDSV7_BltFast(
  LPDIRECTDRAWSURFACE7 lpThis,
  DWORD dwX,                            
  DWORD dwY,                            
  LPDIRECTDRAWSURFACE7 lpDDSrcSurface,  
  LPRECT lpSrcRect,                     
  DWORD dwTrans                         
)
{
	//OutputDebugString("My_IDDSV7_BltFast called.\r\n");
	return ORIG_IDirectDrawSurfaceV7_BltFast (lpThis, dwX, dwY, lpDDSrcSurface, lpSrcRect, dwTrans);
}
HRESULT WINAPI My_IDDSV7_DeleteAttachedSurface(
  LPDIRECTDRAWSURFACE7 lpThis,
  DWORD dwFlags,                             
  LPDIRECTDRAWSURFACE7 lpDDSAttachedSurface  
)
{
	//OutputDebugString("My_IDDSV7_DeleteAttachedSurface called.\r\n");
	return ORIG_IDirectDrawSurfaceV7_DeleteAttachedSurface (lpThis, dwFlags, lpDDSAttachedSurface);
}
HRESULT WINAPI My_IDDSV7_EnumAttachedSurfaces(
  LPDIRECTDRAWSURFACE7 lpThis,
  LPVOID lpContext,                                
  LPDDENUMSURFACESCALLBACK7 lpEnumSurfacesCallback
)
{
	//OutputDebugString("My_IDDSV7_EnumAttachedSurfaces called.\r\n");
	return ORIG_IDirectDrawSurfaceV7_EnumAttachedSurfaces (lpThis, lpContext, lpEnumSurfacesCallback);
}
HRESULT WINAPI My_IDDSV7_EnumOverlayZOrders(
  LPDIRECTDRAWSURFACE7 lpThis,
  DWORD dwFlags,                          
  LPVOID lpContext,                       
  LPDDENUMSURFACESCALLBACK7 lpfnCallback
)
{
	//OutputDebugString("My_IDDSV7_EnumOverlayZOrders called.\r\n");
	return ORIG_IDirectDrawSurfaceV7_EnumOverlayZOrders (lpThis, dwFlags, lpContext, lpfnCallback);
}
HRESULT WINAPI My_IDDSV7_Flip(
  LPDIRECTDRAWSURFACE7 lpThis,
  LPDIRECTDRAWSURFACE7 lpDDSurfaceTargetOverride,  
  DWORD dwFlags                                    
)
{
	//OutputDebugString("My_IDDSV7_Flip called.\r\n");
	return ORIG_IDirectDrawSurfaceV7_Flip (lpThis, lpDDSurfaceTargetOverride, dwFlags);
}
HRESULT WINAPI My_IDDSV7_GetAttachedSurface(
  LPDIRECTDRAWSURFACE7 lpThis,
  LPDDSCAPS2 lpDDSCaps, 
  LPDIRECTDRAWSURFACE7 FAR *lplpDDAttachedSurface  
)
{
	//OutputDebugString("My_IDDSV7_GetAttachedSurface called.\r\n");
	return ORIG_IDirectDrawSurfaceV7_GetAttachedSurface(lpThis, lpDDSCaps, lplpDDAttachedSurface);
}
HRESULT WINAPI My_IDDSV7_GetBltStatus(
  LPDIRECTDRAWSURFACE7 lpThis,
  DWORD dwFlags  
)
{
	//OutputDebugString("My_IDDSV7_GetBltStatus called.\r\n");
	return ORIG_IDirectDrawSurfaceV7_GetBltStatus (lpThis, dwFlags);
}
HRESULT WINAPI My_IDDSV7_GetCaps(
  LPDIRECTDRAWSURFACE7 lpThis,
  LPDDSCAPS2 lpDDSCaps  
)
{
	//OutputDebugString("My_IDDSV7_GetCaps called.\r\n");
	return ORIG_IDirectDrawSurfaceV7_GetCaps(lpThis, lpDDSCaps);
}
HRESULT WINAPI My_IDDSV7_GetClipper(
  LPDIRECTDRAWSURFACE7 lpThis,
  LPDIRECTDRAWCLIPPER FAR *lplpDDClipper  
)
{
	//OutputDebugString("My_IDDSV7_GetClipper called.\r\n");
	return ORIG_IDirectDrawSurfaceV7_GetClipper(lpThis, lplpDDClipper);
}
HRESULT WINAPI My_IDDSV7_GetColorKey(
  LPDIRECTDRAWSURFACE7 lpThis,
  DWORD dwFlags,             
  LPDDCOLORKEY lpDDColorKey  
)
{
	//OutputDebugString("My_IDDSV7_GetColorKey called.\r\n");
	return ORIG_IDirectDrawSurfaceV7_GetColorKey (lpThis, dwFlags, lpDDColorKey);
}
HRESULT WINAPI My_IDDSV7_GetDC(
  LPDIRECTDRAWSURFACE7 lpThis,
  HDC FAR *lphDC  
)
{
	//OutputDebugString("My_IDDSV7_GetDC called.\r\n");
	return ORIG_IDirectDrawSurfaceV7_GetDC (lpThis, lphDC);
}
HRESULT WINAPI My_IDDSV7_GetFlipStatus(
  LPDIRECTDRAWSURFACE7 lpThis,
  DWORD dwFlags  
)
{
	//OutputDebugString("My_IDDSV7_GetFlipStatus called.\r\n");
	return ORIG_IDirectDrawSurfaceV7_GetFlipStatus (lpThis, dwFlags);
}
HRESULT WINAPI My_IDDSV7_GetOverlayPosition(
  LPDIRECTDRAWSURFACE7 lpThis,
  LPLONG lplX, 
  LPLONG lplY  
)
{
	//OutputDebugString("My_IDDSV7_GetOverlayPosition called.\r\n");
	return ORIG_IDirectDrawSurfaceV7_GetOverlayPosition (lpThis, lplX, lplY);
}
HRESULT WINAPI My_IDDSV7_GetPalette(
  LPDIRECTDRAWSURFACE7 lpThis,
  LPDIRECTDRAWPALETTE FAR *lplpDDPalette  
)
{
	//OutputDebugString("My_IDDSV7_GetPalette called.\r\n");
	return ORIG_IDirectDrawSurfaceV7_GetPalette (lpThis, lplpDDPalette);
}

HRESULT WINAPI My_IDDSV7_GetPixelFormat(
  LPDIRECTDRAWSURFACE7 lpThis,
  LPDDPIXELFORMAT lpDDPixelFormat  
)
{
	//OutputDebugString("My_IDDSV7_GetPixelFormat called.\r\n");
	return ORIG_IDirectDrawSurfaceV7_GetPixelFormat(lpThis, lpDDPixelFormat);
}
HRESULT WINAPI My_IDDSV7_GetSurfaceDesc(
  LPDIRECTDRAWSURFACE7 lpThis,
  LPDDSURFACEDESC2 LPDDSURFACEDESC2  
)
{
	//OutputDebugString("My_IDDSV7_GetSurfaceDesc called.\r\n");
	return ORIG_IDirectDrawSurfaceV7_GetSurfaceDesc(lpThis, LPDDSURFACEDESC2);
}
HRESULT WINAPI My_IDDSV7_Initialize(
  LPDIRECTDRAWSURFACE7 lpThis,
  LPDIRECTDRAW lpDD,               
  LPDDSURFACEDESC2 LPDDSURFACEDESC2  
)
{
	//OutputDebugString("My_IDDSV7_Initialize called.\r\n");
	return ORIG_IDirectDrawSurfaceV7_Initialize (lpThis, lpDD, LPDDSURFACEDESC2);
}
HRESULT WINAPI My_IDDSV7_IsLost(
  LPDIRECTDRAWSURFACE7 lpThis
)
{
	//OutputDebugString("My_IDDSV7_IsLost called.\r\n");
	return ORIG_IDirectDrawSurfaceV7_IsLost (lpThis);
}
HRESULT WINAPI My_IDDSV7_Lock(
  LPDIRECTDRAWSURFACE7 lpThis,
  LPRECT lpDestRect,                
  LPDDSURFACEDESC2 LPDDSURFACEDESC2,  
  DWORD dwFlags,                    
  HANDLE hEvent                     
)
{
	//OutputDebugString("My_IDDSV7_Lock called.\r\n");
	return ORIG_IDirectDrawSurfaceV7_Lock (lpThis, lpDestRect, LPDDSURFACEDESC2, dwFlags, hEvent);
}
HRESULT WINAPI My_IDDSV7_ReleaseDC(
  LPDIRECTDRAWSURFACE7 lpThis,
  HDC hDC  
)
{
	//OutputDebugString("My_IDDSV7_ReleaseDC called.\r\n");
	return ORIG_IDirectDrawSurfaceV7_ReleaseDC (lpThis, hDC);
}
HRESULT WINAPI My_IDDSV7_Restore(
  LPDIRECTDRAWSURFACE7 lpThis
)
{
	//OutputDebugString("My_IDDSV7_Restore called.\r\n");
	return ORIG_IDirectDrawSurfaceV7_Restore (lpThis);
}
HRESULT WINAPI My_IDDSV7_SetClipper(
  LPDIRECTDRAWSURFACE7 lpThis,
  LPDIRECTDRAWCLIPPER lpDDClipper  
)
{
	//OutputDebugString("My_IDDSV7_SetClipper called.\r\n");
	return ORIG_IDirectDrawSurfaceV7_SetClipper (lpThis, lpDDClipper);
}
HRESULT WINAPI My_IDDSV7_SetColorKey(
  LPDIRECTDRAWSURFACE7 lpThis,
  DWORD dwFlags,             
  LPDDCOLORKEY lpDDColorKey  
)
{
	//OutputDebugString("My_IDDSV7_SetColorKey called.\r\n");
	return ORIG_IDirectDrawSurfaceV7_SetColorKey (lpThis, dwFlags, lpDDColorKey);
}
HRESULT WINAPI My_IDDSV7_SetOverlayPosition(
  LPDIRECTDRAWSURFACE7 lpThis,
  LONG lX, 
  LONG lY  
)
{
	//OutputDebugString("My_IDDSV7_SetOverlayPosition called.\r\n");
	return ORIG_IDirectDrawSurfaceV7_SetOverlayPosition (lpThis, lX, lY);
}
HRESULT WINAPI My_IDDSV7_SetPalette(
  LPDIRECTDRAWSURFACE7 lpThis,
  LPDIRECTDRAWPALETTE lpDDPalette  
)
{
	//OutputDebugString("My_IDDSV7_SetPalette called.\r\n");
	return ORIG_IDirectDrawSurfaceV7_SetPalette (lpThis, lpDDPalette);
}
HRESULT WINAPI My_IDDSV7_Unlock(
  LPDIRECTDRAWSURFACE7 lpThis,
  LPVOID lpRect 
)
{
	//OutputDebugString("My_IDDSV7_Unlock called.\r\n");
	return ORIG_IDirectDrawSurfaceV7_Unlock (lpThis, lpRect);
}
HRESULT WINAPI My_IDDSV7_UpdateOverlay(
  LPDIRECTDRAWSURFACE7 lpThis,
  LPRECT lpSrcRect,                      
  LPDIRECTDRAWSURFACE7 lpDDDestSurface,  
  LPRECT lpDestRect,                     
  DWORD dwFlags,                         
  LPDDOVERLAYFX lpDDOverlayFx            
)
{
	//OutputDebugString("My_IDDSV7_UpdateOverlay called.\r\n");
	return ORIG_IDirectDrawSurfaceV7_UpdateOverlay (lpThis, lpSrcRect, lpDDDestSurface, lpDestRect,dwFlags,lpDDOverlayFx);
}
HRESULT WINAPI My_IDDSV7_UpdateOverlayDisplay(
  LPDIRECTDRAWSURFACE7 lpThis,
  DWORD dwFlags  
)
{
	//OutputDebugString("My_IDDSV7_UpdateOverlayDisplay called.\r\n");
	return ORIG_IDirectDrawSurfaceV7_UpdateOverlayDisplay (lpThis, dwFlags);
}
HRESULT WINAPI My_IDDSV7_UpdateOverlayZOrder(
  LPDIRECTDRAWSURFACE7 lpThis,
  DWORD dwFlags,                       
  LPDIRECTDRAWSURFACE7 lpDDSReference  
)
{
	//OutputDebugString("My_IDDSV7_UpdateOverlayZOrder called.\r\n");
	return ORIG_IDirectDrawSurfaceV7_UpdateOverlayZOrder (lpThis, dwFlags, lpDDSReference);
}
// IDirectDrawSurface2 functions

HRESULT WINAPI My_IDDSV7_GetDDInterface(
  LPDIRECTDRAWSURFACE7 lpThis,
  LPVOID FAR *lplpDD  
)
{
	//OutputDebugString("My_IDDSV7_GetDDInterface called.\r\n");
	return ORIG_IDirectDrawSurfaceV7_GetDDInterface(lpThis, lplpDD);
}
HRESULT WINAPI My_IDDSV7_PageLock(
  LPDIRECTDRAWSURFACE7 lpThis,
  DWORD dwFlags  
)
{
	//OutputDebugString("My_IDDSV7_PageLock called.\r\n");
	return ORIG_IDirectDrawSurfaceV7_PageLock(lpThis, dwFlags);
}
HRESULT WINAPI My_IDDSV7_PageUnlock(
  LPDIRECTDRAWSURFACE7 lpThis,
  DWORD dwFlags  
)
{
	//OutputDebugString("My_IDDSV7_PageUnlock called.\r\n");
	return ORIG_IDirectDrawSurfaceV7_PageUnlock(lpThis, dwFlags);
}
// IDirectDrawSurface3 functions
HRESULT WINAPI My_IDDSV7_SetSurfaceDesc(
  LPDIRECTDRAWSURFACE7 lpThis,
  LPDDSURFACEDESC2 lpddsd,  
  DWORD dwFlags            
)
{
	//OutputDebugString("My_IDDSV7_SetSurfaceDesc called.\r\n");
	return ORIG_IDirectDrawSurfaceV7_SetSurfaceDesc(lpThis, lpddsd, dwFlags);
}
// IDirectDrawSurface4 functions
HRESULT WINAPI My_IDDSV7_SetPrivateData( 
  LPDIRECTDRAWSURFACE7 lpThis,
  REFGUID guidTag, 
  LPVOID  lpData,
  DWORD   cbSize,
  DWORD   dwFlags 
)
{
	//OutputDebugString("My_IDDSV7_SetPrivateData called.\r\n");
	return ORIG_IDirectDrawSurfaceV7_SetPrivateData(lpThis, guidTag, lpData, cbSize, dwFlags);
}
HRESULT WINAPI My_IDDSV7_GetPrivateData(
  LPDIRECTDRAWSURFACE7 lpThis,
  REFGUID guidTag,
  LPVOID  lpBuffer,
  LPDWORD lpcbBufferSize
)
{
	//OutputDebugString("My_IDDSV7_GetPrivateData called.\r\n");
	return ORIG_IDirectDrawSurfaceV7_GetPrivateData(lpThis, guidTag, lpBuffer, lpcbBufferSize);
}
HRESULT WINAPI My_IDDSV7_FreePrivateData( 
  LPDIRECTDRAWSURFACE7 lpThis,
  REFGUID guidTag 
)
{
	//OutputDebugString("My_IDDSV7_FreePrivateData called.\r\n");
	return ORIG_IDirectDrawSurfaceV7_FreePrivateData(lpThis, guidTag);
}
HRESULT WINAPI My_IDDSV7_GetUniquenessValue( 
  LPDIRECTDRAWSURFACE7 lpThis,
  LPDWORD lpValue 
)
{
	//OutputDebugString("My_IDDSV7_GetUniquenessValue called.\r\n");
	return ORIG_IDirectDrawSurfaceV7_GetUniquenessValue(lpThis, lpValue);
}
HRESULT WINAPI My_IDDSV7_ChangeUniquenessValue(
  LPDIRECTDRAWSURFACE7 lpThis
)
{
	//OutputDebugString("My_IDDSV7_ChangeUniquenessValue called.\r\n");
	return ORIG_IDirectDrawSurfaceV7_ChangeUniquenessValue(lpThis);
}

// IDirectDrawSurface7 functions
HRESULT WINAPI My_IDDSV7_SetPriority(
  LPDIRECTDRAWSURFACE7 lpThis,
  DWORD dwPriority
)
{
	//OutputDebugString("My_IDDSV7_SetPriority called.\r\n");
	return ORIG_IDirectDrawSurfaceV7_SetPriority(lpThis, dwPriority);
}
HRESULT WINAPI My_IDDSV7_GetPriority( 
  LPDIRECTDRAWSURFACE7 lpThis,
  LPDWORD lpdwPriority
)
{
	//OutputDebugString("My_IDDSV7_GetPriority called.\r\n");
	return ORIG_IDirectDrawSurfaceV7_GetPriority(lpThis, lpdwPriority);
}
HRESULT WINAPI My_IDDSV7_SetLOD(
  LPDIRECTDRAWSURFACE7 lpThis,
  DWORD dwMaxLOD
)
{
	//OutputDebugString("My_IDDSV7_SetLOD called.\r\n");
	return ORIG_IDirectDrawSurfaceV7_SetLOD(lpThis, dwMaxLOD);
}
HRESULT WINAPI My_IDDSV7_GetLOD(
  LPDIRECTDRAWSURFACE7 lpThis,
  LPDWORD lpdwMaxLOD
)
{
	//OutputDebugString("My_IDDSV7_GetLOD called.\r\n");
	return ORIG_IDirectDrawSurfaceV7_GetLOD(lpThis, lpdwMaxLOD);
}

void HookDirectDrawSurface7Interface (LPDIRECTDRAWSURFACE7 lpInterface) 
{
	
	//OutputDebugString("HookInterface for IDirectDrawSurface7 reached.\r\n");
/*
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
	ORIG_IDirectDrawSurfaceV7_QueryInterface =(TD_IDirectDrawSurfaceV7_QueryInterface)pVtable[0];
	NEW_IDirectDrawSurfaceV7_QueryInterface =My_IDDSV7_QueryInterface;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV7_QueryInterface, NEW_IDirectDrawSurfaceV7_QueryInterface);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface7::QueryInterface) failed.\r\n");
	}

	ORIG_IDirectDrawSurfaceV7_AddRef=(TD_IDirectDrawSurfaceV7_AddRef)pVtable[1];
	NEW_IDirectDrawSurfaceV7_AddRef=My_IDDSV7_AddRef;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV7_AddRef, NEW_IDirectDrawSurfaceV7_AddRef);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface7::AddRef) failed.\r\n");
	}

	ORIG_IDirectDrawSurfaceV7_Release=(TD_IDirectDrawSurfaceV7_Release)pVtable[2];
	NEW_IDirectDrawSurfaceV7_Release=My_IDDSV7_Release;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV7_Release, NEW_IDirectDrawSurfaceV7_Release);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface7::Release) failed.\r\n");
	}

	ORIG_IDirectDrawSurfaceV7_AddAttachedSurface=(TD_IDirectDrawSurfaceV7_AddAttachedSurface)pVtable[3];
	NEW_IDirectDrawSurfaceV7_AddAttachedSurface=My_IDDSV7_AddAttachedSurface;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV7_AddAttachedSurface, NEW_IDirectDrawSurfaceV7_AddAttachedSurface);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface7::AddAttachedSurface) failed.\r\n");
	}

	ORIG_IDirectDrawSurfaceV7_AddOverlayDirtyRect =(TD_IDirectDrawSurfaceV7_AddOverlayDirtyRect)pVtable[4];
	NEW_IDirectDrawSurfaceV7_AddOverlayDirtyRect =My_IDDSV7_AddOverlayDirtyRect ;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV7_AddOverlayDirtyRect , NEW_IDirectDrawSurfaceV7_AddOverlayDirtyRect);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface7::AddOverlayDirtyRect) failed.\r\n");
	}

	ORIG_IDirectDrawSurfaceV7_Blt =(TD_IDirectDrawSurfaceV7_Blt)pVtable[5];
	NEW_IDirectDrawSurfaceV7_Blt =My_IDDSV7_Blt ;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV7_Blt, NEW_IDirectDrawSurfaceV7_Blt);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface7::Blt) failed.\r\n");
	}

	ORIG_IDirectDrawSurfaceV7_BltBatch =(TD_IDirectDrawSurfaceV7_BltBatch)pVtable[6];
	NEW_IDirectDrawSurfaceV7_BltBatch =My_IDDSV7_BltBatch ;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV7_BltBatch, NEW_IDirectDrawSurfaceV7_BltBatch);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface7::BltBatch) failed.\r\n");
	}

	ORIG_IDirectDrawSurfaceV7_BltFast =(TD_IDirectDrawSurfaceV7_BltFast)pVtable[7];
	NEW_IDirectDrawSurfaceV7_BltFast =My_IDDSV7_BltFast ;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV7_BltFast, NEW_IDirectDrawSurfaceV7_BltFast);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface7::BltFast) failed.\r\n");
	}
	
	ORIG_IDirectDrawSurfaceV7_DeleteAttachedSurface=(TD_IDirectDrawSurfaceV7_DeleteAttachedSurface)pVtable[8];
	NEW_IDirectDrawSurfaceV7_DeleteAttachedSurface =My_IDDSV7_DeleteAttachedSurface;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV7_DeleteAttachedSurface, NEW_IDirectDrawSurfaceV7_DeleteAttachedSurface);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface7::DeleteAttachedSurface) failed.\r\n");
	}

	ORIG_IDirectDrawSurfaceV7_EnumAttachedSurfaces=(TD_IDirectDrawSurfaceV7_EnumAttachedSurfaces)pVtable[9];
	NEW_IDirectDrawSurfaceV7_EnumAttachedSurfaces=My_IDDSV7_EnumAttachedSurfaces;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV7_EnumAttachedSurfaces, NEW_IDirectDrawSurfaceV7_EnumAttachedSurfaces);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface7::EnumAttachedSurfaces) failed.\r\n");
	}

	ORIG_IDirectDrawSurfaceV7_EnumOverlayZOrders=(TD_IDirectDrawSurfaceV7_EnumOverlayZOrders)pVtable[10];
	NEW_IDirectDrawSurfaceV7_EnumOverlayZOrders=My_IDDSV7_EnumOverlayZOrders;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV7_EnumOverlayZOrders, NEW_IDirectDrawSurfaceV7_EnumOverlayZOrders);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface7::EnumOverlayZOrders) failed.\r\n");
	}

	ORIG_IDirectDrawSurfaceV7_Flip=(TD_IDirectDrawSurfaceV7_Flip)pVtable[11];
	NEW_IDirectDrawSurfaceV7_Flip=My_IDDSV7_Flip;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV7_Flip, NEW_IDirectDrawSurfaceV7_Flip);
	if (result != NO_ERROR) {
		OutputDebugString("DetourAttach (IDirectDrawSurface7::Flip) failed.\r\n");
	}
	
	ORIG_IDirectDrawSurfaceV7_GetAttachedSurface=(TD_IDirectDrawSurfaceV7_GetAttachedSurface)pVtable[12];
	NEW_IDirectDrawSurfaceV7_GetAttachedSurface=My_IDDSV7_GetAttachedSurface;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV7_GetAttachedSurface, NEW_IDirectDrawSurfaceV7_GetAttachedSurface);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface7::GetAttachedSurface) failed.\r\n");
	}
	
	ORIG_IDirectDrawSurfaceV7_GetBltStatus=(TD_IDirectDrawSurfaceV7_GetBltStatus)pVtable[13];
	NEW_IDirectDrawSurfaceV7_GetBltStatus=My_IDDSV7_GetBltStatus;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV7_GetBltStatus, NEW_IDirectDrawSurfaceV7_GetBltStatus);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface7::GetBltStatus) failed.\r\n");
	}

	ORIG_IDirectDrawSurfaceV7_GetCaps=(TD_IDirectDrawSurfaceV7_GetCaps)pVtable[14];
	NEW_IDirectDrawSurfaceV7_GetCaps=My_IDDSV7_GetCaps;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV7_GetCaps, NEW_IDirectDrawSurfaceV7_GetCaps);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface7::GetCaps) failed.\r\n");
	}

	ORIG_IDirectDrawSurfaceV7_GetClipper=(TD_IDirectDrawSurfaceV7_GetClipper)pVtable[15];
	NEW_IDirectDrawSurfaceV7_GetClipper=My_IDDSV7_GetClipper;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV7_GetClipper, NEW_IDirectDrawSurfaceV7_GetClipper);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface7::GetClipper) failed.\r\n");
	}
	
	ORIG_IDirectDrawSurfaceV7_GetColorKey=(TD_IDirectDrawSurfaceV7_GetColorKey)pVtable[16];
	NEW_IDirectDrawSurfaceV7_GetColorKey=My_IDDSV7_GetColorKey;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV7_GetColorKey, NEW_IDirectDrawSurfaceV7_GetColorKey);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface7::GetColorKey) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV7_GetDC=(TD_IDirectDrawSurfaceV7_GetDC)pVtable[17];
	NEW_IDirectDrawSurfaceV7_GetDC=My_IDDSV7_GetDC;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV7_GetDC, NEW_IDirectDrawSurfaceV7_GetDC);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface7::GetDC) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV7_GetFlipStatus=(TD_IDirectDrawSurfaceV7_GetFlipStatus)pVtable[18];
	NEW_IDirectDrawSurfaceV7_GetFlipStatus=My_IDDSV7_GetFlipStatus;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV7_GetFlipStatus, NEW_IDirectDrawSurfaceV7_GetFlipStatus);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface7::GetFlipStatus) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV7_GetOverlayPosition=(TD_IDirectDrawSurfaceV7_GetOverlayPosition)pVtable[19];
	NEW_IDirectDrawSurfaceV7_GetOverlayPosition=My_IDDSV7_GetOverlayPosition;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV7_GetOverlayPosition, NEW_IDirectDrawSurfaceV7_GetOverlayPosition);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface7::GetOverlayPosition) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV7_GetPalette=(TD_IDirectDrawSurfaceV7_GetPalette)pVtable[20];
	NEW_IDirectDrawSurfaceV7_GetPalette=My_IDDSV7_GetPalette;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV7_GetPalette, NEW_IDirectDrawSurfaceV7_GetPalette);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface7::GetPalette) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV7_GetPixelFormat=(TD_IDirectDrawSurfaceV7_GetPixelFormat)pVtable[21];
	NEW_IDirectDrawSurfaceV7_GetPixelFormat=My_IDDSV7_GetPixelFormat;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV7_GetPixelFormat, NEW_IDirectDrawSurfaceV7_GetPixelFormat);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface7::GetPixelFormat) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV7_GetSurfaceDesc=(TD_IDirectDrawSurfaceV7_GetSurfaceDesc)pVtable[22];
	NEW_IDirectDrawSurfaceV7_GetSurfaceDesc=My_IDDSV7_GetSurfaceDesc;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV7_GetSurfaceDesc, NEW_IDirectDrawSurfaceV7_GetSurfaceDesc);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface7::GetSurfaceDesc) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV7_Initialize=(TD_IDirectDrawSurfaceV7_Initialize)pVtable[23];
	NEW_IDirectDrawSurfaceV7_Initialize=My_IDDSV7_Initialize;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV7_Initialize, NEW_IDirectDrawSurfaceV7_Initialize);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface7::Initialize) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV7_IsLost=(TD_IDirectDrawSurfaceV7_IsLost)pVtable[24];
	NEW_IDirectDrawSurfaceV7_IsLost=My_IDDSV7_IsLost;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV7_IsLost, NEW_IDirectDrawSurfaceV7_IsLost);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface7::IsLost) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV7_Lock=(TD_IDirectDrawSurfaceV7_Lock)pVtable[25];
	NEW_IDirectDrawSurfaceV7_Lock=My_IDDSV7_Lock;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV7_Lock, NEW_IDirectDrawSurfaceV7_Lock);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface7::Lock) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV7_ReleaseDC=(TD_IDirectDrawSurfaceV7_ReleaseDC)pVtable[26];
	NEW_IDirectDrawSurfaceV7_ReleaseDC=My_IDDSV7_ReleaseDC;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV7_ReleaseDC, NEW_IDirectDrawSurfaceV7_ReleaseDC);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface7::ReleaseDC) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV7_Restore=(TD_IDirectDrawSurfaceV7_Restore)pVtable[27];
	NEW_IDirectDrawSurfaceV7_Restore=My_IDDSV7_Restore;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV7_Restore, NEW_IDirectDrawSurfaceV7_Restore);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface7::Restore) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV7_SetClipper=(TD_IDirectDrawSurfaceV7_SetClipper)pVtable[28];
	NEW_IDirectDrawSurfaceV7_SetClipper=My_IDDSV7_SetClipper;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV7_SetClipper, NEW_IDirectDrawSurfaceV7_SetClipper);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface7::SetClipper) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV7_SetColorKey=(TD_IDirectDrawSurfaceV7_SetColorKey)pVtable[29];
	NEW_IDirectDrawSurfaceV7_SetColorKey=My_IDDSV7_SetColorKey;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV7_SetColorKey, NEW_IDirectDrawSurfaceV7_SetColorKey);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface7::SetColorKey) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV7_SetOverlayPosition=(TD_IDirectDrawSurfaceV7_SetOverlayPosition)pVtable[30];
	NEW_IDirectDrawSurfaceV7_SetOverlayPosition=My_IDDSV7_SetOverlayPosition;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV7_SetOverlayPosition, NEW_IDirectDrawSurfaceV7_SetOverlayPosition);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface7::SetOverlayPosition) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV7_SetPalette=(TD_IDirectDrawSurfaceV7_SetPalette)pVtable[31];
	NEW_IDirectDrawSurfaceV7_SetPalette=My_IDDSV7_SetPalette;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV7_SetPalette, NEW_IDirectDrawSurfaceV7_SetPalette);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface7::SetPalette) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV7_Unlock=(TD_IDirectDrawSurfaceV7_Unlock)pVtable[32];
	NEW_IDirectDrawSurfaceV7_Unlock=My_IDDSV7_Unlock;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV7_Unlock, NEW_IDirectDrawSurfaceV7_Unlock);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface7::Unlock) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV7_UpdateOverlay=(TD_IDirectDrawSurfaceV7_UpdateOverlay)pVtable[33];
	NEW_IDirectDrawSurfaceV7_UpdateOverlay=My_IDDSV7_UpdateOverlay;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV7_UpdateOverlay, NEW_IDirectDrawSurfaceV7_UpdateOverlay);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface7::UpdateOverlay) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV7_UpdateOverlayDisplay=(TD_IDirectDrawSurfaceV7_UpdateOverlayDisplay)pVtable[34];
	NEW_IDirectDrawSurfaceV7_UpdateOverlayDisplay=My_IDDSV7_UpdateOverlayDisplay;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV7_UpdateOverlayDisplay, NEW_IDirectDrawSurfaceV7_UpdateOverlayDisplay);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface7::UpdateOverlayDisplay) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV7_UpdateOverlayZOrder=(TD_IDirectDrawSurfaceV7_UpdateOverlayZOrder)pVtable[35];
	NEW_IDirectDrawSurfaceV7_UpdateOverlayZOrder=My_IDDSV7_UpdateOverlayZOrder;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV7_UpdateOverlayZOrder, NEW_IDirectDrawSurfaceV7_UpdateOverlayZOrder);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface7::UpdateOverlayZOrder) failed.\r\n");
	}

	ORIG_IDirectDrawSurfaceV7_GetDDInterface=(TD_IDirectDrawSurfaceV7_GetDDInterface)pVtable[36];
	NEW_IDirectDrawSurfaceV7_GetDDInterface=My_IDDSV7_GetDDInterface;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV7_GetDDInterface, NEW_IDirectDrawSurfaceV7_GetDDInterface);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface7::GetDDInterface) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV7_PageLock=(TD_IDirectDrawSurfaceV7_PageLock)pVtable[37];
	NEW_IDirectDrawSurfaceV7_PageLock=My_IDDSV7_PageLock;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV7_PageLock, NEW_IDirectDrawSurfaceV7_PageLock);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface7::PageLock) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV7_PageUnlock=(TD_IDirectDrawSurfaceV7_PageUnlock)pVtable[38];
	NEW_IDirectDrawSurfaceV7_PageUnlock=My_IDDSV7_PageUnlock;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV7_PageUnlock, NEW_IDirectDrawSurfaceV7_PageUnlock);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface7::PageUnlock) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV7_SetSurfaceDesc=(TD_IDirectDrawSurfaceV7_SetSurfaceDesc)pVtable[39];
	NEW_IDirectDrawSurfaceV7_SetSurfaceDesc=My_IDDSV7_SetSurfaceDesc;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV7_SetSurfaceDesc, NEW_IDirectDrawSurfaceV7_SetSurfaceDesc);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface7::SetSurfaceDesc) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV7_SetPrivateData=(TD_IDirectDrawSurfaceV7_SetPrivateData)pVtable[40];
	NEW_IDirectDrawSurfaceV7_SetPrivateData=My_IDDSV7_SetPrivateData;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV7_SetPrivateData, NEW_IDirectDrawSurfaceV7_SetPrivateData);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface7::SetPrivateData) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV7_GetPrivateData=(TD_IDirectDrawSurfaceV7_GetPrivateData)pVtable[41];
	NEW_IDirectDrawSurfaceV7_GetPrivateData=My_IDDSV7_GetPrivateData;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV7_GetPrivateData, NEW_IDirectDrawSurfaceV7_GetPrivateData);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface7::GetPrivateData) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV7_FreePrivateData=(TD_IDirectDrawSurfaceV7_FreePrivateData)pVtable[42];
	NEW_IDirectDrawSurfaceV7_FreePrivateData=My_IDDSV7_FreePrivateData;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV7_FreePrivateData, NEW_IDirectDrawSurfaceV7_FreePrivateData);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface7::FreePrivateData) failed.\r\n");
	}
	
	ORIG_IDirectDrawSurfaceV7_GetUniquenessValue=(TD_IDirectDrawSurfaceV7_GetUniquenessValue)pVtable[43];
	NEW_IDirectDrawSurfaceV7_GetUniquenessValue=My_IDDSV7_GetUniquenessValue;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV7_GetUniquenessValue, NEW_IDirectDrawSurfaceV7_GetUniquenessValue);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface7::GetUniquenessValue) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV7_ChangeUniquenessValue=(TD_IDirectDrawSurfaceV7_ChangeUniquenessValue)pVtable[44];
	NEW_IDirectDrawSurfaceV7_ChangeUniquenessValue=My_IDDSV7_ChangeUniquenessValue;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV7_ChangeUniquenessValue, NEW_IDirectDrawSurfaceV7_ChangeUniquenessValue);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface7::ChangeUniquenessValue) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV7_SetPriority=(TD_IDirectDrawSurfaceV7_SetPriority)pVtable[45];
	NEW_IDirectDrawSurfaceV7_SetPriority=My_IDDSV7_SetPriority;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV7_SetPriority, NEW_IDirectDrawSurfaceV7_SetPriority);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface7::SetPriority) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV7_GetPriority=(TD_IDirectDrawSurfaceV7_GetPriority)pVtable[46];
	NEW_IDirectDrawSurfaceV7_GetPriority=My_IDDSV7_GetPriority;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV7_GetPriority, NEW_IDirectDrawSurfaceV7_GetPriority);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface7::GetPriority) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV7_SetLOD=(TD_IDirectDrawSurfaceV7_SetLOD)pVtable[47];
	NEW_IDirectDrawSurfaceV7_SetLOD=My_IDDSV7_SetLOD;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV7_SetLOD, NEW_IDirectDrawSurfaceV7_SetLOD);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface7::SetLOD) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV7_GetLOD=(TD_IDirectDrawSurfaceV7_GetLOD)pVtable[48];
	NEW_IDirectDrawSurfaceV7_GetLOD=My_IDDSV7_GetLOD;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV7_GetLOD, NEW_IDirectDrawSurfaceV7_GetLOD);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface7::GetLOD) failed.\r\n");
	}
	
	result = DetourTransactionCommit();
	if (result != NO_ERROR) {
		OutputDebugString("DetourTransactionCommit failed.\r\n");
	}
	*/
	//OutputDebugString("HookInterface for IDirectDrawSurface7 exited.\r\n");
	
	
}
