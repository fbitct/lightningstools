// gp_ddraw.cpp 
#pragma once
#include "stdafx.h"
#include "resource.h"
// global variables
#pragma data_seg (".DDrawShared")
HINSTANCE           gl_hOriginalDll=NULL;
HINSTANCE           gl_hThisInstance=NULL;
LPDIRECTDRAW7       gl_myIDD=NULL;

#pragma data_seg ()

// ---------------------------------------------------------------------------------------
BOOL APIENTRY DllMain( HANDLE hModule, DWORD  ul_reason_for_call, LPVOID lpReserved)
// ---------------------------------------------------------------------------------------
{
    //OutputDebugString("DDRAWPROXY: DllMain reached.\r\n");
	
	// to avoid compiler lvl4 warnings 
    LPVOID lpDummy = lpReserved;
    lpDummy = NULL;
    
    switch (ul_reason_for_call)
	{
	    case DLL_PROCESS_ATTACH: InitInstance(hModule); break;
	    case DLL_PROCESS_DETACH: ExitInstance(); break;
        
        case DLL_THREAD_ATTACH:  break;
	    case DLL_THREAD_DETACH:  break;
	}
    //OutputDebugString("DDRAWPROXY: DllMain finished.\r\n");
    return(true);
}

// An exported function (faking ddraw.dll's export)
// ---------------------------------------------------------------------------------------
HRESULT WINAPI DirectDrawCreate(GUID FAR *lpGUID, LPDIRECTDRAW FAR *lplpDD, IUnknown FAR *pUnkOuter)
// ---------------------------------------------------------------------------------------
{
	//OutputDebugString("DDRAWPROXY: Exported function DirectDrawCreate reached.\r\n");

	// This dll does not really intercept calls to DirectDrawCreate. We focus DirectDrawCreateEx!!
		
	if (!gl_hOriginalDll) LoadOriginalDll(); // looking for the "right ddraw.dll"
	
	// Hooking DDRAW interface from Original Library IDirectDraw *pDD; 
	typedef HRESULT (WINAPI* DirectDrawCreate_Type)(GUID FAR *, LPDIRECTDRAW FAR *, IUnknown FAR *);
		
    DirectDrawCreate_Type DirectDrawCreate_fn = (DirectDrawCreate_Type) GetProcAddress( gl_hOriginalDll, _T("DirectDrawCreate"));
    
    //Debug
	if (!DirectDrawCreate_fn) 
    {
        //OutputDebugString("DDRAWPROXY: Pointer to original DirectDrawCreate function not received ERROR ****\r\n");
        ::ExitProcess(0);
    }
	//OutputDebugString("DDRAWPROXY: Exported function DirectDrawCreate finished.\r\n");
	
	return (DirectDrawCreate_fn(lpGUID, lplpDD, pUnkOuter));
}


// An exported function (faking ddraw.dll's export)
// ---------------------------------------------------------------------------------------
HRESULT WINAPI DirectDrawCreateEx(GUID FAR * lpGuid, LPVOID  *lplpDD, REFIID  iid,IUnknown FAR *pUnkOuter)
// ---------------------------------------------------------------------------------------
{
	//OutputDebugString("DDRAWPROXY: Exported function DirectDrawCreateEx reached.\r\n");
	
	// This is set up to work for IID_IDirectDraw7 only !!
	//const IID IID_IDirectDraw7_internal = {0x15e65ec0,0x3b9c,0x11d2,0xb9,0x2f,0x00,0x60,0x97,0x97,0xea,0x5b};

	if (!gl_hOriginalDll) LoadOriginalDll(); // looking for the "right ddraw.dll"
	
	// Hooking DDRAW interface from Original Library IDirectDraw *pDD; 
	typedef HRESULT (WINAPI* DirectDrawCreateEx_Type)(GUID FAR *, LPVOID *, REFIID, IUnknown FAR *);
		
    DirectDrawCreateEx_Type DirectDrawCreateEx_fn = (DirectDrawCreateEx_Type) GetProcAddress( gl_hOriginalDll, _T("DirectDrawCreateEx"));
    //Debug
	if (!DirectDrawCreateEx_fn) 
    {
        //OutputDebugString("DDRAWPROXY: Pointer to original DirectDrawCreateEx function not received ERROR ****\r\n");
        ::ExitProcess(0);
    }
	HRESULT h =E_FAIL;
	if (iid == IID_IDirectDraw7 && lplpDD) {
		LPDIRECTDRAW7 dummy=NULL;
		*lplpDD = NULL;
		h = DirectDrawCreateEx_fn(lpGuid, (LPVOID *)&dummy, iid, pUnkOuter);
		if (SUCCEEDED(h) && dummy) {
			gl_myIDD = static_cast<IDirectDraw7 *>( new myIDDraw(dummy));
			*lplpDD = gl_myIDD;
		}
		else 
		{
			//OutputDebugString("DDRAWPROXY: error in DirectDrawCreateEx -- dummy was null or create operation failed.");
		}
	}
	else 
	{
		//OutputDebugString("DDRAWPROXY: DirectDrawCreateEx -- Unknown interface requested.");
		h = DirectDrawCreateEx_fn(lpGuid, lplpDD, iid, pUnkOuter);
	}
	//OutputDebugString("DDRAWPROXY: Exiting DirectDrawCreateEx.\r\n");
	
	return (h);
}

