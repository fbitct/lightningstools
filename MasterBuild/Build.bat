@ECHO OFF
SET ProgFiles86Root=%ProgramFiles(x86)%
IF NOT "%ProgFiles86Root%"=="" GOTO start
SET ProgFiles86Root=%ProgramFiles%

:start
SET MASTERBUILDDIR=%~dp0
SET SOLUTION=%1
IF "%SOLUTION%"=="" SET SOLUTION=%MASTERBUILDDIR%BuildAll.sln
FOR %%G IN ("%SOLUTION%") DO SET SOLUTIONDIR=%%~dpG

REG IMPORT "%MASTERBUILDDIR%EnableCommandLineInstallerBuilds.reg" >NUL 2>NUL
IF NOT EXIST "%MASTERBUILDDIR%nuget.exe" bitsadmin /transfer nuget /priority NORMAL http://dist.nuget.org/win-x86-commandline/latest/nuget.exe  "%MASTERBUILDDIR%nuget.exe" 
SET EnableNuGetPackageRestore=true

Pushd "%SOLUTIONDIR%"
"%MASTERBUILDDIR%nuget.exe" restore "%SOLUTION%" -NoCache -NonInteractive
IF ERRORLEVEL 1 GOTO END
"%ProgFiles86Root%\Microsoft Visual Studio 14.0\Common7\IDE\devenv.com"  /Build "Release" "%SOLUTION%"
IF ERRORLEVEL 1 GOTO END
"%ProgFiles86Root%\Microsoft Visual Studio 14.0\Common7\IDE\devenv.com"  /Build "Debug" "%SOLUTION%"
IF ERRORLEVEL 1 GOTO END
Popd


:END
