#include "stdafx.h"
#ifndef _FALCONPATCHER_
#define _FALCONPATCHER_

#ifdef __cplusplus
extern "C" {
#endif

extern HRESULT WINAPI SetFalconWindowToFullScreen(HWND hwnd);
HRESULT Copy3DCockpitRTTToSharedMemory(const LPDIRECTDRAWSURFACE7 lp3DCockpitRtt, PSIZE_T bytesWritten);
HRESULT WriteSurfaceToMemoryDDS(const LPDIRECTDRAWSURFACE7 lpSourceSurface, LPCVOID lpMemoryDDS, PSIZE_T pBytesWritten);
HRESULT CreateMatchingSystemMemorySurface(LPDIRECTDRAWSURFACE7 lpSourceSurface, LPDIRECTDRAWSURFACE7 *lplpSystemMemorySurface);
HRESULT OpenTexturesSharedMemoryArea(LPHANDLE lphSharedMemFileMapping, LPVOID *lplpViewOfFile);
HRESULT CloseTexturesSharedMemoryArea(HANDLE hSharedMemFileMapping, LPCVOID lpBaseAddress );

#ifdef __cplusplus
}
#endif

#endif
