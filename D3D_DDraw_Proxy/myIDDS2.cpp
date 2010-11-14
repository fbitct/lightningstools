#include "stdafx.h"
/*
DETOURS HOOK CODE FOR THE IDIRECTDRAWSURFACE COM INTERFACE
*/


//function pointers to original methods and new methods

// IUnknown functions
HRESULT WINAPI My_IDDSV2_QueryInterface (LPUNKNOWN lpThis, REFIID riid, LPVOID* obp) 
{
	//OutputDebugString("My_IDDSV2_QueryInterface called.\r\n");
	return ORIG_IDirectDrawSurfaceV2_QueryInterface (lpThis, riid, obp);
}
ULONG   WINAPI My_IDDSV2_AddRef(LPUNKNOWN lpThis) 
{
	//OutputDebugString("My_IDDSV2_AddRef called.\r\n");
	return ORIG_IDirectDrawSurfaceV2_AddRef(lpThis);
}
ULONG   WINAPI My_IDDSV2_Release(LPUNKNOWN lpThis)
{
	//OutputDebugString("My_IDDSV2_Release called.\r\n");
	return ORIG_IDirectDrawSurfaceV2_Release(lpThis);
}

// IDirectDrawSurface (V1) functions
HRESULT WINAPI My_IDDSV2_AddAttachedSurface(
  LPDIRECTDRAWSURFACE2 lpThis,
  LPDIRECTDRAWSURFACE2 lpDDSAttachedSurface  
)
{
	//OutputDebugString("My_IDDSV2_AddAttachedSurface called.\r\n");
	return ORIG_IDirectDrawSurfaceV2_AddAttachedSurface(lpThis, lpDDSAttachedSurface);
}
HRESULT WINAPI My_IDDSV2_AddOverlayDirtyRect(
  LPDIRECTDRAWSURFACE2 lpThis,
  LPRECT lpRect  
)
{
	//OutputDebugString("My_IDDSV2_AddOverlayDirtyRect called.\r\n");
	return ORIG_IDirectDrawSurfaceV2_AddOverlayDirtyRect(lpThis, lpRect);
}
HRESULT WINAPI My_IDDSV2_Blt(
  LPDIRECTDRAWSURFACE2 lpThis,
  LPRECT lpDestRect,                    
  LPDIRECTDRAWSURFACE2 lpDDSrcSurface,  
  LPRECT lpSrcRect,                     
  DWORD dwFlags,                        
  LPDDBLTFX lpDDBltFx                   
)
{
	//OutputDebugString("My_IDDSV2_Blt called.\r\n");
	return ORIG_IDirectDrawSurfaceV2_Blt (lpThis, lpDestRect, lpDDSrcSurface, lpSrcRect, dwFlags, lpDDBltFx);
}
HRESULT WINAPI My_IDDSV2_BltBatch(
  LPDIRECTDRAWSURFACE2 lpThis,
  LPDDBLTBATCH lpDDBltBatch,  
  DWORD dwCount,              
  DWORD dwFlags               
)
{
	//OutputDebugString("My_IDDSV2_BltBatch called.\r\n");
	return ORIG_IDirectDrawSurfaceV2_BltBatch (lpThis, lpDDBltBatch, dwCount, dwFlags);
}
HRESULT WINAPI My_IDDSV2_BltFast(
  LPDIRECTDRAWSURFACE2 lpThis,
  DWORD dwX,                            
  DWORD dwY,                            
  LPDIRECTDRAWSURFACE2 lpDDSrcSurface,  
  LPRECT lpSrcRect,                     
  DWORD dwTrans                         
)
{
	//OutputDebugString("My_IDDSV2_BltFast called.\r\n");
	return ORIG_IDirectDrawSurfaceV2_BltFast (lpThis, dwX, dwY, lpDDSrcSurface, lpSrcRect, dwTrans);
}
HRESULT WINAPI My_IDDSV2_DeleteAttachedSurface(
  LPDIRECTDRAWSURFACE2 lpThis,
  DWORD dwFlags,                             
  LPDIRECTDRAWSURFACE2 lpDDSAttachedSurface  
)
{
	//OutputDebugString("My_IDDSV2_DeleteAttachedSurface called.\r\n");
	return ORIG_IDirectDrawSurfaceV2_DeleteAttachedSurface (lpThis, dwFlags, lpDDSAttachedSurface);
}
HRESULT WINAPI My_IDDSV2_EnumAttachedSurfaces(
  LPDIRECTDRAWSURFACE2 lpThis,
  LPVOID lpContext,                                
  LPDDENUMSURFACESCALLBACK lpEnumSurfacesCallback  
)
{
	//OutputDebugString("My_IDDSV2_EnumAttachedSurfaces called.\r\n");
	return ORIG_IDirectDrawSurfaceV2_EnumAttachedSurfaces (lpThis, lpContext, lpEnumSurfacesCallback);
}
HRESULT WINAPI My_IDDSV2_EnumOverlayZOrders(
  LPDIRECTDRAWSURFACE2 lpThis,
  DWORD dwFlags,                          
  LPVOID lpContext,                       
  LPDDENUMSURFACESCALLBACK lpfnCallback  
)
{
	//OutputDebugString("My_IDDSV2_EnumOverlayZOrders called.\r\n");
	return ORIG_IDirectDrawSurfaceV2_EnumOverlayZOrders (lpThis, dwFlags, lpContext, lpfnCallback);
}
HRESULT WINAPI My_IDDSV2_Flip(
  LPDIRECTDRAWSURFACE2 lpThis,
  LPDIRECTDRAWSURFACE2 lpDDSurfaceTargetOverride,  
  DWORD dwFlags                                    
)
{
	//OutputDebugString("My_IDDSV2_Flip called.\r\n");
	return ORIG_IDirectDrawSurfaceV2_Flip (lpThis, lpDDSurfaceTargetOverride, dwFlags);
}
HRESULT WINAPI My_IDDSV2_GetAttachedSurface(
  LPDIRECTDRAWSURFACE2 lpThis,
  LPDDSCAPS lpDDSCaps, 
  LPDIRECTDRAWSURFACE2 FAR *lplpDDAttachedSurface  
)
{
	//OutputDebugString("My_IDDSV2_GetAttachedSurface called.\r\n");
	return ORIG_IDirectDrawSurfaceV2_GetAttachedSurface(lpThis, lpDDSCaps, lplpDDAttachedSurface);
}
HRESULT WINAPI My_IDDSV2_GetBltStatus(
  LPDIRECTDRAWSURFACE2 lpThis,
  DWORD dwFlags  
)
{
	//OutputDebugString("My_IDDSV2_GetBltStatus called.\r\n");
	return ORIG_IDirectDrawSurfaceV2_GetBltStatus (lpThis, dwFlags);
}
HRESULT WINAPI My_IDDSV2_GetCaps(
  LPDIRECTDRAWSURFACE2 lpThis,
  LPDDSCAPS lpDDSCaps  
)
{
	//OutputDebugString("My_IDDSV2_GetCaps called.\r\n");
	return ORIG_IDirectDrawSurfaceV2_GetCaps(lpThis, lpDDSCaps);
}
HRESULT WINAPI My_IDDSV2_GetClipper(
  LPDIRECTDRAWSURFACE2 lpThis,
  LPDIRECTDRAWCLIPPER FAR *lplpDDClipper  
)
{
	//OutputDebugString("My_IDDSV2_GetClipper called.\r\n");
	return ORIG_IDirectDrawSurfaceV2_GetClipper(lpThis, lplpDDClipper);
}
HRESULT WINAPI My_IDDSV2_GetColorKey(
  LPDIRECTDRAWSURFACE2 lpThis,
  DWORD dwFlags,             
  LPDDCOLORKEY lpDDColorKey  
)
{
	//OutputDebugString("My_IDDSV2_GetColorKey called.\r\n");
	return ORIG_IDirectDrawSurfaceV2_GetColorKey (lpThis, dwFlags, lpDDColorKey);
}
HRESULT WINAPI My_IDDSV2_GetDC(
  LPDIRECTDRAWSURFACE2 lpThis,
  HDC FAR *lphDC  
)
{
	//OutputDebugString("My_IDDSV2_GetDC called.\r\n");
	return ORIG_IDirectDrawSurfaceV2_GetDC (lpThis, lphDC);
}
HRESULT WINAPI My_IDDSV2_GetFlipStatus(
  LPDIRECTDRAWSURFACE2 lpThis,
  DWORD dwFlags  
)
{
	//OutputDebugString("My_IDDSV2_GetFlipStatus called.\r\n");
	return ORIG_IDirectDrawSurfaceV2_GetFlipStatus (lpThis, dwFlags);
}
HRESULT WINAPI My_IDDSV2_GetOverlayPosition(
  LPDIRECTDRAWSURFACE2 lpThis,
  LPLONG lplX, 
  LPLONG lplY  
)
{
	//OutputDebugString("My_IDDSV2_GetOverlayPosition called.\r\n");
	return ORIG_IDirectDrawSurfaceV2_GetOverlayPosition (lpThis, lplX, lplY);
}
HRESULT WINAPI My_IDDSV2_GetPalette(
  LPDIRECTDRAWSURFACE2 lpThis,
  LPDIRECTDRAWPALETTE FAR *lplpDDPalette  
)
{
	//OutputDebugString("My_IDDSV2_GetPalette called.\r\n");
	return ORIG_IDirectDrawSurfaceV2_GetPalette (lpThis, lplpDDPalette);
}

