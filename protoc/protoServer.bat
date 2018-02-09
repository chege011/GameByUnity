@echo off 

set srcPath=proto
 
set distGoPath=protoGo
 
set binPath=bin
 
%binPath%\protoc --gogofaster_out=.\%distGoPath% .\%srcPath%\PixelStarcraft.proto
move .\%distGoPath%\%srcPath%\PixelStarcraft.pb.go .\%distGoPath%
rmdir .\%distGoPath%\%srcPath%
copy .\%distGoPath%\PixelStarcraft.pb.go ..\server\server_temporary\usercmd\

 
echo "ok"
pause