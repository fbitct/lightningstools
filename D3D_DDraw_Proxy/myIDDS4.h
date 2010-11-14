#pragma once
/**************************************************************************
TYPEDEFS FOR STRONGLY-TYPED FUNCTION POINTERS TO IDIRECTDRAWSURFACE (V4) METHODS
**************************************************************************/

// IUnknown functions
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV4_QueryInterface) (LPUNKNOWN lpThis, REFIID riid, LPVOID* obp);
typedef ULONG   (WINAPI* TD_IDirectDrawSurfaceV4_AddRef)(LPUNKNOWN lpThis);
typedef ULONG   (WINAPI* TD_IDirectDrawSurfaceV4_Release)(LPUNKNOWN lpThis);

// IDirectDrawSurface (V1) functions
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV4_AddAttachedSurface)( 
	LPDIRECTDRAWSURFACE4 lpThis,
	LPDIRECTDRAWSURFACE4 lpDDSAttachedSurface  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV4_AddOverlayDirtyRect)(
  LPDIRECTDRAWSURFACE4 lpThis,
  LPRECT lpRect  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV4_Blt)(
  LPDIRECTDRAWSURFACE4 lpThis,
  LPRECT lpDestRect,                    
  LPDIRECTDRAWSURFACE4 lpDDSrcSurface,  
  LPRECT lpSrcRect,                     
  DWORD dwFlags,                        
  LPDDBLTFX lpDDBltFx                   
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV4_BltBatch)(
  LPDIRECTDRAWSURFACE4 lpThis,
  LPDDBLTBATCH lpDDBltBatch,  
  DWORD dwCount,              
  DWORD dwFlags               
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV4_BltFast)(
  LPDIRECTDRAWSURFACE4 lpThis,
  DWORD dwX,                            
  DWORD dwY,                            
  LPDIRECTDRAWSURFACE4 lpDDSrcSurface,  
  LPRECT lpSrcRect,                     
  DWORD dwTrans                         
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV4_DeleteAttachedSurface)(
  LPDIRECTDRAWSURFACE4 lpThis,
  DWORD dwFlags,                             
  LPDIRECTDRAWSURFACE4 lpDDSAttachedSurface  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV4_EnumAttachedSurfaces)(
  LPDIRECTDRAWSURFACE4 lpThis,
  LPVOID lpContext,                                
  LPDDENUMSURFACESCALLBACK2 lpEnumSurfacesCallback  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV4_EnumOverlayZOrders)(
  LPDIRECTDRAWSURFACE4 lpThis,
  DWORD dwFlags,                          
  LPVOID lpContext,                       
  LPDDENUMSURFACESCALLBACK2 lpfnCallback  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV4_Flip)(
  LPDIRECTDRAWSURFACE4 lpThis,
  LPDIRECTDRAWSURFACE4 lpDDSurfaceTargetOverride,  
  DWORD dwFlags                                    
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV4_GetAttachedSurface)(
  LPDIRECTDRAWSURFACE4 lpThis,
  LPDDSCAPS2 LPDDSCAPS2, 
  LPDIRECTDRAWSURFACE4 FAR *lplpDDAttachedSurface  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV4_GetBltStatus)(
  LPDIRECTDRAWSURFACE4 lpThis,
  DWORD dwFlags  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV4_GetCaps)(
  LPDIRECTDRAWSURFACE4 lpThis,
  LPDDSCAPS2 LPDDSCAPS2  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV4_GetClipper)(
  LPDIRECTDRAWSURFACE4 lpThis,
  LPDIRECTDRAWCLIPPER FAR *lplpDDClipper  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV4_GetColorKey)(
  LPDIRECTDRAWSURFACE4 lpThis,
  DWORD dwFlags,             
  LPDDCOLORKEY lpDDColorKey  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV4_GetDC)(
  LPDIRECTDRAWSURFACE4 lpThis,
  HDC FAR *lphDC  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV4_GetFlipStatus)(
  LPDIRECTDRAWSURFACE4 lpThis,
  DWORD dwFlags  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV4_GetOverlayPosition)(
  LPDIRECTDRAWSURFACE4 lpThis,
  LPLONG lplX, 
  LPLONG lplY  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV4_GetPalette)(
  LPDIRECTDRAWSURFACE4 lpThis,
  LPDIRECTDRAWPALETTE FAR *lplpDDPalette  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV4_GetPixelFormat)(
  LPDIRECTDRAWSURFACE4 lpThis,
  LPDDPIXELFORMAT lpDDPixelFormat  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV4_GetSurfaceDesc)(
  LPDIRECTDRAWSURFACE4 lpThis,
  LPDDSURFACEDESC2 LPDDSURFACEDESC2  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV4_Initialize)(
  LPDIRECTDRAWSURFACE4 lpThis,
  LPDIRECTDRAW lpDD,               
  LPDDSURFACEDESC2 LPDDSURFACEDESC2  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV4_IsLost)(LPDIRECTDRAWSURFACE4 lpThis);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV4_Lock)(
  LPDIRECTDRAWSURFACE4 lpThis,
  LPRECT lpDestRect,                
  LPDDSURFACEDESC2 LPDDSURFACEDESC2,  
  DWORD dwFlags,                    
  HANDLE hEvent                     
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV4_ReleaseDC)(
  LPDIRECTDRAWSURFACE4 lpThis,
  HDC hDC  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV4_Restore)(LPDIRECTDRAWSURFACE4 lpThis);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV4_SetClipper)(
  LPDIRECTDRAWSURFACE4 lpThis,
  LPDIRECTDRAWCLIPPER lpDDClipper  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV4_SetColorKey)(
  LPDIRECTDRAWSURFACE4 lpThis,
  DWORD dwFlags,             
  LPDDCOLORKEY lpDDColorKey  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV4_SetOverlayPosition)(
  LPDIRECTDRAWSURFACE4 lpThis,
  LONG lX, 
  LONG lY  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV4_SetPalette)(
  LPDIRECTDRAWSURFACE4 lpThis,
  LPDIRECTDRAWPALETTE lpDDPalette  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV4_Unlock)(
  LPDIRECTDRAWSURFACE4 lpThis,
  LPVOID lpRect 
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV4_UpdateOverlay)(
  LPDIRECTDRAWSURFACE4 lpThis,
  LPRECT lpSrcRect,                      
  LPDIRECTDRAWSURFACE4 lpDDDestSurface,  
  LPRECT lpDestRect,                     
  DWORD dwFlags,                         
  LPDDOVERLAYFX lpDDOverlayFx            
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV4_UpdateOverlayDisplay)(
  LPDIRECTDRAWSURFACE4 lpThis,
  DWORD dwFlags  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV4_UpdateOverlayZOrder)(
  LPDIRECTDRAWSURFACE4 lpThis,
  DWORD dwFlags,                       
  LPDIRECTDRAWSURFACE4 lpDDSReference  
);
// IDirectDrawSurface2 functions
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV4_GetDDInterface)(
  LPDIRECTDRAWSURFACE4 lpThis,
  LPVOID FAR *lplpDD  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV4_PageLock)(
  LPDIRECTDRAWSURFACE4 lpThis,
  DWORD dwFlags  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV4_PageUnlock)(
  LPDIRECTDRAWSURFACE4 lpThis,
  DWORD dwFlags  
);

