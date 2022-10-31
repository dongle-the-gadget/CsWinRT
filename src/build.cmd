@echo off
if /i "%cswinrt_echo%" == "on" @echo on

set this_dir=%~dp0
rem Preserve above for Visual Studio launch inheritance
setlocal ENABLEDELAYEDEXPANSION

:params
set cswinrt_platform=%1
set cswinrt_configuration=%2
set cswinrt_version_number=%3
set cswinrt_version_string=%4
set cswinrt_assembly_version=%5
set "%6"!="" set cswinrt_label=%6

if "%cswinrt_platform%"=="" set cswinrt_platform=x64

if /I "%cswinrt_platform%" equ "all" (
  if "%cswinrt_configuration%"=="" (
    set cswinrt_configuration=all
  )
  call %0 x86 !cswinrt_configuration! !cswinrt_version_number! !cswinrt_version_string! !cswinrt_assembly_version!
  call %0 x64 !cswinrt_configuration! !cswinrt_version_number! !cswinrt_version_string! !cswinrt_assembly_version!
  call %0 arm !cswinrt_configuration! !cswinrt_version_number! !cswinrt_version_string! !cswinrt_assembly_version!
  call %0 arm64 !cswinrt_configuration! !cswinrt_version_number! !cswinrt_version_string! !cswinrt_assembly_version!
  goto :eof
)

if /I "%cswinrt_configuration%" equ "all" (
  call %0 %cswinrt_platform% Debug !cswinrt_version_number! !cswinrt_version_string! !cswinrt_assembly_version!
  call %0 %cswinrt_platform% Release !cswinrt_version_number! !cswinrt_version_string! !cswinrt_assembly_version!
  goto :eof
)

if "%cswinrt_configuration%"=="" (
  set cswinrt_configuration=Release
)

if "%cswinrt_version_number%"=="" set cswinrt_version_number=0.0.0.0
if "%cswinrt_version_string%"=="" set cswinrt_version_string=0.0.0-private.0
if "%cswinrt_assembly_version%"=="" set cswinrt_assembly_version=0.0.0.0

if "%cswinrt_baseline_breaking_compat_errors%"=="" set cswinrt_baseline_breaking_compat_errors=false
if "%cswinrt_baseline_assembly_version_compat_errors%"=="" set cswinrt_baseline_assembly_version_compat_errors=false

goto :skip_build_tools
rem VS 16.X BuildTools support (when a prerelease VS is required, until it is deployed to Azure Devops agents) 
msbuild -ver | findstr 16.X >nul
if ErrorLevel 1 (
  echo Using VS Build Tools 16.X 
  if %cswinrt_platform%==x86 (
    set msbuild_path="%this_dir%.buildtools\MSBuild\Current\Bin\\"
  ) else (
    set msbuild_path="%this_dir%.buildtools\MSBuild\Current\Bin\amd64\\"
  )
  if not exist !msbuild_path! (
    if not exist .buildtools md .buildtools 
    powershell -NoProfile -ExecutionPolicy unrestricted -File .\get_buildtools.ps1
  )
  set nuget_params=-MSBuildPath !msbuild_path!
) else (
  set msbuild_path=
  set nuget_params=
)
:skip_build_tools

set nuget_dir=%this_dir%.nuget

if not "%cswinrt_label%"=="" goto %cswinrt_label%

:restore
rem When a preview nuget is required, update -self doesn't work, so manually update 
rem if exist %nuget_dir%\nuget.exe (
rem   %nuget_dir%\nuget.exe | findstr 5.9 >nul
rem   if ErrorLevel 1 (
rem     echo Updating to nuget 5.9
rem     rd /s/q %nuget_dir% >nul 2>&1
rem   )
rem )
if not exist %nuget_dir% md %nuget_dir%
if not exist %nuget_dir%\nuget.exe powershell -Command "Invoke-WebRequest https://dist.nuget.org/win-x86-commandline/v5.8.0-preview.2/nuget.exe -OutFile %nuget_dir%\nuget.exe"
%nuget_dir%\nuget update -self
rem Note: packages.config-based (vcxproj) projects do not support msbuild /t:restore
set NUGET_RESTORE_MSBUILD_ARGS=/p:platform="%cswinrt_platform%"
if "%CIBuildReason%"=="CI" set NUGET_RESTORE_MSBUILD_ARGS=%NUGET_RESTORE_MSBUILD_ARGS%;CIBuildReason=%CIBuildReason%
call :exec %nuget_dir%\nuget.exe restore %nuget_params% %this_dir%cswinrt.sln

