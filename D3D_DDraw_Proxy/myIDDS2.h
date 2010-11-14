#pragma once
/**************************************************************************
TYPEDEFS FOR STRONGLY-TYPED FUNCTION POINTERS TO IDIRECTDRAWSURFACE (V2) METHODS
**************************************************************************/

// IUnknown functions
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV2_QueryInterface) (LPUNKNOWN lpThis, REFIID riid, LPVOID* obp);
typedef ULONG   (WINAPI* TD_IDirectDrawSurfaceV2_AddRef)(LPUNKNOWN lpThis);
typedef ULONG   (WINAPI* TD_IDirectDrawSurfaceV2_Release)(LPUNKNOWN lpThis);

// IDirectDrawSurface (V1) functions
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV2_AddAttachedSurface)( 
	LPDIRECTDRAWSURFACE2 lpThis,
	LPDIRECTDRAWSURFACE2 lpDDSAttachedSurface  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV2_AddOverlayDirtyRect)(
  LPDIRECTDRAWSURFACE2 lpThis,
  LPRECT lpRect  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV2_Blt)(
  LPDIRECTDRAWSURFACE2 lpThis,
  LPRECT lpDestRect,                    
  LPDIRECTDRAWSURFACE2 lpDDSrcSurface,  
  LPRECT lpSrcRect,                     
  DWORD dwFlags,                        
  LPDDBLTFX lpDDBltFx                   
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV2_BltBatch)(
  LPDIRECTDRAWSURFACE2 lpThis,
  LPDDBLTBATCH lpDDBltBatch,  
  DWORD dwCount,              
  DWORD dwFlags               
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV2_BltFast)(
  LPDIRECTDRAWSURFACE2 lpThis,
  DWORD dwX,                            
  DWORD dwY,                            
  LPDIRECTDRAWSURFACE2 lpDDSrcSurface,  
  LPRECT lpSrcRect,                     
  DWORD dwTrans                         
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV2_DeleteAttachedSurface)(
  LPDIRECTDRAWSURFACE2 lpThis,
  DWORD dwFlags,                             
  LPDIRECTDRAWSURFACE2 lpDDSAttachedSurface  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV2_EnumAttachedSurfaces)(
  LPDIRECTDRAWSURFACE2 lpThis,
  LPVOID lpContext,                                
  LPDDENUMSURFACESCALLBACK lpEnumSurfacesCallback  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV2_EnumOverlayZOrders)(
  LPDIRECTDRAWSURFACE2 lpThis,
  DWORD dwFlags,                          
  LPVOID lpContext,                       
  LPDDENUMSURFACESCALLBACK lpfnCallback  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV2_Flip)(
  LPDIRECTDRAWSURFACE2 lpThis,
  LPDIRECTDRAWSURFACE2 lpDDSurfaceTargetOverride,  
  DWORD dwFlags                                    
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV2_GetAttachedSurface)(
  LPDIRECTDRAWSURFACE2 lpThis,
  LPDDSCAPS lpDDSCaps, 
  LPDIRECTDRAWSURFACE2 FAR *lplpDDAttachedSurface  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV2_GetBltStatus)(
  LPDIRECTDRAWSURFACE2 lpThis,
  DWORD dwFlags  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV2_GetCaps)(
  LPDIRECTDRAWSURFACE2 lpThis,
  LPDDSCAPS lpDDSCaps  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV2_GetClipper)(
  LPDIRECTDRAWSURFACE2 lpThis,
  LPDIRECTDRAWCLIPPER FAR *lplpDDClipper  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV2_GetColorKey)(
  LPDIRECTDRAWSURFACE2 lpThis,
  DWORD dwFlags,             
  LPDDCOLORKEY lpDDColorKey  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV2_GetDC)(
  LPDIRECTDRAWSURFACE2 lpThis,
  HDC FAR *lphDC  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV2_GetFlipStatus)(
  LPDIRECTDRAWSURFACE2 lpThis,
  DWORD dwFlags  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV2_GetOverlayPosition)(
  LPDIRECTDRAWSURFACE2 lpThis,
  LPLONG lplX, 
  LPLONG lplY  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV2_GetPalette)(
  LPDIRECTDRAWSURFACE2 lpThis,
  LPDIRECTDRAWPALETTE FAR *lplpDDPalette  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV2_GetPixelFormat)(
  LPDIRECTDRAWSURFACE2 lpThis,
  LPDDPIXELFORMAT lpDDPixelFormat  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV2_GetSurfaceDesc)(
  LPDIRECTDRAWSURFACE2 lpThis,
  LPDDSURFACEDESC lpDDSurfaceDesc  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV2_Initialize)(
  LPDIRECTDRAWSURFACE2 lpThis,
  LPDIRECTDRAW lpDD,               
  LPDDSURFACEDESC lpDDSurfaceDesc  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV2_IsLost)(LPDIRECTDRAWSURFACE2 lpThis);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV2_Lock)(
  LPDIRECTDRAWSURFACE2 lpThis,
  LPRECT lpDestRect,                
  LPDDSURFACEDESC lpDDSurfaceDesc,  
  DWORD dwFlags,                    
  HANDLE hEvent                     
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV2_ReleaseDC)(
  LPDIRECTDRAWSURFACE2 lpThis,
  HDC hDC  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV2_Restore)(LPDIRECTDRAWSURFACE2 lpThis);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV2_SetClipper)(
  LPDIRECTDRAWSURFACE2 lpThis,
  LPDIRECTDRAWCLIPPER lpDDClipper  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV2_SetColorKey)(
  LPDIRECTDRAWSURFACE2 lpThis,
  DWORD dwFlags,             
  LPDDCOLORKEY lpDDColorKey  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV2_SetOverlayPosition)(
  LPDIRECTDRAWSURFACE2 lpThis,
  LONG lX, 
  LONG lY  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV2_SetPalette)(
  LPDIRECTDRAWSURFACE2 lpThis,
  LPDIRECTDRAWPALETTE lpDDPalette  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV2_Unlock)(
  LPDIRECTDRAWSURFACE2 lpThis,
  LPVOID lpRect 
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV2_UpdateOverlay)(
  LPDIRECTDRAWSURFACE2 lpThis,
  LPRECT lpSrcRect,                      
  LPDIRECTDRAWSURFACE2 lpDDDestSurface,  
  LPRECT lpDestRect,                     
  DWORD dwFlags,                         
  LPDDOVERLAYFX lpDDOverlayFx            
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV2_UpdateOverlayDisplay)(
  LPDIRECTDRAWSURFACE2 lpThis,
  DWORD dwFlags  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV2_UpdateOverlayZOrder)(
  LPDIRECTDRAWSURFACE2 lpThis,
  DWORD dwFlags,                       
  LPDIRECTDRAWSURFACE2 lpDDSReference  
);
// IDirectDrawSurface2 functions
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV2_GetDDInterface)(
  LPDIRECTDRAWSURFACE2 lpThis,
  LPVOID FAR *lplpDD  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV2_PageLock)(
  LPDIRECTDRAWSURFACE2 lpThis,
  DWORD dwFlags  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV2_PageUnlock)(
  LPDIRECTDRAWSURFACE2 lpThis,
  DWORD dwFlags  
);


