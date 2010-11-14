#pragma once
/**************************************************************************
TYPEDEFS FOR STRONGLY-TYPED FUNCTION POINTERS TO IDIRECTDRAWSURFACE (V7) METHODS
**************************************************************************/
// IUnknown functions
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV7_QueryInterface) (LPUNKNOWN lpThis, REFIID riid, LPVOID* obp);
typedef ULONG   (WINAPI* TD_IDirectDrawSurfaceV7_AddRef)(LPUNKNOWN lpThis);
typedef ULONG   (WINAPI* TD_IDirectDrawSurfaceV7_Release)(LPUNKNOWN lpThis);

// IDirectDrawSurface (V1) functions
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV7_AddAttachedSurface)( 
	LPDIRECTDRAWSURFACE7 lpThis,
	LPDIRECTDRAWSURFACE7 lpDDSAttachedSurface  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV7_AddOverlayDirtyRect)(
  LPDIRECTDRAWSURFACE7 lpThis,
  LPRECT lpRect  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV7_Blt)(
  LPDIRECTDRAWSURFACE7 lpThis,
  LPRECT lpDestRect,                    
  LPDIRECTDRAWSURFACE7 lpDDSrcSurface,  
  LPRECT lpSrcRect,                     
  DWORD dwFlags,                        
  LPDDBLTFX lpDDBltFx                   
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV7_BltBatch)(
  LPDIRECTDRAWSURFACE7 lpThis,
  LPDDBLTBATCH lpDDBltBatch,  
  DWORD dwCount,              
  DWORD dwFlags               
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV7_BltFast)(
  LPDIRECTDRAWSURFACE7 lpThis,
  DWORD dwX,                            
  DWORD dwY,                            
  LPDIRECTDRAWSURFACE7 lpDDSrcSurface,  
  LPRECT lpSrcRect,                     
  DWORD dwTrans                         
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV7_DeleteAttachedSurface)(
  LPDIRECTDRAWSURFACE7 lpThis,
  DWORD dwFlags,                             
  LPDIRECTDRAWSURFACE7 lpDDSAttachedSurface  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV7_EnumAttachedSurfaces)(
  LPDIRECTDRAWSURFACE7 lpThis,
  LPVOID lpContext,                                
  LPDDENUMSURFACESCALLBACK7 lpEnumSurfacesCallback  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV7_EnumOverlayZOrders)(
  LPDIRECTDRAWSURFACE7 lpThis,
  DWORD dwFlags,                          
  LPVOID lpContext,                       
  LPDDENUMSURFACESCALLBACK7 lpfnCallback  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV7_Flip)(
  LPDIRECTDRAWSURFACE7 lpThis,
  LPDIRECTDRAWSURFACE7 lpDDSurfaceTargetOverride,  
  DWORD dwFlags                                    
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV7_GetAttachedSurface)(
  LPDIRECTDRAWSURFACE7 lpThis,
  LPDDSCAPS2 LPDDSCAPS2, 
  LPDIRECTDRAWSURFACE7 FAR *lplpDDAttachedSurface  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV7_GetBltStatus)(
  LPDIRECTDRAWSURFACE7 lpThis,
  DWORD dwFlags  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV7_GetCaps)(
  LPDIRECTDRAWSURFACE7 lpThis,
  LPDDSCAPS2 LPDDSCAPS2  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV7_GetClipper)(
  LPDIRECTDRAWSURFACE7 lpThis,
  LPDIRECTDRAWCLIPPER FAR *lplpDDClipper  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV7_GetColorKey)(
  LPDIRECTDRAWSURFACE7 lpThis,
  DWORD dwFlags,             
  LPDDCOLORKEY lpDDColorKey  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV7_GetDC)(
  LPDIRECTDRAWSURFACE7 lpThis,
  HDC FAR *lphDC  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV7_GetFlipStatus)(
  LPDIRECTDRAWSURFACE7 lpThis,
  DWORD dwFlags  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV7_GetOverlayPosition)(
  LPDIRECTDRAWSURFACE7 lpThis,
  LPLONG lplX, 
  LPLONG lplY  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV7_GetPalette)(
  LPDIRECTDRAWSURFACE7 lpThis,
  LPDIRECTDRAWPALETTE FAR *lplpDDPalette  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV7_GetPixelFormat)(
  LPDIRECTDRAWSURFACE7 lpThis,
  LPDDPIXELFORMAT lpDDPixelFormat  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV7_GetSurfaceDesc)(
  LPDIRECTDRAWSURFACE7 lpThis,
  LPDDSURFACEDESC2 LPDDSURFACEDESC2  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV7_Initialize)(
  LPDIRECTDRAWSURFACE7 lpThis,
  LPDIRECTDRAW lpDD,               
  LPDDSURFACEDESC2 LPDDSURFACEDESC2  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV7_IsLost)(LPDIRECTDRAWSURFACE7 lpThis);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV7_Lock)(
  LPDIRECTDRAWSURFACE7 lpThis,
  LPRECT lpDestRect,                
  LPDDSURFACEDESC2 LPDDSURFACEDESC2,  
  DWORD dwFlags,                    
  HANDLE hEvent                     
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV7_ReleaseDC)(
  LPDIRECTDRAWSURFACE7 lpThis,
  HDC hDC  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV7_Restore)(LPDIRECTDRAWSURFACE7 lpThis);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV7_SetClipper)(
  LPDIRECTDRAWSURFACE7 lpThis,
  LPDIRECTDRAWCLIPPER lpDDClipper  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV7_SetColorKey)(
  LPDIRECTDRAWSURFACE7 lpThis,
  DWORD dwFlags,             
  LPDDCOLORKEY lpDDColorKey  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV7_SetOverlayPosition)(
  LPDIRECTDRAWSURFACE7 lpThis,
  LONG lX, 
  LONG lY  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV7_SetPalette)(
  LPDIRECTDRAWSURFACE7 lpThis,
  LPDIRECTDRAWPALETTE lpDDPalette  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV7_Unlock)(
  LPDIRECTDRAWSURFACE7 lpThis,
  LPVOID lpRect 
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV7_UpdateOverlay)(
  LPDIRECTDRAWSURFACE7 lpThis,
  LPRECT lpSrcRect,                      
  LPDIRECTDRAWSURFACE7 lpDDDestSurface,  
  LPRECT lpDestRect,                     
  DWORD dwFlags,                         
  LPDDOVERLAYFX lpDDOverlayFx            
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV7_UpdateOverlayDisplay)(
  LPDIRECTDRAWSURFACE7 lpThis,
  DWORD dwFlags  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV7_UpdateOverlayZOrder)(
  LPDIRECTDRAWSURFACE7 lpThis,
  DWORD dwFlags,                       
  LPDIRECTDRAWSURFACE7 lpDDSReference  
);
// IDirectDrawSurface2 functions
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV7_GetDDInterface)(
  LPDIRECTDRAWSURFACE7 lpThis,
  LPVOID FAR *lplpDD  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV7_PageLock)(
  LPDIRECTDRAWSURFACE7 lpThis,
  DWORD dwFlags  
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV7_PageUnlock)(
  LPDIRECTDRAWSURFACE7 lpThis,
  DWORD dwFlags  
);

