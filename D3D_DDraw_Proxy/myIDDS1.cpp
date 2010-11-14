#include "stdafx.h"
/*
DETOURS HOOK CODE FOR THE IDIRECTDRAWSURFACE COM INTERFACE
*/

//function pointers to original methods and new methods

// IUnknown functions
HRESULT WINAPI My_IDDSV1_QueryInterface (LPUNKNOWN lpThis, REFIID riid, LPVOID* obp) 
{
	//OutputDebugString("My_IDDSV1_QueryInterface called.\r\n");
	return ORIG_IDirectDrawSurfaceV1_QueryInterface (lpThis, riid, obp);
}
ULONG   WINAPI My_IDDSV1_AddRef(LPUNKNOWN lpThis) 
{
	//OutputDebugString("My_IDDSV1_AddRef called.\r\n");
	return ORIG_IDirectDrawSurfaceV1_AddRef(lpThis);
}
ULONG   WINAPI My_IDDSV1_Release(LPUNKNOWN lpThis)
{
	//OutputDebugString("My_IDDSV1_Release called.\r\n");
	return ORIG_IDirectDrawSurfaceV1_Release(lpThis);
}

// IDirectDrawSurface (V1) functions
HRESULT WINAPI My_IDDSV1_AddAttachedSurface(
  LPDIRECTDRAWSURFACE lpThis,
  LPDIRECTDRAWSURFACE lpDDSAttachedSurface  
)
{
	//OutputDebugString("My_IDDSV1_AddAttachedSurface called.\r\n");
	return ORIG_IDirectDrawSurfaceV1_AddAttachedSurface(lpThis, lpDDSAttachedSurface);
}
HRESULT WINAPI My_IDDSV1_AddOverlayDirtyRect(
  LPDIRECTDRAWSURFACE lpThis,
  LPRECT lpRect  
)
{
	//OutputDebugString("My_IDDSV1_AddOverlayDirtyRect called.\r\n");
	return ORIG_IDirectDrawSurfaceV1_AddOverlayDirtyRect(lpThis, lpRect);
}
HRESULT WINAPI My_IDDSV1_Blt(
  LPDIRECTDRAWSURFACE lpThis,
  LPRECT lpDestRect,                    
  LPDIRECTDRAWSURFACE lpDDSrcSurface,  
  LPRECT lpSrcRect,                     
  DWORD dwFlags,                        
  LPDDBLTFX lpDDBltFx                   
)
{
	//OutputDebugString("My_IDDSV1_Blt called.\r\n");
	return ORIG_IDirectDrawSurfaceV1_Blt (lpThis, lpDestRect, lpDDSrcSurface, lpSrcRect, dwFlags, lpDDBltFx);
}
HRESULT WINAPI My_IDDSV1_BltBatch(
  LPDIRECTDRAWSURFACE lpThis,
  LPDDBLTBATCH lpDDBltBatch,  
  DWORD dwCount,              
  DWORD dwFlags               
)
{
	//OutputDebugString("My_IDDSV1_BltBatch called.\r\n");
	return ORIG_IDirectDrawSurfaceV1_BltBatch (lpThis, lpDDBltBatch, dwCount, dwFlags);
}
HRESULT WINAPI My_IDDSV1_BltFast(
  LPDIRECTDRAWSURFACE lpThis,
  DWORD dwX,                            
  DWORD dwY,                            
  LPDIRECTDRAWSURFACE lpDDSrcSurface,  
  LPRECT lpSrcRect,                     
  DWORD dwTrans                         
)
{
	//OutputDebugString("My_IDDSV1_BltFast called.\r\n");
	return ORIG_IDirectDrawSurfaceV1_BltFast (lpThis, dwX, dwY, lpDDSrcSurface, lpSrcRect, dwTrans);
}
HRESULT WINAPI My_IDDSV1_DeleteAttachedSurface(
  LPDIRECTDRAWSURFACE lpThis,
  DWORD dwFlags,                             
  LPDIRECTDRAWSURFACE lpDDSAttachedSurface  
)
{
	//OutputDebugString("My_IDDSV1_DeleteAttachedSurface called.\r\n");
	return ORIG_IDirectDrawSurfaceV1_DeleteAttachedSurface (lpThis, dwFlags, lpDDSAttachedSurface);
}
HRESULT WINAPI My_IDDSV1_EnumAttachedSurfaces(
  LPDIRECTDRAWSURFACE lpThis,
  LPVOID lpContext,                                
  LPDDENUMSURFACESCALLBACK lpEnumSurfacesCallback  
)
{
	//OutputDebugString("My_IDDSV1_EnumAttachedSurfaces called.\r\n");
	return ORIG_IDirectDrawSurfaceV1_EnumAttachedSurfaces (lpThis, lpContext, lpEnumSurfacesCallback);
}
HRESULT WINAPI My_IDDSV1_EnumOverlayZOrders(
  LPDIRECTDRAWSURFACE lpThis,
  DWORD dwFlags,                          
  LPVOID lpContext,                       
  LPDDENUMSURFACESCALLBACK lpfnCallback  
)
{
	//OutputDebugString("My_IDDSV1_EnumOverlayZOrders called.\r\n");
	return ORIG_IDirectDrawSurfaceV1_EnumOverlayZOrders (lpThis, dwFlags, lpContext, lpfnCallback);
}
HRESULT WINAPI My_IDDSV1_Flip(
  LPDIRECTDRAWSURFACE lpThis,
  LPDIRECTDRAWSURFACE lpDDSurfaceTargetOverride,  
  DWORD dwFlags                                    
)
{
	//OutputDebugString("My_IDDSV1_Flip called.\r\n");
	return ORIG_IDirectDrawSurfaceV1_Flip (lpThis, lpDDSurfaceTargetOverride, dwFlags);
}
HRESULT WINAPI My_IDDSV1_GetAttachedSurface(
  LPDIRECTDRAWSURFACE lpThis,
  LPDDSCAPS lpDDSCaps, 
  LPDIRECTDRAWSURFACE FAR *lplpDDAttachedSurface  
)
{
	//OutputDebugString("My_IDDSV1_GetAttachedSurface called.\r\n");
	return ORIG_IDirectDrawSurfaceV1_GetAttachedSurface(lpThis, lpDDSCaps, lplpDDAttachedSurface);
}
HRESULT WINAPI My_IDDSV1_GetBltStatus(
  LPDIRECTDRAWSURFACE lpThis,
  DWORD dwFlags  
)
{
	//OutputDebugString("My_IDDSV1_GetBltStatus called.\r\n");
	return ORIG_IDirectDrawSurfaceV1_GetBltStatus (lpThis, dwFlags);
}
HRESULT WINAPI My_IDDSV1_GetCaps(
  LPDIRECTDRAWSURFACE lpThis,
  LPDDSCAPS lpDDSCaps  
)
{
	//OutputDebugString("My_IDDSV1_GetCaps called.\r\n");
	return ORIG_IDirectDrawSurfaceV1_GetCaps(lpThis, lpDDSCaps);
}
HRESULT WINAPI My_IDDSV1_GetClipper(
  LPDIRECTDRAWSURFACE lpThis,
  LPDIRECTDRAWCLIPPER FAR *lplpDDClipper  
)
{
	//OutputDebugString("My_IDDSV1_GetClipper called.\r\n");
	return ORIG_IDirectDrawSurfaceV1_GetClipper(lpThis, lplpDDClipper);
}
HRESULT WINAPI My_IDDSV1_GetColorKey(
  LPDIRECTDRAWSURFACE lpThis,
  DWORD dwFlags,             
  LPDDCOLORKEY lpDDColorKey  
)
{
	//OutputDebugString("My_IDDSV1_GetColorKey called.\r\n");
	return ORIG_IDirectDrawSurfaceV1_GetColorKey (lpThis, dwFlags, lpDDColorKey);
}
HRESULT WINAPI My_IDDSV1_GetDC(
  LPDIRECTDRAWSURFACE lpThis,
  HDC FAR *lphDC  
)
{
	//OutputDebugString("My_IDDSV1_GetDC called.\r\n");
	return ORIG_IDirectDrawSurfaceV1_GetDC (lpThis, lphDC);
}
HRESULT WINAPI My_IDDSV1_GetFlipStatus(
  LPDIRECTDRAWSURFACE lpThis,
  DWORD dwFlags  
)
{
	//OutputDebugString("My_IDDSV1_GetFlipStatus called.\r\n");
	return ORIG_IDirectDrawSurfaceV1_GetFlipStatus (lpThis, dwFlags);
}
HRESULT WINAPI My_IDDSV1_GetOverlayPosition(
  LPDIRECTDRAWSURFACE lpThis,
  LPLONG lplX, 
  LPLONG lplY  
)
{
	//OutputDebugString("My_IDDSV1_GetOverlayPosition called.\r\n");
	return ORIG_IDirectDrawSurfaceV1_GetOverlayPosition (lpThis, lplX, lplY);
}
HRESULT WINAPI My_IDDSV1_GetPalette(
  LPDIRECTDRAWSURFACE lpThis,
  LPDIRECTDRAWPALETTE FAR *lplpDDPalette  
)
{
	//OutputDebugString("My_IDDSV1_GetPalette called.\r\n");
	return ORIG_IDirectDrawSurfaceV1_GetPalette (lpThis, lplpDDPalette);
}

