#pragma once
/**************************************************************************
TYPEDEFS FOR STRONGLY-TYPED FUNCTION POINTERS TO IDIRECTDRAWSURFACE (V3) METHODS
**************************************************************************/

// IUnknown functions
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV3_QueryInterface) (LPUNKNOWN lpThis, REFIID riid, LPVOID* obp);
typedef ULONG   (WINAPI* TD_IDirectDrawSurfaceV3_AddRef)(LPUNKNOWN lpThis);
typedef ULONG   (WINAPI* TD_IDirectDrawSurfaceV3_Release)(LPUNKNOWN lpThis);

// IDirectDrawSurface (V1) functions
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV3_AddAttachedSurface)( 
	LPDIRECTDRAWSURFACE3 lpThis,
	LPDIRECTDRAWSURFACE3 lpDDSAttachedSurface  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV3_AddOverlayDirtyRect)(
  LPDIRECTDRAWSURFACE3 lpThis,
  LPRECT lpRect  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV3_Blt)(
  LPDIRECTDRAWSURFACE3 lpThis,
  LPRECT lpDestRect,                    
  LPDIRECTDRAWSURFACE3 lpDDSrcSurface,  
  LPRECT lpSrcRect,                     
  DWORD dwFlags,                        
  LPDDBLTFX lpDDBltFx                   
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV3_BltBatch)(
  LPDIRECTDRAWSURFACE3 lpThis,
  LPDDBLTBATCH lpDDBltBatch,  
  DWORD dwCount,              
  DWORD dwFlags               
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV3_BltFast)(
  LPDIRECTDRAWSURFACE3 lpThis,
  DWORD dwX,                            
  DWORD dwY,                            
  LPDIRECTDRAWSURFACE3 lpDDSrcSurface,  
  LPRECT lpSrcRect,                     
  DWORD dwTrans                         
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV3_DeleteAttachedSurface)(
  LPDIRECTDRAWSURFACE3 lpThis,
  DWORD dwFlags,                             
  LPDIRECTDRAWSURFACE3 lpDDSAttachedSurface  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV3_EnumAttachedSurfaces)(
  LPDIRECTDRAWSURFACE3 lpThis,
  LPVOID lpContext,                                
  LPDDENUMSURFACESCALLBACK lpEnumSurfacesCallback  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV3_EnumOverlayZOrders)(
  LPDIRECTDRAWSURFACE3 lpThis,
  DWORD dwFlags,                          
  LPVOID lpContext,                       
  LPDDENUMSURFACESCALLBACK lpfnCallback  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV3_Flip)(
  LPDIRECTDRAWSURFACE3 lpThis,
  LPDIRECTDRAWSURFACE3 lpDDSurfaceTargetOverride,  
  DWORD dwFlags                                    
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV3_GetAttachedSurface)(
  LPDIRECTDRAWSURFACE3 lpThis,
  LPDDSCAPS lpDDSCaps, 
  LPDIRECTDRAWSURFACE3 FAR *lplpDDAttachedSurface  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV3_GetBltStatus)(
  LPDIRECTDRAWSURFACE3 lpThis,
  DWORD dwFlags  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV3_GetCaps)(
  LPDIRECTDRAWSURFACE3 lpThis,
  LPDDSCAPS lpDDSCaps  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV3_GetClipper)(
  LPDIRECTDRAWSURFACE3 lpThis,
  LPDIRECTDRAWCLIPPER FAR *lplpDDClipper  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV3_GetColorKey)(
  LPDIRECTDRAWSURFACE3 lpThis,
  DWORD dwFlags,             
  LPDDCOLORKEY lpDDColorKey  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV3_GetDC)(
  LPDIRECTDRAWSURFACE3 lpThis,
  HDC FAR *lphDC  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV3_GetFlipStatus)(
  LPDIRECTDRAWSURFACE3 lpThis,
  DWORD dwFlags  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV3_GetOverlayPosition)(
  LPDIRECTDRAWSURFACE3 lpThis,
  LPLONG lplX, 
  LPLONG lplY  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV3_GetPalette)(
  LPDIRECTDRAWSURFACE3 lpThis,
  LPDIRECTDRAWPALETTE FAR *lplpDDPalette  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV3_GetPixelFormat)(
  LPDIRECTDRAWSURFACE3 lpThis,
  LPDDPIXELFORMAT lpDDPixelFormat  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV3_GetSurfaceDesc)(
  LPDIRECTDRAWSURFACE3 lpThis,
  LPDDSURFACEDESC lpDDSurfaceDesc  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV3_Initialize)(
  LPDIRECTDRAWSURFACE3 lpThis,
  LPDIRECTDRAW lpDD,               
  LPDDSURFACEDESC lpDDSurfaceDesc  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV3_IsLost)(LPDIRECTDRAWSURFACE3 lpThis);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV3_Lock)(
  LPDIRECTDRAWSURFACE3 lpThis,
  LPRECT lpDestRect,                
  LPDDSURFACEDESC lpDDSurfaceDesc,  
  DWORD dwFlags,                    
  HANDLE hEvent                     
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV3_ReleaseDC)(
  LPDIRECTDRAWSURFACE3 lpThis,
  HDC hDC  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV3_Restore)(LPDIRECTDRAWSURFACE3 lpThis);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV3_SetClipper)(
  LPDIRECTDRAWSURFACE3 lpThis,
  LPDIRECTDRAWCLIPPER lpDDClipper  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV3_SetColorKey)(
  LPDIRECTDRAWSURFACE3 lpThis,
  DWORD dwFlags,             
  LPDDCOLORKEY lpDDColorKey  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV3_SetOverlayPosition)(
  LPDIRECTDRAWSURFACE3 lpThis,
  LONG lX, 
  LONG lY  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV3_SetPalette)(
  LPDIRECTDRAWSURFACE3 lpThis,
  LPDIRECTDRAWPALETTE lpDDPalette  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV3_Unlock)(
  LPDIRECTDRAWSURFACE3 lpThis,
  LPVOID lpRect 
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV3_UpdateOverlay)(
  LPDIRECTDRAWSURFACE3 lpThis,
  LPRECT lpSrcRect,                      
  LPDIRECTDRAWSURFACE3 lpDDDestSurface,  
  LPRECT lpDestRect,                     
  DWORD dwFlags,                         
  LPDDOVERLAYFX lpDDOverlayFx            
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV3_UpdateOverlayDisplay)(
  LPDIRECTDRAWSURFACE3 lpThis,
  DWORD dwFlags  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV3_UpdateOverlayZOrder)(
  LPDIRECTDRAWSURFACE3 lpThis,
  DWORD dwFlags,                       
  LPDIRECTDRAWSURFACE3 lpDDSReference  
);
// IDirectDrawSurface2 functions
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV3_GetDDInterface)(
  LPDIRECTDRAWSURFACE3 lpThis,
  LPVOID FAR *lplpDD  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV3_PageLock)(
  LPDIRECTDRAWSURFACE3 lpThis,
  DWORD dwFlags  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV3_PageUnlock)(
  LPDIRECTDRAWSURFACE3 lpThis,
  DWORD dwFlags  
);