// An exported function (faking ddraw.dll's export)
// ---------------------------------------------------------------------------------------
HRESULT WINAPI DirectDrawCreateClipper(DWORD dwFlags, LPDIRECTDRAWCLIPPER FAR *lplpDDClipper, IUnknown FAR *pUnkOuter)
// ---------------------------------------------------------------------------------------
{
	//OutputDebugString("DDRAWPROXY: Exported function DirectDrawCreateClipper reached.\r\n");
	
	if (!gl_hOriginalDll) LoadOriginalDll(); // looking for the "right ddraw.dll"
	
	// Hooking DDRAW interface from Original Library IDirectDraw *pDD; 
	typedef HRESULT (WINAPI* DirectDrawCreateClipper_Type)(DWORD, LPDIRECTDRAWCLIPPER FAR *, IUnknown FAR *);
		
    DirectDrawCreateClipper_Type DirectDrawCreateClipper_fn = (DirectDrawCreateClipper_Type) GetProcAddress( gl_hOriginalDll, _T("DirectDrawCreateClipper"));
    
    //Debug
	if (!DirectDrawCreateClipper_fn) 
    {
        //OutputDebugString("DDRAWPROXY: Pointer to original DirectDrawCreateClipper function not received ERROR ****\r\n");
        ::ExitProcess(0);
    }
	
	HRESULT h = DirectDrawCreateClipper_fn(dwFlags, lplpDDClipper, pUnkOuter);
	//OutputDebugString("DDRAWPROXY: Exported function DirectDrawCreateClipper finished.\r\n");
		
	return (h);
}

