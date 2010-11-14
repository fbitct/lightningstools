#include "StdAfx.h"
/*
PROXY CLASS FOR THE IDIRECT3DDEVICE7 COM INTERFACE
*/

// This one is set up to work with IDirect3DDevice7 only !!
// ---------------------------------------------------------------------------------------
myID3DDevice7::myID3DDevice7(LPDIRECT3DDEVICE7 pOriginal)
{
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::Constructor reached.\r\n");
	m_pID3DDevice7 = pOriginal;
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::Constructor exited.\r\n");
}

// ---------------------------------------------------------------------------------------
myID3DDevice7::~myID3DDevice7(void)
{
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::default Constructor reached.\r\n");
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::default Constructor exited.\r\n");
}

// ---------------------------------------------------------------------------------------
HRESULT WINAPI myID3DDevice7::QueryInterface (REFIID riid, LPVOID* obp)
{
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::QueryInterface reached.\r\n");
	*obp = NULL;

	// call this to increase AddRef at original object
	// and to check if such an interface is there
	HRESULT hRes= m_pID3DDevice7->QueryInterface(riid, obp); 
	if (riid == IID_IDirect3DDevice7 && *obp && hRes == DD_OK) {
		*obp = static_cast<IDirect3DDevice7 *>(this);
	}
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::QueryInterface exited.\r\n");
	return (hRes);
}

// ---------------------------------------------------------------------------------------
ULONG   WINAPI myID3DDevice7::AddRef(void)
{
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::AddRef reached.\r\n");
	return m_pID3DDevice7->AddRef();
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::AddRef exited.\r\n");
}

// ---------------------------------------------------------------------------------------
ULONG   WINAPI myID3DDevice7::Release(void)
{
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::Release reached.\r\n");
	// call original routine
	ULONG count=0;
	if (m_pID3DDevice7) {
		count = m_pID3DDevice7->Release();
		// in case no further Ref is there, the Original Object has deleted itself
		// so do we here
		if (count == 0) 
		{
			m_pID3DDevice7 = NULL;		
			delete(this); 
		}
	}
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::Release exited.\r\n");
	return(count);
}

