rem If not already loaded, setup VisualStudio
if "%VisualStudioVersion%" EQ "" call "%ProgramFiles(x86)%\Microsoft Visual Studio 14.0\vc\vcvarsall.bat" x86

msbuild FairyGUI.Windows\FairyGUI.Windows.csproj /Property:Configuration=Debug;Platform="Any CPU"
msbuild FairyGUI\FairyGUI.csproj /Property:Configuration=Debug;Platform="Any CPU"