HRESULT WINAPI My_IDDSV1_GetPixelFormat(
  LPDIRECTDRAWSURFACE lpThis,
  LPDDPIXELFORMAT lpDDPixelFormat  
)
{
	//OutputDebugString("My_IDDSV1_GetPixelFormat called.\r\n");
	return ORIG_IDirectDrawSurfaceV1_GetPixelFormat(lpThis, lpDDPixelFormat);
}
HRESULT WINAPI My_IDDSV1_GetSurfaceDesc(
  LPDIRECTDRAWSURFACE lpThis,
  LPDDSURFACEDESC lpDDSurfaceDesc  
)
{
	//OutputDebugString("My_IDDSV1_GetSurfaceDesc called.\r\n");
	return ORIG_IDirectDrawSurfaceV1_GetSurfaceDesc(lpThis, lpDDSurfaceDesc);
}
HRESULT WINAPI My_IDDSV1_Initialize(
  LPDIRECTDRAWSURFACE lpThis,
  LPDIRECTDRAW lpDD,               
  LPDDSURFACEDESC lpDDSurfaceDesc  
)
{
	//OutputDebugString("My_IDDSV1_Initialize called.\r\n");
	return ORIG_IDirectDrawSurfaceV1_Initialize (lpThis, lpDD, lpDDSurfaceDesc);
}
HRESULT WINAPI My_IDDSV1_IsLost(
  LPDIRECTDRAWSURFACE lpThis
)
{
	//OutputDebugString("My_IDDSV1_IsLost called.\r\n");
	return ORIG_IDirectDrawSurfaceV1_IsLost (lpThis);
}
HRESULT WINAPI My_IDDSV1_Lock(
  LPDIRECTDRAWSURFACE lpThis,
  LPRECT lpDestRect,                
  LPDDSURFACEDESC lpDDSurfaceDesc,  
  DWORD dwFlags,                    
  HANDLE hEvent                     
)
{
	//OutputDebugString("My_IDDSV1_Lock called.\r\n");
	return ORIG_IDirectDrawSurfaceV1_Lock (lpThis, lpDestRect, lpDDSurfaceDesc, dwFlags, hEvent);
}
HRESULT WINAPI My_IDDSV1_ReleaseDC(
  LPDIRECTDRAWSURFACE lpThis,
  HDC hDC  
)
{
	//OutputDebugString("My_IDDSV1_ReleaseDC called.\r\n");
	return ORIG_IDirectDrawSurfaceV1_ReleaseDC (lpThis, hDC);
}
HRESULT WINAPI My_IDDSV1_Restore(
  LPDIRECTDRAWSURFACE lpThis
)
{
	//OutputDebugString("My_IDDSV1_Restore called.\r\n");
	return ORIG_IDirectDrawSurfaceV1_Restore (lpThis);
}
HRESULT WINAPI My_IDDSV1_SetClipper(
  LPDIRECTDRAWSURFACE lpThis,
  LPDIRECTDRAWCLIPPER lpDDClipper  
)
{
	//OutputDebugString("My_IDDSV1_SetClipper called.\r\n");
	return ORIG_IDirectDrawSurfaceV1_SetClipper (lpThis, lpDDClipper);
}
HRESULT WINAPI My_IDDSV1_SetColorKey(
  LPDIRECTDRAWSURFACE lpThis,
  DWORD dwFlags,             
  LPDDCOLORKEY lpDDColorKey  
)
{
	//OutputDebugString("My_IDDSV1_SetColorKey called.\r\n");
	return ORIG_IDirectDrawSurfaceV1_SetColorKey (lpThis, dwFlags, lpDDColorKey);
}
HRESULT WINAPI My_IDDSV1_SetOverlayPosition(
  LPDIRECTDRAWSURFACE lpThis,
  LONG lX, 
  LONG lY  
)
{
	//OutputDebugString("My_IDDSV1_SetOverlayPosition called.\r\n");
	return ORIG_IDirectDrawSurfaceV1_SetOverlayPosition (lpThis, lX, lY);
}
HRESULT WINAPI My_IDDSV1_SetPalette(
  LPDIRECTDRAWSURFACE lpThis,
  LPDIRECTDRAWPALETTE lpDDPalette  
)
{
	//OutputDebugString("My_IDDSV1_SetPalette called.\r\n");
	return ORIG_IDirectDrawSurfaceV1_SetPalette (lpThis, lpDDPalette);
}
HRESULT WINAPI My_IDDSV1_Unlock(
  LPDIRECTDRAWSURFACE lpThis,
  LPVOID lpRect 
)
{
	//OutputDebugString("My_IDDSV1_Unlock called.\r\n");
	return ORIG_IDirectDrawSurfaceV1_Unlock (lpThis, lpRect);
}
HRESULT WINAPI My_IDDSV1_UpdateOverlay(
  LPDIRECTDRAWSURFACE lpThis,
  LPRECT lpSrcRect,                      
  LPDIRECTDRAWSURFACE lpDDDestSurface,  
  LPRECT lpDestRect,                     
  DWORD dwFlags,                         
  LPDDOVERLAYFX lpDDOverlayFx            
)
{
	//OutputDebugString("My_IDDSV1_UpdateOverlay called.\r\n");
	return ORIG_IDirectDrawSurfaceV1_UpdateOverlay (lpThis, lpSrcRect, lpDDDestSurface, lpDestRect,dwFlags,lpDDOverlayFx);
}
HRESULT WINAPI My_IDDSV1_UpdateOverlayDisplay(
  LPDIRECTDRAWSURFACE lpThis,
  DWORD dwFlags  
)
{
	//OutputDebugString("My_IDDSV1_UpdateOverlayDisplay called.\r\n");
	return ORIG_IDirectDrawSurfaceV1_UpdateOverlayDisplay (lpThis, dwFlags);
}
HRESULT WINAPI My_IDDSV1_UpdateOverlayZOrder(
  LPDIRECTDRAWSURFACE lpThis,
  DWORD dwFlags,                       
  LPDIRECTDRAWSURFACE lpDDSReference  
)
{
	//OutputDebugString("My_IDDSV1_UpdateOverlayZOrder called.\r\n");
	return ORIG_IDirectDrawSurfaceV1_UpdateOverlayZOrder (lpThis, dwFlags, lpDDSReference);
}

