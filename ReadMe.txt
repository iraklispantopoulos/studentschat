#For production we need to run this script to keep the session open after remote desktop
@echo off
echo Finding active RDP session...
for /f "tokens=2" %%s in ('query session ^| findstr "rdp-tcp#"') do (
    echo Transferring session ID %%s to console...
    tscon %%s /dest:console
)
echo Session transferred. You can safely close RDP.
