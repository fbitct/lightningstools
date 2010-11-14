#pragma once
/**************************************************************************
TYPEDEFS FOR STRONGLY-TYPED FUNCTION POINTERS TO IDIRECTDRAWSURFACE (V1) METHODS
**************************************************************************/

// IUnknown functions
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV1_QueryInterface) (LPUNKNOWN lpThis, REFIID riid, LPVOID* obp);
typedef ULONG   (WINAPI* TD_IDirectDrawSurfaceV1_AddRef)(LPUNKNOWN lpThis);
typedef ULONG   (WINAPI* TD_IDirectDrawSurfaceV1_Release)(LPUNKNOWN lpThis);

// IDirectDrawSurface (V1) functions
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV1_AddAttachedSurface)( 
	LPDIRECTDRAWSURFACE lpThis,
	LPDIRECTDRAWSURFACE lpDDSAttachedSurface  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV1_AddOverlayDirtyRect)(
  LPDIRECTDRAWSURFACE lpThis,
  LPRECT lpRect  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV1_Blt)(
  LPDIRECTDRAWSURFACE lpThis,
  LPRECT lpDestRect,                    
  LPDIRECTDRAWSURFACE lpDDSrcSurface,  
  LPRECT lpSrcRect,                     
  DWORD dwFlags,                        
  LPDDBLTFX lpDDBltFx                   
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV1_BltBatch)(
  LPDIRECTDRAWSURFACE lpThis,
  LPDDBLTBATCH lpDDBltBatch,  
  DWORD dwCount,              
  DWORD dwFlags               
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV1_BltFast)(
  LPDIRECTDRAWSURFACE lpThis,
  DWORD dwX,                            
  DWORD dwY,                            
  LPDIRECTDRAWSURFACE lpDDSrcSurface,  
  LPRECT lpSrcRect,                     
  DWORD dwTrans                         
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV1_DeleteAttachedSurface)(
  LPDIRECTDRAWSURFACE lpThis,
  DWORD dwFlags,                             
  LPDIRECTDRAWSURFACE lpDDSAttachedSurface  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV1_EnumAttachedSurfaces)(
  LPDIRECTDRAWSURFACE lpThis,
  LPVOID lpContext,                                
  LPDDENUMSURFACESCALLBACK lpEnumSurfacesCallback  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV1_EnumOverlayZOrders)(
  LPDIRECTDRAWSURFACE lpThis,
  DWORD dwFlags,                          
  LPVOID lpContext,                       
  LPDDENUMSURFACESCALLBACK lpfnCallback  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV1_Flip)(
  LPDIRECTDRAWSURFACE lpThis,
  LPDIRECTDRAWSURFACE lpDDSurfaceTargetOverride,  
  DWORD dwFlags                                    
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV1_GetAttachedSurface)(
  LPDIRECTDRAWSURFACE lpThis,
  LPDDSCAPS lpDDSCaps, 
  LPDIRECTDRAWSURFACE FAR *lplpDDAttachedSurface  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV1_GetBltStatus)(
  LPDIRECTDRAWSURFACE lpThis,
  DWORD dwFlags  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV1_GetCaps)(
  LPDIRECTDRAWSURFACE lpThis,
  LPDDSCAPS lpDDSCaps  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV1_GetClipper)(
  LPDIRECTDRAWSURFACE lpThis,
  LPDIRECTDRAWCLIPPER FAR *lplpDDClipper  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV1_GetColorKey)(
  LPDIRECTDRAWSURFACE lpThis,
  DWORD dwFlags,             
  LPDDCOLORKEY lpDDColorKey  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV1_GetDC)(
  LPDIRECTDRAWSURFACE lpThis,
  HDC FAR *lphDC  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV1_GetFlipStatus)(
  LPDIRECTDRAWSURFACE lpThis,
  DWORD dwFlags  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV1_GetOverlayPosition)(
  LPDIRECTDRAWSURFACE lpThis,
  LPLONG lplX, 
  LPLONG lplY  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV1_GetPalette)(
  LPDIRECTDRAWSURFACE lpThis,
  LPDIRECTDRAWPALETTE FAR *lplpDDPalette  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV1_GetPixelFormat)(
  LPDIRECTDRAWSURFACE lpThis,
  LPDDPIXELFORMAT lpDDPixelFormat  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV1_GetSurfaceDesc)(
  LPDIRECTDRAWSURFACE lpThis,
  LPDDSURFACEDESC lpDDSurfaceDesc  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV1_Initialize)(
  LPDIRECTDRAWSURFACE lpThis,
  LPDIRECTDRAW lpDD,               
  LPDDSURFACEDESC lpDDSurfaceDesc  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV1_IsLost)(LPDIRECTDRAWSURFACE lpThis);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV1_Lock)(
  LPDIRECTDRAWSURFACE lpThis,
  LPRECT lpDestRect,                
  LPDDSURFACEDESC lpDDSurfaceDesc,  
  DWORD dwFlags,                    
  HANDLE hEvent                     
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV1_ReleaseDC)(
  LPDIRECTDRAWSURFACE lpThis,
  HDC hDC  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV1_Restore)(LPDIRECTDRAWSURFACE lpThis);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV1_SetClipper)(
  LPDIRECTDRAWSURFACE lpThis,
  LPDIRECTDRAWCLIPPER lpDDClipper  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV1_SetColorKey)(
  LPDIRECTDRAWSURFACE lpThis,
  DWORD dwFlags,             
  LPDDCOLORKEY lpDDColorKey  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV1_SetOverlayPosition)(
  LPDIRECTDRAWSURFACE lpThis,
  LONG lX, 
  LONG lY  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV1_SetPalette)(
  LPDIRECTDRAWSURFACE lpThis,
  LPDIRECTDRAWPALETTE lpDDPalette  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV1_Unlock)(
  LPDIRECTDRAWSURFACE lpThis,
  LPVOID lpRect 
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV1_UpdateOverlay)(
  LPDIRECTDRAWSURFACE lpThis,
  LPRECT lpSrcRect,                      
  LPDIRECTDRAWSURFACE lpDDDestSurface,  
  LPRECT lpDestRect,                     
  DWORD dwFlags,                         
  LPDDOVERLAYFX lpDDOverlayFx            
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV1_UpdateOverlayDisplay)(
  LPDIRECTDRAWSURFACE lpThis,
  DWORD dwFlags  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV1_UpdateOverlayZOrder)(
  LPDIRECTDRAWSURFACE lpThis,
  DWORD dwFlags,                       
  LPDIRECTDRAWSURFACE lpDDSReference  
);