// IDirectDrawSurface3  functions
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV3_SetSurfaceDesc)(
  LPDIRECTDRAWSURFACE3 lpThis,
  LPDDSURFACEDESC lpddsd,  
  DWORD dwFlags            
);



/**************************************************************************
STATIC STRONGLY-TYPED FUNCTION POINTERS TO 
ORIGINAL AND REPLACEMENT IDIRECTDRAWSURFACE (V3) METHODS
**************************************************************************/
static TD_IDirectDrawSurfaceV3_QueryInterface ORIG_IDirectDrawSurfaceV3_QueryInterface=NULL;
static TD_IDirectDrawSurfaceV3_QueryInterface NEW_IDirectDrawSurfaceV3_QueryInterface=NULL;
static TD_IDirectDrawSurfaceV3_AddRef ORIG_IDirectDrawSurfaceV3_AddRef=NULL;
static TD_IDirectDrawSurfaceV3_AddRef NEW_IDirectDrawSurfaceV3_AddRef=NULL;
static TD_IDirectDrawSurfaceV3_Release ORIG_IDirectDrawSurfaceV3_Release=NULL;
static TD_IDirectDrawSurfaceV3_Release NEW_IDirectDrawSurfaceV3_Release=NULL;
static TD_IDirectDrawSurfaceV3_AddAttachedSurface ORIG_IDirectDrawSurfaceV3_AddAttachedSurface=NULL;
static TD_IDirectDrawSurfaceV3_AddAttachedSurface NEW_IDirectDrawSurfaceV3_AddAttachedSurface=NULL;
static TD_IDirectDrawSurfaceV3_AddOverlayDirtyRect ORIG_IDirectDrawSurfaceV3_AddOverlayDirtyRect=NULL;
static TD_IDirectDrawSurfaceV3_AddOverlayDirtyRect NEW_IDirectDrawSurfaceV3_AddOverlayDirtyRect=NULL;
static TD_IDirectDrawSurfaceV3_Blt ORIG_IDirectDrawSurfaceV3_Blt=NULL;
static TD_IDirectDrawSurfaceV3_Blt NEW_IDirectDrawSurfaceV3_Blt=NULL;
static TD_IDirectDrawSurfaceV3_BltBatch ORIG_IDirectDrawSurfaceV3_BltBatch=NULL;
static TD_IDirectDrawSurfaceV3_BltBatch NEW_IDirectDrawSurfaceV3_BltBatch=NULL;
static TD_IDirectDrawSurfaceV3_BltFast ORIG_IDirectDrawSurfaceV3_BltFast=NULL;
static TD_IDirectDrawSurfaceV3_BltFast NEW_IDirectDrawSurfaceV3_BltFast=NULL;
static TD_IDirectDrawSurfaceV3_DeleteAttachedSurface ORIG_IDirectDrawSurfaceV3_DeleteAttachedSurface=NULL;
static TD_IDirectDrawSurfaceV3_DeleteAttachedSurface NEW_IDirectDrawSurfaceV3_DeleteAttachedSurface=NULL;
static TD_IDirectDrawSurfaceV3_EnumAttachedSurfaces ORIG_IDirectDrawSurfaceV3_EnumAttachedSurfaces=NULL;
static TD_IDirectDrawSurfaceV3_EnumAttachedSurfaces NEW_IDirectDrawSurfaceV3_EnumAttachedSurfaces=NULL;
static TD_IDirectDrawSurfaceV3_EnumOverlayZOrders ORIG_IDirectDrawSurfaceV3_EnumOverlayZOrders=NULL;
static TD_IDirectDrawSurfaceV3_EnumOverlayZOrders NEW_IDirectDrawSurfaceV3_EnumOverlayZOrders=NULL;
static TD_IDirectDrawSurfaceV3_Flip ORIG_IDirectDrawSurfaceV3_Flip=NULL;
static TD_IDirectDrawSurfaceV3_Flip NEW_IDirectDrawSurfaceV3_Flip=NULL;
static TD_IDirectDrawSurfaceV3_GetAttachedSurface ORIG_IDirectDrawSurfaceV3_GetAttachedSurface=NULL;
static TD_IDirectDrawSurfaceV3_GetAttachedSurface NEW_IDirectDrawSurfaceV3_GetAttachedSurface=NULL;
static TD_IDirectDrawSurfaceV3_GetBltStatus ORIG_IDirectDrawSurfaceV3_GetBltStatus=NULL;
static TD_IDirectDrawSurfaceV3_GetBltStatus NEW_IDirectDrawSurfaceV3_GetBltStatus=NULL;
static TD_IDirectDrawSurfaceV3_GetCaps ORIG_IDirectDrawSurfaceV3_GetCaps=NULL;
static TD_IDirectDrawSurfaceV3_GetCaps NEW_IDirectDrawSurfaceV3_GetCaps=NULL;
static TD_IDirectDrawSurfaceV3_GetClipper ORIG_IDirectDrawSurfaceV3_GetClipper=NULL;
static TD_IDirectDrawSurfaceV3_GetClipper NEW_IDirectDrawSurfaceV3_GetClipper=NULL;
static TD_IDirectDrawSurfaceV3_GetColorKey ORIG_IDirectDrawSurfaceV3_GetColorKey=NULL;
static TD_IDirectDrawSurfaceV3_GetColorKey NEW_IDirectDrawSurfaceV3_GetColorKey=NULL;
static TD_IDirectDrawSurfaceV3_GetDC ORIG_IDirectDrawSurfaceV3_GetDC=NULL;
static TD_IDirectDrawSurfaceV3_GetDC NEW_IDirectDrawSurfaceV3_GetDC=NULL;
static TD_IDirectDrawSurfaceV3_GetFlipStatus ORIG_IDirectDrawSurfaceV3_GetFlipStatus=NULL;
static TD_IDirectDrawSurfaceV3_GetFlipStatus NEW_IDirectDrawSurfaceV3_GetFlipStatus=NULL;
static TD_IDirectDrawSurfaceV3_GetOverlayPosition ORIG_IDirectDrawSurfaceV3_GetOverlayPosition=NULL;
static TD_IDirectDrawSurfaceV3_GetOverlayPosition NEW_IDirectDrawSurfaceV3_GetOverlayPosition=NULL;
static TD_IDirectDrawSurfaceV3_GetPalette ORIG_IDirectDrawSurfaceV3_GetPalette=NULL;
static TD_IDirectDrawSurfaceV3_GetPalette NEW_IDirectDrawSurfaceV3_GetPalette=NULL;
static TD_IDirectDrawSurfaceV3_GetPixelFormat ORIG_IDirectDrawSurfaceV3_GetPixelFormat=NULL;
static TD_IDirectDrawSurfaceV3_GetPixelFormat NEW_IDirectDrawSurfaceV3_GetPixelFormat=NULL;
static TD_IDirectDrawSurfaceV3_GetSurfaceDesc ORIG_IDirectDrawSurfaceV3_GetSurfaceDesc=NULL;
static TD_IDirectDrawSurfaceV3_GetSurfaceDesc NEW_IDirectDrawSurfaceV3_GetSurfaceDesc=NULL;
static TD_IDirectDrawSurfaceV3_Initialize ORIG_IDirectDrawSurfaceV3_Initialize=NULL;
static TD_IDirectDrawSurfaceV3_Initialize NEW_IDirectDrawSurfaceV3_Initialize=NULL;
static TD_IDirectDrawSurfaceV3_IsLost ORIG_IDirectDrawSurfaceV3_IsLost=NULL;
static TD_IDirectDrawSurfaceV3_IsLost NEW_IDirectDrawSurfaceV3_IsLost=NULL;
static TD_IDirectDrawSurfaceV3_Lock ORIG_IDirectDrawSurfaceV3_Lock=NULL;
static TD_IDirectDrawSurfaceV3_Lock NEW_IDirectDrawSurfaceV3_Lock=NULL;
static TD_IDirectDrawSurfaceV3_ReleaseDC ORIG_IDirectDrawSurfaceV3_ReleaseDC=NULL;
static TD_IDirectDrawSurfaceV3_ReleaseDC NEW_IDirectDrawSurfaceV3_ReleaseDC=NULL;
static TD_IDirectDrawSurfaceV3_Restore ORIG_IDirectDrawSurfaceV3_Restore=NULL;
static TD_IDirectDrawSurfaceV3_Restore NEW_IDirectDrawSurfaceV3_Restore=NULL;
static TD_IDirectDrawSurfaceV3_SetClipper ORIG_IDirectDrawSurfaceV3_SetClipper=NULL;
static TD_IDirectDrawSurfaceV3_SetClipper NEW_IDirectDrawSurfaceV3_SetClipper=NULL;
static TD_IDirectDrawSurfaceV3_SetColorKey ORIG_IDirectDrawSurfaceV3_SetColorKey=NULL;
static TD_IDirectDrawSurfaceV3_SetColorKey NEW_IDirectDrawSurfaceV3_SetColorKey=NULL;
static TD_IDirectDrawSurfaceV3_SetOverlayPosition ORIG_IDirectDrawSurfaceV3_SetOverlayPosition=NULL;
static TD_IDirectDrawSurfaceV3_SetOverlayPosition NEW_IDirectDrawSurfaceV3_SetOverlayPosition=NULL;
static TD_IDirectDrawSurfaceV3_SetPalette ORIG_IDirectDrawSurfaceV3_SetPalette=NULL;
static TD_IDirectDrawSurfaceV3_SetPalette NEW_IDirectDrawSurfaceV3_SetPalette=NULL;
static TD_IDirectDrawSurfaceV3_Unlock ORIG_IDirectDrawSurfaceV3_Unlock=NULL;
static TD_IDirectDrawSurfaceV3_Unlock NEW_IDirectDrawSurfaceV3_Unlock=NULL;
static TD_IDirectDrawSurfaceV3_UpdateOverlay ORIG_IDirectDrawSurfaceV3_UpdateOverlay=NULL;
static TD_IDirectDrawSurfaceV3_UpdateOverlay NEW_IDirectDrawSurfaceV3_UpdateOverlay=NULL;
static TD_IDirectDrawSurfaceV3_UpdateOverlayDisplay NEW_IDirectDrawSurfaceV3_UpdateOverlayDisplay=NULL;
static TD_IDirectDrawSurfaceV3_UpdateOverlayDisplay ORIG_IDirectDrawSurfaceV3_UpdateOverlayDisplay=NULL;
static TD_IDirectDrawSurfaceV3_UpdateOverlayZOrder ORIG_IDirectDrawSurfaceV3_UpdateOverlayZOrder=NULL;
static TD_IDirectDrawSurfaceV3_UpdateOverlayZOrder NEW_IDirectDrawSurfaceV3_UpdateOverlayZOrder=NULL;
static TD_IDirectDrawSurfaceV3_GetDDInterface ORIG_IDirectDrawSurfaceV3_GetDDInterface=NULL;
static TD_IDirectDrawSurfaceV3_GetDDInterface NEW_IDirectDrawSurfaceV3_GetDDInterface=NULL;
static TD_IDirectDrawSurfaceV3_PageLock ORIG_IDirectDrawSurfaceV3_PageLock=NULL;
static TD_IDirectDrawSurfaceV3_PageLock NEW_IDirectDrawSurfaceV3_PageLock=NULL;
static TD_IDirectDrawSurfaceV3_PageUnlock ORIG_IDirectDrawSurfaceV3_PageUnlock=NULL;
static TD_IDirectDrawSurfaceV3_PageUnlock NEW_IDirectDrawSurfaceV3_PageUnlock=NULL;
static TD_IDirectDrawSurfaceV3_SetSurfaceDesc ORIG_IDirectDrawSurfaceV3_SetSurfaceDesc=NULL;
static TD_IDirectDrawSurfaceV3_SetSurfaceDesc NEW_IDirectDrawSurfaceV3_SetSurfaceDesc=NULL;


