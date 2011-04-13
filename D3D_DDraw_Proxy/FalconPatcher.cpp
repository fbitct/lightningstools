#pragma once
#include "stdafx.h"
HRESULT WINAPI SetFalconWindowToFullScreen(HWND hwnd) 
{
	return S_OK;
	LONG style = GetWindowLong(hwnd, GWL_STYLE);
	WINDOWINFO info;
	ZeroMemory(&info, sizeof(info));
	info.cbSize = sizeof(info);
	GetWindowInfo(hwnd,&info);
	style &= (~(WS_BORDER | WS_CAPTION | WS_THICKFRAME | WS_SIZEBOX | WS_SYSMENU));
	SetWindowLong(hwnd, GWL_STYLE, style);
	WINDOWPLACEMENT wpl;
	ZeroMemory(&wpl, sizeof(wpl));
	wpl.length = sizeof(wpl);
	GetWindowPlacement(hwnd,&wpl);  
	wpl.showCmd = SW_SHOWMAXIMIZED;  
	wpl.flags = WPF_RESTORETOMAXIMIZED;  

	POINT pt;
	pt.x = info.rcWindow.left;
	pt.y = info.rcWindow.top;
	HMONITOR monitor = MonitorFromPoint(pt, MONITOR_DEFAULTTONULL);
	MONITORINFOEX monitorInfo;
	ZeroMemory(&monitorInfo, sizeof(MONITORINFOEX));
	monitorInfo.cbSize = sizeof(MONITORINFOEX);
	GetMonitorInfo(monitor, &monitorInfo);

	RECT rect;  
	rect.top = monitorInfo.rcMonitor.top;  
	rect.bottom = monitorInfo.rcMonitor.bottom;  
	rect.left = monitorInfo.rcMonitor.left;  
	rect.right = monitorInfo.rcMonitor.right;  
	wpl.rcNormalPosition = rect;  
	SetWindowPlacement(hwnd,&wpl);  
	SetWindowPos(hwnd, NULL, monitorInfo.rcMonitor.left,monitorInfo.rcMonitor.top, monitorInfo.rcMonitor.right - monitorInfo.rcMonitor.left, monitorInfo.rcMonitor.bottom - monitorInfo.rcMonitor.top, 0);
	return S_OK;
}

HRESULT CreateMatchingSystemMemorySurface(const LPDIRECTDRAWSURFACE7 lpSourceSurface, LPDIRECTDRAWSURFACE7 *lplpSystemMemorySurface) 
{
	LPDIRECTDRAW7 dd = NULL;
	HRESULT hr = lpSourceSurface->GetDDInterface((LPVOID*)&dd);  //silently calls AddRef
	if (FAILED(hr)) return hr;

	DDSURFACEDESC2 sourceDesc; memset(&sourceDesc,0, sizeof(DDSURFACEDESC2)); sourceDesc.dwSize= sizeof(DDSURFACEDESC2);
	hr = lpSourceSurface->GetSurfaceDesc(&sourceDesc);
	
	if (FAILED(hr)) 
	{
		if (dd) dd->Release();
		return hr;
	}

	DDSURFACEDESC2 sysMemSurfaceDesc; memset(&sysMemSurfaceDesc,0, sizeof(DDSURFACEDESC2)); sysMemSurfaceDesc.dwSize= sizeof(DDSURFACEDESC2);
	sysMemSurfaceDesc.dwFlags = DDSD_CAPS | DDSD_WIDTH | DDSD_HEIGHT | DDSD_PIXELFORMAT;
	sysMemSurfaceDesc.ddsCaps.dwCaps = DDSCAPS_SYSTEMMEMORY | DDSCAPS_OFFSCREENPLAIN ;
	sysMemSurfaceDesc.dwWidth= sourceDesc.dwWidth;
	sysMemSurfaceDesc.dwHeight = sourceDesc.dwHeight;
	memcpy(&sysMemSurfaceDesc.ddpfPixelFormat, &sourceDesc.ddpfPixelFormat, sizeof(DDPIXELFORMAT));
	sysMemSurfaceDesc.ddpfPixelFormat.dwFlags &= ~(DDPF_ALPHA |DDPF_ALPHAPIXELS |DDPF_ALPHAPREMULT);

	if (sysMemSurfaceDesc.dwFlags & DDSD_PITCH) 
	{
		sysMemSurfaceDesc.lPitch = sourceDesc.lPitch;
		sysMemSurfaceDesc.dwFlags |= DDSD_PITCH;
	}
	else if (sysMemSurfaceDesc.dwFlags & DDSD_LINEARSIZE) 
	{
		sysMemSurfaceDesc.dwLinearSize= sourceDesc.dwLinearSize;
		sysMemSurfaceDesc.dwFlags |= DDSD_LINEARSIZE;
	}

	hr = dd->CreateSurface(&sysMemSurfaceDesc, lplpSystemMemorySurface, NULL);
	if (dd) dd->Release();

	return hr;
}

HRESULT LazyLockSurface(const LPDIRECTDRAWSURFACE7 lpSurface, LPVOID *lpRawBits) 
{
	RECT lockRect; memset(&lockRect, 0, sizeof(RECT));
	DDSURFACEDESC2 desc;memset(&desc, 0, sizeof(DDSURFACEDESC2)); desc.dwSize= sizeof(DDSURFACEDESC2);
	HRESULT hr = lpSurface->Lock(&lockRect, &desc, DDLOCK_WAIT | DDLOCK_READONLY | DDLOCK_NOSYSLOCK | DDLOCK_NODIRTYUPDATE, NULL);
	if (FAILED(hr)) return hr;
	if (!desc.lpSurface) return E_FAIL;
	*lpRawBits = desc.lpSurface;
	hr = lpSurface->Unlock(&lockRect);
	if (FAILED(hr)) return hr;
	return S_OK;
}