/**************************************************************************
STATIC STRONGLY-TYPED FUNCTION POINTERS TO 
ORIGINAL AND REPLACEMENT IDIRECTDRAWSURFACE (V2) METHODS
**************************************************************************/
static TD_IDirectDrawSurfaceV2_QueryInterface ORIG_IDirectDrawSurfaceV2_QueryInterface=NULL;
static TD_IDirectDrawSurfaceV2_QueryInterface NEW_IDirectDrawSurfaceV2_QueryInterface=NULL;
static TD_IDirectDrawSurfaceV2_AddRef ORIG_IDirectDrawSurfaceV2_AddRef=NULL;
static TD_IDirectDrawSurfaceV2_AddRef NEW_IDirectDrawSurfaceV2_AddRef=NULL;
static TD_IDirectDrawSurfaceV2_Release ORIG_IDirectDrawSurfaceV2_Release=NULL;
static TD_IDirectDrawSurfaceV2_Release NEW_IDirectDrawSurfaceV2_Release=NULL;
static TD_IDirectDrawSurfaceV2_AddAttachedSurface ORIG_IDirectDrawSurfaceV2_AddAttachedSurface=NULL;
static TD_IDirectDrawSurfaceV2_AddAttachedSurface NEW_IDirectDrawSurfaceV2_AddAttachedSurface=NULL;
static TD_IDirectDrawSurfaceV2_AddOverlayDirtyRect ORIG_IDirectDrawSurfaceV2_AddOverlayDirtyRect=NULL;
static TD_IDirectDrawSurfaceV2_AddOverlayDirtyRect NEW_IDirectDrawSurfaceV2_AddOverlayDirtyRect=NULL;
static TD_IDirectDrawSurfaceV2_Blt ORIG_IDirectDrawSurfaceV2_Blt=NULL;
static TD_IDirectDrawSurfaceV2_Blt NEW_IDirectDrawSurfaceV2_Blt=NULL;
static TD_IDirectDrawSurfaceV2_BltBatch ORIG_IDirectDrawSurfaceV2_BltBatch=NULL;
static TD_IDirectDrawSurfaceV2_BltBatch NEW_IDirectDrawSurfaceV2_BltBatch=NULL;
static TD_IDirectDrawSurfaceV2_BltFast ORIG_IDirectDrawSurfaceV2_BltFast=NULL;
static TD_IDirectDrawSurfaceV2_BltFast NEW_IDirectDrawSurfaceV2_BltFast=NULL;
static TD_IDirectDrawSurfaceV2_DeleteAttachedSurface ORIG_IDirectDrawSurfaceV2_DeleteAttachedSurface=NULL;
static TD_IDirectDrawSurfaceV2_DeleteAttachedSurface NEW_IDirectDrawSurfaceV2_DeleteAttachedSurface=NULL;
static TD_IDirectDrawSurfaceV2_EnumAttachedSurfaces ORIG_IDirectDrawSurfaceV2_EnumAttachedSurfaces=NULL;
static TD_IDirectDrawSurfaceV2_EnumAttachedSurfaces NEW_IDirectDrawSurfaceV2_EnumAttachedSurfaces=NULL;
static TD_IDirectDrawSurfaceV2_EnumOverlayZOrders ORIG_IDirectDrawSurfaceV2_EnumOverlayZOrders=NULL;
static TD_IDirectDrawSurfaceV2_EnumOverlayZOrders NEW_IDirectDrawSurfaceV2_EnumOverlayZOrders=NULL;
static TD_IDirectDrawSurfaceV2_Flip ORIG_IDirectDrawSurfaceV2_Flip=NULL;
static TD_IDirectDrawSurfaceV2_Flip NEW_IDirectDrawSurfaceV2_Flip=NULL;
static TD_IDirectDrawSurfaceV2_GetAttachedSurface ORIG_IDirectDrawSurfaceV2_GetAttachedSurface=NULL;
static TD_IDirectDrawSurfaceV2_GetAttachedSurface NEW_IDirectDrawSurfaceV2_GetAttachedSurface=NULL;
static TD_IDirectDrawSurfaceV2_GetBltStatus ORIG_IDirectDrawSurfaceV2_GetBltStatus=NULL;
static TD_IDirectDrawSurfaceV2_GetBltStatus NEW_IDirectDrawSurfaceV2_GetBltStatus=NULL;
static TD_IDirectDrawSurfaceV2_GetCaps ORIG_IDirectDrawSurfaceV2_GetCaps=NULL;
static TD_IDirectDrawSurfaceV2_GetCaps NEW_IDirectDrawSurfaceV2_GetCaps=NULL;
static TD_IDirectDrawSurfaceV2_GetClipper ORIG_IDirectDrawSurfaceV2_GetClipper=NULL;
static TD_IDirectDrawSurfaceV2_GetClipper NEW_IDirectDrawSurfaceV2_GetClipper=NULL;
static TD_IDirectDrawSurfaceV2_GetColorKey ORIG_IDirectDrawSurfaceV2_GetColorKey=NULL;
static TD_IDirectDrawSurfaceV2_GetColorKey NEW_IDirectDrawSurfaceV2_GetColorKey=NULL;
static TD_IDirectDrawSurfaceV2_GetDC ORIG_IDirectDrawSurfaceV2_GetDC=NULL;
static TD_IDirectDrawSurfaceV2_GetDC NEW_IDirectDrawSurfaceV2_GetDC=NULL;
static TD_IDirectDrawSurfaceV2_GetFlipStatus ORIG_IDirectDrawSurfaceV2_GetFlipStatus=NULL;
static TD_IDirectDrawSurfaceV2_GetFlipStatus NEW_IDirectDrawSurfaceV2_GetFlipStatus=NULL;
static TD_IDirectDrawSurfaceV2_GetOverlayPosition ORIG_IDirectDrawSurfaceV2_GetOverlayPosition=NULL;
static TD_IDirectDrawSurfaceV2_GetOverlayPosition NEW_IDirectDrawSurfaceV2_GetOverlayPosition=NULL;
static TD_IDirectDrawSurfaceV2_GetPalette ORIG_IDirectDrawSurfaceV2_GetPalette=NULL;
static TD_IDirectDrawSurfaceV2_GetPalette NEW_IDirectDrawSurfaceV2_GetPalette=NULL;
static TD_IDirectDrawSurfaceV2_GetPixelFormat ORIG_IDirectDrawSurfaceV2_GetPixelFormat=NULL;
static TD_IDirectDrawSurfaceV2_GetPixelFormat NEW_IDirectDrawSurfaceV2_GetPixelFormat=NULL;
static TD_IDirectDrawSurfaceV2_GetSurfaceDesc ORIG_IDirectDrawSurfaceV2_GetSurfaceDesc=NULL;
static TD_IDirectDrawSurfaceV2_GetSurfaceDesc NEW_IDirectDrawSurfaceV2_GetSurfaceDesc=NULL;
static TD_IDirectDrawSurfaceV2_Initialize ORIG_IDirectDrawSurfaceV2_Initialize=NULL;
static TD_IDirectDrawSurfaceV2_Initialize NEW_IDirectDrawSurfaceV2_Initialize=NULL;
static TD_IDirectDrawSurfaceV2_IsLost ORIG_IDirectDrawSurfaceV2_IsLost=NULL;
static TD_IDirectDrawSurfaceV2_IsLost NEW_IDirectDrawSurfaceV2_IsLost=NULL;
static TD_IDirectDrawSurfaceV2_Lock ORIG_IDirectDrawSurfaceV2_Lock=NULL;
static TD_IDirectDrawSurfaceV2_Lock NEW_IDirectDrawSurfaceV2_Lock=NULL;
static TD_IDirectDrawSurfaceV2_ReleaseDC ORIG_IDirectDrawSurfaceV2_ReleaseDC=NULL;
static TD_IDirectDrawSurfaceV2_ReleaseDC NEW_IDirectDrawSurfaceV2_ReleaseDC=NULL;
static TD_IDirectDrawSurfaceV2_Restore ORIG_IDirectDrawSurfaceV2_Restore=NULL;
static TD_IDirectDrawSurfaceV2_Restore NEW_IDirectDrawSurfaceV2_Restore=NULL;
static TD_IDirectDrawSurfaceV2_SetClipper ORIG_IDirectDrawSurfaceV2_SetClipper=NULL;
static TD_IDirectDrawSurfaceV2_SetClipper NEW_IDirectDrawSurfaceV2_SetClipper=NULL;
static TD_IDirectDrawSurfaceV2_SetColorKey ORIG_IDirectDrawSurfaceV2_SetColorKey=NULL;
static TD_IDirectDrawSurfaceV2_SetColorKey NEW_IDirectDrawSurfaceV2_SetColorKey=NULL;
static TD_IDirectDrawSurfaceV2_SetOverlayPosition ORIG_IDirectDrawSurfaceV2_SetOverlayPosition=NULL;
static TD_IDirectDrawSurfaceV2_SetOverlayPosition NEW_IDirectDrawSurfaceV2_SetOverlayPosition=NULL;
static TD_IDirectDrawSurfaceV2_SetPalette ORIG_IDirectDrawSurfaceV2_SetPalette=NULL;
static TD_IDirectDrawSurfaceV2_SetPalette NEW_IDirectDrawSurfaceV2_SetPalette=NULL;
static TD_IDirectDrawSurfaceV2_Unlock ORIG_IDirectDrawSurfaceV2_Unlock=NULL;
static TD_IDirectDrawSurfaceV2_Unlock NEW_IDirectDrawSurfaceV2_Unlock=NULL;
static TD_IDirectDrawSurfaceV2_UpdateOverlay ORIG_IDirectDrawSurfaceV2_UpdateOverlay=NULL;
static TD_IDirectDrawSurfaceV2_UpdateOverlay NEW_IDirectDrawSurfaceV2_UpdateOverlay=NULL;
static TD_IDirectDrawSurfaceV2_UpdateOverlayDisplay NEW_IDirectDrawSurfaceV2_UpdateOverlayDisplay=NULL;
static TD_IDirectDrawSurfaceV2_UpdateOverlayDisplay ORIG_IDirectDrawSurfaceV2_UpdateOverlayDisplay=NULL;
static TD_IDirectDrawSurfaceV2_UpdateOverlayZOrder ORIG_IDirectDrawSurfaceV2_UpdateOverlayZOrder=NULL;
static TD_IDirectDrawSurfaceV2_UpdateOverlayZOrder NEW_IDirectDrawSurfaceV2_UpdateOverlayZOrder=NULL;
static TD_IDirectDrawSurfaceV2_GetDDInterface ORIG_IDirectDrawSurfaceV2_GetDDInterface=NULL;
static TD_IDirectDrawSurfaceV2_GetDDInterface NEW_IDirectDrawSurfaceV2_GetDDInterface=NULL;
static TD_IDirectDrawSurfaceV2_PageLock ORIG_IDirectDrawSurfaceV2_PageLock=NULL;
static TD_IDirectDrawSurfaceV2_PageLock NEW_IDirectDrawSurfaceV2_PageLock=NULL;
static TD_IDirectDrawSurfaceV2_PageUnlock ORIG_IDirectDrawSurfaceV2_PageUnlock=NULL;
static TD_IDirectDrawSurfaceV2_PageUnlock NEW_IDirectDrawSurfaceV2_PageUnlock=NULL;


