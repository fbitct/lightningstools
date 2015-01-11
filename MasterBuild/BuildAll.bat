@ECHO OFF
"C:\Program Files (x86)\MSBuild\12.0\Bin\MSBuild.exe" /p:Configuration=Release /p:Platform=x86 ..\AnalogDevices\AnalogDevices.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\MSBuild\12.0\Bin\MSBuild.exe" /p:Configuration=Release /p:Platform=x86 ..\BIUSBWrapper\BIUSBWrapper.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\MSBuild\12.0\Bin\MSBuild.exe" /p:Configuration=Release /p:Platform=x86 ..\Common\Common.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\MSBuild\12.0\Bin\MSBuild.exe" /p:Configuration=Release /p:Platform=x86 ..\F16CPD\F16CPD.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\MSBuild\12.0\Bin\MSBuild.exe" /p:Configuration=Release /p:Platform=x86 ..\F4KeyFile\F4KeyFile.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\MSBuild\12.0\Bin\MSBuild.exe" /p:Configuration=Release /p:Platform=x86 ..\F4KeyFileEditor\F4KeyFileEditor.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\MSBuild\12.0\Bin\MSBuild.exe" /p:Configuration=Release /p:Platform=x86 ..\F4ResourceFileEditor\F4ResourceFileEditor.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\MSBuild\12.0\Bin\MSBuild.exe" /p:Configuration=Release /p:Platform=x86 ..\F4SharedMem\F4SharedMem.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\MSBuild\12.0\Bin\MSBuild.exe" /p:Configuration=Release /p:Platform=x86 ..\F4SharedMemMirror\F4SharedMemMirror.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\MSBuild\12.0\Bin\MSBuild.exe" /p:Configuration=Release /p:Platform=x86 ..\F4TexSharedMem\F4TexSharedMem.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\MSBuild\12.0\Bin\MSBuild.exe" /p:Configuration=Release /p:Platform=x86 ..\F4TexSharedMemTester\F4TexSharedMemTester.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\MSBuild\12.0\Bin\MSBuild.exe" /p:Configuration=Release /p:Platform=x86 ..\F4Utils\F4Utils.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\MSBuild\12.0\Bin\MSBuild.exe" /p:Configuration=Release /p:Platform=x86 ..\F4Utils\Speech\F4Utils.Speech.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\MSBuild\12.0\Bin\MSBuild.exe" /p:Configuration=Release /p:Platform=x86 ..\JoyMapper\JoyMapper.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\MSBuild\12.0\Bin\MSBuild.exe" /p:Configuration=Release /p:Platform=x86 ..\LightningGauges\LightningGauges.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\MSBuild\12.0\Bin\MSBuild.exe" /p:Configuration=Release /p:Platform=x86 ..\Lzss\Lzss.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\MSBuild\12.0\Bin\MSBuild.exe" /p:Configuration=Release /p:Platform=x86 ..\MasterBuild\BuildAll.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\MSBuild\12.0\Bin\MSBuild.exe" /p:Configuration=Release /p:Platform=x86 ..\MFDExtractor\MFDExtractor.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\MSBuild\12.0\Bin\MSBuild.exe" /p:Configuration=Release /p:Platform=x86 ..\PHCC\PHCC.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\MSBuild\12.0\Bin\MSBuild.exe" /p:Configuration=Release /p:Platform=x86 ..\PhccDeviceManager\PhccDeviceManager.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\MSBuild\12.0\Bin\MSBuild.exe" /p:Configuration=Release /p:Platform=x86 ..\PhccTestTool\PhccTestTool\PhccTestTool.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\MSBuild\12.0\Bin\MSBuild.exe" /p:Configuration=Release /p:Platform=x86 ..\PPJoyWrapper\PPJoyWrapper.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\MSBuild\12.0\Bin\MSBuild.exe" /p:Configuration=Release /p:Platform=x86 ..\SimLinkup\SimLinkup.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\MSBuild\12.0\Bin\MSBuild.exe" /p:Configuration=Release /p:Platform=x86 ..\SpeexInvoke\SpeexInvoke.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\MSBuild\12.0\Bin\MSBuild.exe" /p:Configuration=Release /p:Platform=x86 ..\TlkTool\TlkTool.sln
IF ERRORLEVEL 1 GOTO END
"C:\Program Files (x86)\MSBuild\12.0\Bin\MSBuild.exe" /p:Configuration=Release /p:Platform=x86 ..\TlkTool\UI\TlkTool.UI\TlkTool.UI.sln
:END