void HookDirectDrawSurface1Interface  (LPDIRECTDRAWSURFACE lpInterface) 
{
	
	//OutputDebugString("HookInterface for IDirectDrawSurface V1 reached.\r\n");
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
	ORIG_IDirectDrawSurfaceV1_QueryInterface =(TD_IDirectDrawSurfaceV1_QueryInterface)pVtable[0];
	NEW_IDirectDrawSurfaceV1_QueryInterface =My_IDDSV1_QueryInterface;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV1_QueryInterface, NEW_IDirectDrawSurfaceV1_QueryInterface);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface::QueryInterface) failed.\r\n");
	}

	ORIG_IDirectDrawSurfaceV1_AddRef=(TD_IDirectDrawSurfaceV1_AddRef)pVtable[1];
	NEW_IDirectDrawSurfaceV1_AddRef=My_IDDSV1_AddRef;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV1_AddRef, NEW_IDirectDrawSurfaceV1_AddRef);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface::AddRef) failed.\r\n");
	}

	ORIG_IDirectDrawSurfaceV1_Release=(TD_IDirectDrawSurfaceV1_Release)pVtable[2];
	NEW_IDirectDrawSurfaceV1_Release=My_IDDSV1_Release;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV1_Release, NEW_IDirectDrawSurfaceV1_Release);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface::Release) failed.\r\n");
	}

	ORIG_IDirectDrawSurfaceV1_AddAttachedSurface=(TD_IDirectDrawSurfaceV1_AddAttachedSurface)pVtable[3];
	NEW_IDirectDrawSurfaceV1_AddAttachedSurface=My_IDDSV1_AddAttachedSurface;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV1_AddAttachedSurface, NEW_IDirectDrawSurfaceV1_AddAttachedSurface);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface::AddAttachedSurface) failed.\r\n");
	}

	ORIG_IDirectDrawSurfaceV1_AddOverlayDirtyRect =(TD_IDirectDrawSurfaceV1_AddOverlayDirtyRect)pVtable[4];
	NEW_IDirectDrawSurfaceV1_AddOverlayDirtyRect =My_IDDSV1_AddOverlayDirtyRect ;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV1_AddOverlayDirtyRect , NEW_IDirectDrawSurfaceV1_AddOverlayDirtyRect);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface::AddOverlayDirtyRect) failed.\r\n");
	}

	ORIG_IDirectDrawSurfaceV1_Blt =(TD_IDirectDrawSurfaceV1_Blt)pVtable[5];
	NEW_IDirectDrawSurfaceV1_Blt =My_IDDSV1_Blt ;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV1_Blt, NEW_IDirectDrawSurfaceV1_Blt);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface::Blt) failed.\r\n");
	}

	ORIG_IDirectDrawSurfaceV1_BltBatch =(TD_IDirectDrawSurfaceV1_BltBatch)pVtable[6];
	NEW_IDirectDrawSurfaceV1_BltBatch =My_IDDSV1_BltBatch ;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV1_BltBatch, NEW_IDirectDrawSurfaceV1_BltBatch);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface::BltBatch) failed.\r\n");
	}

	ORIG_IDirectDrawSurfaceV1_BltFast =(TD_IDirectDrawSurfaceV1_BltFast)pVtable[7];
	NEW_IDirectDrawSurfaceV1_BltFast =My_IDDSV1_BltFast ;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV1_BltFast, NEW_IDirectDrawSurfaceV1_BltFast);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface::BltFast) failed.\r\n");
	}
	
	ORIG_IDirectDrawSurfaceV1_DeleteAttachedSurface=(TD_IDirectDrawSurfaceV1_DeleteAttachedSurface)pVtable[8];
	NEW_IDirectDrawSurfaceV1_DeleteAttachedSurface =My_IDDSV1_DeleteAttachedSurface;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV1_DeleteAttachedSurface, NEW_IDirectDrawSurfaceV1_DeleteAttachedSurface);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface::DeleteAttachedSurface) failed.\r\n");
	}

	ORIG_IDirectDrawSurfaceV1_EnumAttachedSurfaces=(TD_IDirectDrawSurfaceV1_EnumAttachedSurfaces)pVtable[9];
	NEW_IDirectDrawSurfaceV1_EnumAttachedSurfaces=My_IDDSV1_EnumAttachedSurfaces;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV1_EnumAttachedSurfaces, NEW_IDirectDrawSurfaceV1_EnumAttachedSurfaces);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface::EnumAttachedSurfaces) failed.\r\n");
	}

	ORIG_IDirectDrawSurfaceV1_EnumOverlayZOrders=(TD_IDirectDrawSurfaceV1_EnumOverlayZOrders)pVtable[10];
	NEW_IDirectDrawSurfaceV1_EnumOverlayZOrders=My_IDDSV1_EnumOverlayZOrders;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV1_EnumOverlayZOrders, NEW_IDirectDrawSurfaceV1_EnumOverlayZOrders);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface::EnumOverlayZOrders) failed.\r\n");
	}

	ORIG_IDirectDrawSurfaceV1_Flip=(TD_IDirectDrawSurfaceV1_Flip)pVtable[11];
	NEW_IDirectDrawSurfaceV1_Flip=My_IDDSV1_Flip;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV1_Flip, NEW_IDirectDrawSurfaceV1_Flip);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface::Flip) failed.\r\n");
	}

	ORIG_IDirectDrawSurfaceV1_GetAttachedSurface=(TD_IDirectDrawSurfaceV1_GetAttachedSurface)pVtable[12];
	NEW_IDirectDrawSurfaceV1_GetAttachedSurface=My_IDDSV1_GetAttachedSurface;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV1_GetAttachedSurface, NEW_IDirectDrawSurfaceV1_GetAttachedSurface);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface::GetAttachedSurface) failed.\r\n");
	}
	
	ORIG_IDirectDrawSurfaceV1_GetBltStatus=(TD_IDirectDrawSurfaceV1_GetBltStatus)pVtable[13];
	NEW_IDirectDrawSurfaceV1_GetBltStatus=My_IDDSV1_GetBltStatus;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV1_GetBltStatus, NEW_IDirectDrawSurfaceV1_GetBltStatus);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface::GetBltStatus) failed.\r\n");
	}

	ORIG_IDirectDrawSurfaceV1_GetCaps=(TD_IDirectDrawSurfaceV1_GetCaps)pVtable[14];
	NEW_IDirectDrawSurfaceV1_GetCaps=My_IDDSV1_GetCaps;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV1_GetCaps, NEW_IDirectDrawSurfaceV1_GetCaps);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface::GetCaps) failed.\r\n");
	}

	ORIG_IDirectDrawSurfaceV1_GetClipper=(TD_IDirectDrawSurfaceV1_GetClipper)pVtable[15];
	NEW_IDirectDrawSurfaceV1_GetClipper=My_IDDSV1_GetClipper;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV1_GetClipper, NEW_IDirectDrawSurfaceV1_GetClipper);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface::GetClipper) failed.\r\n");
	}
	
	ORIG_IDirectDrawSurfaceV1_GetColorKey=(TD_IDirectDrawSurfaceV1_GetColorKey)pVtable[16];
	NEW_IDirectDrawSurfaceV1_GetColorKey=My_IDDSV1_GetColorKey;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV1_GetColorKey, NEW_IDirectDrawSurfaceV1_GetColorKey);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface::GetColorKey) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV1_GetDC=(TD_IDirectDrawSurfaceV1_GetDC)pVtable[17];
	NEW_IDirectDrawSurfaceV1_GetDC=My_IDDSV1_GetDC;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV1_GetDC, NEW_IDirectDrawSurfaceV1_GetDC);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface::GetDC) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV1_GetFlipStatus=(TD_IDirectDrawSurfaceV1_GetFlipStatus)pVtable[18];
	NEW_IDirectDrawSurfaceV1_GetFlipStatus=My_IDDSV1_GetFlipStatus;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV1_GetFlipStatus, NEW_IDirectDrawSurfaceV1_GetFlipStatus);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface::GetFlipStatus) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV1_GetOverlayPosition=(TD_IDirectDrawSurfaceV1_GetOverlayPosition)pVtable[19];
	NEW_IDirectDrawSurfaceV1_GetOverlayPosition=My_IDDSV1_GetOverlayPosition;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV1_GetOverlayPosition, NEW_IDirectDrawSurfaceV1_GetOverlayPosition);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface::GetOverlayPosition) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV1_GetPalette=(TD_IDirectDrawSurfaceV1_GetPalette)pVtable[20];
	NEW_IDirectDrawSurfaceV1_GetPalette=My_IDDSV1_GetPalette;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV1_GetPalette, NEW_IDirectDrawSurfaceV1_GetPalette);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface::GetPalette) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV1_GetPixelFormat=(TD_IDirectDrawSurfaceV1_GetPixelFormat)pVtable[21];
	NEW_IDirectDrawSurfaceV1_GetPixelFormat=My_IDDSV1_GetPixelFormat;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV1_GetPixelFormat, NEW_IDirectDrawSurfaceV1_GetPixelFormat);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface::GetPixelFormat) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV1_GetSurfaceDesc=(TD_IDirectDrawSurfaceV1_GetSurfaceDesc)pVtable[22];
	NEW_IDirectDrawSurfaceV1_GetSurfaceDesc=My_IDDSV1_GetSurfaceDesc;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV1_GetSurfaceDesc, NEW_IDirectDrawSurfaceV1_GetSurfaceDesc);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface::GetSurfaceDesc) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV1_Initialize=(TD_IDirectDrawSurfaceV1_Initialize)pVtable[23];
	NEW_IDirectDrawSurfaceV1_Initialize=My_IDDSV1_Initialize;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV1_Initialize, NEW_IDirectDrawSurfaceV1_Initialize);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface::Initialize) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV1_IsLost=(TD_IDirectDrawSurfaceV1_IsLost)pVtable[24];
	NEW_IDirectDrawSurfaceV1_IsLost=My_IDDSV1_IsLost;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV1_IsLost, NEW_IDirectDrawSurfaceV1_IsLost);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface::IsLost) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV1_Lock=(TD_IDirectDrawSurfaceV1_Lock)pVtable[25];
	NEW_IDirectDrawSurfaceV1_Lock=My_IDDSV1_Lock;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV1_Lock, NEW_IDirectDrawSurfaceV1_Lock);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface::Lock) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV1_ReleaseDC=(TD_IDirectDrawSurfaceV1_ReleaseDC)pVtable[26];
	NEW_IDirectDrawSurfaceV1_ReleaseDC=My_IDDSV1_ReleaseDC;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV1_ReleaseDC, NEW_IDirectDrawSurfaceV1_ReleaseDC);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface::ReleaseDC) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV1_Restore=(TD_IDirectDrawSurfaceV1_Restore)pVtable[27];
	NEW_IDirectDrawSurfaceV1_Restore=My_IDDSV1_Restore;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV1_Restore, NEW_IDirectDrawSurfaceV1_Restore);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface::Restore) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV1_SetClipper=(TD_IDirectDrawSurfaceV1_SetClipper)pVtable[28];
	NEW_IDirectDrawSurfaceV1_SetClipper=My_IDDSV1_SetClipper;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV1_SetClipper, NEW_IDirectDrawSurfaceV1_SetClipper);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface::SetClipper) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV1_SetColorKey=(TD_IDirectDrawSurfaceV1_SetColorKey)pVtable[29];
	NEW_IDirectDrawSurfaceV1_SetColorKey=My_IDDSV1_SetColorKey;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV1_SetColorKey, NEW_IDirectDrawSurfaceV1_SetColorKey);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface::SetColorKey) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV1_SetOverlayPosition=(TD_IDirectDrawSurfaceV1_SetOverlayPosition)pVtable[30];
	NEW_IDirectDrawSurfaceV1_SetOverlayPosition=My_IDDSV1_SetOverlayPosition;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV1_SetOverlayPosition, NEW_IDirectDrawSurfaceV1_SetOverlayPosition);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface::SetOverlayPosition) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV1_SetPalette=(TD_IDirectDrawSurfaceV1_SetPalette)pVtable[31];
	NEW_IDirectDrawSurfaceV1_SetPalette=My_IDDSV1_SetPalette;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV1_SetPalette, NEW_IDirectDrawSurfaceV1_SetPalette);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface::SetPalette) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV1_Unlock=(TD_IDirectDrawSurfaceV1_Unlock)pVtable[32];
	NEW_IDirectDrawSurfaceV1_Unlock=My_IDDSV1_Unlock;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV1_Unlock, NEW_IDirectDrawSurfaceV1_Unlock);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface::Unlock) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV1_UpdateOverlay=(TD_IDirectDrawSurfaceV1_UpdateOverlay)pVtable[33];
	NEW_IDirectDrawSurfaceV1_UpdateOverlay=My_IDDSV1_UpdateOverlay;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV1_UpdateOverlay, NEW_IDirectDrawSurfaceV1_UpdateOverlay);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface::UpdateOverlay) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV1_UpdateOverlayDisplay=(TD_IDirectDrawSurfaceV1_UpdateOverlayDisplay)pVtable[34];
	NEW_IDirectDrawSurfaceV1_UpdateOverlayDisplay=My_IDDSV1_UpdateOverlayDisplay;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV1_UpdateOverlayDisplay, NEW_IDirectDrawSurfaceV1_UpdateOverlayDisplay);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface::UpdateOverlayDisplay) failed.\r\n");
	}
	ORIG_IDirectDrawSurfaceV1_UpdateOverlayZOrder=(TD_IDirectDrawSurfaceV1_UpdateOverlayZOrder)pVtable[35];
	NEW_IDirectDrawSurfaceV1_UpdateOverlayZOrder=My_IDDSV1_UpdateOverlayZOrder;
	result = DetourAttach(&(PVOID&)ORIG_IDirectDrawSurfaceV1_UpdateOverlayZOrder, NEW_IDirectDrawSurfaceV1_UpdateOverlayZOrder);
	if (result != NO_ERROR) {
		//OutputDebugString("DetourAttach (IDirectDrawSurface::UpdateOverlayZOrder) failed.\r\n");
	}

	result = DetourTransactionCommit();
	if (result != NO_ERROR) {
		//OutputDebugString("DetourTransactionCommit failed.\r\n");
	}
	//OutputDebugString("HookInterface for IDirectDrawSurface V1 exited.\r\n");
	
}
