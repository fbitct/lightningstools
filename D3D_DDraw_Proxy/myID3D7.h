#pragma once

// This one is set up to work with IDirect3D7 only !!

class myID3D7 : public IDirect3D7
{
public:
    myID3D7(LPDIRECT3D7 FAR pOriginal);
    virtual ~myID3D7(void);

	// IUnknown functions
	HRESULT WINAPI IUnknown::QueryInterface (REFIID riid, LPVOID* obp);
    ULONG   WINAPI IUnknown::AddRef(void);
    ULONG   WINAPI IUnknown::Release(void);

	// v7 functions
	HRESULT WINAPI IDirect3D7::EnumDevices(
		LPD3DENUMDEVICESCALLBACK7 lpEnumDevicesCallback,  
		LPVOID lpUserArg                                 
	);
	HRESULT WINAPI IDirect3D7::CreateDevice(
		REFCLSID rclsid,                    
		LPDIRECTDRAWSURFACE7 lpDDS,         
		LPDIRECT3DDEVICE7 *  lplpD3DDevice
		);
    HRESULT WINAPI IDirect3D7::CreateVertexBuffer(
		LPD3DVERTEXBUFFERDESC    lpVBDesc, 
		LPDIRECT3DVERTEXBUFFER7* lplpD3DVertexBuffer, 
		DWORD dwFlags 
		);
    HRESULT WINAPI IDirect3D7::EnumZBufferFormats(
		REFCLSID riidDevice, 
		LPD3DENUMPIXELFORMATSCALLBACK lpEnumCallback,
		LPVOID   lpContext
		);
    HRESULT WINAPI IDirect3D7::EvictManagedTextures(void);



private:
   	LPDIRECT3D7 FAR m_pID3D7;
	LPDIRECT3DDEVICE7 FAR m_pID3DDevice7;
};

