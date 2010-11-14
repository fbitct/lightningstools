#include "stdafx.h"
/*
DETOURS HOOK CODE FOR THE IDIRECTDRAWSURFACE COM INTERFACE
*/

//function pointers to original methods and new methods

// IUnknown functions
HRESULT WINAPI My_IDDSV4_QueryInterface (LPUNKNOWN lpThis, REFIID riid, LPVOID* obp) 
{
	//OutputDebugString("My_IDDSV4_QueryInterface called.\r\n");
	return ORIG_IDirectDrawSurfaceV4_QueryInterface (lpThis, riid, obp);
}
ULONG   WINAPI My_IDDSV4_AddRef(LPUNKNOWN lpThis) 
{
	//OutputDebugString("My_IDDSV4_AddRef called.\r\n");
	return ORIG_IDirectDrawSurfaceV4_AddRef(lpThis);
}
ULONG   WINAPI My_IDDSV4_Release(LPUNKNOWN lpThis)
{
	//OutputDebugString("My_IDDSV4_Release called.\r\n");
	return ORIG_IDirectDrawSurfaceV4_Release(lpThis);
}

// IDirectDrawSurface (V1) functions
HRESULT WINAPI My_IDDSV4_AddAttachedSurface(
  LPDIRECTDRAWSURFACE4 lpThis,
  LPDIRECTDRAWSURFACE4 lpDDSAttachedSurface  
)
{
	//OutputDebugString("My_IDDSV4_AddAttachedSurface called.\r\n");
	return ORIG_IDirectDrawSurfaceV4_AddAttachedSurface(lpThis, lpDDSAttachedSurface);
}
HRESULT WINAPI My_IDDSV4_AddOverlayDirtyRect(
  LPDIRECTDRAWSURFACE4 lpThis,
  LPRECT lpRect  
)
{
	//OutputDebugString("My_IDDSV4_AddOverlayDirtyRect called.\r\n");
	return ORIG_IDirectDrawSurfaceV4_AddOverlayDirtyRect(lpThis, lpRect);
}
HRESULT WINAPI My_IDDSV4_Blt(
  LPDIRECTDRAWSURFACE4 lpThis,
  LPRECT lpDestRect,                    
  LPDIRECTDRAWSURFACE4 lpDDSrcSurface,  
  LPRECT lpSrcRect,                     
  DWORD dwFlags,                        
  LPDDBLTFX lpDDBltFx                   
)
{
	//OutputDebugString("My_IDDSV4_Blt called.\r\n");
	return ORIG_IDirectDrawSurfaceV4_Blt (lpThis, lpDestRect, lpDDSrcSurface, lpSrcRect, dwFlags, lpDDBltFx);
}
HRESULT WINAPI My_IDDSV4_BltBatch(
  LPDIRECTDRAWSURFACE4 lpThis,
  LPDDBLTBATCH lpDDBltBatch,  
  DWORD dwCount,              
  DWORD dwFlags               
)
{
	//OutputDebugString("My_IDDSV4_BltBatch called.\r\n");
	return ORIG_IDirectDrawSurfaceV4_BltBatch (lpThis, lpDDBltBatch, dwCount, dwFlags);
}
HRESULT WINAPI My_IDDSV4_BltFast(
  LPDIRECTDRAWSURFACE4 lpThis,
  DWORD dwX,                            
  DWORD dwY,                            
  LPDIRECTDRAWSURFACE4 lpDDSrcSurface,  
  LPRECT lpSrcRect,                     
  DWORD dwTrans                         
)
{
	//OutputDebugString("My_IDDSV4_BltFast called.\r\n");
	return ORIG_IDirectDrawSurfaceV4_BltFast (lpThis, dwX, dwY, lpDDSrcSurface, lpSrcRect, dwTrans);
}
HRESULT WINAPI My_IDDSV4_DeleteAttachedSurface(
  LPDIRECTDRAWSURFACE4 lpThis,
  DWORD dwFlags,                             
  LPDIRECTDRAWSURFACE4 lpDDSAttachedSurface  
)
{
	//OutputDebugString("My_IDDSV4_DeleteAttachedSurface called.\r\n");
	return ORIG_IDirectDrawSurfaceV4_DeleteAttachedSurface (lpThis, dwFlags, lpDDSAttachedSurface);
}
HRESULT WINAPI My_IDDSV4_EnumAttachedSurfaces(
  LPDIRECTDRAWSURFACE4 lpThis,
  LPVOID lpContext,                                
  LPDDENUMSURFACESCALLBACK2 lpEnumSurfacesCallback
)
{
	//OutputDebugString("My_IDDSV4_EnumAttachedSurfaces called.\r\n");
	return ORIG_IDirectDrawSurfaceV4_EnumAttachedSurfaces (lpThis, lpContext, lpEnumSurfacesCallback);
}
HRESULT WINAPI My_IDDSV4_EnumOverlayZOrders(
  LPDIRECTDRAWSURFACE4 lpThis,
  DWORD dwFlags,                          
  LPVOID lpContext,                       
  LPDDENUMSURFACESCALLBACK2 lpfnCallback
)
{
	//OutputDebugString("My_IDDSV4_EnumOverlayZOrders called.\r\n");
	return ORIG_IDirectDrawSurfaceV4_EnumOverlayZOrders (lpThis, dwFlags, lpContext, lpfnCallback);
}
HRESULT WINAPI My_IDDSV4_Flip(
  LPDIRECTDRAWSURFACE4 lpThis,
  LPDIRECTDRAWSURFACE4 lpDDSurfaceTargetOverride,  
  DWORD dwFlags                                    
)
{
	//OutputDebugString("My_IDDSV4_Flip called.\r\n");
	return ORIG_IDirectDrawSurfaceV4_Flip (lpThis, lpDDSurfaceTargetOverride, dwFlags);
}
HRESULT WINAPI My_IDDSV4_GetAttachedSurface(
  LPDIRECTDRAWSURFACE4 lpThis,
  LPDDSCAPS2 lpDDSCaps, 
  LPDIRECTDRAWSURFACE4 FAR *lplpDDAttachedSurface  
)
{
	//OutputDebugString("My_IDDSV4_GetAttachedSurface called.\r\n");
	return ORIG_IDirectDrawSurfaceV4_GetAttachedSurface(lpThis, lpDDSCaps, lplpDDAttachedSurface);
}
HRESULT WINAPI My_IDDSV4_GetBltStatus(
  LPDIRECTDRAWSURFACE4 lpThis,
  DWORD dwFlags  
)
{
	//OutputDebugString("My_IDDSV4_GetBltStatus called.\r\n");
	return ORIG_IDirectDrawSurfaceV4_GetBltStatus (lpThis, dwFlags);
}
HRESULT WINAPI My_IDDSV4_GetCaps(
  LPDIRECTDRAWSURFACE4 lpThis,
  LPDDSCAPS2 lpDDSCaps  
)
{
	//OutputDebugString("My_IDDSV4_GetCaps called.\r\n");
	return ORIG_IDirectDrawSurfaceV4_GetCaps(lpThis, lpDDSCaps);
}
HRESULT WINAPI My_IDDSV4_GetClipper(
  LPDIRECTDRAWSURFACE4 lpThis,
  LPDIRECTDRAWCLIPPER FAR *lplpDDClipper  
)
{
	//OutputDebugString("My_IDDSV4_GetClipper called.\r\n");
	return ORIG_IDirectDrawSurfaceV4_GetClipper(lpThis, lplpDDClipper);
}
HRESULT WINAPI My_IDDSV4_GetColorKey(
  LPDIRECTDRAWSURFACE4 lpThis,
  DWORD dwFlags,             
  LPDDCOLORKEY lpDDColorKey  
)
{
	//OutputDebugString("My_IDDSV4_GetColorKey called.\r\n");
	return ORIG_IDirectDrawSurfaceV4_GetColorKey (lpThis, dwFlags, lpDDColorKey);
}
HRESULT WINAPI My_IDDSV4_GetDC(
  LPDIRECTDRAWSURFACE4 lpThis,
  HDC FAR *lphDC  
)
{
	//OutputDebugString("My_IDDSV4_GetDC called.\r\n");
	return ORIG_IDirectDrawSurfaceV4_GetDC (lpThis, lphDC);
}
HRESULT WINAPI My_IDDSV4_GetFlipStatus(
  LPDIRECTDRAWSURFACE4 lpThis,
  DWORD dwFlags  
)
{
	//OutputDebugString("My_IDDSV4_GetFlipStatus called.\r\n");
	return ORIG_IDirectDrawSurfaceV4_GetFlipStatus (lpThis, dwFlags);
}
HRESULT WINAPI My_IDDSV4_GetOverlayPosition(
  LPDIRECTDRAWSURFACE4 lpThis,
  LPLONG lplX, 
  LPLONG lplY  
)
{
	//OutputDebugString("My_IDDSV4_GetOverlayPosition called.\r\n");
	return ORIG_IDirectDrawSurfaceV4_GetOverlayPosition (lpThis, lplX, lplY);
}
HRESULT WINAPI My_IDDSV4_GetPalette(
  LPDIRECTDRAWSURFACE4 lpThis,
  LPDIRECTDRAWPALETTE FAR *lplpDDPalette  
)
{
	//OutputDebugString("My_IDDSV4_GetPalette called.\r\n");
	return ORIG_IDirectDrawSurfaceV4_GetPalette (lpThis, lplpDDPalette);
}