/**************************************************************************
STATIC STRONGLY-TYPED FUNCTION POINTERS TO 
ORIGINAL AND REPLACEMENT IDIRECTDRAWSURFACE (V1) METHODS
**************************************************************************/
static TD_IDirectDrawSurfaceV1_QueryInterface ORIG_IDirectDrawSurfaceV1_QueryInterface=NULL;
static TD_IDirectDrawSurfaceV1_QueryInterface NEW_IDirectDrawSurfaceV1_QueryInterface=NULL;
static TD_IDirectDrawSurfaceV1_AddRef ORIG_IDirectDrawSurfaceV1_AddRef=NULL;
static TD_IDirectDrawSurfaceV1_AddRef NEW_IDirectDrawSurfaceV1_AddRef=NULL;
static TD_IDirectDrawSurfaceV1_Release ORIG_IDirectDrawSurfaceV1_Release=NULL;
static TD_IDirectDrawSurfaceV1_Release NEW_IDirectDrawSurfaceV1_Release=NULL;
static TD_IDirectDrawSurfaceV1_AddAttachedSurface ORIG_IDirectDrawSurfaceV1_AddAttachedSurface=NULL;
static TD_IDirectDrawSurfaceV1_AddAttachedSurface NEW_IDirectDrawSurfaceV1_AddAttachedSurface=NULL;
static TD_IDirectDrawSurfaceV1_AddOverlayDirtyRect ORIG_IDirectDrawSurfaceV1_AddOverlayDirtyRect=NULL;
static TD_IDirectDrawSurfaceV1_AddOverlayDirtyRect NEW_IDirectDrawSurfaceV1_AddOverlayDirtyRect=NULL;
static TD_IDirectDrawSurfaceV1_Blt ORIG_IDirectDrawSurfaceV1_Blt=NULL;
static TD_IDirectDrawSurfaceV1_Blt NEW_IDirectDrawSurfaceV1_Blt=NULL;
static TD_IDirectDrawSurfaceV1_BltBatch ORIG_IDirectDrawSurfaceV1_BltBatch=NULL;
static TD_IDirectDrawSurfaceV1_BltBatch NEW_IDirectDrawSurfaceV1_BltBatch=NULL;
static TD_IDirectDrawSurfaceV1_BltFast ORIG_IDirectDrawSurfaceV1_BltFast=NULL;
static TD_IDirectDrawSurfaceV1_BltFast NEW_IDirectDrawSurfaceV1_BltFast=NULL;
static TD_IDirectDrawSurfaceV1_DeleteAttachedSurface ORIG_IDirectDrawSurfaceV1_DeleteAttachedSurface=NULL;
static TD_IDirectDrawSurfaceV1_DeleteAttachedSurface NEW_IDirectDrawSurfaceV1_DeleteAttachedSurface=NULL;
static TD_IDirectDrawSurfaceV1_EnumAttachedSurfaces ORIG_IDirectDrawSurfaceV1_EnumAttachedSurfaces=NULL;
static TD_IDirectDrawSurfaceV1_EnumAttachedSurfaces NEW_IDirectDrawSurfaceV1_EnumAttachedSurfaces=NULL;
static TD_IDirectDrawSurfaceV1_EnumOverlayZOrders ORIG_IDirectDrawSurfaceV1_EnumOverlayZOrders=NULL;
static TD_IDirectDrawSurfaceV1_EnumOverlayZOrders NEW_IDirectDrawSurfaceV1_EnumOverlayZOrders=NULL;
static TD_IDirectDrawSurfaceV1_Flip ORIG_IDirectDrawSurfaceV1_Flip=NULL;
static TD_IDirectDrawSurfaceV1_Flip NEW_IDirectDrawSurfaceV1_Flip=NULL;
static TD_IDirectDrawSurfaceV1_GetAttachedSurface ORIG_IDirectDrawSurfaceV1_GetAttachedSurface=NULL;
static TD_IDirectDrawSurfaceV1_GetAttachedSurface NEW_IDirectDrawSurfaceV1_GetAttachedSurface=NULL;
static TD_IDirectDrawSurfaceV1_GetBltStatus ORIG_IDirectDrawSurfaceV1_GetBltStatus=NULL;
static TD_IDirectDrawSurfaceV1_GetBltStatus NEW_IDirectDrawSurfaceV1_GetBltStatus=NULL;
static TD_IDirectDrawSurfaceV1_GetCaps ORIG_IDirectDrawSurfaceV1_GetCaps=NULL;
static TD_IDirectDrawSurfaceV1_GetCaps NEW_IDirectDrawSurfaceV1_GetCaps=NULL;
static TD_IDirectDrawSurfaceV1_GetClipper ORIG_IDirectDrawSurfaceV1_GetClipper=NULL;
static TD_IDirectDrawSurfaceV1_GetClipper NEW_IDirectDrawSurfaceV1_GetClipper=NULL;
static TD_IDirectDrawSurfaceV1_GetColorKey ORIG_IDirectDrawSurfaceV1_GetColorKey=NULL;
static TD_IDirectDrawSurfaceV1_GetColorKey NEW_IDirectDrawSurfaceV1_GetColorKey=NULL;
static TD_IDirectDrawSurfaceV1_GetDC ORIG_IDirectDrawSurfaceV1_GetDC=NULL;
static TD_IDirectDrawSurfaceV1_GetDC NEW_IDirectDrawSurfaceV1_GetDC=NULL;
static TD_IDirectDrawSurfaceV1_GetFlipStatus ORIG_IDirectDrawSurfaceV1_GetFlipStatus=NULL;
static TD_IDirectDrawSurfaceV1_GetFlipStatus NEW_IDirectDrawSurfaceV1_GetFlipStatus=NULL;
static TD_IDirectDrawSurfaceV1_GetOverlayPosition ORIG_IDirectDrawSurfaceV1_GetOverlayPosition=NULL;
static TD_IDirectDrawSurfaceV1_GetOverlayPosition NEW_IDirectDrawSurfaceV1_GetOverlayPosition=NULL;
static TD_IDirectDrawSurfaceV1_GetPalette ORIG_IDirectDrawSurfaceV1_GetPalette=NULL;
static TD_IDirectDrawSurfaceV1_GetPalette NEW_IDirectDrawSurfaceV1_GetPalette=NULL;
static TD_IDirectDrawSurfaceV1_GetPixelFormat ORIG_IDirectDrawSurfaceV1_GetPixelFormat=NULL;
static TD_IDirectDrawSurfaceV1_GetPixelFormat NEW_IDirectDrawSurfaceV1_GetPixelFormat=NULL;
static TD_IDirectDrawSurfaceV1_GetSurfaceDesc ORIG_IDirectDrawSurfaceV1_GetSurfaceDesc=NULL;
static TD_IDirectDrawSurfaceV1_GetSurfaceDesc NEW_IDirectDrawSurfaceV1_GetSurfaceDesc=NULL;
static TD_IDirectDrawSurfaceV1_Initialize ORIG_IDirectDrawSurfaceV1_Initialize=NULL;
static TD_IDirectDrawSurfaceV1_Initialize NEW_IDirectDrawSurfaceV1_Initialize=NULL;
static TD_IDirectDrawSurfaceV1_IsLost ORIG_IDirectDrawSurfaceV1_IsLost=NULL;
static TD_IDirectDrawSurfaceV1_IsLost NEW_IDirectDrawSurfaceV1_IsLost=NULL;
static TD_IDirectDrawSurfaceV1_Lock ORIG_IDirectDrawSurfaceV1_Lock=NULL;
static TD_IDirectDrawSurfaceV1_Lock NEW_IDirectDrawSurfaceV1_Lock=NULL;
static TD_IDirectDrawSurfaceV1_ReleaseDC ORIG_IDirectDrawSurfaceV1_ReleaseDC=NULL;
static TD_IDirectDrawSurfaceV1_ReleaseDC NEW_IDirectDrawSurfaceV1_ReleaseDC=NULL;
static TD_IDirectDrawSurfaceV1_Restore ORIG_IDirectDrawSurfaceV1_Restore=NULL;
static TD_IDirectDrawSurfaceV1_Restore NEW_IDirectDrawSurfaceV1_Restore=NULL;
static TD_IDirectDrawSurfaceV1_SetClipper ORIG_IDirectDrawSurfaceV1_SetClipper=NULL;
static TD_IDirectDrawSurfaceV1_SetClipper NEW_IDirectDrawSurfaceV1_SetClipper=NULL;
static TD_IDirectDrawSurfaceV1_SetColorKey ORIG_IDirectDrawSurfaceV1_SetColorKey=NULL;
static TD_IDirectDrawSurfaceV1_SetColorKey NEW_IDirectDrawSurfaceV1_SetColorKey=NULL;
static TD_IDirectDrawSurfaceV1_SetOverlayPosition ORIG_IDirectDrawSurfaceV1_SetOverlayPosition=NULL;
static TD_IDirectDrawSurfaceV1_SetOverlayPosition NEW_IDirectDrawSurfaceV1_SetOverlayPosition=NULL;
static TD_IDirectDrawSurfaceV1_SetPalette ORIG_IDirectDrawSurfaceV1_SetPalette=NULL;
static TD_IDirectDrawSurfaceV1_SetPalette NEW_IDirectDrawSurfaceV1_SetPalette=NULL;
static TD_IDirectDrawSurfaceV1_Unlock ORIG_IDirectDrawSurfaceV1_Unlock=NULL;
static TD_IDirectDrawSurfaceV1_Unlock NEW_IDirectDrawSurfaceV1_Unlock=NULL;
static TD_IDirectDrawSurfaceV1_UpdateOverlay ORIG_IDirectDrawSurfaceV1_UpdateOverlay=NULL;
static TD_IDirectDrawSurfaceV1_UpdateOverlay NEW_IDirectDrawSurfaceV1_UpdateOverlay=NULL;
static TD_IDirectDrawSurfaceV1_UpdateOverlayDisplay NEW_IDirectDrawSurfaceV1_UpdateOverlayDisplay=NULL;
static TD_IDirectDrawSurfaceV1_UpdateOverlayDisplay ORIG_IDirectDrawSurfaceV1_UpdateOverlayDisplay=NULL;
static TD_IDirectDrawSurfaceV1_UpdateOverlayZOrder ORIG_IDirectDrawSurfaceV1_UpdateOverlayZOrder=NULL;
static TD_IDirectDrawSurfaceV1_UpdateOverlayZOrder NEW_IDirectDrawSurfaceV1_UpdateOverlayZOrder=NULL;