:build
echo Building cswinrt for %cswinrt_platform% %cswinrt_configuration%
call :exec %msbuild_path%msbuild.exe %cswinrt_build_params% /p:platform=%cswinrt_platform%;configuration=%cswinrt_configuration%;VersionNumber=%cswinrt_version_number%;VersionString=%cswinrt_version_string%;AssemblyVersionNumber=%cswinrt_assembly_version%;GenerateTestProjection=true;BaselineAllAPICompatError=%cswinrt_baseline_breaking_compat_errors%;BaselineAllMatchingRefApiCompatError=%cswinrt_baseline_assembly_version_compat_errors% %this_dir%cswinrt.sln 
if ErrorLevel 1 (
  echo.
  echo ERROR: Build failed
  exit /b !ErrorLevel!
)

if "%cswinrt_build_only%"=="true" goto :eof

:package
rem We set the properties of the CsWinRT.nuspec here, and pass them as the -Properties option when we call `nuget pack`
set cswinrt_bin_dir=%this_dir%_build\%cswinrt_platform%\%cswinrt_configuration%\cswinrt\bin\
set cswinrt_exe=%cswinrt_bin_dir%cswinrt.exe
set interop_winmd=%this_dir%_build\%cswinrt_platform%\%cswinrt_configuration%\cswinrt\obj\merged\WinRT.Interop.winmd
set netstandard2_runtime=%this_dir%WinRT.Runtime\bin\%cswinrt_configuration%\netstandard2.0\WinRT.Runtime.dll
set net6_runtime=%this_dir%WinRT.Runtime\bin\%cswinrt_configuration%\net6.0\WinRT.Runtime.dll
set source_generator=%this_dir%Authoring\WinRT.SourceGenerator\bin\%cswinrt_configuration%\netstandard2.0\WinRT.SourceGenerator.dll
set winrt_host_%cswinrt_platform%=%this_dir%_build\%cswinrt_platform%\%cswinrt_configuration%\WinRT.Host\bin\WinRT.Host.dll
set winrt_host_resource_%cswinrt_platform%=%this_dir%_build\%cswinrt_platform%\%cswinrt_configuration%\WinRT.Host\bin\WinRT.Host.dll.mui
set winrt_shim=%this_dir%Authoring\WinRT.Host.Shim\bin\%cswinrt_configuration%\net6.0\WinRT.Host.Shim.dll
set guid_patch=%this_dir%Perf\IIDOptimizer\bin\%cswinrt_configuration%\net6.0\*.*
rem Now call pack
echo Creating nuget package
call :exec %nuget_dir%\nuget pack %this_dir%..\nuget\Microsoft.Windows.CsWinRT.nuspec -Properties cswinrt_exe=%cswinrt_exe%;interop_winmd=%interop_winmd%;netstandard2_runtime=%netstandard2_runtime%;net6_runtime=%net6_runtime%;source_generator=%source_generator%;cswinrt_nuget_version=%cswinrt_version_string%;winrt_host_x86=%winrt_host_x86%;winrt_host_x64=%winrt_host_x64%;winrt_host_arm=%winrt_host_arm%;winrt_host_arm64=%winrt_host_arm64%;winrt_host_resource_x86=%winrt_host_resource_x86%;winrt_host_resource_x64=%winrt_host_resource_x64%;winrt_host_resource_arm=%winrt_host_resource_arm%;winrt_host_resource_arm64=%winrt_host_resource_arm64%;winrt_shim=%winrt_shim%;guid_patch=%guid_patch% -OutputDirectory %cswinrt_bin_dir% -NonInteractive -Verbosity Detailed -NoPackageAnalysis
goto :eof

:exec
if /i "%cswinrt_echo%" == "only" (
echo Command Line:
echo %*
echo.
) else (
%*
)
goto :eof