/**************************************************************************
REPLACEMENT IMPLEMENTATION FOR IDIRECTDRAWSURFACE (V2) METHODS
**************************************************************************/

// IUnknown functions
HRESULT WINAPI My_IDDSV2_QueryInterface (LPUNKNOWN lpThis, REFIID riid, LPVOID* obp);
ULONG   WINAPI My_IDDSV2_AddRef(LPUNKNOWN lpThis);
ULONG   WINAPI My_IDDSV2_Release(LPUNKNOWN lpThis);
// IDirectDrawSurface (V1) functions
HRESULT WINAPI My_IDDSV2_AddAttachedSurface(
  LPDIRECTDRAWSURFACE2 lpThis,
  LPDIRECTDRAWSURFACE2 lpDDSAttachedSurface  
);
HRESULT WINAPI My_IDDSV2_AddOverlayDirtyRect(
  LPDIRECTDRAWSURFACE2 lpThis,
  LPRECT lpRect  
);
HRESULT WINAPI My_IDDSV2_Blt(
  LPDIRECTDRAWSURFACE2 lpThis,
  LPRECT lpDestRect,                    
  LPDIRECTDRAWSURFACE2 lpDDSrcSurface,  
  LPRECT lpSrcRect,                     
  DWORD dwFlags,                        
  LPDDBLTFX lpDDBltFx                   
);
HRESULT WINAPI My_IDDSV2_BltBatch(
  LPDIRECTDRAWSURFACE2 lpThis,
  LPDDBLTBATCH lpDDBltBatch,  
  DWORD dwCount,              
  DWORD dwFlags               
);
HRESULT WINAPI My_IDDSV2_BltFast(
  LPDIRECTDRAWSURFACE2 lpThis,
  DWORD dwX,                            
  DWORD dwY,                            
  LPDIRECTDRAWSURFACE2 lpDDSrcSurface,  
  LPRECT lpSrcRect,                     
  DWORD dwTrans                         
);
HRESULT WINAPI My_IDDSV2_DeleteAttachedSurface(
  LPDIRECTDRAWSURFACE2 lpThis,
  DWORD dwFlags,                             
  LPDIRECTDRAWSURFACE2 lpDDSAttachedSurface  
);
HRESULT WINAPI My_IDDSV2_EnumAttachedSurfaces(
  LPDIRECTDRAWSURFACE2 lpThis,
  LPVOID lpContext,                                
  LPDDENUMSURFACESCALLBACK lpEnumSurfacesCallback  
);
HRESULT WINAPI My_IDDSV2_EnumOverlayZOrders(
  LPDIRECTDRAWSURFACE2 lpThis,
  DWORD dwFlags,                          
  LPVOID lpContext,                       
  LPDDENUMSURFACESCALLBACK lpfnCallback  
);
HRESULT WINAPI My_IDDSV2_Flip(
  LPDIRECTDRAWSURFACE2 lpThis,
  LPDIRECTDRAWSURFACE2 lpDDSurfaceTargetOverride,  
  DWORD dwFlags                                    
);
HRESULT WINAPI My_IDDSV2_GetAttachedSurface(
  LPDIRECTDRAWSURFACE2 lpThis,
  LPDDSCAPS lpDDSCaps, 
  LPDIRECTDRAWSURFACE2 FAR *lplpDDAttachedSurface  
);
HRESULT WINAPI My_IDDSV2_GetBltStatus(
  LPDIRECTDRAWSURFACE2 lpThis,
  DWORD dwFlags  
);
HRESULT WINAPI My_IDDSV2_GetCaps(
  LPDIRECTDRAWSURFACE2 lpThis,
  LPDDSCAPS lpDDSCaps  
);
HRESULT WINAPI My_IDDSV2_GetClipper(
  LPDIRECTDRAWSURFACE2 lpThis,
  LPDIRECTDRAWCLIPPER FAR *lplpDDClipper  
);
HRESULT WINAPI My_IDDSV2_GetColorKey(
  LPDIRECTDRAWSURFACE2 lpThis,
  DWORD dwFlags,             
  LPDDCOLORKEY lpDDColorKey  
);
HRESULT WINAPI My_IDDSV2_GetDC(
  LPDIRECTDRAWSURFACE2 lpThis,
  HDC FAR *lphDC  
);
HRESULT WINAPI My_IDDSV2_GetFlipStatus(
  LPDIRECTDRAWSURFACE2 lpThis,
  DWORD dwFlags  
);
HRESULT WINAPI My_IDDSV2_GetOverlayPosition(
  LPDIRECTDRAWSURFACE2 lpThis,
  LPLONG lplX, 
  LPLONG lplY  
);
HRESULT WINAPI My_IDDSV2_GetPalette(
  LPDIRECTDRAWSURFACE2 lpThis,
  LPDIRECTDRAWPALETTE FAR *lplpDDPalette  
);