// IDirectDrawSurface3  functions
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV7_SetSurfaceDesc)(
  LPDIRECTDRAWSURFACE7 lpThis,
  LPDDSURFACEDESC2 lpddsd,  
  DWORD dwFlags            
);
// IDirectDrawSurface4  functions
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV7_SetPrivateData)( 
  LPDIRECTDRAWSURFACE7 lpThis,
  REFGUID guidTag, 
  LPVOID  lpData,
  DWORD   cbSize,
  DWORD   dwFlags 
); 
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV7_GetPrivateData)(
  LPDIRECTDRAWSURFACE7 lpThis,
  REFGUID guidTag,
  LPVOID  lpBuffer,
  LPDWORD lpcbBufferSize
); 
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV7_FreePrivateData)( 
  LPDIRECTDRAWSURFACE7 lpThis,
  REFGUID guidTag 
); 
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV7_GetUniquenessValue)( 
  LPDIRECTDRAWSURFACE7 lpThis,
  LPDWORD lpValue 
); 
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV7_ChangeUniquenessValue)(
  LPDIRECTDRAWSURFACE7 lpThis
); 

// IDirectDrawSurface7  functions
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV7_SetPriority)(
  LPDIRECTDRAWSURFACE7 lpThis,
  DWORD dwPriority
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV7_GetPriority)( 
  LPDIRECTDRAWSURFACE7 lpThis,
  LPDWORD lpdwPriority
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV7_SetLOD)(
  LPDIRECTDRAWSURFACE7 lpThis,
  DWORD dwMaxLOD
);
typedef HRESULT (WINAPI* TD_IDirectDrawSurfaceV7_GetLOD)(
  LPDIRECTDRAWSURFACE7 lpThis,
  LPDWORD lpdwMaxLOD
);


