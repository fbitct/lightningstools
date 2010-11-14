#pragma once
// Exported function
HRESULT WINAPI DirectDrawCreate(GUID FAR *lpGUID, LPDIRECTDRAW FAR *lplpDD, IUnknown FAR *pUnkOuter);
HRESULT WINAPI DirectDrawCreateEx(GUID FAR *lpGuid, LPVOID  *lplpDD, REFIID  iid,IUnknown FAR *pUnkOuter);
HRESULT WINAPI DirectDrawCreateClipper(DWORD dwFlags, LPDIRECTDRAWCLIPPER FAR *lplpDDClipper, IUnknown FAR *pUnkOuter);
HRESULT WINAPI DirectDrawEnumerateW(LPDDENUMCALLBACKW lpCallback, LPVOID lpContext);
HRESULT WINAPI DirectDrawEnumerateA(LPDDENUMCALLBACKA lpCallback, LPVOID lpContext);
HRESULT WINAPI DirectDrawEnumerateExW(LPDDENUMCALLBACKEXW lpCallback, LPVOID lpContext, DWORD dwFlags);
HRESULT WINAPI DirectDrawEnumerateExA(LPDDENUMCALLBACKEXA lpCallback, LPVOID lpContext, DWORD dwFlags);


HRESULT WINAPI  AcquireDDThreadLock();
HRESULT WINAPI  ReleaseDDThreadLock();
DWORD WINAPI D3DParseUnknownCommand(LPVOID lpCmd, LPVOID *lpRetCmd);
HRESULT WINAPI DllCanUnloadNow(void);
HRESULT WINAPI DllGetClassObject (REFCLSID rclsid,REFIID riid,LPVOID * ppv);
HRESULT WINAPI GetSurfaceFromDC(int a1, int a2, int a3);
HRESULT WINAPI CheckFullscreen();
HRESULT WINAPI DDGetAttachedSurfaceLcl(int a1, int a2, int a3);
HRESULT WINAPI DDInternalLock(int a1, int a2);
HRESULT WINAPI DDInternalUnlock(int a1);
HRESULT WINAPI DSoundHelp(HWND hWnd, int a2, int a3);
HRESULT WINAPI GetDDSurfaceLocal(int a1, int a2, int a3);
HANDLE WINAPI GetOLEThunkData(int a1);
HRESULT WINAPI RegisterSpecialCase(int a1, int a2, int a3, int a4);
HRESULT WINAPI CompleteCreateSysmemSurface(int ths, int a2);

// regular functions
void InitInstance(HANDLE hModule);
void ExitInstance(void);
void LoadOriginalDll(void);

