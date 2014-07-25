rem run from project dir
ECHO OFF
xcopy ..\..\Files\* ..\..\..\..\..\..\release\last\Files /s /i
copy ..\..\TwainWeb.Standalone.exe ..\..\..\..\..\..\release\last\TwainWeb.Standalone.exe
copy ..\..\PdfSharp.dll ..\..\..\..\..\..\release\last\PdfSharp.dll
copy ..\..\TwainWeb.Standalone.exe.config ..\..\..\..\..\..\release\last\TwainWeb.Standalone.exe.config