HRESULT WINAPI My_IDDSV2_GetPixelFormat(
  LPDIRECTDRAWSURFACE2 lpThis,
  LPDDPIXELFORMAT lpDDPixelFormat  
);
HRESULT WINAPI My_IDDSV2_GetSurfaceDesc(
  LPDIRECTDRAWSURFACE2 lpThis,
  LPDDSURFACEDESC lpDDSurfaceDesc  
);
HRESULT WINAPI My_IDDSV2_Initialize(
  LPDIRECTDRAWSURFACE2 lpThis,
  LPDIRECTDRAW lpDD,               
  LPDDSURFACEDESC lpDDSurfaceDesc  
);
HRESULT WINAPI My_IDDSV2_IsLost(
  LPDIRECTDRAWSURFACE2 lpThis
);
HRESULT WINAPI My_IDDSV2_Lock(
  LPDIRECTDRAWSURFACE2 lpThis,
  LPRECT lpDestRect,                
  LPDDSURFACEDESC lpDDSurfaceDesc,  
  DWORD dwFlags,                    
  HANDLE hEvent                     
);
HRESULT WINAPI My_IDDSV2_ReleaseDC(
  LPDIRECTDRAWSURFACE2 lpThis,
  HDC hDC  
);
HRESULT WINAPI My_IDDSV2_Restore(
  LPDIRECTDRAWSURFACE2 lpThis
);
HRESULT WINAPI My_IDDSV2_SetClipper(
  LPDIRECTDRAWSURFACE2 lpThis,
  LPDIRECTDRAWCLIPPER lpDDClipper  
);
HRESULT WINAPI My_IDDSV2_SetColorKey(
  LPDIRECTDRAWSURFACE2 lpThis,
  DWORD dwFlags,             
  LPDDCOLORKEY lpDDColorKey  
);
HRESULT WINAPI My_IDDSV2_SetOverlayPosition(
  LPDIRECTDRAWSURFACE2 lpThis,
  LONG lX, 
  LONG lY  
);
HRESULT WINAPI My_IDDSV2_SetPalette(
  LPDIRECTDRAWSURFACE2 lpThis,
  LPDIRECTDRAWPALETTE lpDDPalette  
);
HRESULT WINAPI My_IDDSV2_Unlock(
  LPDIRECTDRAWSURFACE2 lpThis,
  LPVOID lpRect 
);
HRESULT WINAPI My_IDDSV2_UpdateOverlay(
  LPDIRECTDRAWSURFACE2 lpThis,
  LPRECT lpSrcRect,                      
  LPDIRECTDRAWSURFACE2 lpDDDestSurface,  
  LPRECT lpDestRect,                     
  DWORD dwFlags,                         
  LPDDOVERLAYFX lpDDOverlayFx            
);
HRESULT WINAPI My_IDDSV2_UpdateOverlayDisplay(
  LPDIRECTDRAWSURFACE2 lpThis,
  DWORD dwFlags  
);
HRESULT WINAPI My_IDDSV2_UpdateOverlayZOrder(
  LPDIRECTDRAWSURFACE2 lpThis,
  DWORD dwFlags,                       
  LPDIRECTDRAWSURFACE2 lpDDSReference  
);
// IDirectDrawSurface2 functions
HRESULT WINAPI My_IDDSV2_GetDDInterface(
  LPDIRECTDRAWSURFACE2 lpThis,
  LPVOID FAR *lplpDD  
);
HRESULT WINAPI My_IDDSV2_PageLock(
  LPDIRECTDRAWSURFACE2 lpThis,
  DWORD dwFlags  
);
HRESULT WINAPI My_IDDSV2_PageUnlock(
  LPDIRECTDRAWSURFACE2 lpThis,
  DWORD dwFlags  
);


/**************************************************************************
DETOURS SETUP AND TEARDOWN FUNCTIONS
**************************************************************************/
void HookDirectDrawSurface2Interface  (LPDIRECTDRAWSURFACE2 lpInterface);