/**************************************************************************
STATIC STRONGLY-TYPED FUNCTION POINTERS TO 
ORIGINAL AND REPLACEMENT IDIRECTDRAWSURFACE (V7) METHODS
**************************************************************************/
static TD_IDirectDrawSurfaceV7_QueryInterface ORIG_IDirectDrawSurfaceV7_QueryInterface=NULL;
static TD_IDirectDrawSurfaceV7_QueryInterface NEW_IDirectDrawSurfaceV7_QueryInterface=NULL;
static TD_IDirectDrawSurfaceV7_AddRef ORIG_IDirectDrawSurfaceV7_AddRef=NULL;
static TD_IDirectDrawSurfaceV7_AddRef NEW_IDirectDrawSurfaceV7_AddRef=NULL;
static TD_IDirectDrawSurfaceV7_Release ORIG_IDirectDrawSurfaceV7_Release=NULL;
static TD_IDirectDrawSurfaceV7_Release NEW_IDirectDrawSurfaceV7_Release=NULL;
static TD_IDirectDrawSurfaceV7_AddAttachedSurface ORIG_IDirectDrawSurfaceV7_AddAttachedSurface=NULL;
static TD_IDirectDrawSurfaceV7_AddAttachedSurface NEW_IDirectDrawSurfaceV7_AddAttachedSurface=NULL;
static TD_IDirectDrawSurfaceV7_AddOverlayDirtyRect ORIG_IDirectDrawSurfaceV7_AddOverlayDirtyRect=NULL;
static TD_IDirectDrawSurfaceV7_AddOverlayDirtyRect NEW_IDirectDrawSurfaceV7_AddOverlayDirtyRect=NULL;
static TD_IDirectDrawSurfaceV7_Blt ORIG_IDirectDrawSurfaceV7_Blt=NULL;
static TD_IDirectDrawSurfaceV7_Blt NEW_IDirectDrawSurfaceV7_Blt=NULL;
static TD_IDirectDrawSurfaceV7_BltBatch ORIG_IDirectDrawSurfaceV7_BltBatch=NULL;
static TD_IDirectDrawSurfaceV7_BltBatch NEW_IDirectDrawSurfaceV7_BltBatch=NULL;
static TD_IDirectDrawSurfaceV7_BltFast ORIG_IDirectDrawSurfaceV7_BltFast=NULL;
static TD_IDirectDrawSurfaceV7_BltFast NEW_IDirectDrawSurfaceV7_BltFast=NULL;
static TD_IDirectDrawSurfaceV7_DeleteAttachedSurface ORIG_IDirectDrawSurfaceV7_DeleteAttachedSurface=NULL;
static TD_IDirectDrawSurfaceV7_DeleteAttachedSurface NEW_IDirectDrawSurfaceV7_DeleteAttachedSurface=NULL;
static TD_IDirectDrawSurfaceV7_EnumAttachedSurfaces ORIG_IDirectDrawSurfaceV7_EnumAttachedSurfaces=NULL;
static TD_IDirectDrawSurfaceV7_EnumAttachedSurfaces NEW_IDirectDrawSurfaceV7_EnumAttachedSurfaces=NULL;
static TD_IDirectDrawSurfaceV7_EnumOverlayZOrders ORIG_IDirectDrawSurfaceV7_EnumOverlayZOrders=NULL;
static TD_IDirectDrawSurfaceV7_EnumOverlayZOrders NEW_IDirectDrawSurfaceV7_EnumOverlayZOrders=NULL;
static TD_IDirectDrawSurfaceV7_Flip ORIG_IDirectDrawSurfaceV7_Flip=NULL;
static TD_IDirectDrawSurfaceV7_Flip NEW_IDirectDrawSurfaceV7_Flip=NULL;
static TD_IDirectDrawSurfaceV7_GetAttachedSurface ORIG_IDirectDrawSurfaceV7_GetAttachedSurface=NULL;
static TD_IDirectDrawSurfaceV7_GetAttachedSurface NEW_IDirectDrawSurfaceV7_GetAttachedSurface=NULL;
static TD_IDirectDrawSurfaceV7_GetBltStatus ORIG_IDirectDrawSurfaceV7_GetBltStatus=NULL;
static TD_IDirectDrawSurfaceV7_GetBltStatus NEW_IDirectDrawSurfaceV7_GetBltStatus=NULL;
static TD_IDirectDrawSurfaceV7_GetCaps ORIG_IDirectDrawSurfaceV7_GetCaps=NULL;
static TD_IDirectDrawSurfaceV7_GetCaps NEW_IDirectDrawSurfaceV7_GetCaps=NULL;
static TD_IDirectDrawSurfaceV7_GetClipper ORIG_IDirectDrawSurfaceV7_GetClipper=NULL;
static TD_IDirectDrawSurfaceV7_GetClipper NEW_IDirectDrawSurfaceV7_GetClipper=NULL;
static TD_IDirectDrawSurfaceV7_GetColorKey ORIG_IDirectDrawSurfaceV7_GetColorKey=NULL;
static TD_IDirectDrawSurfaceV7_GetColorKey NEW_IDirectDrawSurfaceV7_GetColorKey=NULL;
static TD_IDirectDrawSurfaceV7_GetDC ORIG_IDirectDrawSurfaceV7_GetDC=NULL;
static TD_IDirectDrawSurfaceV7_GetDC NEW_IDirectDrawSurfaceV7_GetDC=NULL;
static TD_IDirectDrawSurfaceV7_GetFlipStatus ORIG_IDirectDrawSurfaceV7_GetFlipStatus=NULL;
static TD_IDirectDrawSurfaceV7_GetFlipStatus NEW_IDirectDrawSurfaceV7_GetFlipStatus=NULL;
static TD_IDirectDrawSurfaceV7_GetOverlayPosition ORIG_IDirectDrawSurfaceV7_GetOverlayPosition=NULL;
static TD_IDirectDrawSurfaceV7_GetOverlayPosition NEW_IDirectDrawSurfaceV7_GetOverlayPosition=NULL;
static TD_IDirectDrawSurfaceV7_GetPalette ORIG_IDirectDrawSurfaceV7_GetPalette=NULL;
static TD_IDirectDrawSurfaceV7_GetPalette NEW_IDirectDrawSurfaceV7_GetPalette=NULL;
static TD_IDirectDrawSurfaceV7_GetPixelFormat ORIG_IDirectDrawSurfaceV7_GetPixelFormat=NULL;
static TD_IDirectDrawSurfaceV7_GetPixelFormat NEW_IDirectDrawSurfaceV7_GetPixelFormat=NULL;
static TD_IDirectDrawSurfaceV7_GetSurfaceDesc ORIG_IDirectDrawSurfaceV7_GetSurfaceDesc=NULL;
static TD_IDirectDrawSurfaceV7_GetSurfaceDesc NEW_IDirectDrawSurfaceV7_GetSurfaceDesc=NULL;
static TD_IDirectDrawSurfaceV7_Initialize ORIG_IDirectDrawSurfaceV7_Initialize=NULL;
static TD_IDirectDrawSurfaceV7_Initialize NEW_IDirectDrawSurfaceV7_Initialize=NULL;
static TD_IDirectDrawSurfaceV7_IsLost ORIG_IDirectDrawSurfaceV7_IsLost=NULL;
static TD_IDirectDrawSurfaceV7_IsLost NEW_IDirectDrawSurfaceV7_IsLost=NULL;
static TD_IDirectDrawSurfaceV7_Lock ORIG_IDirectDrawSurfaceV7_Lock=NULL;
static TD_IDirectDrawSurfaceV7_Lock NEW_IDirectDrawSurfaceV7_Lock=NULL;
static TD_IDirectDrawSurfaceV7_ReleaseDC ORIG_IDirectDrawSurfaceV7_ReleaseDC=NULL;
static TD_IDirectDrawSurfaceV7_ReleaseDC NEW_IDirectDrawSurfaceV7_ReleaseDC=NULL;
static TD_IDirectDrawSurfaceV7_Restore ORIG_IDirectDrawSurfaceV7_Restore=NULL;
static TD_IDirectDrawSurfaceV7_Restore NEW_IDirectDrawSurfaceV7_Restore=NULL;
static TD_IDirectDrawSurfaceV7_SetClipper ORIG_IDirectDrawSurfaceV7_SetClipper=NULL;
static TD_IDirectDrawSurfaceV7_SetClipper NEW_IDirectDrawSurfaceV7_SetClipper=NULL;
static TD_IDirectDrawSurfaceV7_SetColorKey ORIG_IDirectDrawSurfaceV7_SetColorKey=NULL;
static TD_IDirectDrawSurfaceV7_SetColorKey NEW_IDirectDrawSurfaceV7_SetColorKey=NULL;
static TD_IDirectDrawSurfaceV7_SetOverlayPosition ORIG_IDirectDrawSurfaceV7_SetOverlayPosition=NULL;
static TD_IDirectDrawSurfaceV7_SetOverlayPosition NEW_IDirectDrawSurfaceV7_SetOverlayPosition=NULL;
static TD_IDirectDrawSurfaceV7_SetPalette ORIG_IDirectDrawSurfaceV7_SetPalette=NULL;
static TD_IDirectDrawSurfaceV7_SetPalette NEW_IDirectDrawSurfaceV7_SetPalette=NULL;
static TD_IDirectDrawSurfaceV7_Unlock ORIG_IDirectDrawSurfaceV7_Unlock=NULL;
static TD_IDirectDrawSurfaceV7_Unlock NEW_IDirectDrawSurfaceV7_Unlock=NULL;
static TD_IDirectDrawSurfaceV7_UpdateOverlay ORIG_IDirectDrawSurfaceV7_UpdateOverlay=NULL;
static TD_IDirectDrawSurfaceV7_UpdateOverlay NEW_IDirectDrawSurfaceV7_UpdateOverlay=NULL;
static TD_IDirectDrawSurfaceV7_UpdateOverlayDisplay NEW_IDirectDrawSurfaceV7_UpdateOverlayDisplay=NULL;
static TD_IDirectDrawSurfaceV7_UpdateOverlayDisplay ORIG_IDirectDrawSurfaceV7_UpdateOverlayDisplay=NULL;
static TD_IDirectDrawSurfaceV7_UpdateOverlayZOrder ORIG_IDirectDrawSurfaceV7_UpdateOverlayZOrder=NULL;
static TD_IDirectDrawSurfaceV7_UpdateOverlayZOrder NEW_IDirectDrawSurfaceV7_UpdateOverlayZOrder=NULL;
static TD_IDirectDrawSurfaceV7_GetDDInterface ORIG_IDirectDrawSurfaceV7_GetDDInterface=NULL;
static TD_IDirectDrawSurfaceV7_GetDDInterface NEW_IDirectDrawSurfaceV7_GetDDInterface=NULL;
static TD_IDirectDrawSurfaceV7_PageLock ORIG_IDirectDrawSurfaceV7_PageLock=NULL;
static TD_IDirectDrawSurfaceV7_PageLock NEW_IDirectDrawSurfaceV7_PageLock=NULL;
static TD_IDirectDrawSurfaceV7_PageUnlock ORIG_IDirectDrawSurfaceV7_PageUnlock=NULL;
static TD_IDirectDrawSurfaceV7_PageUnlock NEW_IDirectDrawSurfaceV7_PageUnlock=NULL;
static TD_IDirectDrawSurfaceV7_SetSurfaceDesc ORIG_IDirectDrawSurfaceV7_SetSurfaceDesc=NULL;
static TD_IDirectDrawSurfaceV7_SetSurfaceDesc NEW_IDirectDrawSurfaceV7_SetSurfaceDesc=NULL;
static TD_IDirectDrawSurfaceV7_SetPrivateData ORIG_IDirectDrawSurfaceV7_SetPrivateData=NULL;
static TD_IDirectDrawSurfaceV7_SetPrivateData NEW_IDirectDrawSurfaceV7_SetPrivateData=NULL;
static TD_IDirectDrawSurfaceV7_GetPrivateData ORIG_IDirectDrawSurfaceV7_GetPrivateData=NULL;
static TD_IDirectDrawSurfaceV7_GetPrivateData NEW_IDirectDrawSurfaceV7_GetPrivateData=NULL;
static TD_IDirectDrawSurfaceV7_FreePrivateData ORIG_IDirectDrawSurfaceV7_FreePrivateData=NULL;
static TD_IDirectDrawSurfaceV7_FreePrivateData NEW_IDirectDrawSurfaceV7_FreePrivateData=NULL;
static TD_IDirectDrawSurfaceV7_GetUniquenessValue ORIG_IDirectDrawSurfaceV7_GetUniquenessValue=NULL;
static TD_IDirectDrawSurfaceV7_GetUniquenessValue NEW_IDirectDrawSurfaceV7_GetUniquenessValue=NULL;
static TD_IDirectDrawSurfaceV7_ChangeUniquenessValue ORIG_IDirectDrawSurfaceV7_ChangeUniquenessValue=NULL;
static TD_IDirectDrawSurfaceV7_ChangeUniquenessValue NEW_IDirectDrawSurfaceV7_ChangeUniquenessValue=NULL;
static TD_IDirectDrawSurfaceV7_SetPriority ORIG_IDirectDrawSurfaceV7_SetPriority=NULL;
static TD_IDirectDrawSurfaceV7_SetPriority NEW_IDirectDrawSurfaceV7_SetPriority=NULL;
static TD_IDirectDrawSurfaceV7_GetPriority ORIG_IDirectDrawSurfaceV7_GetPriority=NULL;
static TD_IDirectDrawSurfaceV7_GetPriority NEW_IDirectDrawSurfaceV7_GetPriority=NULL;
static TD_IDirectDrawSurfaceV7_SetLOD ORIG_IDirectDrawSurfaceV7_SetLOD=NULL;
static TD_IDirectDrawSurfaceV7_SetLOD NEW_IDirectDrawSurfaceV7_SetLOD=NULL;
static TD_IDirectDrawSurfaceV7_GetLOD ORIG_IDirectDrawSurfaceV7_GetLOD=NULL;
static TD_IDirectDrawSurfaceV7_GetLOD NEW_IDirectDrawSurfaceV7_GetLOD=NULL;

