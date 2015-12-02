@ECHO OFF
SET ProgFiles86Root=%ProgramFiles(x86)%
IF NOT "%ProgFiles86Root%"=="" GOTO start
SET ProgFiles86Root=%ProgramFiles%

:start
"%ProgFiles86Root%\MSBuild\14.0\Bin\MSBuild.exe" /p:Configuration=Release /p:Platform=x86 ..\AnalogDevices\AnalogDevices.sln
IF ERRORLEVEL 1 GOTO END
"%ProgFiles86Root%\MSBuild\14.0\Bin\MSBuild.exe" /p:Configuration=Debug /p:Platform=x86 ..\AnalogDevices\AnalogDevices.sln
IF ERRORLEVEL 1 GOTO END
"%ProgFiles86Root%\MSBuild\14.0\Bin\MSBuild.exe" /p:Configuration=Release /p:Platform=x86 ..\BIUSBWrapper\BIUSBWrapper.sln
IF ERRORLEVEL 1 GOTO END
"%ProgFiles86Root%\MSBuild\14.0\Bin\MSBuild.exe" /p:Configuration=Debug /p:Platform=x86 ..\BIUSBWrapper\BIUSBWrapper.sln
IF ERRORLEVEL 1 GOTO END
"%ProgFiles86Root%\MSBuild\14.0\Bin\MSBuild.exe" /p:Configuration=Release /p:Platform=x86 ..\Common\Common.sln
IF ERRORLEVEL 1 GOTO END
"%ProgFiles86Root%\MSBuild\14.0\Bin\MSBuild.exe" /p:Configuration=Debug /p:Platform=x86 ..\Common\Common.sln
IF ERRORLEVEL 1 GOTO END
"%ProgFiles86Root%\MSBuild\14.0\Bin\MSBuild.exe" /p:Configuration=Release /p:Platform=x86 ..\F16CPD\F16CPD.sln
IF ERRORLEVEL 1 GOTO END
"%ProgFiles86Root%\MSBuild\14.0\Bin\MSBuild.exe" /p:Configuration=Debug /p:Platform=x86 ..\F16CPD\F16CPD.sln
IF ERRORLEVEL 1 GOTO END
"%ProgFiles86Root%\MSBuild\14.0\Bin\MSBuild.exe" /p:Configuration=Release /p:Platform=x86 ..\F4KeyFile\F4KeyFile.sln
IF ERRORLEVEL 1 GOTO END
"%ProgFiles86Root%\MSBuild\14.0\Bin\MSBuild.exe" /p:Configuration=Debug /p:Platform=x86 ..\F4KeyFile\F4KeyFile.sln
IF ERRORLEVEL 1 GOTO END
"%ProgFiles86Root%\MSBuild\14.0\Bin\MSBuild.exe" /p:Configuration=Release /p:Platform=x86 ..\F4KeyFileViewer\F4KeyFileViewer.sln
IF ERRORLEVEL 1 GOTO END
"%ProgFiles86Root%\MSBuild\14.0\Bin\MSBuild.exe" /p:Configuration=Debug /p:Platform=x86 ..\F4KeyFileViewer\F4KeyFileViewer.sln
IF ERRORLEVEL 1 GOTO END
"%ProgFiles86Root%\MSBuild\14.0\Bin\MSBuild.exe" /p:Configuration=Release /p:Platform=x86 ..\F4ResourceFileEditor\F4ResourceFileEditor.sln
IF ERRORLEVEL 1 GOTO END
"%ProgFiles86Root%\MSBuild\14.0\Bin\MSBuild.exe" /p:Configuration=Debug /p:Platform=x86 ..\F4ResourceFileEditor\F4ResourceFileEditor.sln
IF ERRORLEVEL 1 GOTO END
"%ProgFiles86Root%\MSBuild\14.0\Bin\MSBuild.exe" /p:Configuration=Release /p:Platform=x86 ..\F4SharedMem\F4SharedMem.sln
IF ERRORLEVEL 1 GOTO END
"%ProgFiles86Root%\MSBuild\14.0\Bin\MSBuild.exe" /p:Configuration=Debug /p:Platform=x86 ..\F4SharedMem\F4SharedMem.sln
IF ERRORLEVEL 1 GOTO END
"%ProgFiles86Root%\MSBuild\14.0\Bin\MSBuild.exe" /p:Configuration=Release /p:Platform=x86 ..\F4SharedMemViewer\F4SharedMemViewer.sln
IF ERRORLEVEL 1 GOTO END
"%ProgFiles86Root%\MSBuild\14.0\Bin\MSBuild.exe" /p:Configuration=Debug /p:Platform=x86 ..\F4SharedMemViewer\F4SharedMemViewer.sln
IF ERRORLEVEL 1 GOTO END
"%ProgFiles86Root%\MSBuild\14.0\Bin\MSBuild.exe" /p:Configuration=Release /p:Platform=x86 ..\F4SharedMemMirror\F4SharedMemMirror.sln
IF ERRORLEVEL 1 GOTO END
"%ProgFiles86Root%\MSBuild\14.0\Bin\MSBuild.exe" /p:Configuration=Debug /p:Platform=x86 ..\F4SharedMemMirror\F4SharedMemMirror.sln
IF ERRORLEVEL 1 GOTO END
"%ProgFiles86Root%\MSBuild\14.0\Bin\MSBuild.exe" /p:Configuration=Release /p:Platform=x86 ..\F4SharedMemoryRecorder\F4SharedMemoryRecorder.sln
IF ERRORLEVEL 1 GOTO END
"%ProgFiles86Root%\MSBuild\14.0\Bin\MSBuild.exe" /p:Configuration=Debug /p:Platform=x86 ..\F4SharedMemoryRecorder\F4SharedMemoryRecorder.sln
IF ERRORLEVEL 1 GOTO END
"%ProgFiles86Root%\MSBuild\14.0\Bin\MSBuild.exe" /p:Configuration=Release /p:Platform=x86 ..\F4TexSharedMem\F4TexSharedMem.sln
IF ERRORLEVEL 1 GOTO END
"%ProgFiles86Root%\MSBuild\14.0\Bin\MSBuild.exe" /p:Configuration=Debug /p:Platform=x86 ..\F4TexSharedMem\F4TexSharedMem.sln
IF ERRORLEVEL 1 GOTO END
"%ProgFiles86Root%\MSBuild\14.0\Bin\MSBuild.exe" /p:Configuration=Release /p:Platform=x86 ..\F4TexSharedMemTester\F4TexSharedMemTester.sln
IF ERRORLEVEL 1 GOTO END
"%ProgFiles86Root%\MSBuild\14.0\Bin\MSBuild.exe" /p:Configuration=Debug /p:Platform=x86 ..\F4TexSharedMemTester\F4TexSharedMemTester.sln
IF ERRORLEVEL 1 GOTO END
"%ProgFiles86Root%\MSBuild\14.0\Bin\MSBuild.exe" /p:Configuration=Release /p:Platform=x86 ..\F4Utils\F4Utils.sln
IF ERRORLEVEL 1 GOTO END
"%ProgFiles86Root%\MSBuild\14.0\Bin\MSBuild.exe" /p:Configuration=Debug /p:Platform=x86 ..\F4Utils\F4Utils.sln
IF ERRORLEVEL 1 GOTO END
"%ProgFiles86Root%\MSBuild\14.0\Bin\MSBuild.exe" /p:Configuration=Release /p:Platform=x86 ..\F4Utils\Speech\F4Utils.Speech.sln
IF ERRORLEVEL 1 GOTO END
"%ProgFiles86Root%\MSBuild\14.0\Bin\MSBuild.exe" /p:Configuration=Debug /p:Platform=x86 ..\F4Utils\Speech\F4Utils.Speech.sln
IF ERRORLEVEL 1 GOTO END
"%ProgFiles86Root%\MSBuild\14.0\Bin\MSBuild.exe" /p:Configuration=Release /p:Platform=x86 ..\JoyMapper\JoyMapper.sln
IF ERRORLEVEL 1 GOTO END
"%ProgFiles86Root%\MSBuild\14.0\Bin\MSBuild.exe" /p:Configuration=Debug /p:Platform=x86 ..\JoyMapper\JoyMapper.sln
IF ERRORLEVEL 1 GOTO END
"%ProgFiles86Root%\MSBuild\14.0\Bin\MSBuild.exe" /p:Configuration=Release /p:Platform=x86 ..\LightningGauges\LightningGauges.sln
IF ERRORLEVEL 1 GOTO END
"%ProgFiles86Root%\MSBuild\14.0\Bin\MSBuild.exe" /p:Configuration=Debug /p:Platform=x86 ..\LightningGauges\LightningGauges.sln
IF ERRORLEVEL 1 GOTO END
"%ProgFiles86Root%\MSBuild\14.0\Bin\MSBuild.exe" /p:Configuration=Release /p:Platform=x86 ..\Lzss\Lzss.sln
IF ERRORLEVEL 1 GOTO END
"%ProgFiles86Root%\MSBuild\14.0\Bin\MSBuild.exe" /p:Configuration=Debug /p:Platform=x86 ..\Lzss\Lzss.sln
IF ERRORLEVEL 1 GOTO END
"%ProgFiles86Root%\MSBuild\14.0\Bin\MSBuild.exe" /p:Configuration=Release /p:Platform=x86 ..\MasterBuild\BuildAll.sln
IF ERRORLEVEL 1 GOTO END
"%ProgFiles86Root%\MSBuild\14.0\Bin\MSBuild.exe" /p:Configuration=Debug /p:Platform=x86 ..\MasterBuild\BuildAll.sln
IF ERRORLEVEL 1 GOTO END
"%ProgFiles86Root%\MSBuild\14.0\Bin\MSBuild.exe" /p:Configuration=Release /p:Platform=x86 ..\MFDExtractor\MFDExtractor.sln
IF ERRORLEVEL 1 GOTO END
"%ProgFiles86Root%\MSBuild\14.0\Bin\MSBuild.exe" /p:Configuration=Debug /p:Platform=x86 ..\MFDExtractor\MFDExtractor.sln
IF ERRORLEVEL 1 GOTO END
"%ProgFiles86Root%\MSBuild\14.0\Bin\MSBuild.exe" /p:Configuration=Release /p:Platform=x86 ..\PHCC\PHCC.sln
IF ERRORLEVEL 1 GOTO END
"%ProgFiles86Root%\MSBuild\14.0\Bin\MSBuild.exe" /p:Configuration=Debug /p:Platform=x86 ..\PHCC\PHCC.sln
IF ERRORLEVEL 1 GOTO END
"%ProgFiles86Root%\MSBuild\14.0\Bin\MSBuild.exe" /p:Configuration=Release /p:Platform=x86 ..\PhccDeviceManager\PhccDeviceManager.sln
IF ERRORLEVEL 1 GOTO END
"%ProgFiles86Root%\MSBuild\14.0\Bin\MSBuild.exe" /p:Configuration=Debug /p:Platform=x86 ..\PhccDeviceManager\PhccDeviceManager.sln
IF ERRORLEVEL 1 GOTO END
"%ProgFiles86Root%\MSBuild\14.0\Bin\MSBuild.exe" /p:Configuration=Release /p:Platform=x86 ..\PhccTestTool\PhccTestTool\PhccTestTool.sln
IF ERRORLEVEL 1 GOTO END
"%ProgFiles86Root%\MSBuild\14.0\Bin\MSBuild.exe" /p:Configuration=Debug /p:Platform=x86 ..\PhccTestTool\PhccTestTool\PhccTestTool.sln
IF ERRORLEVEL 1 GOTO END
"%ProgFiles86Root%\MSBuild\14.0\Bin\MSBuild.exe" /p:Configuration=Release /p:Platform=x86 ..\PPJoyWrapper\PPJoyWrapper.sln
IF ERRORLEVEL 1 GOTO END
"%ProgFiles86Root%\MSBuild\14.0\Bin\MSBuild.exe" /p:Configuration=Debug /p:Platform=x86 ..\PPJoyWrapper\PPJoyWrapper.sln
IF ERRORLEVEL 1 GOTO END
"%ProgFiles86Root%\MSBuild\14.0\Bin\MSBuild.exe" /p:Configuration=Release /p:Platform=x86 ..\SimLinkup\SimLinkup.sln
IF ERRORLEVEL 1 GOTO END
"%ProgFiles86Root%\MSBuild\14.0\Bin\MSBuild.exe" /p:Configuration=Debug /p:Platform=x86 ..\SimLinkup\SimLinkup.sln
IF ERRORLEVEL 1 GOTO END
"%ProgFiles86Root%\MSBuild\14.0\Bin\MSBuild.exe" /p:Configuration=Release /p:Platform=x86 ..\SpeexInvoke\SpeexInvoke.sln
IF ERRORLEVEL 1 GOTO END
"%ProgFiles86Root%\MSBuild\14.0\Bin\MSBuild.exe" /p:Configuration=Debug /p:Platform=x86 ..\SpeexInvoke\SpeexInvoke.sln
IF ERRORLEVEL 1 GOTO END
"%ProgFiles86Root%\MSBuild\14.0\Bin\MSBuild.exe" /p:Configuration=Release /p:Platform=x86 ..\TlkTool\TlkTool.sln
IF ERRORLEVEL 1 GOTO END
"%ProgFiles86Root%\MSBuild\14.0\Bin\MSBuild.exe" /p:Configuration=Debug /p:Platform=x86 ..\TlkTool\TlkTool.sln
IF ERRORLEVEL 1 GOTO END
"%ProgFiles86Root%\MSBuild\14.0\Bin\MSBuild.exe" /p:Configuration=Release /p:Platform=x86 ..\TlkTool\UI\TlkTool.UI\TlkTool.UI.sln
IF ERRORLEVEL 1 GOTO END
"%ProgFiles86Root%\MSBuild\14.0\Bin\MSBuild.exe" /p:Configuration=Debug /p:Platform=x86 ..\TlkTool\UI\TlkTool.UI\TlkTool.UI.sln
IF ERRORLEVEL 1 GOTO END
:END