// An exported function (faking ddraw.dll's export)
// ---------------------------------------------------------------------------------------
HRESULT WINAPI GetSurfaceFromDC(int a1, int a2, int a3)
// ---------------------------------------------------------------------------------------
{
	//OutputDebugString("DDRAWPROXY: Exported function GetSurfaceFromDC reached.\r\n");

	if (!gl_hOriginalDll) LoadOriginalDll(); // looking for the "right ddraw.dll"
	
	typedef HRESULT (WINAPI *GetSurfaceFromDC_Type)(int, int, int);
		
    GetSurfaceFromDC_Type GetSurfaceFromDC_fn = (GetSurfaceFromDC_Type) GetProcAddress( gl_hOriginalDll, _T("GetSurfaceFromDC"));
    
    //Debug
	if (!GetSurfaceFromDC_fn) 
    {
        //OutputDebugString("DDRAWPROXY: Pointer to original GetSurfaceFromDC function not received ERROR ****\r\n");
        ::ExitProcess(0);
    }
	
	HRESULT retVal = GetSurfaceFromDC_fn(a1, a2, a3);
	//OutputDebugString("DDRAWPROXY: Exported function GetSurfaceFromDC finished.\r\n");
		
	return (retVal);
}
// An exported function (faking ddraw.dll's export)
// ---------------------------------------------------------------------------------------
HRESULT WINAPI DDGetAttachedSurfaceLcl(int a1, int a2, int a3)
// ---------------------------------------------------------------------------------------
{
	//OutputDebugString("DDRAWPROXY: Exported function DDGetAttachedSurfaceLcl reached.\r\n");
	
	if (!gl_hOriginalDll) LoadOriginalDll(); // looking for the "right ddraw.dll"
	
	typedef HRESULT (WINAPI *DDGetAttachedSurfaceLcl_Type)(int, int, int);
		
    DDGetAttachedSurfaceLcl_Type DDGetAttachedSurfaceLcl_fn = (DDGetAttachedSurfaceLcl_Type) GetProcAddress( gl_hOriginalDll, _T("DDGetAttachedSurfaceLcl"));
    
    //Debug
	if (!DDGetAttachedSurfaceLcl_fn) 
    {
        //OutputDebugString("DDRAWPROXY: Pointer to original DDGetAttachedSurfaceLcl function not received ERROR ****\r\n");
        ::ExitProcess(0);
    }
	
	HRESULT retVal = DDGetAttachedSurfaceLcl_fn(a1, a2, a3);
	//OutputDebugString("DDRAWPROXY: Exported function DDGetAttachedSurfaceLcl finished.\r\n");
		
	return (retVal);
}
// An exported function (faking ddraw.dll's export)
// ---------------------------------------------------------------------------------------
HRESULT WINAPI DDInternalLock(int a1, int a2)
// ---------------------------------------------------------------------------------------
{
	//OutputDebugString("DDRAWPROXY: Exported function DDInternalLock reached.\r\n");

	if (!gl_hOriginalDll) LoadOriginalDll(); // looking for the "right ddraw.dll"
	
	typedef HRESULT (WINAPI *DDInternalLock_Type)(int, int);
		
    DDInternalLock_Type DDInternalLock_fn = (DDInternalLock_Type) GetProcAddress( gl_hOriginalDll, _T("DDInternalLock"));
    
    //Debug
	if (!DDInternalLock_fn) 
    {
        //OutputDebugString("DDRAWPROXY: Pointer to original DDInternalLock function not received ERROR ****\r\n");
        ::ExitProcess(0);
    }
	
	HRESULT retVal = DDInternalLock_fn(a1, a2);
	//OutputDebugString("DDRAWPROXY: Exported function DDInternalLock finished.\r\n");
		
	return (retVal);
}

// An exported function (faking ddraw.dll's export)
// ---------------------------------------------------------------------------------------
HRESULT WINAPI DDInternalUnlock(int a1)
// ---------------------------------------------------------------------------------------
{
	//OutputDebugString("DDRAWPROXY: Exported function DDInternalUnlock reached.\r\n");

	if (!gl_hOriginalDll) LoadOriginalDll(); // looking for the "right ddraw.dll"
	
	typedef HRESULT (WINAPI *DDInternalUnlock_Type)(int);
		
    DDInternalUnlock_Type DDInternalUnlock_fn = (DDInternalUnlock_Type) GetProcAddress( gl_hOriginalDll, _T("DDInternalUnlock"));
    
    //Debug
	if (!DDInternalUnlock_fn) 
    {
        //OutputDebugString("DDRAWPROXY: Pointer to original DDInternalUnlock function not received ERROR ****\r\n");
        ::ExitProcess(0);
    }
	
	HRESULT retVal = DDInternalUnlock_fn(a1);
	//OutputDebugString("DDRAWPROXY: Exported function DDInternalUnlock finished.\r\n");
		
	return (retVal);
}
// An exported function (faking ddraw.dll's export)
// ---------------------------------------------------------------------------------------
HRESULT WINAPI DSoundHelp(HWND hWnd, int a2, int a3)
// ---------------------------------------------------------------------------------------
{
	//OutputDebugString("DDRAWPROXY: Exported function DSoundHelp reached.\r\n");

	if (!gl_hOriginalDll) LoadOriginalDll(); // looking for the "right ddraw.dll"
	
	typedef HRESULT (WINAPI *DSoundHelp_Type)(HWND, int, int);
		
    DSoundHelp_Type DSoundHelp_fn = (DSoundHelp_Type) GetProcAddress( gl_hOriginalDll, _T("DSoundHelp"));
    
    //Debug
	if (!DSoundHelp_fn) 
    {
        //OutputDebugString("DDRAWPROXY: Pointer to original DSoundHelp function not received ERROR ****\r\n");
        ::ExitProcess(0);
    }
	
	HRESULT retVal = DSoundHelp_fn(hWnd, a2, a3);
	//OutputDebugString("DDRAWPROXY: Exported function DSoundHelp finished.\r\n");
		
	return (retVal);
}

