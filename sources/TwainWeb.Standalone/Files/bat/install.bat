@ECHO OFF

REM The following directory is for .NET 2.0
set DOTNETFX2=%SystemRoot%\Microsoft.NET\Framework\v2.0.50727
set PATH=%PATH%;%DOTNETFX2%

echo Installing TwainWeb.Standalone...
echo ---------------------------------------------------
InstallUtil /i TwainWeb.Standalone.exe
echo ---------------------------------------------------
net start twain@web
echo Done.