// IDirectDrawSurface3  functions
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV4_SetSurfaceDesc)(
  LPDIRECTDRAWSURFACE4 lpThis,
  LPDDSURFACEDESC2 lpddsd,  
  DWORD dwFlags            
);
// IDirectDrawSurface4  functions
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV4_SetPrivateData)( 
  LPDIRECTDRAWSURFACE4 lpThis,
  REFGUID guidTag, 
  LPVOID  lpData,
  DWORD   cbSize,
  DWORD   dwFlags 
); 
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV4_GetPrivateData)(
  LPDIRECTDRAWSURFACE4 lpThis,
  REFGUID guidTag,
  LPVOID  lpBuffer,
  LPDWORD lpcbBufferSize
); 
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV4_FreePrivateData)( 
  LPDIRECTDRAWSURFACE4 lpThis,
  REFGUID guidTag 
); 
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV4_GetUniquenessValue)( 
  LPDIRECTDRAWSURFACE4 lpThis,
  LPDWORD lpValue 
); 
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV4_ChangeUniquenessValue)(
  LPDIRECTDRAWSURFACE4 lpThis
); 



/**************************************************************************
STATIC STRONGLY-TYPED FUNCTION POINTERS TO 
ORIGINAL AND REPLACEMENT IDIRECTDRAWSURFACE (V4) METHODS
**************************************************************************/
static TD_IDirectDrawSurfaceV4_QueryInterface ORIG_IDirectDrawSurfaceV4_QueryInterface=NULL;
static TD_IDirectDrawSurfaceV4_QueryInterface NEW_IDirectDrawSurfaceV4_QueryInterface=NULL;
static TD_IDirectDrawSurfaceV4_AddRef ORIG_IDirectDrawSurfaceV4_AddRef=NULL;
static TD_IDirectDrawSurfaceV4_AddRef NEW_IDirectDrawSurfaceV4_AddRef=NULL;
static TD_IDirectDrawSurfaceV4_Release ORIG_IDirectDrawSurfaceV4_Release=NULL;
static TD_IDirectDrawSurfaceV4_Release NEW_IDirectDrawSurfaceV4_Release=NULL;
static TD_IDirectDrawSurfaceV4_AddAttachedSurface ORIG_IDirectDrawSurfaceV4_AddAttachedSurface=NULL;
static TD_IDirectDrawSurfaceV4_AddAttachedSurface NEW_IDirectDrawSurfaceV4_AddAttachedSurface=NULL;
static TD_IDirectDrawSurfaceV4_AddOverlayDirtyRect ORIG_IDirectDrawSurfaceV4_AddOverlayDirtyRect=NULL;
static TD_IDirectDrawSurfaceV4_AddOverlayDirtyRect NEW_IDirectDrawSurfaceV4_AddOverlayDirtyRect=NULL;
static TD_IDirectDrawSurfaceV4_Blt ORIG_IDirectDrawSurfaceV4_Blt=NULL;
static TD_IDirectDrawSurfaceV4_Blt NEW_IDirectDrawSurfaceV4_Blt=NULL;
static TD_IDirectDrawSurfaceV4_BltBatch ORIG_IDirectDrawSurfaceV4_BltBatch=NULL;
static TD_IDirectDrawSurfaceV4_BltBatch NEW_IDirectDrawSurfaceV4_BltBatch=NULL;
static TD_IDirectDrawSurfaceV4_BltFast ORIG_IDirectDrawSurfaceV4_BltFast=NULL;
static TD_IDirectDrawSurfaceV4_BltFast NEW_IDirectDrawSurfaceV4_BltFast=NULL;
static TD_IDirectDrawSurfaceV4_DeleteAttachedSurface ORIG_IDirectDrawSurfaceV4_DeleteAttachedSurface=NULL;
static TD_IDirectDrawSurfaceV4_DeleteAttachedSurface NEW_IDirectDrawSurfaceV4_DeleteAttachedSurface=NULL;
static TD_IDirectDrawSurfaceV4_EnumAttachedSurfaces ORIG_IDirectDrawSurfaceV4_EnumAttachedSurfaces=NULL;
static TD_IDirectDrawSurfaceV4_EnumAttachedSurfaces NEW_IDirectDrawSurfaceV4_EnumAttachedSurfaces=NULL;
static TD_IDirectDrawSurfaceV4_EnumOverlayZOrders ORIG_IDirectDrawSurfaceV4_EnumOverlayZOrders=NULL;
static TD_IDirectDrawSurfaceV4_EnumOverlayZOrders NEW_IDirectDrawSurfaceV4_EnumOverlayZOrders=NULL;
static TD_IDirectDrawSurfaceV4_Flip ORIG_IDirectDrawSurfaceV4_Flip=NULL;
static TD_IDirectDrawSurfaceV4_Flip NEW_IDirectDrawSurfaceV4_Flip=NULL;
static TD_IDirectDrawSurfaceV4_GetAttachedSurface ORIG_IDirectDrawSurfaceV4_GetAttachedSurface=NULL;
static TD_IDirectDrawSurfaceV4_GetAttachedSurface NEW_IDirectDrawSurfaceV4_GetAttachedSurface=NULL;
static TD_IDirectDrawSurfaceV4_GetBltStatus ORIG_IDirectDrawSurfaceV4_GetBltStatus=NULL;
static TD_IDirectDrawSurfaceV4_GetBltStatus NEW_IDirectDrawSurfaceV4_GetBltStatus=NULL;
static TD_IDirectDrawSurfaceV4_GetCaps ORIG_IDirectDrawSurfaceV4_GetCaps=NULL;
static TD_IDirectDrawSurfaceV4_GetCaps NEW_IDirectDrawSurfaceV4_GetCaps=NULL;
static TD_IDirectDrawSurfaceV4_GetClipper ORIG_IDirectDrawSurfaceV4_GetClipper=NULL;
static TD_IDirectDrawSurfaceV4_GetClipper NEW_IDirectDrawSurfaceV4_GetClipper=NULL;
static TD_IDirectDrawSurfaceV4_GetColorKey ORIG_IDirectDrawSurfaceV4_GetColorKey=NULL;
static TD_IDirectDrawSurfaceV4_GetColorKey NEW_IDirectDrawSurfaceV4_GetColorKey=NULL;
static TD_IDirectDrawSurfaceV4_GetDC ORIG_IDirectDrawSurfaceV4_GetDC=NULL;
static TD_IDirectDrawSurfaceV4_GetDC NEW_IDirectDrawSurfaceV4_GetDC=NULL;
static TD_IDirectDrawSurfaceV4_GetFlipStatus ORIG_IDirectDrawSurfaceV4_GetFlipStatus=NULL;
static TD_IDirectDrawSurfaceV4_GetFlipStatus NEW_IDirectDrawSurfaceV4_GetFlipStatus=NULL;
static TD_IDirectDrawSurfaceV4_GetOverlayPosition ORIG_IDirectDrawSurfaceV4_GetOverlayPosition=NULL;
static TD_IDirectDrawSurfaceV4_GetOverlayPosition NEW_IDirectDrawSurfaceV4_GetOverlayPosition=NULL;
static TD_IDirectDrawSurfaceV4_GetPalette ORIG_IDirectDrawSurfaceV4_GetPalette=NULL;
static TD_IDirectDrawSurfaceV4_GetPalette NEW_IDirectDrawSurfaceV4_GetPalette=NULL;
static TD_IDirectDrawSurfaceV4_GetPixelFormat ORIG_IDirectDrawSurfaceV4_GetPixelFormat=NULL;
static TD_IDirectDrawSurfaceV4_GetPixelFormat NEW_IDirectDrawSurfaceV4_GetPixelFormat=NULL;
static TD_IDirectDrawSurfaceV4_GetSurfaceDesc ORIG_IDirectDrawSurfaceV4_GetSurfaceDesc=NULL;
static TD_IDirectDrawSurfaceV4_GetSurfaceDesc NEW_IDirectDrawSurfaceV4_GetSurfaceDesc=NULL;
static TD_IDirectDrawSurfaceV4_Initialize ORIG_IDirectDrawSurfaceV4_Initialize=NULL;
static TD_IDirectDrawSurfaceV4_Initialize NEW_IDirectDrawSurfaceV4_Initialize=NULL;
static TD_IDirectDrawSurfaceV4_IsLost ORIG_IDirectDrawSurfaceV4_IsLost=NULL;
static TD_IDirectDrawSurfaceV4_IsLost NEW_IDirectDrawSurfaceV4_IsLost=NULL;
static TD_IDirectDrawSurfaceV4_Lock ORIG_IDirectDrawSurfaceV4_Lock=NULL;
static TD_IDirectDrawSurfaceV4_Lock NEW_IDirectDrawSurfaceV4_Lock=NULL;
static TD_IDirectDrawSurfaceV4_ReleaseDC ORIG_IDirectDrawSurfaceV4_ReleaseDC=NULL;
static TD_IDirectDrawSurfaceV4_ReleaseDC NEW_IDirectDrawSurfaceV4_ReleaseDC=NULL;
static TD_IDirectDrawSurfaceV4_Restore ORIG_IDirectDrawSurfaceV4_Restore=NULL;
static TD_IDirectDrawSurfaceV4_Restore NEW_IDirectDrawSurfaceV4_Restore=NULL;
static TD_IDirectDrawSurfaceV4_SetClipper ORIG_IDirectDrawSurfaceV4_SetClipper=NULL;
static TD_IDirectDrawSurfaceV4_SetClipper NEW_IDirectDrawSurfaceV4_SetClipper=NULL;
static TD_IDirectDrawSurfaceV4_SetColorKey ORIG_IDirectDrawSurfaceV4_SetColorKey=NULL;
static TD_IDirectDrawSurfaceV4_SetColorKey NEW_IDirectDrawSurfaceV4_SetColorKey=NULL;
static TD_IDirectDrawSurfaceV4_SetOverlayPosition ORIG_IDirectDrawSurfaceV4_SetOverlayPosition=NULL;
static TD_IDirectDrawSurfaceV4_SetOverlayPosition NEW_IDirectDrawSurfaceV4_SetOverlayPosition=NULL;
static TD_IDirectDrawSurfaceV4_SetPalette ORIG_IDirectDrawSurfaceV4_SetPalette=NULL;
static TD_IDirectDrawSurfaceV4_SetPalette NEW_IDirectDrawSurfaceV4_SetPalette=NULL;
static TD_IDirectDrawSurfaceV4_Unlock ORIG_IDirectDrawSurfaceV4_Unlock=NULL;
static TD_IDirectDrawSurfaceV4_Unlock NEW_IDirectDrawSurfaceV4_Unlock=NULL;
static TD_IDirectDrawSurfaceV4_UpdateOverlay ORIG_IDirectDrawSurfaceV4_UpdateOverlay=NULL;
static TD_IDirectDrawSurfaceV4_UpdateOverlay NEW_IDirectDrawSurfaceV4_UpdateOverlay=NULL;
static TD_IDirectDrawSurfaceV4_UpdateOverlayDisplay NEW_IDirectDrawSurfaceV4_UpdateOverlayDisplay=NULL;
static TD_IDirectDrawSurfaceV4_UpdateOverlayDisplay ORIG_IDirectDrawSurfaceV4_UpdateOverlayDisplay=NULL;
static TD_IDirectDrawSurfaceV4_UpdateOverlayZOrder ORIG_IDirectDrawSurfaceV4_UpdateOverlayZOrder=NULL;
static TD_IDirectDrawSurfaceV4_UpdateOverlayZOrder NEW_IDirectDrawSurfaceV4_UpdateOverlayZOrder=NULL;
static TD_IDirectDrawSurfaceV4_GetDDInterface ORIG_IDirectDrawSurfaceV4_GetDDInterface=NULL;
static TD_IDirectDrawSurfaceV4_GetDDInterface NEW_IDirectDrawSurfaceV4_GetDDInterface=NULL;
static TD_IDirectDrawSurfaceV4_PageLock ORIG_IDirectDrawSurfaceV4_PageLock=NULL;
static TD_IDirectDrawSurfaceV4_PageLock NEW_IDirectDrawSurfaceV4_PageLock=NULL;
static TD_IDirectDrawSurfaceV4_PageUnlock ORIG_IDirectDrawSurfaceV4_PageUnlock=NULL;
static TD_IDirectDrawSurfaceV4_PageUnlock NEW_IDirectDrawSurfaceV4_PageUnlock=NULL;
static TD_IDirectDrawSurfaceV4_SetSurfaceDesc ORIG_IDirectDrawSurfaceV4_SetSurfaceDesc=NULL;
static TD_IDirectDrawSurfaceV4_SetSurfaceDesc NEW_IDirectDrawSurfaceV4_SetSurfaceDesc=NULL;
static TD_IDirectDrawSurfaceV4_SetPrivateData ORIG_IDirectDrawSurfaceV4_SetPrivateData=NULL;
static TD_IDirectDrawSurfaceV4_SetPrivateData NEW_IDirectDrawSurfaceV4_SetPrivateData=NULL;
static TD_IDirectDrawSurfaceV4_GetPrivateData ORIG_IDirectDrawSurfaceV4_GetPrivateData=NULL;
static TD_IDirectDrawSurfaceV4_GetPrivateData NEW_IDirectDrawSurfaceV4_GetPrivateData=NULL;
static TD_IDirectDrawSurfaceV4_FreePrivateData ORIG_IDirectDrawSurfaceV4_FreePrivateData=NULL;
static TD_IDirectDrawSurfaceV4_FreePrivateData NEW_IDirectDrawSurfaceV4_FreePrivateData=NULL;
static TD_IDirectDrawSurfaceV4_GetUniquenessValue ORIG_IDirectDrawSurfaceV4_GetUniquenessValue=NULL;
static TD_IDirectDrawSurfaceV4_GetUniquenessValue NEW_IDirectDrawSurfaceV4_GetUniquenessValue=NULL;
static TD_IDirectDrawSurfaceV4_ChangeUniquenessValue ORIG_IDirectDrawSurfaceV4_ChangeUniquenessValue=NULL;
static TD_IDirectDrawSurfaceV4_ChangeUniquenessValue NEW_IDirectDrawSurfaceV4_ChangeUniquenessValue=NULL;

