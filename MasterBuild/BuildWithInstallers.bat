@ECHO OFF
SET ProgFiles86Root=%ProgramFiles(x86)%
IF NOT "%ProgFiles86Root%"=="" GOTO start
SET ProgFiles86Root=%ProgramFiles%

:start
"%ProgFiles86Root%\Microsoft Visual Studio 12.0\Common7\IDE\devenv.com"  /Build "Release" %1
IF ERRORLEVEL 1 GOTO END
"%ProgFiles86Root%\Microsoft Visual Studio 12.0\Common7\IDE\devenv.com"  /Build "Debug" %1
IF ERRORLEVEL 1 GOTO END
:END