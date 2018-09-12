ECHO APPVEYOR_REPO_BRANCH: %APPVEYOR_REPO_BRANCH%
ECHO APPVEYOR_REPO_TAG: %APPVEYOR_REPO_TAG%
ECHO APPVEYOR_BUILD_NUMBER : %APPVEYOR_BUILD_NUMBER%
ECHO APPVEYOR_BUILD_VERSION : %APPVEYOR_BUILD_VERSION%

CALL NuGet.exe restore src\UmbracoFileSystemProviders.Azure.sln

cd build

SET toolsFolder=%CD%\tools\
IF NOT EXIST "%toolsFolder%" (
	MD tools
)

IF NOT EXIST "%toolsFolder%vswhere.exe" (
	ECHO vswhere not found - fetching now
	nuget install vswhere -Version 2.0.2 -Source nuget.org -OutputDirectory tools
)

FOR /f "delims=" %%A in ('dir "%toolsFolder%vswhere.*" /b') DO SET "vswhereExePath=%toolsFolder%%%A\"
MOVE "%vswhereExePath%tools\vswhere.exe" "%toolsFolder%vswhere.exe"

for /f "usebackq tokens=1* delims=: " %%i in (`"%CD%\tools\vswhere.exe" -latest -requires Microsoft.Component.MSBuild`) do (
  if /i "%%i"=="installationPath" set InstallDir=%%j
)

SET VSWherePath="%InstallDir%\MSBuild"

ECHO.
ECHO Visual Studio is installed in: %InstallDir%

CALL "%InstallDir%\MSBuild\15.0\Bin\amd64\MsBuild.exe" package.proj %~1

@IF %ERRORLEVEL% NEQ 0 GOTO err
@EXIT /B 0
:err
@EXIT /B 1
