ECHO off

SET /P APPVEYOR_BUILD_NUMBER=Please enter a build number (e.g. 134):
SET /P PACKAGE_VERISON=Please enter your package version (e.g. 1.0.5):
SET /P version_suffix=Please enter your package release suffix or leave empty (e.g. beta):

SET /P APPVEYOR_REPO_TAG=If you want to simulate a GitHub tag for a release (e.g. true):

if "%APPVEYOR_BUILD_NUMBER%" == "" (
  SET APPVEYOR_BUILD_NUMBER=100
)
if "%PACKAGE_VERISON%" == "" (
  SET PACKAGE_VERISON=0.1.0
)

SET mssemver=%PACKAGE_VERISON%-beta-%APPVEYOR_BUILD_NUMBER%

SET CONFIGURATION=Debug

cd build
call npm install
call node appveyor-nuspec-patch.js
cd..

build-appveyor.cmd

cd..

@IF %ERRORLEVEL% NEQ 0 GOTO err
@EXIT /B 0
:err
@PAUSE
@EXIT /B 1