// An exported function (faking ddraw.dll's export)
// ---------------------------------------------------------------------------------------
HRESULT WINAPI GetDDSurfaceLocal(int a1, int a2, int a3)
// ---------------------------------------------------------------------------------------
{
	//OutputDebugString("DDRAWPROXY: Exported function GetDDSurfaceLocal reached.\r\n");

	if (!gl_hOriginalDll) LoadOriginalDll(); // looking for the "right ddraw.dll"
	
	typedef HRESULT (WINAPI *GetDDSurfaceLocal_Type)(int, int, int);
		
    GetDDSurfaceLocal_Type GetDDSurfaceLocal_fn = (GetDDSurfaceLocal_Type) GetProcAddress( gl_hOriginalDll, _T("GetDDSurfaceLocal"));
    
    //Debug
	if (!GetDDSurfaceLocal_fn) 
    {
        //OutputDebugString("DDRAWPROXY: Pointer to original GetDDSurfaceLocal function not received ERROR ****\r\n");
        ::ExitProcess(0);
    }
	
	HRESULT retVal = GetDDSurfaceLocal_fn(a1, a2, a3);
	//OutputDebugString("DDRAWPROXY: Exported function GetDDSurfaceLocal finished.\r\n");
		
	return (retVal);
}

// An exported function (faking ddraw.dll's export)
// ---------------------------------------------------------------------------------------
HANDLE WINAPI GetOLEThunkData(int a1)
// ---------------------------------------------------------------------------------------
{
	//OutputDebugString("DDRAWPROXY: Exported function GetOLEThunkData reached.\r\n");

	if (!gl_hOriginalDll) LoadOriginalDll(); // looking for the "right ddraw.dll"
	
	typedef HANDLE (WINAPI *GetOLEThunkData_Type)(int);
		
    GetOLEThunkData_Type GetOLEThunkData_fn = (GetOLEThunkData_Type) GetProcAddress( gl_hOriginalDll, _T("GetOLEThunkData"));
    
    //Debug
	if (!GetOLEThunkData_fn) 
    {
        //OutputDebugString("DDRAWPROXY: Pointer to original GetOLEThunkData function not received ERROR ****\r\n");
        ::ExitProcess(0);
    }
	
	HANDLE retVal = GetOLEThunkData_fn(a1);
	//OutputDebugString("DDRAWPROXY: Exported function GetDDSurfacGetOLEThunkData finished.\r\n");
		
	return (retVal);
}
// An exported function (faking ddraw.dll's export)
// ---------------------------------------------------------------------------------------
HRESULT WINAPI RegisterSpecialCase(int a1, int a2, int a3, int a4)
// ---------------------------------------------------------------------------------------
{
	//OutputDebugString("DDRAWPROXY: Exported function RegisterSpecialCase reached.\r\n");

	if (!gl_hOriginalDll) LoadOriginalDll(); // looking for the "right ddraw.dll"
	
	typedef HRESULT (WINAPI *RegisterSpecialCase_Type)(int, int, int, int);
		
    RegisterSpecialCase_Type RegisterSpecialCase_fn = (RegisterSpecialCase_Type) GetProcAddress( gl_hOriginalDll, _T("RegisterSpecialCase"));
    
    //Debug
	if (!RegisterSpecialCase_fn) 
    {
        //OutputDebugString("DDRAWPROXY: Pointer to original RegisterSpecialCase function not received ERROR ****\r\n");
        ::ExitProcess(0);
    }
	
	HRESULT retVal = RegisterSpecialCase_fn(a1, a2, a3, a4);
	//OutputDebugString("DDRAWPROXY: Exported function RegisterSpecialCase finished.\r\n");
		
	return (retVal);
}