/**************************************************************************
REPLACEMENT IMPLEMENTATION FOR IDIRECTDRAWSURFACE (V4) METHODS
**************************************************************************/

// IUnknown functions
HRESULT WINAPI My_IDDSV4_QueryInterface (LPUNKNOWN lpThis, REFIID riid, LPVOID* obp);
ULONG   WINAPI My_IDDSV4_AddRef(LPUNKNOWN lpThis);
ULONG   WINAPI My_IDDSV4_Release(LPUNKNOWN lpThis);
// IDirectDrawSurface (V1) functions
HRESULT WINAPI My_IDDSV4_AddAttachedSurface(
  LPDIRECTDRAWSURFACE4 lpThis,
  LPDIRECTDRAWSURFACE4 lpDDSAttachedSurface  
);
HRESULT WINAPI My_IDDSV4_AddOverlayDirtyRect(
  LPDIRECTDRAWSURFACE4 lpThis,
  LPRECT lpRect  
);
HRESULT WINAPI My_IDDSV4_Blt(
  LPDIRECTDRAWSURFACE4 lpThis,
  LPRECT lpDestRect,                    
  LPDIRECTDRAWSURFACE4 lpDDSrcSurface,  
  LPRECT lpSrcRect,                     
  DWORD dwFlags,                        
  LPDDBLTFX lpDDBltFx                   
);
HRESULT WINAPI My_IDDSV4_BltBatch(
  LPDIRECTDRAWSURFACE4 lpThis,
  LPDDBLTBATCH lpDDBltBatch,  
  DWORD dwCount,              
  DWORD dwFlags               
);
HRESULT WINAPI My_IDDSV4_BltFast(
  LPDIRECTDRAWSURFACE4 lpThis,
  DWORD dwX,                            
  DWORD dwY,                            
  LPDIRECTDRAWSURFACE4 lpDDSrcSurface,  
  LPRECT lpSrcRect,                     
  DWORD dwTrans                         
);
HRESULT WINAPI My_IDDSV4_DeleteAttachedSurface(
  LPDIRECTDRAWSURFACE4 lpThis,
  DWORD dwFlags,                             
  LPDIRECTDRAWSURFACE4 lpDDSAttachedSurface  
);
HRESULT WINAPI My_IDDSV4_EnumAttachedSurfaces(
  LPDIRECTDRAWSURFACE4 lpThis,
  LPVOID lpContext,                                
  LPDDENUMSURFACESCALLBACK2 lpEnumSurfacesCallback  
);
HRESULT WINAPI My_IDDSV4_EnumOverlayZOrders(
  LPDIRECTDRAWSURFACE4 lpThis,
  DWORD dwFlags,                          
  LPVOID lpContext,                       
  LPDDENUMSURFACESCALLBACK2 lpfnCallback  
);
HRESULT WINAPI My_IDDSV4_Flip(
  LPDIRECTDRAWSURFACE4 lpThis,
  LPDIRECTDRAWSURFACE4 lpDDSurfaceTargetOverride,  
  DWORD dwFlags                                    
);
HRESULT WINAPI My_IDDSV4_GetAttachedSurface(
  LPDIRECTDRAWSURFACE4 lpThis,
  LPDDSCAPS2 LPDDSCAPS2, 
  LPDIRECTDRAWSURFACE4 FAR *lplpDDAttachedSurface  
);
HRESULT WINAPI My_IDDSV4_GetBltStatus(
  LPDIRECTDRAWSURFACE4 lpThis,
  DWORD dwFlags  
);
HRESULT WINAPI My_IDDSV4_GetCaps(
  LPDIRECTDRAWSURFACE4 lpThis,
  LPDDSCAPS2 LPDDSCAPS2  
);
HRESULT WINAPI My_IDDSV4_GetClipper(
  LPDIRECTDRAWSURFACE4 lpThis,
  LPDIRECTDRAWCLIPPER FAR *lplpDDClipper  
);
HRESULT WINAPI My_IDDSV4_GetColorKey(
  LPDIRECTDRAWSURFACE4 lpThis,
  DWORD dwFlags,             
  LPDDCOLORKEY lpDDColorKey  
);
HRESULT WINAPI My_IDDSV4_GetDC(
  LPDIRECTDRAWSURFACE4 lpThis,
  HDC FAR *lphDC  
);
HRESULT WINAPI My_IDDSV4_GetFlipStatus(
  LPDIRECTDRAWSURFACE4 lpThis,
  DWORD dwFlags  
);
HRESULT WINAPI My_IDDSV4_GetOverlayPosition(
  LPDIRECTDRAWSURFACE4 lpThis,
  LPLONG lplX, 
  LPLONG lplY  
);
HRESULT WINAPI My_IDDSV4_GetPalette(
  LPDIRECTDRAWSURFACE4 lpThis,
  LPDIRECTDRAWPALETTE FAR *lplpDDPalette  
);