/**************************************************************************
REPLACEMENT IMPLEMENTATION FOR IDIRECTDRAWSURFACE (V3) METHODS
**************************************************************************/

// IUnknown functions
HRESULT WINAPI My_IDDSV3_QueryInterface (LPUNKNOWN lpThis, REFIID riid, LPVOID* obp);
ULONG   WINAPI My_IDDSV3_AddRef(LPUNKNOWN lpThis);
ULONG   WINAPI My_IDDSV3_Release(LPUNKNOWN lpThis);
// IDirectDrawSurface (V1) functions
HRESULT WINAPI My_IDDSV3_AddAttachedSurface(
  LPDIRECTDRAWSURFACE3 lpThis,
  LPDIRECTDRAWSURFACE3 lpDDSAttachedSurface  
);
HRESULT WINAPI My_IDDSV3_AddOverlayDirtyRect(
  LPDIRECTDRAWSURFACE3 lpThis,
  LPRECT lpRect  
);
HRESULT WINAPI My_IDDSV3_Blt(
  LPDIRECTDRAWSURFACE3 lpThis,
  LPRECT lpDestRect,                    
  LPDIRECTDRAWSURFACE3 lpDDSrcSurface,  
  LPRECT lpSrcRect,                     
  DWORD dwFlags,                        
  LPDDBLTFX lpDDBltFx                   
);
HRESULT WINAPI My_IDDSV3_BltBatch(
  LPDIRECTDRAWSURFACE3 lpThis,
  LPDDBLTBATCH lpDDBltBatch,  
  DWORD dwCount,              
  DWORD dwFlags               
);
HRESULT WINAPI My_IDDSV3_BltFast(
  LPDIRECTDRAWSURFACE3 lpThis,
  DWORD dwX,                            
  DWORD dwY,                            
  LPDIRECTDRAWSURFACE3 lpDDSrcSurface,  
  LPRECT lpSrcRect,                     
  DWORD dwTrans                         
);
HRESULT WINAPI My_IDDSV3_DeleteAttachedSurface(
  LPDIRECTDRAWSURFACE3 lpThis,
  DWORD dwFlags,                             
  LPDIRECTDRAWSURFACE3 lpDDSAttachedSurface  
);
HRESULT WINAPI My_IDDSV3_EnumAttachedSurfaces(
  LPDIRECTDRAWSURFACE3 lpThis,
  LPVOID lpContext,                                
  LPDDENUMSURFACESCALLBACK lpEnumSurfacesCallback  
);
HRESULT WINAPI My_IDDSV3_EnumOverlayZOrders(
  LPDIRECTDRAWSURFACE3 lpThis,
  DWORD dwFlags,                          
  LPVOID lpContext,                       
  LPDDENUMSURFACESCALLBACK lpfnCallback  
);
HRESULT WINAPI My_IDDSV3_Flip(
  LPDIRECTDRAWSURFACE3 lpThis,
  LPDIRECTDRAWSURFACE3 lpDDSurfaceTargetOverride,  
  DWORD dwFlags                                    
);
HRESULT WINAPI My_IDDSV3_GetAttachedSurface(
  LPDIRECTDRAWSURFACE3 lpThis,
  LPDDSCAPS lpDDSCaps, 
  LPDIRECTDRAWSURFACE3 FAR *lplpDDAttachedSurface  
);
HRESULT WINAPI My_IDDSV3_GetBltStatus(
  LPDIRECTDRAWSURFACE3 lpThis,
  DWORD dwFlags  
);
HRESULT WINAPI My_IDDSV3_GetCaps(
  LPDIRECTDRAWSURFACE3 lpThis,
  LPDDSCAPS lpDDSCaps  
);
HRESULT WINAPI My_IDDSV3_GetClipper(
  LPDIRECTDRAWSURFACE3 lpThis,
  LPDIRECTDRAWCLIPPER FAR *lplpDDClipper  
);
HRESULT WINAPI My_IDDSV3_GetColorKey(
  LPDIRECTDRAWSURFACE3 lpThis,
  DWORD dwFlags,             
  LPDDCOLORKEY lpDDColorKey  
);
HRESULT WINAPI My_IDDSV3_GetDC(
  LPDIRECTDRAWSURFACE3 lpThis,
  HDC FAR *lphDC  
);
HRESULT WINAPI My_IDDSV3_GetFlipStatus(
  LPDIRECTDRAWSURFACE3 lpThis,
  DWORD dwFlags  
);
HRESULT WINAPI My_IDDSV3_GetOverlayPosition(
  LPDIRECTDRAWSURFACE3 lpThis,
  LPLONG lplX, 
  LPLONG lplY  
);
HRESULT WINAPI My_IDDSV3_GetPalette(
  LPDIRECTDRAWSURFACE3 lpThis,
  LPDIRECTDRAWPALETTE FAR *lplpDDPalette  
);