// An exported function (faking ddraw.dll's export)
// ---------------------------------------------------------------------------------------
HRESULT WINAPI CheckFullscreen()
// ---------------------------------------------------------------------------------------
{
	//OutputDebugString("DDRAWPROXY: Exported function CheckFullscreen reached.\r\n");
	
	if (!gl_hOriginalDll) LoadOriginalDll(); // looking for the "right ddraw.dll"
	
	typedef HRESULT (WINAPI *CheckFullscreen_Type)();
		
    CheckFullscreen_Type CheckFullscreen_fn = (CheckFullscreen_Type) GetProcAddress( gl_hOriginalDll, _T("CheckFullscreen"));
    
    //Debug
	if (!CheckFullscreen_fn) 
    {
        //OutputDebugString("DDRAWPROXY: Pointer to original CheckFullscreen function not received ERROR ****\r\n");
        ::ExitProcess(0);
    }
	
	HRESULT retVal = CheckFullscreen_fn();
	//OutputDebugString("DDRAWPROXY: Exported function CheckFullscreen finished.\r\n");
		
	return (retVal);
}



// An exported function (faking ddraw.dll's export)
// ---------------------------------------------------------------------------------------
HRESULT WINAPI DirectDrawEnumerateW(LPDDENUMCALLBACKW lpCallback, LPVOID lpContext)
// ---------------------------------------------------------------------------------------
{
	//OutputDebugString("DDRAWPROXY: Exported function DirectDrawEnumerateW reached.\r\n");

	if (!gl_hOriginalDll) LoadOriginalDll(); // looking for the "right ddraw.dll"
	
	// Hooking DDRAW interface from Original Library IDirectDraw *pDD; 
	typedef HRESULT (WINAPI* DirectDrawEnumerateW_Type)(LPDDENUMCALLBACKW, LPVOID);
		
    DirectDrawEnumerateW_Type DirectDrawEnumerateW_fn = (DirectDrawEnumerateW_Type) GetProcAddress( gl_hOriginalDll, _T("DirectDrawEnumerateW"));
    
    //Debug
	if (!DirectDrawEnumerateW_fn) 
    {
        //OutputDebugString("DDRAWPROXY: Pointer to original DirectDrawEnumerateW function not received ERROR ****\r\n");
        ::ExitProcess(0);
    }
	
	HRESULT h = DirectDrawEnumerateW_fn(lpCallback, lpContext);
	
	//OutputDebugString("DDRAWPROXY: Exported function DirectDrawEnumerateW finished.\r\n");
	return (h); 
}

// An exported function (faking ddraw.dll's export)
// ---------------------------------------------------------------------------------------
HRESULT WINAPI DirectDrawEnumerateA(LPDDENUMCALLBACKA lpCallback, LPVOID lpContext)
// ---------------------------------------------------------------------------------------
{
	//OutputDebugString("DDRAWPROXY: Exported function DirectDrawEnumerateA reached.\r\n");
	
	if (!gl_hOriginalDll) LoadOriginalDll(); // looking for the "right ddraw.dll"
	
	// Hooking DDRAW interface from Original Library IDirectDraw *pDD; 
	typedef HRESULT (WINAPI* DirectDrawEnumerateA_Type)(LPDDENUMCALLBACKA, LPVOID);
		
    DirectDrawEnumerateA_Type DirectDrawEnumerateA_fn = (DirectDrawEnumerateA_Type) GetProcAddress( gl_hOriginalDll, _T("DirectDrawEnumerateA"));
    
    //Debug
	if (!DirectDrawEnumerateA_fn) 
    {
        //OutputDebugString("DDRAWPROXY: Pointer to original DirectDrawEnumerateA function not received ERROR ****\r\n");
        ::ExitProcess(0);
    }
	
	HRESULT h = DirectDrawEnumerateA_fn(lpCallback, lpContext);
	//OutputDebugString("DDRAWPROXY: Exported function DirectDrawEnumerateA finished.\r\n");

	return (h);
}

