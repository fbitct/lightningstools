#include "StdAfx.h"
/*
PROXY CLASS FOR THE IDIRECT3D7 COM INTERFACE
*/

// This one is set up to work with IDirect3D7 only !!
// ---------------------------------------------------------------------------------------
myID3D7::myID3D7(LPDIRECT3D7 pOriginal)
{
   	m_pID3D7=NULL;
	m_pID3DDevice7=NULL;

	//OutputDebugString("DDRAWPROXY: myID3D7::Constructor reached.\r\n");
	if (pOriginal) 
	{
		m_pID3D7 = pOriginal;
		m_pID3D7->AddRef();
	}
	//OutputDebugString("DDRAWPROXY: myID3D7::Constructor exited.\r\n");
}

// ---------------------------------------------------------------------------------------
myID3D7::~myID3D7(void)
{
	if (m_pID3D7) 
	{
		m_pID3D7->Release();
	}
	//OutputDebugString("DDRAWPROXY: myID3D7::default destructor reached.\r\n");
	//OutputDebugString("DDRAWPROXY: myID3D7::default destructor exited.\r\n");
}

// ---------------------------------------------------------------------------------------
HRESULT WINAPI myID3D7::QueryInterface (REFIID riid, LPVOID* obp)
{
	//OutputDebugString("DDRAWPROXY: myID3D7::QueryInterface reached.\r\n");
	*obp = NULL;

	// call this to increase AddRef at original object
	// and to check if such an interface is there
	//OutputDebugString("DDRAWPROXY: myID3D7::QueryInterface (v7) reached.\r\n");
	 HRESULT hRes = m_pID3D7->QueryInterface(riid, obp); 

	 if (riid==IID_IDirect3D7 && SUCCEEDED(hRes) && *obp) {
		 *obp = static_cast<IDirect3D7 *>(this);
	 }
	//OutputDebugString("DDRAWPROXY: myID3D7::QueryInterface exited.\r\n");
	return (hRes);
}

// ---------------------------------------------------------------------------------------
ULONG   WINAPI myID3D7::AddRef(void)
{
	//OutputDebugString("DDRAWPROXY: myID3D7::AddRef reached.\r\n");
	return m_pID3D7->AddRef();
	//OutputDebugString("DDRAWPROXY: myID3D7::AddRef exited.\r\n");
}

// ---------------------------------------------------------------------------------------
ULONG   WINAPI myID3D7::Release(void)
{
	//OutputDebugString("DDRAWPROXY: myID3D7::Release reached.\r\n");
	// call original routine
	ULONG count=0;
    // in case no further Ref is there, the Original Object has deleted itself
	// so do we here
	if (m_pID3D7) {
		count = m_pID3D7->Release();
		if (count == 0) 
		{
			m_pID3D7 = NULL;		
			delete(this); 
		}
	}
	//OutputDebugString("DDRAWPROXY: myID3D7::Release exited.\r\n");
	return(count);
}

HRESULT WINAPI myID3D7::EnumDevices(LPD3DENUMDEVICESCALLBACK7 lpEnumDevicesCallback,  LPVOID lpUserArg)
{
	//OutputDebugString("DDRAWPROXY: myID3D7::EnumDevices reached.\r\n");
	return m_pID3D7->EnumDevices(lpEnumDevicesCallback, lpUserArg);
	//OutputDebugString("DDRAWPROXY: myID3D7::EnumDevices exited.\r\n");
}
HRESULT WINAPI myID3D7::CreateDevice(
		REFCLSID rclsid,                    
		LPDIRECTDRAWSURFACE7 lpDDS,         
		LPDIRECT3DDEVICE7 *  lplpD3DDevice)
{
	//OutputDebugString("DDRAWPROXY: myID3D7::CreateDevice reached.\r\n");
	HRESULT toReturn=m_pID3D7->CreateDevice(rclsid, lpDDS, lplpD3DDevice);
	if (SUCCEEDED(toReturn) && lplpD3DDevice && *lplpD3DDevice) 
	{
		m_pID3DDevice7 = static_cast<IDirect3DDevice7 *>( new myID3DDevice7(*lplpD3DDevice));
		*lplpD3DDevice= m_pID3DDevice7;
	}
	//OutputDebugString("DDRAWPROXY: myID3D7::CreateDevice exited.\r\n");
	return (toReturn);
}
HRESULT WINAPI myID3D7::CreateVertexBuffer(
		LPD3DVERTEXBUFFERDESC    lpVBDesc, 
		LPDIRECT3DVERTEXBUFFER7* lplpD3DVertexBuffer, 
		DWORD dwFlags 
	)
{
	//OutputDebugString("DDRAWPROXY: myID3D7::CreateVertexBuffer reached.\r\n");
	return m_pID3D7->CreateVertexBuffer(lpVBDesc, lplpD3DVertexBuffer, dwFlags);
	//OutputDebugString("DDRAWPROXY: myID3D7::CreateVertexBuffer exited.\r\n");
}
HRESULT WINAPI myID3D7::EnumZBufferFormats(
		REFCLSID riidDevice, 
		LPD3DENUMPIXELFORMATSCALLBACK lpEnumCallback,
		LPVOID   lpContext
	)
{
	//OutputDebugString("DDRAWPROXY: myID3D7::EnumZBufferFormats reached.\r\n");
	return m_pID3D7->EnumZBufferFormats(riidDevice, lpEnumCallback, lpContext);
	//OutputDebugString("DDRAWPROXY: myID3D7::EnumZBufferFormats exited.\r\n");
}
HRESULT WINAPI myID3D7::EvictManagedTextures(void)
{
	//OutputDebugString("DDRAWPROXY: myID3D7::EvictManagedTextures reached.\r\n");
	return m_pID3D7->EvictManagedTextures();
	//OutputDebugString("DDRAWPROXY: myID3D7::EvictManagedTextures exited.\r\n");
}


