@echo off
set /p pass=Please enter your private key password: 
"C:\Program Files (x86)\Microsoft SDKs\Windows\v7.1A\Bin\signtool" sign /v /f "aivolkov code sign.pfx" /p %pass%  "TwainWeb.Standalone.exe"
pause