HRESULT WINAPI My_IDDSV4_GetPixelFormat(
  LPDIRECTDRAWSURFACE4 lpThis,
  LPDDPIXELFORMAT lpDDPixelFormat  
)
{
	//OutputDebugString("My_IDDSV4_GetPixelFormat called.\r\n");
	return ORIG_IDirectDrawSurfaceV4_GetPixelFormat(lpThis, lpDDPixelFormat);
}
HRESULT WINAPI My_IDDSV4_GetSurfaceDesc(
  LPDIRECTDRAWSURFACE4 lpThis,
  LPDDSURFACEDESC2 lpDDSurfaceDesc  
)
{
	//OutputDebugString("My_IDDSV4_GetSurfaceDesc called.\r\n");
	return ORIG_IDirectDrawSurfaceV4_GetSurfaceDesc(lpThis, lpDDSurfaceDesc);
}
HRESULT WINAPI My_IDDSV4_Initialize(
  LPDIRECTDRAWSURFACE4 lpThis,
  LPDIRECTDRAW lpDD,               
  LPDDSURFACEDESC2 lpDDSurfaceDesc  
)
{
	//OutputDebugString("My_IDDSV4_Initialize called.\r\n");
	return ORIG_IDirectDrawSurfaceV4_Initialize (lpThis, lpDD, lpDDSurfaceDesc);
}
HRESULT WINAPI My_IDDSV4_IsLost(
  LPDIRECTDRAWSURFACE4 lpThis
)
{
	//OutputDebugString("My_IDDSV4_IsLost called.\r\n");
	return ORIG_IDirectDrawSurfaceV4_IsLost (lpThis);
}
HRESULT WINAPI My_IDDSV4_Lock(
  LPDIRECTDRAWSURFACE4 lpThis,
  LPRECT lpDestRect,                
  LPDDSURFACEDESC2 lpDDSurfaceDesc,  
  DWORD dwFlags,                    
  HANDLE hEvent                     
)
{
	//OutputDebugString("My_IDDSV4_Lock called.\r\n");
	return ORIG_IDirectDrawSurfaceV4_Lock (lpThis, lpDestRect, lpDDSurfaceDesc, dwFlags, hEvent);
}
HRESULT WINAPI My_IDDSV4_ReleaseDC(
  LPDIRECTDRAWSURFACE4 lpThis,
  HDC hDC  
)
{
	//OutputDebugString("My_IDDSV4_ReleaseDC called.\r\n");
	return ORIG_IDirectDrawSurfaceV4_ReleaseDC (lpThis, hDC);
}
HRESULT WINAPI My_IDDSV4_Restore(
  LPDIRECTDRAWSURFACE4 lpThis
)
{
	//OutputDebugString("My_IDDSV4_Restore called.\r\n");
	return ORIG_IDirectDrawSurfaceV4_Restore (lpThis);
}
HRESULT WINAPI My_IDDSV4_SetClipper(
  LPDIRECTDRAWSURFACE4 lpThis,
  LPDIRECTDRAWCLIPPER lpDDClipper  
)
{
	//OutputDebugString("My_IDDSV4_SetClipper called.\r\n");
	return ORIG_IDirectDrawSurfaceV4_SetClipper (lpThis, lpDDClipper);
}
HRESULT WINAPI My_IDDSV4_SetColorKey(
  LPDIRECTDRAWSURFACE4 lpThis,
  DWORD dwFlags,             
  LPDDCOLORKEY lpDDColorKey  
)
{
	//OutputDebugString("My_IDDSV4_SetColorKey called.\r\n");
	return ORIG_IDirectDrawSurfaceV4_SetColorKey (lpThis, dwFlags, lpDDColorKey);
}
HRESULT WINAPI My_IDDSV4_SetOverlayPosition(
  LPDIRECTDRAWSURFACE4 lpThis,
  LONG lX, 
  LONG lY  
)
{
	//OutputDebugString("My_IDDSV4_SetOverlayPosition called.\r\n");
	return ORIG_IDirectDrawSurfaceV4_SetOverlayPosition (lpThis, lX, lY);
}
HRESULT WINAPI My_IDDSV4_SetPalette(
  LPDIRECTDRAWSURFACE4 lpThis,
  LPDIRECTDRAWPALETTE lpDDPalette  
)
{
	//OutputDebugString("My_IDDSV4_SetPalette called.\r\n");
	return ORIG_IDirectDrawSurfaceV4_SetPalette (lpThis, lpDDPalette);
}
HRESULT WINAPI My_IDDSV4_Unlock(
  LPDIRECTDRAWSURFACE4 lpThis,
  LPVOID lpRect 
)
{
	//OutputDebugString("My_IDDSV4_Unlock called.\r\n");
	return ORIG_IDirectDrawSurfaceV4_Unlock (lpThis, lpRect);
}
HRESULT WINAPI My_IDDSV4_UpdateOverlay(
  LPDIRECTDRAWSURFACE4 lpThis,
  LPRECT lpSrcRect,                      
  LPDIRECTDRAWSURFACE4 lpDDDestSurface,  
  LPRECT lpDestRect,                     
  DWORD dwFlags,                         
  LPDDOVERLAYFX lpDDOverlayFx            
)
{
	//OutputDebugString("My_IDDSV4_UpdateOverlay called.\r\n");
	return ORIG_IDirectDrawSurfaceV4_UpdateOverlay (lpThis, lpSrcRect, lpDDDestSurface, lpDestRect,dwFlags,lpDDOverlayFx);
}
HRESULT WINAPI My_IDDSV4_UpdateOverlayDisplay(
  LPDIRECTDRAWSURFACE4 lpThis,
  DWORD dwFlags  
)
{
	//OutputDebugString("My_IDDSV4_UpdateOverlayDisplay called.\r\n");
	return ORIG_IDirectDrawSurfaceV4_UpdateOverlayDisplay (lpThis, dwFlags);
}
HRESULT WINAPI My_IDDSV4_UpdateOverlayZOrder(
  LPDIRECTDRAWSURFACE4 lpThis,
  DWORD dwFlags,                       
  LPDIRECTDRAWSURFACE4 lpDDSReference  
)
{
	//OutputDebugString("My_IDDSV4_UpdateOverlayZOrder called.\r\n");
	return ORIG_IDirectDrawSurfaceV4_UpdateOverlayZOrder (lpThis, dwFlags, lpDDSReference);
}
// IDirectDrawSurface2 functions

