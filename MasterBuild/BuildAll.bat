@ECHO OFF
SET ProgFiles86Root=%ProgramFiles(x86)%
IF NOT "%ProgFiles86Root%"=="" GOTO start
SET ProgFiles86Root=%ProgramFiles%

:start
SET MASTERBUILDDIR=%~dp0
REG IMPORT "%MASTERBUILDDIR%EnableCommandLineInstallerBuilds.reg" >NUL 2>NUL
IF NOT EXIST "%MASTERBUILDDIR%nuget.exe" bitsadmin /transfer nuget /priority NORMAL http://dist.nuget.org/win-x86-commandline/latest/nuget.exe  "%MASTERBUILDDIR%nuget.exe" 
SET EnableNuGetPackageRestore=true

FOR /R "%MASTERBUILDDIR%".. %%S IN (*.sln) DO (
	Pushd "%%~dpS"
	ECHO Restoring packages for solution: %%S
	"%MASTERBUILDDIR%nuget.exe" restore "%%S" -NoCache -NonInteractive
	IF ERRORLEVEL 1 GOTO END
	Popd
)

FOR /R "%MASTERBUILDDIR%".. %%S IN (*.sln) DO (
	"%ProgFiles86Root%\Microsoft Visual Studio 14.0\Common7\IDE\devenv.com"  /Build "Release" "%%S"
	IF ERRORLEVEL 1 GOTO END
	"%ProgFiles86Root%\Microsoft Visual Studio 14.0\Common7\IDE\devenv.com"  /Build "Debug" "%%S"
	IF ERRORLEVEL 1 GOTO END
)
:END