// An exported function (faking ddraw.dll's export)
// ---------------------------------------------------------------------------------------
HRESULT WINAPI DirectDrawEnumerateExW(LPDDENUMCALLBACKEXW lpCallback, LPVOID lpContext, DWORD dwFlags)
// ---------------------------------------------------------------------------------------
{
	//OutputDebugString("DDRAWPROXY: Exported function DirectDrawEnumerateExW reached.\r\n");
	
	if (!gl_hOriginalDll) LoadOriginalDll(); // looking for the "right ddraw.dll"
	
	// Hooking DDRAW interface from Original Library IDirectDraw *pDD; 
	typedef HRESULT (WINAPI* DirectDrawEnumerateExW_Type)(LPDDENUMCALLBACKEXW, LPVOID, DWORD);
		
    DirectDrawEnumerateExW_Type DirectDrawEnumerateExW_fn = (DirectDrawEnumerateExW_Type) GetProcAddress( gl_hOriginalDll, _T("DirectDrawEnumerateExW"));
    
    //Debug
	if (!DirectDrawEnumerateExW_fn) 
    {
        //OutputDebugString("DDRAWPROXY: Pointer to original DirectDrawEnumerateExW function not received ERROR ****\r\n");
        ::ExitProcess(0);
    }
	
	HRESULT h = DirectDrawEnumerateExW_fn(lpCallback, lpContext, dwFlags);
	//OutputDebugString("DDRAWPROXY: Exported function DirectDrawEnumerateExW finished.\r\n");

	return (h); 
}

// An exported function (faking ddraw.dll's export)
// ---------------------------------------------------------------------------------------
HRESULT WINAPI DirectDrawEnumerateExA(LPDDENUMCALLBACKEXA lpCallback, LPVOID lpContext, DWORD dwFlags)
// ---------------------------------------------------------------------------------------
{
	//OutputDebugString("DDRAWPROXY: Exported function DirectDrawEnumerateExA reached.\r\n");
	
	if (!gl_hOriginalDll) LoadOriginalDll(); // looking for the "right ddraw.dll"
	
	// Hooking DDRAW interface from Original Library IDirectDraw *pDD; 
	typedef HRESULT (WINAPI* DirectDrawEnumerateExA_Type)(LPDDENUMCALLBACKEXA, LPVOID, DWORD);
		
    DirectDrawEnumerateExA_Type DirectDrawEnumerateExA_fn = (DirectDrawEnumerateExA_Type) GetProcAddress( gl_hOriginalDll, _T("DirectDrawEnumerateExA"));
    
    //Debug
	if (!DirectDrawEnumerateExA_fn) 
    {
        //OutputDebugString("DDRAWPROXY: Pointer to original DirectDrawEnumerateExA function not received ERROR ****\r\n");
        ::ExitProcess(0);
    }

	HRESULT h = DirectDrawEnumerateExA_fn(lpCallback, lpContext, dwFlags);
	//OutputDebugString("DDRAWPROXY: Exported function DirectDrawEnumerateExA finished.\r\n");

	return (h);
}

// An exported function (faking ddraw.dll's export) added 2/2008
// ---------------------------------------------------------------------------------------
HRESULT WINAPI AcquireDDThreadLock()
// ---------------------------------------------------------------------------------------
{
	//OutputDebugString("DDRAWPROXY: Exported function AcquireDDThreadLock reached.\r\n");
	
	if (!gl_hOriginalDll) LoadOriginalDll(); // looking for the "right ddraw.dll"
	
	// Hooking DDRAW interface from Original Library IDirectDraw *pDD; 
	typedef HRESULT (WINAPI *AcquireDDThreadLock_Type)();
		
    AcquireDDThreadLock_Type AcquireDDThreadLock_fn = (AcquireDDThreadLock_Type) GetProcAddress( gl_hOriginalDll, _T("AcquireDDThreadLock"));
    
    //Debug
	if (!AcquireDDThreadLock_fn) 
    {
        //OutputDebugString("DDRAWPROXY: Pointer to original AcquireDDThreadLock function not received ERROR ****\r\n");
        ::ExitProcess(0);
    }
	//OutputDebugString("DDRAWPROXY: Exported function AcquireDDThreadLock finished.\r\n");

	return AcquireDDThreadLock_fn();
}

