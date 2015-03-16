@echo off
set /p pass=Please enter your private key password: 
set /p version=Please enter program version: 
"C:\Program Files (x86)\Microsoft SDKs\Windows\v7.1A\Bin\signtool" sign /v /f "aivolkov code sign.pfx" /p %pass%  "TWAIN@Web-v1beta%version%-setup.exe"
pause