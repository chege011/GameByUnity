@echo off 

set srcPath=proto

set distCsPath=protoCsharp

set distCsPathToProject=..\client\TowerDefenseGame\Assets\Script\Network\Proto

set binPath=bin

%binPath%\protoGen -i:%srcPath%\PixelStarcraft.proto -o:%distCsPath%\PixelStarcraft.cs

%binPath%\protoGen -i:%srcPath%\PixelStarcraft.proto -o:%distCsPathToProject%\PixelStarcraft.cs
 
echo "ok"

pause