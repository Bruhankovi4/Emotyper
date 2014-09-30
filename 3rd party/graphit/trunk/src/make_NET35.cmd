@echo off
C:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild.exe GraphIt\GraphIt.csproj /property:Framework=NET35 /p:Configuration=Release /t:Rebuild
IF %ERRORLEVEL% == 0 goto QUIT
pause
:QUIT
exit