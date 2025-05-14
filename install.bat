set "DIR=%~dp0"
set "SERVICE_NAME=%1"
set "EXE=%DIR%%2"
echo %EXE%
sc create %SERVICE_NAME% binPath= "%EXE%.exe" start= auto
net start %SERVICE_NAME%