HRESULT Copy3DCockpitRTTToSharedMemory(const LPDIRECTDRAWSURFACE7 lp3DCockpitRtt, PSIZE_T bytesWritten)
{
	static HANDLE hTexturesSharedMem = NULL;
	static LPVOID pTexturesSharedMem =NULL;

	HRESULT hr;
	if (!lp3DCockpitRtt) return E_FAIL;
	if (!hTexturesSharedMem) 
	{
		hr = OpenTexturesSharedMemoryArea(&hTexturesSharedMem,&pTexturesSharedMem);
		if (FAILED(hr)) return hr;
	}

	hr = WriteSurfaceToMemoryDDS (lp3DCockpitRtt, pTexturesSharedMem, bytesWritten);
	if (FAILED(hr)) return hr;

	return S_OK;
}

HRESULT WriteSurfaceToMemoryDDS(const LPDIRECTDRAWSURFACE7 lpSourceSurface, LPCVOID lpBuffer, PSIZE_T pBytesWritten) 
{
	if (!lpSourceSurface || !lpBuffer) return E_INVALIDARG;

	HRESULT hr;

	static LPDIRECTDRAWSURFACE7 lpLastSourceSurface=NULL;
	static DDSURFACEDESC2 sourceSurfaceDesc; 
	static LPDIRECTDRAWSURFACE7 lpSysmemSurface=NULL;
	static DDSURFACEDESC2 sysmemSurfaceDesc; 
	static LPVOID lpSysmemSurfaceBits=NULL;
	
	if (lpSourceSurface != lpLastSourceSurface)
	{
		if (lpSysmemSurface) 
		{
			lpSysmemSurface->Release();
			lpSysmemSurface=NULL;
			lpSysmemSurfaceBits=NULL;
			memset(&sysmemSurfaceDesc, 0, sizeof(DDSURFACEDESC2));sysmemSurfaceDesc.dwSize = sizeof(DDSURFACEDESC2);
		}

		memset(&sourceSurfaceDesc, 0, sizeof(DDSURFACEDESC2));sourceSurfaceDesc.dwSize= sizeof(DDSURFACEDESC2);
		hr = lpSourceSurface->GetSurfaceDesc(&sourceSurfaceDesc);
		if (FAILED(hr)) return hr;
	}
	
	if (!lpSysmemSurface)
	{
		hr =  CreateMatchingSystemMemorySurface(lpSourceSurface, &lpSysmemSurface);
		if (FAILED(hr)) return hr;

		if (!lpSysmemSurface) return E_FAIL;
		hr = lpSysmemSurface->GetSurfaceDesc(&sysmemSurfaceDesc);
		if (FAILED(hr)) return hr;
	}

	if (!lpSysmemSurfaceBits) 
	{
		hr = LazyLockSurface(lpSysmemSurface, &lpSysmemSurfaceBits);
		if (FAILED(hr)) return hr;
	}
	
	if (!lpSysmemSurface || !lpSysmemSurfaceBits) return E_FAIL;

	hr = lpSysmemSurface->BltFast(0,0, lpSourceSurface, NULL, DDBLTFAST_DONOTWAIT | DDBLTFAST_NOCOLORKEY);
	if (FAILED(hr)) return hr;

	SIZE_T dataLength=sourceSurfaceDesc.lPitch * sourceSurfaceDesc.dwHeight;
	DWORD magic = 0x20534444;
	memcpy ((LPVOID)lpBuffer, &magic, sizeof (DWORD));
	memcpy ((((BYTE *)(LPVOID)lpBuffer) + sizeof(DWORD)), &sourceSurfaceDesc, sizeof (DDSURFACEDESC2));
	memcpy ((((BYTE *)(LPVOID)lpBuffer) + sizeof(DWORD)+ sizeof (DDSURFACEDESC2)), lpSysmemSurfaceBits, dataLength);

	if (pBytesWritten)
	{
		*pBytesWritten = sizeof(DWORD)+ sizeof (DDSURFACEDESC2) + dataLength;
	}

	return S_OK;
}

HRESULT OpenTexturesSharedMemoryArea(LPHANDLE lphSharedMemFileMapping, LPVOID *lplpViewOfFile) 
{
	if (!lphSharedMemFileMapping) return E_INVALIDARG;
	if (!lplpViewOfFile) return E_INVALIDARG;

	HANDLE fileMapping = CreateFileMapping((HANDLE)0xFFFFFFFF, NULL, PAGE_READWRITE, 0, (1024*1024*4) +256, "FalconTexturesSharedMemoryArea");
	if (!(fileMapping))
	{
		DWORD lastErr = GetLastError();	
		HRESULT hr = HRESULT_FROM_WIN32(lastErr);
		return hr;
	}
	*lphSharedMemFileMapping = fileMapping;

	*lplpViewOfFile = MapViewOfFile(*lphSharedMemFileMapping , FILE_MAP_WRITE, 0, 0, 0);
	if(!(*lplpViewOfFile))
	{
		DWORD lastErr = GetLastError();	
		HRESULT hr = HRESULT_FROM_WIN32(lastErr);
		return hr;
	}


	return S_OK;
}

HRESULT CloseTexturesSharedMemoryArea(HANDLE hSharedMemFileMapping, LPCVOID lpBaseAddress ) 
{
	if (hSharedMemFileMapping) 
	{
		CloseHandle(hSharedMemFileMapping);
	}
	if (lpBaseAddress) 
	{
		UnmapViewOfFile(lpBaseAddress);
	}
	return S_OK;
}