HRESULT WINAPI My_IDDSV4_GetDDInterface(
  LPDIRECTDRAWSURFACE4 lpThis,
  LPVOID FAR *lplpDD  
)
{
	//OutputDebugString("My_IDDSV4_GetDDInterface called.\r\n");
	return ORIG_IDirectDrawSurfaceV4_GetDDInterface(lpThis, lplpDD);
}
HRESULT WINAPI My_IDDSV4_PageLock(
  LPDIRECTDRAWSURFACE4 lpThis,
  DWORD dwFlags  
)
{
	//OutputDebugString("My_IDDSV4_PageLock called.\r\n");
	return ORIG_IDirectDrawSurfaceV4_PageLock(lpThis, dwFlags);
}
HRESULT WINAPI My_IDDSV4_PageUnlock(
  LPDIRECTDRAWSURFACE4 lpThis,
  DWORD dwFlags  
)
{
	//OutputDebugString("My_IDDSV4_PageUnlock called.\r\n");
	return ORIG_IDirectDrawSurfaceV4_PageUnlock(lpThis, dwFlags);
}
// IDirectDrawSurface3 functions
HRESULT WINAPI My_IDDSV4_SetSurfaceDesc(
  LPDIRECTDRAWSURFACE4 lpThis,
  LPDDSURFACEDESC2 lpddsd,  
  DWORD dwFlags            
)
{
	//OutputDebugString("My_IDDSV4_SetSurfaceDesc called.\r\n");
	return ORIG_IDirectDrawSurfaceV4_SetSurfaceDesc(lpThis, lpddsd, dwFlags);
}
// IDirectDrawSurface4 functions
HRESULT WINAPI My_IDDSV4_SetPrivateData( 
  LPDIRECTDRAWSURFACE4 lpThis,
  REFGUID guidTag, 
  LPVOID  lpData,
  DWORD   cbSize,
  DWORD   dwFlags 
)
{
	//OutputDebugString("My_IDDSV4_SetPrivateData called.\r\n");
	return ORIG_IDirectDrawSurfaceV4_SetPrivateData(lpThis, guidTag, lpData, cbSize, dwFlags);
}
HRESULT WINAPI My_IDDSV4_GetPrivateData(
  LPDIRECTDRAWSURFACE4 lpThis,
  REFGUID guidTag,
  LPVOID  lpBuffer,
  LPDWORD lpcbBufferSize
)
{
	//OutputDebugString("My_IDDSV4_GetPrivateData called.\r\n");
	return ORIG_IDirectDrawSurfaceV4_GetPrivateData(lpThis, guidTag, lpBuffer, lpcbBufferSize);
}
HRESULT WINAPI My_IDDSV4_FreePrivateData( 
  LPDIRECTDRAWSURFACE4 lpThis,
  REFGUID guidTag 
)
{
	//OutputDebugString("My_IDDSV4_FreePrivateData called.\r\n");
	return ORIG_IDirectDrawSurfaceV4_FreePrivateData(lpThis, guidTag);
}
HRESULT WINAPI My_IDDSV4_GetUniquenessValue( 
  LPDIRECTDRAWSURFACE4 lpThis,
  LPDWORD lpValue 
)
{
	//OutputDebugString("My_IDDSV4_GetUniquenessValue called.\r\n");
	return ORIG_IDirectDrawSurfaceV4_GetUniquenessValue(lpThis, lpValue);
}
HRESULT WINAPI My_IDDSV4_ChangeUniquenessValue(
  LPDIRECTDRAWSURFACE4 lpThis
)
{
	//OutputDebugString("My_IDDSV4_ChangeUniquenessValue called.\r\n");
	return ORIG_IDirectDrawSurfaceV4_ChangeUniquenessValue(lpThis);
}