HRESULT WINAPI My_IDDSV3_GetPixelFormat(
  LPDIRECTDRAWSURFACE3 lpThis,
  LPDDPIXELFORMAT lpDDPixelFormat  
);
HRESULT WINAPI My_IDDSV3_GetSurfaceDesc(
  LPDIRECTDRAWSURFACE3 lpThis,
  LPDDSURFACEDESC lpDDSurfaceDesc  
);
HRESULT WINAPI My_IDDSV3_Initialize(
  LPDIRECTDRAWSURFACE3 lpThis,
  LPDIRECTDRAW lpDD,               
  LPDDSURFACEDESC lpDDSurfaceDesc  
);
HRESULT WINAPI My_IDDSV3_IsLost(
  LPDIRECTDRAWSURFACE3 lpThis
);
HRESULT WINAPI My_IDDSV3_Lock(
  LPDIRECTDRAWSURFACE3 lpThis,
  LPRECT lpDestRect,                
  LPDDSURFACEDESC lpDDSurfaceDesc,  
  DWORD dwFlags,                    
  HANDLE hEvent                     
);
HRESULT WINAPI My_IDDSV3_ReleaseDC(
  LPDIRECTDRAWSURFACE3 lpThis,
  HDC hDC  
);
HRESULT WINAPI My_IDDSV3_Restore(
  LPDIRECTDRAWSURFACE3 lpThis
);
HRESULT WINAPI My_IDDSV3_SetClipper(
  LPDIRECTDRAWSURFACE3 lpThis,
  LPDIRECTDRAWCLIPPER lpDDClipper  
);
HRESULT WINAPI My_IDDSV3_SetColorKey(
  LPDIRECTDRAWSURFACE3 lpThis,
  DWORD dwFlags,             
  LPDDCOLORKEY lpDDColorKey  
);
HRESULT WINAPI My_IDDSV3_SetOverlayPosition(
  LPDIRECTDRAWSURFACE3 lpThis,
  LONG lX, 
  LONG lY  
);
HRESULT WINAPI My_IDDSV3_SetPalette(
  LPDIRECTDRAWSURFACE3 lpThis,
  LPDIRECTDRAWPALETTE lpDDPalette  
);
HRESULT WINAPI My_IDDSV3_Unlock(
  LPDIRECTDRAWSURFACE3 lpThis,
  LPVOID lpRect 
);
HRESULT WINAPI My_IDDSV3_UpdateOverlay(
  LPDIRECTDRAWSURFACE3 lpThis,
  LPRECT lpSrcRect,                      
  LPDIRECTDRAWSURFACE3 lpDDDestSurface,  
  LPRECT lpDestRect,                     
  DWORD dwFlags,                         
  LPDDOVERLAYFX lpDDOverlayFx            
);
HRESULT WINAPI My_IDDSV3_UpdateOverlayDisplay(
  LPDIRECTDRAWSURFACE3 lpThis,
  DWORD dwFlags  
);
HRESULT WINAPI My_IDDSV3_UpdateOverlayZOrder(
  LPDIRECTDRAWSURFACE3 lpThis,
  DWORD dwFlags,                       
  LPDIRECTDRAWSURFACE3 lpDDSReference  
);
// IDirectDrawSurface2 functions
HRESULT WINAPI My_IDDSV3_GetDDInterface(
  LPDIRECTDRAWSURFACE3 lpThis,
  LPVOID FAR *lplpDD  
);
HRESULT WINAPI My_IDDSV3_PageLock(
  LPDIRECTDRAWSURFACE3 lpThis,
  DWORD dwFlags  
);
HRESULT WINAPI My_IDDSV3_PageUnlock(
  LPDIRECTDRAWSURFACE3 lpThis,
  DWORD dwFlags  
);

// IDirectDrawSurface3 functions
HRESULT WINAPI My_IDDSV3_SetSurfaceDesc(
  LPDIRECTDRAWSURFACE3 lpThis,
  LPDDSURFACEDESC lpddsd,  
  DWORD dwFlags            
);

/**************************************************************************
DETOURS SETUP AND TEARDOWN FUNCTIONS
**************************************************************************/
void HookDirectDrawSurface3Interface  (LPDIRECTDRAWSURFACE3 lpInterface);