HRESULT WINAPI My_IDDSV2_GetPixelFormat(
  LPDIRECTDRAWSURFACE2 lpThis,
  LPDDPIXELFORMAT lpDDPixelFormat  
)
{
	//OutputDebugString("My_IDDSV2_GetPixelFormat called.\r\n");
	return ORIG_IDirectDrawSurfaceV2_GetPixelFormat(lpThis, lpDDPixelFormat);
}
HRESULT WINAPI My_IDDSV2_GetSurfaceDesc(
  LPDIRECTDRAWSURFACE2 lpThis,
  LPDDSURFACEDESC lpDDSurfaceDesc  
)
{
	//OutputDebugString("My_IDDSV2_GetSurfaceDesc called.\r\n");
	return ORIG_IDirectDrawSurfaceV2_GetSurfaceDesc(lpThis, lpDDSurfaceDesc);
}
HRESULT WINAPI My_IDDSV2_Initialize(
  LPDIRECTDRAWSURFACE2 lpThis,
  LPDIRECTDRAW lpDD,               
  LPDDSURFACEDESC lpDDSurfaceDesc  
)
{
	//OutputDebugString("My_IDDSV2_Initialize called.\r\n");
	return ORIG_IDirectDrawSurfaceV2_Initialize (lpThis, lpDD, lpDDSurfaceDesc);
}
HRESULT WINAPI My_IDDSV2_IsLost(
  LPDIRECTDRAWSURFACE2 lpThis
)
{
	//OutputDebugString("My_IDDSV2_IsLost called.\r\n");
	return ORIG_IDirectDrawSurfaceV2_IsLost (lpThis);
}
HRESULT WINAPI My_IDDSV2_Lock(
  LPDIRECTDRAWSURFACE2 lpThis,
  LPRECT lpDestRect,                
  LPDDSURFACEDESC lpDDSurfaceDesc,  
  DWORD dwFlags,                    
  HANDLE hEvent                     
)
{
	//OutputDebugString("My_IDDSV2_Lock called.\r\n");
	return ORIG_IDirectDrawSurfaceV2_Lock (lpThis, lpDestRect, lpDDSurfaceDesc, dwFlags, hEvent);
}
HRESULT WINAPI My_IDDSV2_ReleaseDC(
  LPDIRECTDRAWSURFACE2 lpThis,
  HDC hDC  
)
{
	//OutputDebugString("My_IDDSV2_ReleaseDC called.\r\n");
	return ORIG_IDirectDrawSurfaceV2_ReleaseDC (lpThis, hDC);
}
HRESULT WINAPI My_IDDSV2_Restore(
  LPDIRECTDRAWSURFACE2 lpThis
)
{
	//OutputDebugString("My_IDDSV2_Restore called.\r\n");
	return ORIG_IDirectDrawSurfaceV2_Restore (lpThis);
}
HRESULT WINAPI My_IDDSV2_SetClipper(
  LPDIRECTDRAWSURFACE2 lpThis,
  LPDIRECTDRAWCLIPPER lpDDClipper  
)
{
	//OutputDebugString("My_IDDSV2_SetClipper called.\r\n");
	return ORIG_IDirectDrawSurfaceV2_SetClipper (lpThis, lpDDClipper);
}
HRESULT WINAPI My_IDDSV2_SetColorKey(
  LPDIRECTDRAWSURFACE2 lpThis,
  DWORD dwFlags,             
  LPDDCOLORKEY lpDDColorKey  
)
{
	//OutputDebugString("My_IDDSV2_SetColorKey called.\r\n");
	return ORIG_IDirectDrawSurfaceV2_SetColorKey (lpThis, dwFlags, lpDDColorKey);
}
HRESULT WINAPI My_IDDSV2_SetOverlayPosition(
  LPDIRECTDRAWSURFACE2 lpThis,
  LONG lX, 
  LONG lY  
)
{
	//OutputDebugString("My_IDDSV2_SetOverlayPosition called.\r\n");
	return ORIG_IDirectDrawSurfaceV2_SetOverlayPosition (lpThis, lX, lY);
}
HRESULT WINAPI My_IDDSV2_SetPalette(
  LPDIRECTDRAWSURFACE2 lpThis,
  LPDIRECTDRAWPALETTE lpDDPalette  
)
{
	//OutputDebugString("My_IDDSV2_SetPalette called.\r\n");
	return ORIG_IDirectDrawSurfaceV2_SetPalette (lpThis, lpDDPalette);
}
HRESULT WINAPI My_IDDSV2_Unlock(
  LPDIRECTDRAWSURFACE2 lpThis,
  LPVOID lpRect 
)
{
	//OutputDebugString("My_IDDSV2_Unlock called.\r\n");
	return ORIG_IDirectDrawSurfaceV2_Unlock (lpThis, lpRect);
}
HRESULT WINAPI My_IDDSV2_UpdateOverlay(
  LPDIRECTDRAWSURFACE2 lpThis,
  LPRECT lpSrcRect,                      
  LPDIRECTDRAWSURFACE2 lpDDDestSurface,  
  LPRECT lpDestRect,                     
  DWORD dwFlags,                         
  LPDDOVERLAYFX lpDDOverlayFx            
)
{
	//OutputDebugString("My_IDDSV2_UpdateOverlay called.\r\n");
	return ORIG_IDirectDrawSurfaceV2_UpdateOverlay (lpThis, lpSrcRect, lpDDDestSurface, lpDestRect,dwFlags,lpDDOverlayFx);
}
HRESULT WINAPI My_IDDSV2_UpdateOverlayDisplay(
  LPDIRECTDRAWSURFACE2 lpThis,
  DWORD dwFlags  
)
{
	//OutputDebugString("My_IDDSV2_UpdateOverlayDisplay called.\r\n");
	return ORIG_IDirectDrawSurfaceV2_UpdateOverlayDisplay (lpThis, dwFlags);
}
HRESULT WINAPI My_IDDSV2_UpdateOverlayZOrder(
  LPDIRECTDRAWSURFACE2 lpThis,
  DWORD dwFlags,                       
  LPDIRECTDRAWSURFACE2 lpDDSReference  
)
{
	//OutputDebugString("My_IDDSV2_UpdateOverlayZOrder called.\r\n");
	return ORIG_IDirectDrawSurfaceV2_UpdateOverlayZOrder (lpThis, dwFlags, lpDDSReference);
}
// IDirectDrawSurface2 functions

