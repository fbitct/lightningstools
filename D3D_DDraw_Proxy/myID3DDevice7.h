#pragma once

// This one is set up to work with IDirect3DDevice7 only !!

class myID3DDevice7 : public IDirect3DDevice7
{
public:
    myID3DDevice7(LPDIRECT3DDEVICE7 FAR pOriginal);
    virtual ~myID3DDevice7(void);

	// IUnknown functions
	HRESULT WINAPI IUnknown::QueryInterface (REFIID riid, LPVOID* obp);
    ULONG   WINAPI IUnknown::AddRef(void);
    ULONG   WINAPI IUnknown::Release(void);

	// v7 functions
    HRESULT  WINAPI IDirect3DDevice7::GetCaps(LPD3DDEVICEDESC7 lpD3DDevDesc);
    HRESULT  WINAPI IDirect3DDevice7::EnumTextureFormats( LPD3DENUMPIXELFORMATSCALLBACK lpd3dEnumPixelProc,LPVOID lpArg );
    HRESULT  WINAPI IDirect3DDevice7::BeginScene(void);
    HRESULT  WINAPI IDirect3DDevice7::EndScene(void);
    HRESULT  WINAPI IDirect3DDevice7::GetDirect3D(LPDIRECT3D7 *lplpD3D);
    HRESULT  WINAPI IDirect3DDevice7::SetRenderTarget(LPDIRECTDRAWSURFACE7 lpNewRenderTarget, DWORD dwFlags);
    HRESULT  WINAPI IDirect3DDevice7::GetRenderTarget(LPDIRECTDRAWSURFACE7 *lplpRenderTarget);
    HRESULT  WINAPI IDirect3DDevice7::Clear(DWORD dwCount, LPD3DRECT lpRects, DWORD dwFlags,DWORD dwColor, D3DVALUE dvZ,DWORD dwStencil);
    HRESULT  WINAPI IDirect3DDevice7::SetTransform(D3DTRANSFORMSTATETYPE dtstTransformStateType,LPD3DMATRIX lpD3DMatrix   );
    HRESULT  WINAPI IDirect3DDevice7::GetTransform(D3DTRANSFORMSTATETYPE dtstTransformStateType, LPD3DMATRIX lpD3DMatrix);
    HRESULT  WINAPI IDirect3DDevice7::SetViewport(LPD3DVIEWPORT7 lpViewport);
    HRESULT  WINAPI IDirect3DDevice7::MultiplyTransform(D3DTRANSFORMSTATETYPE dtstTransformStateType, LPD3DMATRIX lpD3DMatrix);
    HRESULT  WINAPI IDirect3DDevice7::GetViewport(LPD3DVIEWPORT7 lpViewport);
    HRESULT  WINAPI IDirect3DDevice7::SetMaterial(LPD3DMATERIAL7 lpMaterial);
    HRESULT  WINAPI IDirect3DDevice7::GetMaterial(LPD3DMATERIAL7 lpMaterial);
    HRESULT  WINAPI IDirect3DDevice7::SetLight(DWORD dwLightIndex, LPD3DLIGHT7 lpLight);
    HRESULT  WINAPI IDirect3DDevice7::GetLight(DWORD dwLightIndex, LPD3DLIGHT7 lpLight);
    HRESULT  WINAPI IDirect3DDevice7::SetRenderState(D3DRENDERSTATETYPE dwRenderStateType,DWORD dwRenderState);
    HRESULT  WINAPI IDirect3DDevice7::GetRenderState(D3DRENDERSTATETYPE dwRenderStateType, LPDWORD lpdwRenderState);
    HRESULT  WINAPI IDirect3DDevice7::BeginStateBlock(void);
    HRESULT  WINAPI IDirect3DDevice7::EndStateBlock(LPDWORD lpdwBlockHandle);
    HRESULT  WINAPI IDirect3DDevice7::PreLoad(LPDIRECTDRAWSURFACE7 lpddsTexture);
    HRESULT  WINAPI IDirect3DDevice7::DrawPrimitive(D3DPRIMITIVETYPE dptPrimitiveType,DWORD  dwVertexTypeDesc,LPVOID lpvVertices,DWORD  dwVertexCount,DWORD dwFlags);
    HRESULT  WINAPI IDirect3DDevice7::DrawIndexedPrimitive(D3DPRIMITIVETYPE d3dptPrimitiveType,DWORD  dwVertexTypeDesc,LPVOID lpvVertices,DWORD  dwVertexCount,LPWORD lpwIndices,DWORD  dwIndexCount,DWORD  dwFlags);
    HRESULT  WINAPI IDirect3DDevice7::SetClipStatus(LPD3DCLIPSTATUS lpD3DClipStatus);
    HRESULT  WINAPI IDirect3DDevice7::GetClipStatus(LPD3DCLIPSTATUS lpD3DClipStatus);
    HRESULT  WINAPI IDirect3DDevice7::DrawPrimitiveStrided(D3DPRIMITIVETYPE dptPrimitiveType,DWORD  dwVertexTypeDesc,LPD3DDRAWPRIMITIVESTRIDEDDATA lpVertexArray,DWORD  dwVertexCount,DWORD  dwFlags);
    HRESULT  WINAPI IDirect3DDevice7::DrawIndexedPrimitiveStrided(D3DPRIMITIVETYPE d3dptPrimitiveType,DWORD  dwVertexTypeDesc,LPD3DDRAWPRIMITIVESTRIDEDDATA lpVertexArray,DWORD  dwVertexCount,LPWORD lpwIndices,DWORD  dwIndexCount,DWORD  dwFlags);
    HRESULT  WINAPI IDirect3DDevice7::DrawPrimitiveVB(D3DPRIMITIVETYPE d3dptPrimitiveType,LPDIRECT3DVERTEXBUFFER7 lpd3dVertexBuffer,DWORD dwStartVertex,DWORD dwNumVertices,DWORD dwFlags);
    HRESULT  WINAPI IDirect3DDevice7::DrawIndexedPrimitiveVB(D3DPRIMITIVETYPE d3dptPrimitiveType,LPDIRECT3DVERTEXBUFFER7 lpd3dVertexBuffer,DWORD  dwStartVertex,DWORD  dwNumVertices,LPWORD lpwIndices,DWORD  dwIndexCount,DWORD  dwFlags);
    HRESULT  WINAPI IDirect3DDevice7::ComputeSphereVisibility(LPD3DVECTOR lpCenters,LPD3DVALUE lpRadii,DWORD dwNumSpheres,DWORD dwFlags,LPDWORD lpdwReturnValues);
    HRESULT  WINAPI IDirect3DDevice7::GetTexture(DWORD dwStage,LPDIRECTDRAWSURFACE7 * lplpTexture);
    HRESULT  WINAPI IDirect3DDevice7::SetTexture(DWORD dwStage,LPDIRECTDRAWSURFACE7 lpTexture);
    HRESULT  WINAPI IDirect3DDevice7::GetTextureStageState(DWORD dwStage,D3DTEXTURESTAGESTATETYPE dwState,LPDWORD lpdwValue);
    HRESULT  WINAPI IDirect3DDevice7::SetTextureStageState(DWORD dwStage,D3DTEXTURESTAGESTATETYPE dwState,DWORD dwValue);
    HRESULT  WINAPI IDirect3DDevice7::ValidateDevice(LPDWORD lpdwPasses);
    HRESULT  WINAPI IDirect3DDevice7::ApplyStateBlock(DWORD dwBlockHandle);
    HRESULT  WINAPI IDirect3DDevice7::CaptureStateBlock(DWORD dwBlockHandle);
    HRESULT  WINAPI IDirect3DDevice7::DeleteStateBlock(DWORD dwBlockHandle);
    HRESULT  WINAPI IDirect3DDevice7::CreateStateBlock(D3DSTATEBLOCKTYPE d3dsbType,LPDWORD lpdwBlockHandle);
    HRESULT  WINAPI IDirect3DDevice7::Load(LPDIRECTDRAWSURFACE7 lpDestTex,LPPOINT lpDestPoint,LPDIRECTDRAWSURFACE7 lpSrcTex,LPRECT lprcSrcRect,DWORD dwFlags);
    HRESULT  WINAPI IDirect3DDevice7::LightEnable(DWORD dwLightIndex,BOOL bEnable);
    HRESULT  WINAPI IDirect3DDevice7::GetLightEnable(DWORD dwLightIndex,BOOL* pbEnable);
    HRESULT  WINAPI IDirect3DDevice7::SetClipPlane(DWORD dwIndex, D3DVALUE* pPlaneEquation);
    HRESULT  WINAPI IDirect3DDevice7::GetClipPlane(DWORD dwIndex, D3DVALUE* pPlaneEquation);
    HRESULT  WINAPI IDirect3DDevice7::GetInfo(DWORD  dwDevInfoID, LPVOID pDevInfoStruct, DWORD  dwSize);


private:
   	LPDIRECT3DDEVICE7 FAR myID3DDevice7::m_pID3DDevice7;
	
};