/**************************************************************************
REPLACEMENT IMPLEMENTATION FOR IDIRECTDRAWSURFACE (V7) METHODS
**************************************************************************/

// IUnknown functions
HRESULT WINAPI My_IDDSV7_QueryInterface (LPUNKNOWN lpThis, REFIID riid, LPVOID* obp);
ULONG   WINAPI My_IDDSV7_AddRef(LPUNKNOWN lpThis);
ULONG   WINAPI My_IDDSV7_Release(LPUNKNOWN lpThis);
// IDirectDrawSurface (V1) functions
HRESULT WINAPI My_IDDSV7_AddAttachedSurface(
  LPDIRECTDRAWSURFACE7 lpThis,
  LPDIRECTDRAWSURFACE7 lpDDSAttachedSurface  
);
HRESULT WINAPI My_IDDSV7_AddOverlayDirtyRect(
  LPDIRECTDRAWSURFACE7 lpThis,
  LPRECT lpRect  
);
HRESULT WINAPI My_IDDSV7_Blt(
  LPDIRECTDRAWSURFACE7 lpThis,
  LPRECT lpDestRect,                    
  LPDIRECTDRAWSURFACE7 lpDDSrcSurface,  
  LPRECT lpSrcRect,                     
  DWORD dwFlags,                        
  LPDDBLTFX lpDDBltFx                   
);
HRESULT WINAPI My_IDDSV7_BltBatch(
  LPDIRECTDRAWSURFACE7 lpThis,
  LPDDBLTBATCH lpDDBltBatch,  
  DWORD dwCount,              
  DWORD dwFlags               
);
HRESULT WINAPI My_IDDSV7_BltFast(
  LPDIRECTDRAWSURFACE7 lpThis,
  DWORD dwX,                            
  DWORD dwY,                            
  LPDIRECTDRAWSURFACE7 lpDDSrcSurface,  
  LPRECT lpSrcRect,                     
  DWORD dwTrans                         
);
HRESULT WINAPI My_IDDSV7_DeleteAttachedSurface(
  LPDIRECTDRAWSURFACE7 lpThis,
  DWORD dwFlags,                             
  LPDIRECTDRAWSURFACE7 lpDDSAttachedSurface  
);
HRESULT WINAPI My_IDDSV7_EnumAttachedSurfaces(
  LPDIRECTDRAWSURFACE7 lpThis,
  LPVOID lpContext,                                
  LPDDENUMSURFACESCALLBACK7 lpEnumSurfacesCallback  
);
HRESULT WINAPI My_IDDSV7_EnumOverlayZOrders(
  LPDIRECTDRAWSURFACE7 lpThis,
  DWORD dwFlags,                          
  LPVOID lpContext,                       
  LPDDENUMSURFACESCALLBACK7 lpfnCallback  
);
HRESULT WINAPI My_IDDSV7_Flip(
  LPDIRECTDRAWSURFACE7 lpThis,
  LPDIRECTDRAWSURFACE7 lpDDSurfaceTargetOverride,  
  DWORD dwFlags                                    
);
HRESULT WINAPI My_IDDSV7_GetAttachedSurface(
  LPDIRECTDRAWSURFACE7 lpThis,
  LPDDSCAPS2 LPDDSCAPS2, 
  LPDIRECTDRAWSURFACE7 FAR *lplpDDAttachedSurface  
);
HRESULT WINAPI My_IDDSV7_GetBltStatus(
  LPDIRECTDRAWSURFACE7 lpThis,
  DWORD dwFlags  
);
HRESULT WINAPI My_IDDSV7_GetCaps(
  LPDIRECTDRAWSURFACE7 lpThis,
  LPDDSCAPS2 LPDDSCAPS2  
);
HRESULT WINAPI My_IDDSV7_GetClipper(
  LPDIRECTDRAWSURFACE7 lpThis,
  LPDIRECTDRAWCLIPPER FAR *lplpDDClipper  
);
HRESULT WINAPI My_IDDSV7_GetColorKey(
  LPDIRECTDRAWSURFACE7 lpThis,
  DWORD dwFlags,             
  LPDDCOLORKEY lpDDColorKey  
);
HRESULT WINAPI My_IDDSV7_GetDC(
  LPDIRECTDRAWSURFACE7 lpThis,
  HDC FAR *lphDC  
);
HRESULT WINAPI My_IDDSV7_GetFlipStatus(
  LPDIRECTDRAWSURFACE7 lpThis,
  DWORD dwFlags  
);
HRESULT WINAPI My_IDDSV7_GetOverlayPosition(
  LPDIRECTDRAWSURFACE7 lpThis,
  LPLONG lplX, 
  LPLONG lplY  
);
HRESULT WINAPI My_IDDSV7_GetPalette(
  LPDIRECTDRAWSURFACE7 lpThis,
  LPDIRECTDRAWPALETTE FAR *lplpDDPalette  
);