HRESULT WINAPI My_IDDSV4_GetPixelFormat(
  LPDIRECTDRAWSURFACE4 lpThis,
  LPDDPIXELFORMAT lpDDPixelFormat  
);
HRESULT WINAPI My_IDDSV4_GetSurfaceDesc(
  LPDIRECTDRAWSURFACE4 lpThis,
  LPDDSURFACEDESC2 LPDDSURFACEDESC2  
);
HRESULT WINAPI My_IDDSV4_Initialize(
  LPDIRECTDRAWSURFACE4 lpThis,
  LPDIRECTDRAW lpDD,               
  LPDDSURFACEDESC2 LPDDSURFACEDESC2  
);
HRESULT WINAPI My_IDDSV4_IsLost(
  LPDIRECTDRAWSURFACE4 lpThis
);
HRESULT WINAPI My_IDDSV4_Lock(
  LPDIRECTDRAWSURFACE4 lpThis,
  LPRECT lpDestRect,                
  LPDDSURFACEDESC2 LPDDSURFACEDESC2,  
  DWORD dwFlags,                    
  HANDLE hEvent                     
);
HRESULT WINAPI My_IDDSV4_ReleaseDC(
  LPDIRECTDRAWSURFACE4 lpThis,
  HDC hDC  
);
HRESULT WINAPI My_IDDSV4_Restore(
  LPDIRECTDRAWSURFACE4 lpThis
);
HRESULT WINAPI My_IDDSV4_SetClipper(
  LPDIRECTDRAWSURFACE4 lpThis,
  LPDIRECTDRAWCLIPPER lpDDClipper  
);
HRESULT WINAPI My_IDDSV4_SetColorKey(
  LPDIRECTDRAWSURFACE4 lpThis,
  DWORD dwFlags,             
  LPDDCOLORKEY lpDDColorKey  
);
HRESULT WINAPI My_IDDSV4_SetOverlayPosition(
  LPDIRECTDRAWSURFACE4 lpThis,
  LONG lX, 
  LONG lY  
);
HRESULT WINAPI My_IDDSV4_SetPalette(
  LPDIRECTDRAWSURFACE4 lpThis,
  LPDIRECTDRAWPALETTE lpDDPalette  
);
HRESULT WINAPI My_IDDSV4_Unlock(
  LPDIRECTDRAWSURFACE4 lpThis,
  LPVOID lpRect 
);
HRESULT WINAPI My_IDDSV4_UpdateOverlay(
  LPDIRECTDRAWSURFACE4 lpThis,
  LPRECT lpSrcRect,                      
  LPDIRECTDRAWSURFACE4 lpDDDestSurface,  
  LPRECT lpDestRect,                     
  DWORD dwFlags,                         
  LPDDOVERLAYFX lpDDOverlayFx            
);
HRESULT WINAPI My_IDDSV4_UpdateOverlayDisplay(
  LPDIRECTDRAWSURFACE4 lpThis,
  DWORD dwFlags  
);
HRESULT WINAPI My_IDDSV4_UpdateOverlayZOrder(
  LPDIRECTDRAWSURFACE4 lpThis,
  DWORD dwFlags,                       
  LPDIRECTDRAWSURFACE4 lpDDSReference  
);
// IDirectDrawSurface2 functions
HRESULT WINAPI My_IDDSV4_GetDDInterface(
  LPDIRECTDRAWSURFACE4 lpThis,
  LPVOID FAR *lplpDD  
);
HRESULT WINAPI My_IDDSV4_PageLock(
  LPDIRECTDRAWSURFACE4 lpThis,
  DWORD dwFlags  
);
HRESULT WINAPI My_IDDSV4_PageUnlock(
  LPDIRECTDRAWSURFACE4 lpThis,
  DWORD dwFlags  
);