HRESULT WINAPI My_IDDSV2_GetDDInterface(
  LPDIRECTDRAWSURFACE2 lpThis,
  LPVOID FAR *lplpDD  
)
{
	//OutputDebugString("My_IDDSV2_GetDDInterface called.\r\n");
	return ORIG_IDirectDrawSurfaceV2_GetDDInterface(lpThis, lplpDD);
}
HRESULT WINAPI My_IDDSV2_PageLock(
  LPDIRECTDRAWSURFACE2 lpThis,
  DWORD dwFlags  
)
{
	//OutputDebugString("My_IDDSV2_PageLock called.\r\n");
	return ORIG_IDirectDrawSurfaceV2_PageLock(lpThis, dwFlags);
}
HRESULT WINAPI My_IDDSV2_PageUnlock(
  LPDIRECTDRAWSURFACE2 lpThis,
  DWORD dwFlags  
)
{
	//OutputDebugString("My_IDDSV2_PageUnlock called.\r\n");
	return ORIG_IDirectDrawSurfaceV2_PageUnlock(lpThis, dwFlags);
}


void HookDirectDrawSurface2Interface  (LPDIRECTDRAWSURFACE2 lpInterface) 
{
	//OutputDebugString("HookInterface for IDirectDrawSurface2 reached.\r\n");
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
	ORIG_IDirectDrawSurfaceV2_QueryInterface =(TD_IDirectDrawSurfaceV2_QueryInterface)pVtable[0];
	NEW_IDirectDrawSurfaceV2_QueryInterface =My_IDDSV2_QueryInterface;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV2_QueryInterface, NEW_IDirectDrawSurfaceV2_QueryInterface);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface2::QueryInterface) failed.\r\n");
	}

	ORIG_IDirectDrawSurfaceV2_AddRef=(TD_IDirectDrawSurfaceV2_AddRef)pVtable[1];
	NEW_IDirectDrawSurfaceV2_AddRef=My_IDDSV2_AddRef;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV2_AddRef, NEW_IDirectDrawSurfaceV2_AddRef);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface2::AddRef) failed.\r\n");
	}

	ORIG_IDirectDrawSurfaceV2_Release=(TD_IDirectDrawSurfaceV2_Release)pVtable[2];
	NEW_IDirectDrawSurfaceV2_Release=My_IDDSV2_Release;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV2_Release, NEW_IDirectDrawSurfaceV2_Release);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface2::Release) failed.\r\n");
	}

	ORIG_IDirectDrawSurfaceV2_AddAttachedSurface=(TD_IDirectDrawSurfaceV2_AddAttachedSurface)pVtable[3];
	NEW_IDirectDrawSurfaceV2_AddAttachedSurface=My_IDDSV2_AddAttachedSurface;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV2_AddAttachedSurface, NEW_IDirectDrawSurfaceV2_AddAttachedSurface);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface2::AddAttachedSurface) failed.\r\n");
	}

	ORIG_IDirectDrawSurfaceV2_AddOverlayDirtyRect =(TD_IDirectDrawSurfaceV2_AddOverlayDirtyRect)pVtable[4];
	NEW_IDirectDrawSurfaceV2_AddOverlayDirtyRect =My_IDDSV2_AddOverlayDirtyRect ;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV2_AddOverlayDirtyRect , NEW_IDirectDrawSurfaceV2_AddOverlayDirtyRect);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface2::AddOverlayDirtyRect) failed.\r\n");
	}

	ORIG_IDirectDrawSurfaceV2_Blt =(TD_IDirectDrawSurfaceV2_Blt)pVtable[5];
	NEW_IDirectDrawSurfaceV2_Blt =My_IDDSV2_Blt ;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV2_Blt, NEW_IDirectDrawSurfaceV2_Blt);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface2::Blt) failed.\r\n");
	}

	ORIG_IDirectDrawSurfaceV2_BltBatch =(TD_IDirectDrawSurfaceV2_BltBatch)pVtable[6];
	NEW_IDirectDrawSurfaceV2_BltBatch =My_IDDSV2_BltBatch ;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV2_BltBatch, NEW_IDirectDrawSurfaceV2_BltBatch);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface2::BltBatch) failed.\r\n");
	}

	ORIG_IDirectDrawSurfaceV2_BltFast =(TD_IDirectDrawSurfaceV2_BltFast)pVtable[7];
	NEW_IDirectDrawSurfaceV2_BltFast =My_IDDSV2_BltFast ;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV2_BltFast, NEW_IDirectDrawSurfaceV2_BltFast);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface2::BltFast) failed.\r\n");
	}
	
	ORIG_IDirectDrawSurfaceV2_DeleteAttachedSurface=(TD_IDirectDrawSurfaceV2_DeleteAttachedSurface)pVtable[8];
	NEW_IDirectDrawSurfaceV2_DeleteAttachedSurface =My_IDDSV2_DeleteAttachedSurface;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV2_DeleteAttachedSurface, NEW_IDirectDrawSurfaceV2_DeleteAttachedSurface);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface2::DeleteAttachedSurface) failed.\r\n");
	}

	ORIG_IDirectDrawSurfaceV2_EnumAttachedSurfaces=(TD_IDirectDrawSurfaceV2_EnumAttachedSurfaces)pVtable[9];
	NEW_IDirectDrawSurfaceV2_EnumAttachedSurfaces=My_IDDSV2_EnumAttachedSurfaces;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV2_EnumAttachedSurfaces, NEW_IDirectDrawSurfaceV2_EnumAttachedSurfaces);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface2::EnumAttachedSurfaces) failed.\r\n");
	}

	ORIG_IDirectDrawSurfaceV2_EnumOverlayZOrders=(TD_IDirectDrawSurfaceV2_EnumOverlayZOrders)pVtable[10];
	NEW_IDirectDrawSurfaceV2_EnumOverlayZOrders=My_IDDSV2_EnumOverlayZOrders;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV2_EnumOverlayZOrders, NEW_IDirectDrawSurfaceV2_EnumOverlayZOrders);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface2::EnumOverlayZOrders) failed.\r\n");
	}

	ORIG_IDirectDrawSurfaceV2_Flip=(TD_IDirectDrawSurfaceV2_Flip)pVtable[11];
	NEW_IDirectDrawSurfaceV2_Flip=My_IDDSV2_Flip;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV2_Flip, NEW_IDirectDrawSurfaceV2_Flip);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface2::Flip) failed.\r\n");
	}

	ORIG_IDirectDrawSurfaceV2_GetAttachedSurface=(TD_IDirectDrawSurfaceV2_GetAttachedSurface)pVtable[12];
	NEW_IDirectDrawSurfaceV2_GetAttachedSurface=My_IDDSV2_GetAttachedSurface;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV2_GetAttachedSurface, NEW_IDirectDrawSurfaceV2_GetAttachedSurface);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface2::GetAttachedSurface) failed.\r\n");
	}
	
	ORIG_IDirectDrawSurfaceV2_GetBltStatus=(TD_IDirectDrawSurfaceV2_GetBltStatus)pVtable[13];
	NEW_IDirectDrawSurfaceV2_GetBltStatus=My_IDDSV2_GetBltStatus;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV2_GetBltStatus, NEW_IDirectDrawSurfaceV2_GetBltStatus);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface2::GetBltStatus) failed.\r\n");
	}

	ORIG_IDirectDrawSurfaceV2_GetCaps=(TD_IDirectDrawSurfaceV2_GetCaps)pVtable[14];
	NEW_IDirectDrawSurfaceV2_GetCaps=My_IDDSV2_GetCaps;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV2_GetCaps, NEW_IDirectDrawSurfaceV2_GetCaps);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface2::GetCaps) failed.\r\n");
	}

	ORIG_IDirectDrawSurfaceV2_GetClipper=(TD_IDirectDrawSurfaceV2_GetClipper)pVtable[15];
	NEW_IDirectDrawSurfaceV2_GetClipper=My_IDDSV2_GetClipper;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV2_GetClipper, NEW_IDirectDrawSurfaceV2_GetClipper);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface2::GetClipper) failed.\r\n");
	}
	
	ORIG_IDirectDrawSurfaceV2_GetColorKey=(TD_IDirectDrawSurfaceV2_GetColorKey)pVtable[16];
	NEW_IDirectDrawSurfaceV2_GetColorKey=My_IDDSV2_GetColorKey;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV2_GetColorKey, NEW_IDirectDrawSurfaceV2_GetColorKey);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface2::GetColorKey) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV2_GetDC=(TD_IDirectDrawSurfaceV2_GetDC)pVtable[17];
	NEW_IDirectDrawSurfaceV2_GetDC=My_IDDSV2_GetDC;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV2_GetDC, NEW_IDirectDrawSurfaceV2_GetDC);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface2::GetDC) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV2_GetFlipStatus=(TD_IDirectDrawSurfaceV2_GetFlipStatus)pVtable[18];
	NEW_IDirectDrawSurfaceV2_GetFlipStatus=My_IDDSV2_GetFlipStatus;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV2_GetFlipStatus, NEW_IDirectDrawSurfaceV2_GetFlipStatus);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface2::GetFlipStatus) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV2_GetOverlayPosition=(TD_IDirectDrawSurfaceV2_GetOverlayPosition)pVtable[19];
	NEW_IDirectDrawSurfaceV2_GetOverlayPosition=My_IDDSV2_GetOverlayPosition;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV2_GetOverlayPosition, NEW_IDirectDrawSurfaceV2_GetOverlayPosition);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface2::GetOverlayPosition) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV2_GetPalette=(TD_IDirectDrawSurfaceV2_GetPalette)pVtable[20];
	NEW_IDirectDrawSurfaceV2_GetPalette=My_IDDSV2_GetPalette;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV2_GetPalette, NEW_IDirectDrawSurfaceV2_GetPalette);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface2::GetPalette) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV2_GetPixelFormat=(TD_IDirectDrawSurfaceV2_GetPixelFormat)pVtable[21];
	NEW_IDirectDrawSurfaceV2_GetPixelFormat=My_IDDSV2_GetPixelFormat;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV2_GetPixelFormat, NEW_IDirectDrawSurfaceV2_GetPixelFormat);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface2::GetPixelFormat) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV2_GetSurfaceDesc=(TD_IDirectDrawSurfaceV2_GetSurfaceDesc)pVtable[22];
	NEW_IDirectDrawSurfaceV2_GetSurfaceDesc=My_IDDSV2_GetSurfaceDesc;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV2_GetSurfaceDesc, NEW_IDirectDrawSurfaceV2_GetSurfaceDesc);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface2::GetSurfaceDesc) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV2_Initialize=(TD_IDirectDrawSurfaceV2_Initialize)pVtable[23];
	NEW_IDirectDrawSurfaceV2_Initialize=My_IDDSV2_Initialize;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV2_Initialize, NEW_IDirectDrawSurfaceV2_Initialize);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface2::Initialize) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV2_IsLost=(TD_IDirectDrawSurfaceV2_IsLost)pVtable[24];
	NEW_IDirectDrawSurfaceV2_IsLost=My_IDDSV2_IsLost;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV2_IsLost, NEW_IDirectDrawSurfaceV2_IsLost);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface2::IsLost) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV2_Lock=(TD_IDirectDrawSurfaceV2_Lock)pVtable[25];
	NEW_IDirectDrawSurfaceV2_Lock=My_IDDSV2_Lock;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV2_Lock, NEW_IDirectDrawSurfaceV2_Lock);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface2::Lock) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV2_ReleaseDC=(TD_IDirectDrawSurfaceV2_ReleaseDC)pVtable[26];
	NEW_IDirectDrawSurfaceV2_ReleaseDC=My_IDDSV2_ReleaseDC;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV2_ReleaseDC, NEW_IDirectDrawSurfaceV2_ReleaseDC);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface2::ReleaseDC) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV2_Restore=(TD_IDirectDrawSurfaceV2_Restore)pVtable[27];
	NEW_IDirectDrawSurfaceV2_Restore=My_IDDSV2_Restore;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV2_Restore, NEW_IDirectDrawSurfaceV2_Restore);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface2::Restore) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV2_SetClipper=(TD_IDirectDrawSurfaceV2_SetClipper)pVtable[28];
	NEW_IDirectDrawSurfaceV2_SetClipper=My_IDDSV2_SetClipper;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV2_SetClipper, NEW_IDirectDrawSurfaceV2_SetClipper);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface2::SetClipper) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV2_SetColorKey=(TD_IDirectDrawSurfaceV2_SetColorKey)pVtable[29];
	NEW_IDirectDrawSurfaceV2_SetColorKey=My_IDDSV2_SetColorKey;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV2_SetColorKey, NEW_IDirectDrawSurfaceV2_SetColorKey);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface2::SetColorKey) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV2_SetOverlayPosition=(TD_IDirectDrawSurfaceV2_SetOverlayPosition)pVtable[30];
	NEW_IDirectDrawSurfaceV2_SetOverlayPosition=My_IDDSV2_SetOverlayPosition;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV2_SetOverlayPosition, NEW_IDirectDrawSurfaceV2_SetOverlayPosition);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface2::SetOverlayPosition) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV2_SetPalette=(TD_IDirectDrawSurfaceV2_SetPalette)pVtable[31];
	NEW_IDirectDrawSurfaceV2_SetPalette=My_IDDSV2_SetPalette;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV2_SetPalette, NEW_IDirectDrawSurfaceV2_SetPalette);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface2::SetPalette) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV2_Unlock=(TD_IDirectDrawSurfaceV2_Unlock)pVtable[32];
	NEW_IDirectDrawSurfaceV2_Unlock=My_IDDSV2_Unlock;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV2_Unlock, NEW_IDirectDrawSurfaceV2_Unlock);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface2::Unlock) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV2_UpdateOverlay=(TD_IDirectDrawSurfaceV2_UpdateOverlay)pVtable[33];
	NEW_IDirectDrawSurfaceV2_UpdateOverlay=My_IDDSV2_UpdateOverlay;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV2_UpdateOverlay, NEW_IDirectDrawSurfaceV2_UpdateOverlay);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface2::UpdateOverlay) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV2_UpdateOverlayDisplay=(TD_IDirectDrawSurfaceV2_UpdateOverlayDisplay)pVtable[34];
	NEW_IDirectDrawSurfaceV2_UpdateOverlayDisplay=My_IDDSV2_UpdateOverlayDisplay;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV2_UpdateOverlayDisplay, NEW_IDirectDrawSurfaceV2_UpdateOverlayDisplay);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface2::UpdateOverlayDisplay) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV2_UpdateOverlayZOrder=(TD_IDirectDrawSurfaceV2_UpdateOverlayZOrder)pVtable[35];
	NEW_IDirectDrawSurfaceV2_UpdateOverlayZOrder=My_IDDSV2_UpdateOverlayZOrder;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV2_UpdateOverlayZOrder, NEW_IDirectDrawSurfaceV2_UpdateOverlayZOrder);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface2::UpdateOverlayZOrder) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV2_GetDDInterface=(TD_IDirectDrawSurfaceV2_GetDDInterface)pVtable[36];
	NEW_IDirectDrawSurfaceV2_GetDDInterface=My_IDDSV2_GetDDInterface;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV2_GetDDInterface, NEW_IDirectDrawSurfaceV2_GetDDInterface);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface2::GetDDInterface) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV2_PageLock=(TD_IDirectDrawSurfaceV2_PageLock)pVtable[37];
	NEW_IDirectDrawSurfaceV2_PageLock=My_IDDSV2_PageLock;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV2_PageLock, NEW_IDirectDrawSurfaceV2_PageLock);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface2::PageLock) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV2_PageUnlock=(TD_IDirectDrawSurfaceV2_PageUnlock)pVtable[38];
	NEW_IDirectDrawSurfaceV2_PageUnlock=My_IDDSV2_PageUnlock;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV2_PageUnlock, NEW_IDirectDrawSurfaceV2_PageUnlock);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface2::PageUnlock) failed.\r\n");
	}


	result = DetourTransactionCommit();
	if (result != NO_ERROR) {
		//OutputDebugString("DetourTransactionCommit failed.\r\n");
	}
	//OutputDebugString("HookInterface for IDirectDrawSurface2 exited.\r\n");
	
}
