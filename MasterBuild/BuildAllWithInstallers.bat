@ECHO OFF
"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\devenv.com"  /Build "Release" ..\AnalogDevices\AnalogDevices.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\devenv.com"  /Build "Debug" ..\AnalogDevices\AnalogDevices.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\devenv.com"  /Build "Release" ..\BIUSBWrapper\BIUSBWrapper.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\devenv.com"  /Build "Debug" ..\BIUSBWrapper\BIUSBWrapper.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\devenv.com"  /Build "Release" ..\Common\Common.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\devenv.com"  /Build "Debug" ..\Common\Common.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\devenv.com"  /Build "Release" ..\F16CPD\F16CPD.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\devenv.com"  /Build "Debug" ..\F16CPD\F16CPD.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\devenv.com"  /Build "Release" ..\F4KeyFile\F4KeyFile.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\devenv.com"  /Build "Debug" ..\F4KeyFile\F4KeyFile.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\devenv.com"  /Build "Release" ..\F4KeyFileViewer\F4KeyFileViewer.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\devenv.com"  /Build "Debug" ..\F4KeyFileViewer\F4KeyFileViewer.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\devenv.com"  /Build "Release" ..\F4ResourceFileEditor\F4ResourceFileEditor.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\devenv.com"  /Build "Debug" ..\F4ResourceFileEditor\F4ResourceFileEditor.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\devenv.com"  /Build "Release" ..\F4SharedMem\F4SharedMem.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\devenv.com"  /Build "Debug" ..\F4SharedMem\F4SharedMem.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\devenv.com"  /Build "Release" ..\F4SharedMemViewer\F4SharedMemViewer.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\devenv.com"  /Build "Debug" ..\F4SharedMemViewer\F4SharedMemViewer.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\devenv.com"  /Build "Release" ..\F4SharedMemMirror\F4SharedMemMirror.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\devenv.com"  /Build "Debug" ..\F4SharedMemMirror\F4SharedMemMirror.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\devenv.com"  /Build "Release" ..\F4TexSharedMem\F4TexSharedMem.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\devenv.com"  /Build "Debug" ..\F4TexSharedMem\F4TexSharedMem.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\devenv.com"  /Build "Release" ..\F4TexSharedMemTester\F4TexSharedMemTester.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\devenv.com"  /Build "Debug" ..\F4TexSharedMemTester\F4TexSharedMemTester.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\devenv.com"  /Build "Release" ..\F4Utils\F4Utils.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\devenv.com"  /Build "Debug" ..\F4Utils\F4Utils.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\devenv.com"  /Build "Release" ..\F4Utils\Speech\F4Utils.Speech.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\devenv.com"  /Build "Debug" ..\F4Utils\Speech\F4Utils.Speech.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\devenv.com"  /Build "Release" ..\JoyMapper\JoyMapper.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\devenv.com"  /Build "Debug" ..\JoyMapper\JoyMapper.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\devenv.com"  /Build "Release" ..\LightningGauges\LightningGauges.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\devenv.com"  /Build "Debug" ..\LightningGauges\LightningGauges.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\devenv.com"  /Build "Release" ..\Lzss\Lzss.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\devenv.com"  /Build "Debug" ..\Lzss\Lzss.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\devenv.com"  /Build "Release" ..\MasterBuild\BuildAll.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\devenv.com"  /Build "Debug" ..\MasterBuild\BuildAll.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\devenv.com"  /Build "Release" ..\MFDExtractor\MFDExtractor.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\devenv.com"  /Build "Debug" ..\MFDExtractor\MFDExtractor.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\devenv.com"  /Build "Release" ..\PHCC\PHCC.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\devenv.com"  /Build "Debug" ..\PHCC\PHCC.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\devenv.com"  /Build "Release" ..\PhccDeviceManager\PhccDeviceManager.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\devenv.com"  /Build "Debug" ..\PhccDeviceManager\PhccDeviceManager.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\devenv.com"  /Build "Release" ..\PhccTestTool\PhccTestTool\PhccTestTool.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\devenv.com"  /Build "Debug" ..\PhccTestTool\PhccTestTool\PhccTestTool.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\devenv.com"  /Build "Release" ..\PPJoyWrapper\PPJoyWrapper.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\devenv.com"  /Build "Debug" ..\PPJoyWrapper\PPJoyWrapper.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\devenv.com"  /Build "Release" ..\SimLinkup\SimLinkup.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\devenv.com"  /Build "Debug" ..\SimLinkup\SimLinkup.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\devenv.com"  /Build "Release" ..\SpeexInvoke\SpeexInvoke.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\devenv.com"  /Build "Debug" ..\SpeexInvoke\SpeexInvoke.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\devenv.com"  /Build "Release" ..\TlkTool\TlkTool.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\devenv.com"  /Build "Debug" ..\TlkTool\TlkTool.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\devenv.com"  /Build "Release" ..\TlkTool\UI\TlkTool.UI\TlkTool.UI.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\devenv.com"  /Build "Debug" ..\TlkTool\UI\TlkTool.UI\TlkTool.UI.sln
IF ERRORLEVEL 1 GOTO END
:END