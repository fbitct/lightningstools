@ECHO OFF
:start
SET MASTERBUILDDIR=%~dp0
IF NOT EXIST "%MASTERBUILDDIR%nuget.exe" bitsadmin /transfer nuget /priority NORMAL http://dist.nuget.org/win-x86-commandline/latest/nuget.exe  "%MASTERBUILDDIR%nuget.exe" 
IF ERRORLEVEL 1 GOTO END
SET EnableNuGetPackageRestore=true

FOR /R "%MASTERBUILDDIR%".. %%S IN (*.sln) DO (
	Pushd "%%~dpS"
	ECHO Restoring packages for solution: %%S
	"%MASTERBUILDDIR%nuget.exe" restore "%%S" -NoCache -NonInteractive
	IF ERRORLEVEL 1 GOTO END
	Popd
)
:END