// IDirectDrawSurface3 functions
HRESULT WINAPI My_IDDSV4_SetSurfaceDesc(
  LPDIRECTDRAWSURFACE4 lpThis,
  LPDDSURFACEDESC2 lpddsd,  
  DWORD dwFlags            
);
// IDirectDrawSurface4 functions
HRESULT WINAPI My_IDDSV4_SetPrivateData( 
  LPDIRECTDRAWSURFACE4 lpThis,
  REFGUID guidTag, 
  LPVOID  lpData,
  DWORD   cbSize,
  DWORD   dwFlags 
); 
HRESULT WINAPI My_IDDSV4_GetPrivateData(
  LPDIRECTDRAWSURFACE4 lpThis,
  REFGUID guidTag,
  LPVOID  lpBuffer,
  LPDWORD lpcbBufferSize
); 
HRESULT WINAPI My_IDDSV4_FreePrivateData( 
  LPDIRECTDRAWSURFACE4 lpThis,
  REFGUID guidTag 
); 
HRESULT WINAPI My_IDDSV4_GetUniquenessValue( 
  LPDIRECTDRAWSURFACE4 lpThis,
  LPDWORD lpValue 
); 
HRESULT WINAPI My_IDDSV4_ChangeUniquenessValue(
  LPDIRECTDRAWSURFACE4 lpThis
); 


/**************************************************************************
DETOURS SETUP AND TEARDOWN FUNCTIONS
**************************************************************************/
void HookDirectDrawSurface4Interface  (LPDIRECTDRAWSURFACE4 lpInterface);