HRESULT WINAPI My_IDDSV7_GetPixelFormat(
  LPDIRECTDRAWSURFACE7 lpThis,
  LPDDPIXELFORMAT lpDDPixelFormat  
);
HRESULT WINAPI My_IDDSV7_GetSurfaceDesc(
  LPDIRECTDRAWSURFACE7 lpThis,
  LPDDSURFACEDESC2 LPDDSURFACEDESC2  
);
HRESULT WINAPI My_IDDSV7_Initialize(
  LPDIRECTDRAWSURFACE7 lpThis,
  LPDIRECTDRAW lpDD,               
  LPDDSURFACEDESC2 LPDDSURFACEDESC2  
);
HRESULT WINAPI My_IDDSV7_IsLost(
  LPDIRECTDRAWSURFACE7 lpThis
);
HRESULT WINAPI My_IDDSV7_Lock(
  LPDIRECTDRAWSURFACE7 lpThis,
  LPRECT lpDestRect,                
  LPDDSURFACEDESC2 LPDDSURFACEDESC2,  
  DWORD dwFlags,                    
  HANDLE hEvent                     
);
HRESULT WINAPI My_IDDSV7_ReleaseDC(
  LPDIRECTDRAWSURFACE7 lpThis,
  HDC hDC  
);
HRESULT WINAPI My_IDDSV7_Restore(
  LPDIRECTDRAWSURFACE7 lpThis
);
HRESULT WINAPI My_IDDSV7_SetClipper(
  LPDIRECTDRAWSURFACE7 lpThis,
  LPDIRECTDRAWCLIPPER lpDDClipper  
);
HRESULT WINAPI My_IDDSV7_SetColorKey(
  LPDIRECTDRAWSURFACE7 lpThis,
  DWORD dwFlags,             
  LPDDCOLORKEY lpDDColorKey  
);
HRESULT WINAPI My_IDDSV7_SetOverlayPosition(
  LPDIRECTDRAWSURFACE7 lpThis,
  LONG lX, 
  LONG lY  
);
HRESULT WINAPI My_IDDSV7_SetPalette(
  LPDIRECTDRAWSURFACE7 lpThis,
  LPDIRECTDRAWPALETTE lpDDPalette  
);
HRESULT WINAPI My_IDDSV7_Unlock(
  LPDIRECTDRAWSURFACE7 lpThis,
  LPVOID lpRect 
);
HRESULT WINAPI My_IDDSV7_UpdateOverlay(
  LPDIRECTDRAWSURFACE7 lpThis,
  LPRECT lpSrcRect,                      
  LPDIRECTDRAWSURFACE7 lpDDDestSurface,  
  LPRECT lpDestRect,                     
  DWORD dwFlags,                         
  LPDDOVERLAYFX lpDDOverlayFx            
);
HRESULT WINAPI My_IDDSV7_UpdateOverlayDisplay(
  LPDIRECTDRAWSURFACE7 lpThis,
  DWORD dwFlags  
);
HRESULT WINAPI My_IDDSV7_UpdateOverlayZOrder(
  LPDIRECTDRAWSURFACE7 lpThis,
  DWORD dwFlags,                       
  LPDIRECTDRAWSURFACE7 lpDDSReference  
);
// IDirectDrawSurface2 functions
HRESULT WINAPI My_IDDSV7_GetDDInterface(
  LPDIRECTDRAWSURFACE7 lpThis,
  LPVOID FAR *lplpDD  
);
HRESULT WINAPI My_IDDSV7_PageLock(
  LPDIRECTDRAWSURFACE7 lpThis,
  DWORD dwFlags  
);
HRESULT WINAPI My_IDDSV7_PageUnlock(
  LPDIRECTDRAWSURFACE7 lpThis,
  DWORD dwFlags  
);