// An exported function (faking ddraw.dll's export) added 2/2008
// ---------------------------------------------------------------------------------------
HRESULT WINAPI ReleaseDDThreadLock()
// ---------------------------------------------------------------------------------------
{
	//OutputDebugString("DDRAWPROXY: Exported function ReleaseDDThreadLock reached.\r\n");
	
	if (!gl_hOriginalDll) LoadOriginalDll(); // looking for the "right ddraw.dll"
	
	// Hooking DDRAW interface from Original Library IDirectDraw *pDD; 
	typedef HRESULT (WINAPI *ReleaseDDThreadLock_Type)();
		
    ReleaseDDThreadLock_Type ReleaseDDThreadLock_fn = (ReleaseDDThreadLock_Type) GetProcAddress( gl_hOriginalDll, _T("ReleaseDDThreadLock"));
    
    //Debug
	if (!ReleaseDDThreadLock_fn) 
    {
        //OutputDebugString("DDRAWPROXY: Pointer to original ReleaseDDThreadLock function not received ERROR ****\r\n");
        ::ExitProcess(0);
    }
	//OutputDebugString("DDRAWPROXY: Exported function ReleaseDDThreadLock finished.\r\n");

	return ReleaseDDThreadLock_fn();
}

// An exported function (faking ddraw.dll's export) added 2/2008
// ---------------------------------------------------------------------------------------
DWORD WINAPI D3DParseUnknownCommand(LPVOID lpCmd, LPVOID *lpRetCmd)
// ---------------------------------------------------------------------------------------
{

	//OutputDebugString("DDRAWPROXY: Exported function D3DParseUnknownCommand reached.\r\n");
	
	if (!gl_hOriginalDll) LoadOriginalDll(); // looking for the "right ddraw.dll"
	
	// Hooking DDRAW interface from Original Library IDirectDraw *pDD; 
	typedef DWORD (WINAPI* D3DParseUnknownCommand_Type)(LPVOID lpCmd, LPVOID *lpRetCmd);
		
    D3DParseUnknownCommand_Type D3DParseUnknownCommand_fn = (D3DParseUnknownCommand_Type) GetProcAddress( gl_hOriginalDll, _T("D3DParseUnknownCommand"));
    
    //Debug
	if (!D3DParseUnknownCommand_fn) 
    {
        //OutputDebugString("DDRAWPROXY: Pointer to original D3DParseUnknownCommand function not received ERROR ****\r\n");
        ::ExitProcess(0);
    }

	DWORD dw = D3DParseUnknownCommand_fn(lpCmd, lpRetCmd);
	//OutputDebugString("DDRAWPROXY: Exported function D3DParseUnknownCommand finished.\r\n");
	
	return(dw);
}

// An exported function (faking ddraw.dll's export) added 2/2008
// ---------------------------------------------------------------------------------------
HRESULT WINAPI CompleteCreateSysmemSurface(int ths, int a2)
// ---------------------------------------------------------------------------------------
{
	//OutputDebugString("DDRAWPROXY: Exported function CompleteCreateSysmemSurface reached.\r\n");

	if (!gl_hOriginalDll) LoadOriginalDll(); // looking for the "right ddraw.dll"
	
	typedef HRESULT (WINAPI  *CompleteCreateSysmemSurface_Type)(int, int);
		
	CompleteCreateSysmemSurface_Type CompleteCreateSysmemSurface_fn = (CompleteCreateSysmemSurface_Type) GetProcAddress( gl_hOriginalDll, _T("CompleteCreateSysmemSurface"));
    
	//Debug
	if (!CompleteCreateSysmemSurface_fn) 
	{
		//OutputDebugString("DDRAWPROXY: Pointer to original CompleteCreateSysmemSurface function not received ERROR ****\r\n");
		::ExitProcess(0);
	}

	HRESULT retVal = CompleteCreateSysmemSurface_fn(ths, a2);
	//OutputDebugString("DDRAWPROXY: Exported function CompleteCreateSysmemSurface finished.\r\n");
	
	return(retVal);
};