HRESULT  WINAPI myID3DDevice7::GetCaps(LPD3DDEVICEDESC7 lpD3DDevDesc)
{
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::GetCaps reached.\r\n");
	return m_pID3DDevice7->GetCaps(lpD3DDevDesc);
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::GetCaps exited.\r\n");
}
HRESULT  WINAPI myID3DDevice7::EnumTextureFormats(LPD3DENUMPIXELFORMATSCALLBACK lpd3dEnumPixelProc,LPVOID lpArg )
{
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::EnumTextureFormats reached.\r\n");
	return m_pID3DDevice7->EnumTextureFormats(lpd3dEnumPixelProc, lpArg);
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::EnumTextureFormats exited.\r\n");
}
HRESULT  WINAPI myID3DDevice7::BeginScene(void)
{
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::BeginScene reached.\r\n");
	return m_pID3DDevice7->BeginScene();
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::BeginScene exited.\r\n");
}
HRESULT  WINAPI myID3DDevice7::EndScene(void)
{
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::EndScene reached.\r\n");
	return m_pID3DDevice7->EndScene();
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::EndScene exited.\r\n");
}
HRESULT  WINAPI myID3DDevice7::GetDirect3D(LPDIRECT3D7 *lplpD3D)
{
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::GetDirect3D reached.\r\n");
	HRESULT toReturn=E_FAIL;
	LPDIRECT3D7 newDirect3D;		
	*lplpD3D = NULL;
	toReturn=m_pID3DDevice7->GetDirect3D(&newDirect3D);
	if (newDirect3D && toReturn == DD_OK) {
		*lplpD3D = static_cast<IDirect3D7 *>( new myID3D7(newDirect3D));
	}
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::GetDirect3D exited.\r\n");
	return (toReturn);
}
HRESULT  WINAPI myID3DDevice7::SetRenderTarget(LPDIRECTDRAWSURFACE7 lpNewRenderTarget, DWORD dwFlags)
{
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::SetRenderTarget reached.\r\n");
	return m_pID3DDevice7->SetRenderTarget(lpNewRenderTarget,dwFlags);
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::SetRenderTarget exited.\r\n");
}
HRESULT  WINAPI myID3DDevice7::GetRenderTarget(LPDIRECTDRAWSURFACE7 *lplpRenderTarget)
{
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::GetRenderTarget reached.\r\n");
	return m_pID3DDevice7->GetRenderTarget(lplpRenderTarget);
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::GetRenderTarget exited.\r\n");
}
HRESULT  WINAPI myID3DDevice7::Clear(DWORD dwCount, LPD3DRECT lpRects, DWORD dwFlags,DWORD dwColor, D3DVALUE dvZ,DWORD dwStencil)
{
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::Clear reached.\r\n");
	return m_pID3DDevice7->Clear(dwCount, lpRects, dwFlags, dwColor, dvZ,dwStencil);
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::Clear exited.\r\n");
}
HRESULT  WINAPI myID3DDevice7::SetTransform(D3DTRANSFORMSTATETYPE dtstTransformStateType,LPD3DMATRIX lpD3DMatrix)
{
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::SetTransform reached.\r\n");
	HRESULT toReturn=E_FAIL;
	if (m_pID3DDevice7) {
		toReturn=m_pID3DDevice7->SetTransform(dtstTransformStateType, lpD3DMatrix);
	}
	return (toReturn);
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::SetTransform exited.\r\n");
}
HRESULT  WINAPI myID3DDevice7::GetTransform(D3DTRANSFORMSTATETYPE dtstTransformStateType, LPD3DMATRIX lpD3DMatrix)
{
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::GetTransform reached.\r\n");
	HRESULT toReturn=E_FAIL;
	if (m_pID3DDevice7) {
		toReturn=m_pID3DDevice7->GetTransform(dtstTransformStateType, lpD3DMatrix);
	}
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::GetTransform exited.\r\n");
	return (toReturn);
}
HRESULT  WINAPI myID3DDevice7::SetViewport(LPD3DVIEWPORT7 lpViewport)
{
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::SetViewport reached.\r\n");
	return m_pID3DDevice7->SetViewport(lpViewport);
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::SetViewport exited.\r\n");
}
HRESULT  WINAPI myID3DDevice7::MultiplyTransform(D3DTRANSFORMSTATETYPE dtstTransformStateType, LPD3DMATRIX lpD3DMatrix)
{
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::MultiplyTransform reached.\r\n");
	return m_pID3DDevice7->MultiplyTransform(dtstTransformStateType, lpD3DMatrix);
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::MultiplyTransform exited.\r\n");
}
HRESULT  WINAPI myID3DDevice7::GetViewport(LPD3DVIEWPORT7 lpViewport)
{
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::GetViewport reached.\r\n");
	return m_pID3DDevice7->GetViewport(lpViewport);
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::GetViewport exited.\r\n");
}
HRESULT  WINAPI myID3DDevice7::SetMaterial(LPD3DMATERIAL7 lpMaterial)
{
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::SetMaterial reached.\r\n");
	return m_pID3DDevice7->SetMaterial(lpMaterial);
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::SetMaterial exited.\r\n");
}
HRESULT  WINAPI myID3DDevice7::GetMaterial(LPD3DMATERIAL7 lpMaterial)
{
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::GetMaterial reached.\r\n");
	return m_pID3DDevice7->GetMaterial(lpMaterial);
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::GetMaterial exited.\r\n");
}
HRESULT  WINAPI myID3DDevice7::SetLight(DWORD dwLightIndex, LPD3DLIGHT7 lpLight)
{
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::SetLight reached.\r\n");
	return m_pID3DDevice7->SetLight(dwLightIndex, lpLight);
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::SetLight exited.\r\n");
}
HRESULT  WINAPI myID3DDevice7::GetLight(DWORD dwLightIndex, LPD3DLIGHT7 lpLight)
{
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::GetLight reached.\r\n");
	return m_pID3DDevice7->GetLight(dwLightIndex, lpLight);
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::GetLight exited.\r\n");
}
HRESULT  WINAPI myID3DDevice7::SetRenderState(D3DRENDERSTATETYPE dwRenderStateType,DWORD dwRenderState)
{
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::SetRenderState reached.\r\n");
	return m_pID3DDevice7->SetRenderState(dwRenderStateType, dwRenderState);
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::SetRenderState exited.\r\n");
}
HRESULT  WINAPI myID3DDevice7::GetRenderState(D3DRENDERSTATETYPE dwRenderStateType, LPDWORD lpdwRenderState)
{
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::GetRenderState reached.\r\n");
	return m_pID3DDevice7->GetRenderState(dwRenderStateType, lpdwRenderState);
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::GetRenderState exited.\r\n");
}
HRESULT  WINAPI myID3DDevice7::BeginStateBlock(void)
{
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::BeginStateBlock reached.\r\n");
    return m_pID3DDevice7->BeginStateBlock();
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::BeginStateBlock exited.\r\n");
}
HRESULT  WINAPI myID3DDevice7::EndStateBlock(LPDWORD lpdwBlockHandle)
{
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::EndStateBlock reached.\r\n");
	return m_pID3DDevice7->EndStateBlock(lpdwBlockHandle);
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::EndStateBlock exited.\r\n");
}
HRESULT  WINAPI myID3DDevice7::PreLoad(LPDIRECTDRAWSURFACE7 lpddsTexture)
{
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::PreLoad reached.\r\n");
	return m_pID3DDevice7->PreLoad(lpddsTexture);
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::PreLoad exited.\r\n");
}
HRESULT  WINAPI myID3DDevice7::DrawPrimitive(D3DPRIMITIVETYPE dptPrimitiveType,DWORD  dwVertexTypeDesc,LPVOID lpvVertices,DWORD  dwVertexCount,DWORD dwFlags)
{
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::DrawPrimitive reached.\r\n");
	return m_pID3DDevice7->DrawPrimitive(dptPrimitiveType, dwVertexTypeDesc,lpvVertices , dwVertexCount, dwFlags);
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::DrawPrimitive exited.\r\n");
}
HRESULT  WINAPI myID3DDevice7::DrawIndexedPrimitive(D3DPRIMITIVETYPE d3dptPrimitiveType,DWORD  dwVertexTypeDesc,LPVOID lpvVertices,DWORD  dwVertexCount,LPWORD lpwIndices,DWORD  dwIndexCount,DWORD  dwFlags)
{
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::DrawIndexedPrimitive reached.\r\n");
	return m_pID3DDevice7->DrawIndexedPrimitive(d3dptPrimitiveType, dwVertexTypeDesc, lpvVertices, dwVertexCount, lpwIndices,dwIndexCount, dwFlags);
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::DrawIndexedPrimitive exited.\r\n");
}
HRESULT  WINAPI myID3DDevice7::SetClipStatus(LPD3DCLIPSTATUS lpD3DClipStatus)
{
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::SetClipStatus reached.\r\n");
	return m_pID3DDevice7->SetClipStatus(lpD3DClipStatus);
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::SetClipStatus exited.\r\n");
}
HRESULT  WINAPI myID3DDevice7::GetClipStatus(LPD3DCLIPSTATUS lpD3DClipStatus)
{
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::GetClipStatus reached.\r\n");
	return m_pID3DDevice7->GetClipStatus(lpD3DClipStatus);
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::GetClipStatus exited.\r\n");
}
HRESULT  WINAPI myID3DDevice7::DrawPrimitiveStrided(D3DPRIMITIVETYPE dptPrimitiveType,DWORD  dwVertexTypeDesc,LPD3DDRAWPRIMITIVESTRIDEDDATA lpVertexArray,DWORD  dwVertexCount,DWORD  dwFlags)
{
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::DrawPrimitiveStrided reached.\r\n");
	return m_pID3DDevice7->DrawPrimitiveStrided(dptPrimitiveType, dwVertexTypeDesc, lpVertexArray, dwVertexCount, dwFlags);
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::DrawPrimitiveStrided exited.\r\n");
}
HRESULT  WINAPI myID3DDevice7::DrawIndexedPrimitiveStrided(D3DPRIMITIVETYPE d3dptPrimitiveType,DWORD  dwVertexTypeDesc,LPD3DDRAWPRIMITIVESTRIDEDDATA lpVertexArray,DWORD  dwVertexCount,LPWORD lpwIndices,DWORD  dwIndexCount,DWORD  dwFlags)
{
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::DrawIndexedPrimitiveStrided reached.\r\n");
	return m_pID3DDevice7->DrawIndexedPrimitiveStrided(d3dptPrimitiveType, dwVertexTypeDesc, lpVertexArray,dwVertexCount, lpwIndices, dwIndexCount, dwFlags);
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::DrawIndexedPrimitiveStrided exited.\r\n");
}
HRESULT  WINAPI myID3DDevice7::DrawPrimitiveVB(D3DPRIMITIVETYPE d3dptPrimitiveType,LPDIRECT3DVERTEXBUFFER7 lpd3dVertexBuffer,DWORD dwStartVertex,DWORD dwNumVertices,DWORD dwFlags)
{
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::DrawPrimitiveVB reached.\r\n");
	return m_pID3DDevice7->DrawPrimitiveVB(d3dptPrimitiveType,lpd3dVertexBuffer, dwStartVertex, dwNumVertices, dwFlags);
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::DrawPrimitiveVB exited.\r\n");
}
HRESULT  WINAPI myID3DDevice7::DrawIndexedPrimitiveVB(D3DPRIMITIVETYPE d3dptPrimitiveType,LPDIRECT3DVERTEXBUFFER7 lpd3dVertexBuffer,DWORD  dwStartVertex,DWORD  dwNumVertices,LPWORD lpwIndices,DWORD  dwIndexCount,DWORD  dwFlags)
{
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::DrawIndexedPrimitiveVB reached.\r\n");
	return m_pID3DDevice7->DrawIndexedPrimitiveVB(d3dptPrimitiveType, lpd3dVertexBuffer, dwStartVertex, dwNumVertices, lpwIndices, dwIndexCount, dwFlags);
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::DrawIndexedPrimitiveVB exited.\r\n");
}
HRESULT  WINAPI myID3DDevice7::ComputeSphereVisibility(LPD3DVECTOR lpCenters,LPD3DVALUE lpRadii,DWORD dwNumSpheres,DWORD dwFlags,LPDWORD lpdwReturnValues)
{
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::ComputeSphereVisibility reached.\r\n");
	return m_pID3DDevice7->ComputeSphereVisibility(lpCenters, lpRadii, dwNumSpheres, dwFlags, lpdwReturnValues);
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::ComputeSphereVisibility exite.\r\n");
}
HRESULT  WINAPI myID3DDevice7::GetTexture(DWORD dwStage,LPDIRECTDRAWSURFACE7 * lplpTexture)
{
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::GetTexture reached.\r\n");
	return m_pID3DDevice7->GetTexture(dwStage, lplpTexture);
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::GetTexture exited.\r\n");
}
HRESULT  WINAPI myID3DDevice7::SetTexture(DWORD dwStage,LPDIRECTDRAWSURFACE7 lpTexture)
{
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::SetTexture reached.\r\n");
	HRESULT toReturn = NULL;
	try {
		if (m_pID3DDevice7) {
			toReturn = m_pID3DDevice7->SetTexture(dwStage, lpTexture);
		}
		else 
		{
			toReturn = S_FALSE;
		}
	}
	catch (...) 
	{
		toReturn = S_FALSE;
	}
	return toReturn;					
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::SetTexture exited.\r\n");
}
HRESULT  WINAPI myID3DDevice7::GetTextureStageState(DWORD dwStage,D3DTEXTURESTAGESTATETYPE dwState,LPDWORD lpdwValue)
{
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::GetTextureStageState reached.\r\n");
	return m_pID3DDevice7->GetTextureStageState(dwStage, dwState, lpdwValue);
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::GetTextureStageState exited.\r\n");
}
HRESULT  WINAPI myID3DDevice7::SetTextureStageState(DWORD dwStage,D3DTEXTURESTAGESTATETYPE dwState,DWORD dwValue)
{
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::SetTextureStageState reached.\r\n");
	return m_pID3DDevice7->SetTextureStageState(dwStage, dwState, dwValue);
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::SetTextureStageState exited.\r\n");
}
HRESULT  WINAPI myID3DDevice7::ValidateDevice(LPDWORD lpdwPasses)
{
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::ValidateDevice reached.\r\n");
	return m_pID3DDevice7->ValidateDevice(lpdwPasses);
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::ValidateDevice exited.\r\n");
}
HRESULT  WINAPI myID3DDevice7::ApplyStateBlock(DWORD dwBlockHandle)
{
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::ApplyStateBlock reached.\r\n");
	return m_pID3DDevice7->ApplyStateBlock(dwBlockHandle);
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::ApplyStateBlock exited.\r\n");
}
HRESULT  WINAPI myID3DDevice7::CaptureStateBlock(DWORD dwBlockHandle)
{
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::CaptureStateBlock reached.\r\n");
	return m_pID3DDevice7->CaptureStateBlock(dwBlockHandle);
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::CaptureStateBlock exited.\r\n");
}
HRESULT  WINAPI myID3DDevice7::DeleteStateBlock(DWORD dwBlockHandle)
{
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::DeleteStateBlock reached.\r\n");
	return m_pID3DDevice7->DeleteStateBlock(dwBlockHandle);
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::DeleteStateBlock exited.\r\n");
}
HRESULT  WINAPI myID3DDevice7::CreateStateBlock(D3DSTATEBLOCKTYPE d3dsbType,LPDWORD lpdwBlockHandle)
{
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::CreateStateBlock reached.\r\n");
	return m_pID3DDevice7->CreateStateBlock(d3dsbType,lpdwBlockHandle);
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::CreateStateBlock exited.\r\n");
}
HRESULT  WINAPI myID3DDevice7::Load(LPDIRECTDRAWSURFACE7 lpDestTex,LPPOINT lpDestPoint,LPDIRECTDRAWSURFACE7 lpSrcTex,LPRECT lprcSrcRect,DWORD dwFlags)
{
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::Load reached.\r\n");
	return m_pID3DDevice7->Load(lpDestTex,lpDestPoint,lpSrcTex,lprcSrcRect,dwFlags);
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::Load exited.\r\n");
}
HRESULT  WINAPI myID3DDevice7::LightEnable(DWORD dwLightIndex,BOOL bEnable)
{
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::LightEnable reached.\r\n");
	return m_pID3DDevice7->LightEnable(dwLightIndex,bEnable);
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::LightEnable exited.\r\n");
}
HRESULT  WINAPI myID3DDevice7::GetLightEnable(DWORD dwLightIndex,BOOL* pbEnable)
{
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::GetLightEnable reached.\r\n");
	return m_pID3DDevice7->GetLightEnable(dwLightIndex,pbEnable);
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::GetLightEnable exited.\r\n");
}
HRESULT  WINAPI myID3DDevice7::SetClipPlane(DWORD dwIndex, D3DVALUE* pPlaneEquation)
{
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::SetClipPlane reached.\r\n");
	return m_pID3DDevice7->SetClipPlane(dwIndex,pPlaneEquation);
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::SetClipPlane exited.\r\n");
}
HRESULT  WINAPI myID3DDevice7::GetClipPlane(DWORD dwIndex, D3DVALUE* pPlaneEquation)
{
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::GetClipPlane reached.\r\n");
	return m_pID3DDevice7->GetClipPlane(dwIndex,pPlaneEquation);
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::GetClipPlane exited.\r\n");
}
HRESULT  WINAPI myID3DDevice7::GetInfo(DWORD  dwDevInfoID, LPVOID pDevInfoStruct, DWORD  dwSize)
{
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::GetInfo reached.\r\n");
	return m_pID3DDevice7->GetInfo(dwDevInfoID,pDevInfoStruct, dwSize);
	//OutputDebugString("DDRAWPROXY: myID3DDevice7::GetInfo exited.\r\n");
}

