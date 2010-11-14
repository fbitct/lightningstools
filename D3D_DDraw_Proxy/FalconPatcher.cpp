#pragma once
#include "stdafx.h"

HRESULT WINAPI SetFalconWindowToFullScreen(HWND hwnd) 
{
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