// An exported function (faking ddraw.dll's export) added 2/2008
// ---------------------------------------------------------------------------------------
HRESULT WINAPI DllCanUnloadNow(void)
// ---------------------------------------------------------------------------------------
{
	//OutputDebugString("DDRAWPROXY: Exported function DllCanUnloadNow reached.\r\n");
	
	if (!gl_hOriginalDll) LoadOriginalDll(); // looking for the "right ddraw.dll"
	
	typedef HRESULT (WINAPI *DllCanUnloadNow_Type)(void);
	DllCanUnloadNow_Type DllCanUnloadNow_fn = (DllCanUnloadNow_Type) GetProcAddress( gl_hOriginalDll, "DllCanUnloadNow");
    
    // Debug
	if (!DllCanUnloadNow_fn) 
    {
        //OutputDebugString("DI-DDRAWPROXY: Pointer to original DllCanUnloadNow function not received ERROR ****");
        ::ExitProcess(0);
    }

	// Call original dll and return
	HRESULT h = DllCanUnloadNow_fn();
	//OutputDebugString("DDRAWPROXY: Exported function DllCanUnloadNow finished.\r\n");

	return(h);
}


// An exported function (faking ddraw.dll's export) added 2/2008
// ---------------------------------------------------------------------------------------
HRESULT WINAPI DllGetClassObject (REFCLSID rclsid,REFIID riid,LPVOID * ppv)
// ---------------------------------------------------------------------------------------
{
	//OutputDebugString("DDRAWPROXY: Exported function DllGetClassObject reached.\r\n");
	
	if (!gl_hOriginalDll) LoadOriginalDll(); // looking for the "right ddraw.dll"
	
	typedef HRESULT (WINAPI *DllGetClassObject_Type)(REFCLSID rclsid,REFIID riid,LPVOID * ppv);
	DllGetClassObject_Type DllGetClassObject_fn = (DllGetClassObject_Type) GetProcAddress( gl_hOriginalDll, "DllGetClassObject");
    
    // Debug
	if (!DllGetClassObject_fn) 
    {
        //OutputDebugString("DDRAWPROXY: Pointer to original DllGetClassObject function not received ERROR ****");
        ::ExitProcess(0);
    }
	HRESULT h=E_FAIL;
	h = DllGetClassObject_fn(rclsid, riid, ppv);
	//OutputDebugString("DDRAWPROXY: Exported function DllGetClassObject finished.\r\n");

	return(h);
}


// ---------------------------------------------------------------------------------------
void InitInstance(HANDLE hModule) 
// ---------------------------------------------------------------------------------------
{
	//OutputDebugString("DDRAWPROXY: InitInstance reached.");

	// Initialisation
	gl_hOriginalDll        = NULL;
	gl_hThisInstance       = NULL;
	// Storing Instance handle into global var
	gl_hThisInstance = (HINSTANCE)  hModule;

	//OutputDebugString("DDRAWPROXY: InitInstance finished.");
}

// ---------------------------------------------------------------------------------------
void LoadOriginalDll(void)
// ---------------------------------------------------------------------------------------
{
    //OutputDebugString("DDRAWPROXY: LoadOriginalDll reached.\r\n");
	
	char buffer[MAX_PATH]; 
    
    // Getting path to system dir and to ddraw.dll
	::GetSystemDirectory(buffer,MAX_PATH);
	// Append dll name
	strcat_s(buffer,"\\ddraw.dll");
	if (!gl_hOriginalDll) gl_hOriginalDll = ::LoadLibrary(buffer);

	// Debug
	if (!gl_hOriginalDll)
	{
		//OutputDebugString("DDRAWPROXY: Original ddraw.dll not loaded ERROR ****\r\n");
		::ExitProcess(0); // exit the hard way
	}
    //OutputDebugString("DDRAWPROXY: LoadOriginalDll finished.\r\n");
}


// ---------------------------------------------------------------------------------------
void ExitInstance() 
// ---------------------------------------------------------------------------------------
{
	//OutputDebugString("DDRAWPROXY: ExitInstance reached.\r\n");
    
    if (gl_hOriginalDll)
	{
		::FreeLibrary(gl_hOriginalDll);
	    gl_hOriginalDll = NULL;  
	}
	//OutputDebugString("DDRAWPROXY: ExitInstance finished.\r\n");
}