void HookDirectDrawSurface4Interface  (LPDIRECTDRAWSURFACE4 lpInterface) 
{
	//OutputDebugString("HookInterface for IDirectDrawSurface4 reached.\r\n");
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
	ORIG_IDirectDrawSurfaceV4_QueryInterface =(TD_IDirectDrawSurfaceV4_QueryInterface)pVtable[0];
	NEW_IDirectDrawSurfaceV4_QueryInterface =My_IDDSV4_QueryInterface;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV4_QueryInterface, NEW_IDirectDrawSurfaceV4_QueryInterface);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface4::QueryInterface) failed.\r\n");
	}

	ORIG_IDirectDrawSurfaceV4_AddRef=(TD_IDirectDrawSurfaceV4_AddRef)pVtable[1];
	NEW_IDirectDrawSurfaceV4_AddRef=My_IDDSV4_AddRef;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV4_AddRef, NEW_IDirectDrawSurfaceV4_AddRef);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface4::AddRef) failed.\r\n");
	}

	ORIG_IDirectDrawSurfaceV4_Release=(TD_IDirectDrawSurfaceV4_Release)pVtable[2];
	NEW_IDirectDrawSurfaceV4_Release=My_IDDSV4_Release;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV4_Release, NEW_IDirectDrawSurfaceV4_Release);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface4::Release) failed.\r\n");
	}

	ORIG_IDirectDrawSurfaceV4_AddAttachedSurface=(TD_IDirectDrawSurfaceV4_AddAttachedSurface)pVtable[3];
	NEW_IDirectDrawSurfaceV4_AddAttachedSurface=My_IDDSV4_AddAttachedSurface;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV4_AddAttachedSurface, NEW_IDirectDrawSurfaceV4_AddAttachedSurface);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface4::AddAttachedSurface) failed.\r\n");
	}

	ORIG_IDirectDrawSurfaceV4_AddOverlayDirtyRect =(TD_IDirectDrawSurfaceV4_AddOverlayDirtyRect)pVtable[4];
	NEW_IDirectDrawSurfaceV4_AddOverlayDirtyRect =My_IDDSV4_AddOverlayDirtyRect ;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV4_AddOverlayDirtyRect , NEW_IDirectDrawSurfaceV4_AddOverlayDirtyRect);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface4::AddOverlayDirtyRect) failed.\r\n");
	}

	ORIG_IDirectDrawSurfaceV4_Blt =(TD_IDirectDrawSurfaceV4_Blt)pVtable[5];
	NEW_IDirectDrawSurfaceV4_Blt =My_IDDSV4_Blt ;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV4_Blt, NEW_IDirectDrawSurfaceV4_Blt);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface4::Blt) failed.\r\n");
	}

	ORIG_IDirectDrawSurfaceV4_BltBatch =(TD_IDirectDrawSurfaceV4_BltBatch)pVtable[6];
	NEW_IDirectDrawSurfaceV4_BltBatch =My_IDDSV4_BltBatch ;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV4_BltBatch, NEW_IDirectDrawSurfaceV4_BltBatch);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface4::BltBatch) failed.\r\n");
	}

	ORIG_IDirectDrawSurfaceV4_BltFast =(TD_IDirectDrawSurfaceV4_BltFast)pVtable[7];
	NEW_IDirectDrawSurfaceV4_BltFast =My_IDDSV4_BltFast ;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV4_BltFast, NEW_IDirectDrawSurfaceV4_BltFast);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface4::BltFast) failed.\r\n");
	}
	
	ORIG_IDirectDrawSurfaceV4_DeleteAttachedSurface=(TD_IDirectDrawSurfaceV4_DeleteAttachedSurface)pVtable[8];
	NEW_IDirectDrawSurfaceV4_DeleteAttachedSurface =My_IDDSV4_DeleteAttachedSurface;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV4_DeleteAttachedSurface, NEW_IDirectDrawSurfaceV4_DeleteAttachedSurface);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface4::DeleteAttachedSurface) failed.\r\n");
	}

	ORIG_IDirectDrawSurfaceV4_EnumAttachedSurfaces=(TD_IDirectDrawSurfaceV4_EnumAttachedSurfaces)pVtable[9];
	NEW_IDirectDrawSurfaceV4_EnumAttachedSurfaces=My_IDDSV4_EnumAttachedSurfaces;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV4_EnumAttachedSurfaces, NEW_IDirectDrawSurfaceV4_EnumAttachedSurfaces);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface4::EnumAttachedSurfaces) failed.\r\n");
	}

	ORIG_IDirectDrawSurfaceV4_EnumOverlayZOrders=(TD_IDirectDrawSurfaceV4_EnumOverlayZOrders)pVtable[10];
	NEW_IDirectDrawSurfaceV4_EnumOverlayZOrders=My_IDDSV4_EnumOverlayZOrders;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV4_EnumOverlayZOrders, NEW_IDirectDrawSurfaceV4_EnumOverlayZOrders);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface4::EnumOverlayZOrders) failed.\r\n");
	}

	ORIG_IDirectDrawSurfaceV4_Flip=(TD_IDirectDrawSurfaceV4_Flip)pVtable[11];
	NEW_IDirectDrawSurfaceV4_Flip=My_IDDSV4_Flip;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV4_Flip, NEW_IDirectDrawSurfaceV4_Flip);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface4::Flip) failed.\r\n");
	}

	ORIG_IDirectDrawSurfaceV4_GetAttachedSurface=(TD_IDirectDrawSurfaceV4_GetAttachedSurface)pVtable[12];
	NEW_IDirectDrawSurfaceV4_GetAttachedSurface=My_IDDSV4_GetAttachedSurface;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV4_GetAttachedSurface, NEW_IDirectDrawSurfaceV4_GetAttachedSurface);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface4::GetAttachedSurface) failed.\r\n");
	}
	
	ORIG_IDirectDrawSurfaceV4_GetBltStatus=(TD_IDirectDrawSurfaceV4_GetBltStatus)pVtable[13];
	NEW_IDirectDrawSurfaceV4_GetBltStatus=My_IDDSV4_GetBltStatus;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV4_GetBltStatus, NEW_IDirectDrawSurfaceV4_GetBltStatus);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface4::GetBltStatus) failed.\r\n");
	}

	ORIG_IDirectDrawSurfaceV4_GetCaps=(TD_IDirectDrawSurfaceV4_GetCaps)pVtable[14];
	NEW_IDirectDrawSurfaceV4_GetCaps=My_IDDSV4_GetCaps;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV4_GetCaps, NEW_IDirectDrawSurfaceV4_GetCaps);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface4::GetCaps) failed.\r\n");
	}

	ORIG_IDirectDrawSurfaceV4_GetClipper=(TD_IDirectDrawSurfaceV4_GetClipper)pVtable[15];
	NEW_IDirectDrawSurfaceV4_GetClipper=My_IDDSV4_GetClipper;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV4_GetClipper, NEW_IDirectDrawSurfaceV4_GetClipper);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface4::GetClipper) failed.\r\n");
	}
	
	ORIG_IDirectDrawSurfaceV4_GetColorKey=(TD_IDirectDrawSurfaceV4_GetColorKey)pVtable[16];
	NEW_IDirectDrawSurfaceV4_GetColorKey=My_IDDSV4_GetColorKey;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV4_GetColorKey, NEW_IDirectDrawSurfaceV4_GetColorKey);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface4::GetColorKey) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV4_GetDC=(TD_IDirectDrawSurfaceV4_GetDC)pVtable[17];
	NEW_IDirectDrawSurfaceV4_GetDC=My_IDDSV4_GetDC;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV4_GetDC, NEW_IDirectDrawSurfaceV4_GetDC);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface4::GetDC) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV4_GetFlipStatus=(TD_IDirectDrawSurfaceV4_GetFlipStatus)pVtable[18];
	NEW_IDirectDrawSurfaceV4_GetFlipStatus=My_IDDSV4_GetFlipStatus;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV4_GetFlipStatus, NEW_IDirectDrawSurfaceV4_GetFlipStatus);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface4::GetFlipStatus) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV4_GetOverlayPosition=(TD_IDirectDrawSurfaceV4_GetOverlayPosition)pVtable[19];
	NEW_IDirectDrawSurfaceV4_GetOverlayPosition=My_IDDSV4_GetOverlayPosition;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV4_GetOverlayPosition, NEW_IDirectDrawSurfaceV4_GetOverlayPosition);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface4::GetOverlayPosition) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV4_GetPalette=(TD_IDirectDrawSurfaceV4_GetPalette)pVtable[20];
	NEW_IDirectDrawSurfaceV4_GetPalette=My_IDDSV4_GetPalette;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV4_GetPalette, NEW_IDirectDrawSurfaceV4_GetPalette);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface4::GetPalette) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV4_GetPixelFormat=(TD_IDirectDrawSurfaceV4_GetPixelFormat)pVtable[21];
	NEW_IDirectDrawSurfaceV4_GetPixelFormat=My_IDDSV4_GetPixelFormat;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV4_GetPixelFormat, NEW_IDirectDrawSurfaceV4_GetPixelFormat);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface4::GetPixelFormat) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV4_GetSurfaceDesc=(TD_IDirectDrawSurfaceV4_GetSurfaceDesc)pVtable[22];
	NEW_IDirectDrawSurfaceV4_GetSurfaceDesc=My_IDDSV4_GetSurfaceDesc;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV4_GetSurfaceDesc, NEW_IDirectDrawSurfaceV4_GetSurfaceDesc);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface4::GetSurfaceDesc) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV4_Initialize=(TD_IDirectDrawSurfaceV4_Initialize)pVtable[23];
	NEW_IDirectDrawSurfaceV4_Initialize=My_IDDSV4_Initialize;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV4_Initialize, NEW_IDirectDrawSurfaceV4_Initialize);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface4::Initialize) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV4_IsLost=(TD_IDirectDrawSurfaceV4_IsLost)pVtable[24];
	NEW_IDirectDrawSurfaceV4_IsLost=My_IDDSV4_IsLost;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV4_IsLost, NEW_IDirectDrawSurfaceV4_IsLost);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface4::IsLost) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV4_Lock=(TD_IDirectDrawSurfaceV4_Lock)pVtable[25];
	NEW_IDirectDrawSurfaceV4_Lock=My_IDDSV4_Lock;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV4_Lock, NEW_IDirectDrawSurfaceV4_Lock);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface4::Lock) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV4_ReleaseDC=(TD_IDirectDrawSurfaceV4_ReleaseDC)pVtable[26];
	NEW_IDirectDrawSurfaceV4_ReleaseDC=My_IDDSV4_ReleaseDC;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV4_ReleaseDC, NEW_IDirectDrawSurfaceV4_ReleaseDC);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface4::ReleaseDC) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV4_Restore=(TD_IDirectDrawSurfaceV4_Restore)pVtable[27];
	NEW_IDirectDrawSurfaceV4_Restore=My_IDDSV4_Restore;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV4_Restore, NEW_IDirectDrawSurfaceV4_Restore);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface4::Restore) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV4_SetClipper=(TD_IDirectDrawSurfaceV4_SetClipper)pVtable[28];
	NEW_IDirectDrawSurfaceV4_SetClipper=My_IDDSV4_SetClipper;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV4_SetClipper, NEW_IDirectDrawSurfaceV4_SetClipper);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface4::SetClipper) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV4_SetColorKey=(TD_IDirectDrawSurfaceV4_SetColorKey)pVtable[29];
	NEW_IDirectDrawSurfaceV4_SetColorKey=My_IDDSV4_SetColorKey;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV4_SetColorKey, NEW_IDirectDrawSurfaceV4_SetColorKey);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface4::SetColorKey) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV4_SetOverlayPosition=(TD_IDirectDrawSurfaceV4_SetOverlayPosition)pVtable[30];
	NEW_IDirectDrawSurfaceV4_SetOverlayPosition=My_IDDSV4_SetOverlayPosition;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV4_SetOverlayPosition, NEW_IDirectDrawSurfaceV4_SetOverlayPosition);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface4::SetOverlayPosition) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV4_SetPalette=(TD_IDirectDrawSurfaceV4_SetPalette)pVtable[31];
	NEW_IDirectDrawSurfaceV4_SetPalette=My_IDDSV4_SetPalette;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV4_SetPalette, NEW_IDirectDrawSurfaceV4_SetPalette);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface4::SetPalette) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV4_Unlock=(TD_IDirectDrawSurfaceV4_Unlock)pVtable[32];
	NEW_IDirectDrawSurfaceV4_Unlock=My_IDDSV4_Unlock;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV4_Unlock, NEW_IDirectDrawSurfaceV4_Unlock);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface4::Unlock) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV4_UpdateOverlay=(TD_IDirectDrawSurfaceV4_UpdateOverlay)pVtable[33];
	NEW_IDirectDrawSurfaceV4_UpdateOverlay=My_IDDSV4_UpdateOverlay;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV4_UpdateOverlay, NEW_IDirectDrawSurfaceV4_UpdateOverlay);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface4::UpdateOverlay) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV4_UpdateOverlayDisplay=(TD_IDirectDrawSurfaceV4_UpdateOverlayDisplay)pVtable[34];
	NEW_IDirectDrawSurfaceV4_UpdateOverlayDisplay=My_IDDSV4_UpdateOverlayDisplay;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV4_UpdateOverlayDisplay, NEW_IDirectDrawSurfaceV4_UpdateOverlayDisplay);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface4::UpdateOverlayDisplay) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV4_UpdateOverlayZOrder=(TD_IDirectDrawSurfaceV4_UpdateOverlayZOrder)pVtable[35];
	NEW_IDirectDrawSurfaceV4_UpdateOverlayZOrder=My_IDDSV4_UpdateOverlayZOrder;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV4_UpdateOverlayZOrder, NEW_IDirectDrawSurfaceV4_UpdateOverlayZOrder);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface4::UpdateOverlayZOrder) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV4_GetDDInterface=(TD_IDirectDrawSurfaceV4_GetDDInterface)pVtable[36];
	NEW_IDirectDrawSurfaceV4_GetDDInterface=My_IDDSV4_GetDDInterface;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV4_GetDDInterface, NEW_IDirectDrawSurfaceV4_GetDDInterface);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface4::GetDDInterface) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV4_PageLock=(TD_IDirectDrawSurfaceV4_PageLock)pVtable[37];
	NEW_IDirectDrawSurfaceV4_PageLock=My_IDDSV4_PageLock;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV4_PageLock, NEW_IDirectDrawSurfaceV4_PageLock);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface4::PageLock) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV4_PageUnlock=(TD_IDirectDrawSurfaceV4_PageUnlock)pVtable[38];
	NEW_IDirectDrawSurfaceV4_PageUnlock=My_IDDSV4_PageUnlock;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV4_PageUnlock, NEW_IDirectDrawSurfaceV4_PageUnlock);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface4::PageUnlock) failed.\r\n");
	}

	ORIG_IDirectDrawSurfaceV4_SetSurfaceDesc=(TD_IDirectDrawSurfaceV4_SetSurfaceDesc)pVtable[39];
	NEW_IDirectDrawSurfaceV4_SetSurfaceDesc=My_IDDSV4_SetSurfaceDesc;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV4_SetSurfaceDesc, NEW_IDirectDrawSurfaceV4_SetSurfaceDesc);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface4::SetSurfaceDesc) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV4_SetPrivateData=(TD_IDirectDrawSurfaceV4_SetPrivateData)pVtable[40];
	NEW_IDirectDrawSurfaceV4_SetPrivateData=My_IDDSV4_SetPrivateData;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV4_SetPrivateData, NEW_IDirectDrawSurfaceV4_SetPrivateData);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface4::SetPrivateData) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV4_GetPrivateData=(TD_IDirectDrawSurfaceV4_GetPrivateData)pVtable[41];
	NEW_IDirectDrawSurfaceV4_GetPrivateData=My_IDDSV4_GetPrivateData;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV4_GetPrivateData, NEW_IDirectDrawSurfaceV4_GetPrivateData);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface4::GetPrivateData) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV4_FreePrivateData=(TD_IDirectDrawSurfaceV4_FreePrivateData)pVtable[42];
	NEW_IDirectDrawSurfaceV4_FreePrivateData=My_IDDSV4_FreePrivateData;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV4_FreePrivateData, NEW_IDirectDrawSurfaceV4_FreePrivateData);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface4::FreePrivateData) failed.\r\n");
	}

	ORIG_IDirectDrawSurfaceV4_GetUniquenessValue=(TD_IDirectDrawSurfaceV4_GetUniquenessValue)pVtable[43];
	NEW_IDirectDrawSurfaceV4_GetUniquenessValue=My_IDDSV4_GetUniquenessValue;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV4_GetUniquenessValue, NEW_IDirectDrawSurfaceV4_GetUniquenessValue);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface4::GetUniquenessValue) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV4_ChangeUniquenessValue=(TD_IDirectDrawSurfaceV4_ChangeUniquenessValue)pVtable[44];
	NEW_IDirectDrawSurfaceV4_ChangeUniquenessValue=My_IDDSV4_ChangeUniquenessValue;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV4_ChangeUniquenessValue, NEW_IDirectDrawSurfaceV4_ChangeUniquenessValue);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface4::ChangeUniquenessValue) failed.\r\n");
	}
	
	result = DetourTransactionCommit();
	if (result != NO_ERROR) {
		//OutputDebugString("DetourTransactionCommit failed.\r\n");
	}
	//OutputDebugString("HookInterface for IDirectDrawSurface4 exited.\r\n");
	
}
