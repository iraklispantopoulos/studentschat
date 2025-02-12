rem For production we need to run this script to keep the session open after remote desktop
@echo off
:: Script to keep RDP session alive by transferring it to the console

echo Finding active RDP session...

:: Get the session ID of the active RDP session (ignores ">" marker)
for /f "tokens=3" %%s in ('query session ^| findstr /R /C:"rdp-tcp#.*Active"') do (
    echo Transferring session ID %%s to console...
    tscon %%s /dest:console
    goto :done
)

echo No active RDP session found!
exit /b

:done
echo Session transferred. You can safely close RDP.

