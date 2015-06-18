@ECHO OFF
SET ProgFiles86Root=%ProgramFiles(x86)%
IF NOT "%ProgFiles86Root%"=="" GOTO start
SET ProgFiles86Root=%ProgramFiles%

:start
"%ProgFiles86Root%\MSBuild\12.0\Bin\MSBuild.exe" /p:Configuration=Release /p:Platform=x86 %1
IF ERRORLEVEL 1 GOTO END
"%ProgFiles86Root%\MSBuild\12.0\Bin\MSBuild.exe" /p:Configuration=Debug /p:Platform=x86 %1
IF ERRORLEVEL 1 GOTO END
:END