/**************************************************************************
REPLACEMENT IMPLEMENTATION FOR IDIRECTDRAWSURFACE (V1) METHODS
**************************************************************************/

// IUnknown functions
HRESULT WINAPI My_IDDSV1_QueryInterface (LPUNKNOWN lpThis, REFIID riid, LPVOID* obp);
ULONG   WINAPI My_IDDSV1_AddRef(LPUNKNOWN lpThis);
ULONG   WINAPI My_IDDSV1_Release(LPUNKNOWN lpThis);
// IDirectDrawSurface (V1) functions
HRESULT WINAPI My_IDDSV1_AddAttachedSurface(
  LPDIRECTDRAWSURFACE lpThis,
  LPDIRECTDRAWSURFACE lpDDSAttachedSurface  
);
HRESULT WINAPI My_IDDSV1_AddOverlayDirtyRect(
  LPDIRECTDRAWSURFACE lpThis,
  LPRECT lpRect  
);
HRESULT WINAPI My_IDDSV1_Blt(
  LPDIRECTDRAWSURFACE lpThis,
  LPRECT lpDestRect,                    
  LPDIRECTDRAWSURFACE lpDDSrcSurface,  
  LPRECT lpSrcRect,                     
  DWORD dwFlags,                        
  LPDDBLTFX lpDDBltFx                   
);
HRESULT WINAPI My_IDDSV1_BltBatch(
  LPDIRECTDRAWSURFACE lpThis,
  LPDDBLTBATCH lpDDBltBatch,  
  DWORD dwCount,              
  DWORD dwFlags               
);
HRESULT WINAPI My_IDDSV1_BltFast(
  LPDIRECTDRAWSURFACE lpThis,
  DWORD dwX,                            
  DWORD dwY,                            
  LPDIRECTDRAWSURFACE lpDDSrcSurface,  
  LPRECT lpSrcRect,                     
  DWORD dwTrans                         
);
HRESULT WINAPI My_IDDSV1_DeleteAttachedSurface(
  LPDIRECTDRAWSURFACE lpThis,
  DWORD dwFlags,                             
  LPDIRECTDRAWSURFACE lpDDSAttachedSurface  
);
HRESULT WINAPI My_IDDSV1_EnumAttachedSurfaces(
  LPDIRECTDRAWSURFACE lpThis,
  LPVOID lpContext,                                
  LPDDENUMSURFACESCALLBACK lpEnumSurfacesCallback  
);
HRESULT WINAPI My_IDDSV1_EnumOverlayZOrders(
  LPDIRECTDRAWSURFACE lpThis,
  DWORD dwFlags,                          
  LPVOID lpContext,                       
  LPDDENUMSURFACESCALLBACK lpfnCallback  
);
HRESULT WINAPI My_IDDSV1_Flip(
  LPDIRECTDRAWSURFACE lpThis,
  LPDIRECTDRAWSURFACE lpDDSurfaceTargetOverride,  
  DWORD dwFlags                                    
);
HRESULT WINAPI My_IDDSV1_GetAttachedSurface(
  LPDIRECTDRAWSURFACE lpThis,
  LPDDSCAPS lpDDSCaps, 
  LPDIRECTDRAWSURFACE FAR *lplpDDAttachedSurface  
);
HRESULT WINAPI My_IDDSV1_GetBltStatus(
  LPDIRECTDRAWSURFACE lpThis,
  DWORD dwFlags  
);
HRESULT WINAPI My_IDDSV1_GetCaps(
  LPDIRECTDRAWSURFACE lpThis,
  LPDDSCAPS lpDDSCaps  
);
HRESULT WINAPI My_IDDSV1_GetClipper(
  LPDIRECTDRAWSURFACE lpThis,
  LPDIRECTDRAWCLIPPER FAR *lplpDDClipper  
);
HRESULT WINAPI My_IDDSV1_GetColorKey(
  LPDIRECTDRAWSURFACE lpThis,
  DWORD dwFlags,             
  LPDDCOLORKEY lpDDColorKey  
);
HRESULT WINAPI My_IDDSV1_GetDC(
  LPDIRECTDRAWSURFACE lpThis,
  HDC FAR *lphDC  
);
HRESULT WINAPI My_IDDSV1_GetFlipStatus(
  LPDIRECTDRAWSURFACE lpThis,
  DWORD dwFlags  
);
HRESULT WINAPI My_IDDSV1_GetOverlayPosition(
  LPDIRECTDRAWSURFACE lpThis,
  LPLONG lplX, 
  LPLONG lplY  
);
HRESULT WINAPI My_IDDSV1_GetPalette(
  LPDIRECTDRAWSURFACE lpThis,
  LPDIRECTDRAWPALETTE FAR *lplpDDPalette  
);