// IDirectDrawSurface3 functions
HRESULT WINAPI My_IDDSV7_SetSurfaceDesc(
  LPDIRECTDRAWSURFACE7 lpThis,
  LPDDSURFACEDESC2 lpddsd,  
  DWORD dwFlags            
);
// IDirectDrawSurface4 functions
HRESULT WINAPI My_IDDSV7_SetPrivateData( 
  LPDIRECTDRAWSURFACE7 lpThis,
  REFGUID guidTag, 
  LPVOID  lpData,
  DWORD   cbSize,
  DWORD   dwFlags 
); 
HRESULT WINAPI My_IDDSV7_GetPrivateData(
  LPDIRECTDRAWSURFACE7 lpThis,
  REFGUID guidTag,
  LPVOID  lpBuffer,
  LPDWORD lpcbBufferSize
); 
HRESULT WINAPI My_IDDSV7_FreePrivateData( 
  LPDIRECTDRAWSURFACE7 lpThis,
  REFGUID guidTag 
); 
HRESULT WINAPI My_IDDSV7_GetUniquenessValue( 
  LPDIRECTDRAWSURFACE7 lpThis,
  LPDWORD lpValue 
); 
HRESULT WINAPI My_IDDSV7_ChangeUniquenessValue(
  LPDIRECTDRAWSURFACE7 lpThis
); 

// IDirectDrawSurface7 functions
HRESULT WINAPI My_IDDSV7_SetPriority(
  LPDIRECTDRAWSURFACE7 lpThis,
  DWORD dwPriority
);
HRESULT WINAPI My_IDDSV7_GetPriority( 
  LPDIRECTDRAWSURFACE7 lpThis,
  LPDWORD lpdwPriority
);
HRESULT WINAPI My_IDDSV7_SetLOD(
  LPDIRECTDRAWSURFACE7 lpThis,
  DWORD dwMaxLOD
);
HRESULT WINAPI My_IDDSV7_GetLOD(
  LPDIRECTDRAWSURFACE7 lpThis,
  LPDWORD lpdwMaxLOD
);

/**************************************************************************
DETOURS SETUP AND TEARDOWN FUNCTIONS
**************************************************************************/
void HookDirectDrawSurface7Interface  (LPDIRECTDRAWSURFACE7 lpInterface);


