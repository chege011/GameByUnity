@echo off 

set srcPath=%cd%\
 
set distGoPath=%srcPath%..\server\server_temporary\src\usercmd
 
set binPath=%srcPath%\bin

%binPath%\protoc --gogofaster_out=%distGoPath% PixelStarcraft.proto
echo "ok"
pause