HRESULT WINAPI My_IDDSV1_GetPixelFormat(
  LPDIRECTDRAWSURFACE lpThis,
  LPDDPIXELFORMAT lpDDPixelFormat  
);
HRESULT WINAPI My_IDDSV1_GetSurfaceDesc(
  LPDIRECTDRAWSURFACE lpThis,
  LPDDSURFACEDESC lpDDSurfaceDesc  
);
HRESULT WINAPI My_IDDSV1_Initialize(
  LPDIRECTDRAWSURFACE lpThis,
  LPDIRECTDRAW lpDD,               
  LPDDSURFACEDESC lpDDSurfaceDesc  
);
HRESULT WINAPI My_IDDSV1_IsLost(
  LPDIRECTDRAWSURFACE lpThis
);
HRESULT WINAPI My_IDDSV1_Lock(
  LPDIRECTDRAWSURFACE lpThis,
  LPRECT lpDestRect,                
  LPDDSURFACEDESC lpDDSurfaceDesc,  
  DWORD dwFlags,                    
  HANDLE hEvent                     
);
HRESULT WINAPI My_IDDSV1_ReleaseDC(
  LPDIRECTDRAWSURFACE lpThis,
  HDC hDC  
);
HRESULT WINAPI My_IDDSV1_Restore(
  LPDIRECTDRAWSURFACE lpThis
);
HRESULT WINAPI My_IDDSV1_SetClipper(
  LPDIRECTDRAWSURFACE lpThis,
  LPDIRECTDRAWCLIPPER lpDDClipper  
);
HRESULT WINAPI My_IDDSV1_SetColorKey(
  LPDIRECTDRAWSURFACE lpThis,
  DWORD dwFlags,             
  LPDDCOLORKEY lpDDColorKey  
);
HRESULT WINAPI My_IDDSV1_SetOverlayPosition(
  LPDIRECTDRAWSURFACE lpThis,
  LONG lX, 
  LONG lY  
);
HRESULT WINAPI My_IDDSV1_SetPalette(
  LPDIRECTDRAWSURFACE lpThis,
  LPDIRECTDRAWPALETTE lpDDPalette  
);
HRESULT WINAPI My_IDDSV1_Unlock(
  LPDIRECTDRAWSURFACE lpThis,
  LPVOID lpRect 
);
HRESULT WINAPI My_IDDSV1_UpdateOverlay(
  LPDIRECTDRAWSURFACE lpThis,
  LPRECT lpSrcRect,                      
  LPDIRECTDRAWSURFACE lpDDDestSurface,  
  LPRECT lpDestRect,                     
  DWORD dwFlags,                         
  LPDDOVERLAYFX lpDDOverlayFx            
);
HRESULT WINAPI My_IDDSV1_UpdateOverlayDisplay(
  LPDIRECTDRAWSURFACE lpThis,
  DWORD dwFlags  
);
HRESULT WINAPI My_IDDSV1_UpdateOverlayZOrder(
  LPDIRECTDRAWSURFACE lpThis,
  DWORD dwFlags,                       
  LPDIRECTDRAWSURFACE lpDDSReference  
);

/**************************************************************************
DETOURS SETUP AND TEARDOWN FUNCTIONS
**************************************************************************/
void HookDirectDrawSurface1Interface  (LPDIRECTDRAWSURFACE lpInterface);

