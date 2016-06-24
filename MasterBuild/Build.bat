@ECHO OFF
SET ProgFiles86Root=%ProgramFiles(x86)%
IF NOT "%ProgFiles86Root%"=="" GOTO start
SET ProgFiles86Root=%ProgramFiles%

:start
CALL RestorePackages.bat

SET MASTERBUILDDIR=%~dp0
REG IMPORT "%MASTERBUILDDIR%EnableCommandLineInstallerBuilds.reg" >NUL 2>NUL

SET SOLUTION=%1
IF "%SOLUTION%"=="" SET SOLUTION=%MASTERBUILDDIR%BuildAll.sln

"%ProgFiles86Root%\Microsoft Visual Studio 14.0\Common7\IDE\devenv.com"  /Build "Release" "%SOLUTION%"
IF ERRORLEVEL 1 GOTO END
"%ProgFiles86Root%\Microsoft Visual Studio 14.0\Common7\IDE\devenv.com"  /Build "Debug" "%SOLUTION%"
IF ERRORLEVEL 1 GOTO END

:END


