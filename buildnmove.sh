#!/bin/bash

buildConfig=Release
while getopts ':d' option; do
   case $option in
      d) # display Help
				buildConfig=Debug
				;;
     \?) # Invalid option
        echo "Error: Invalid option"
        exit;;
   esac
done

echo building for $buildConfig

packageDir="../unity-hawk/Packages/org.plunderludics.UnityHawk"
bizhawkDir="$packageDir/BizHawk~"

mkdir -p $bizhawkDir/dll $bizhawkDir/gamedb

# For Mac:
#buildProject=src/BizHawk.Client.Headless/
#exeName=EmuHawk-Headless.exe

buildProject=src/BizHawk.Client.EmuHawk/
exeName=EmuHawk.exe


dotnet build $buildProject -c $buildConfig -p:UnityHawk=true &&
# for dlls that are needed by unity, make a second copy outside of the BizHawk~ dir:
# TODO wonder if we could do this with automatic dependencies as part of dotnet build command or something
for fn in \
BizHawk.BizInvoke \
BizHawk.Bizware.BizwareGL \
BizHawk.Client.Common \
BizHawk.Common \
BizHawk.Emulation.Common \
BizHawk.Emulation.Cores \
BizHawk.Emulation.DiscSystem \
Cyotek.Drawing.BitmapFont \
FlatBuffers.GenOutput \
Google.FlatBuffers \
ISOParser \
Microsoft.Bcl.HashCode \
Microsoft.Extensions.FileSystemGlobbing \
NLua \
Plunderludics \
Plunderludics.UnityHawk \
SharedMemory \
SharpCompress \
System.Collections.Immutable \
System.Drawing.Common \
System.Runtime.CompilerServices.Unsafe \
Virtu
do
	cp output/dll/$fn.dll $packageDir/Plugins/$fn.dll || exit 1;
done
# everything else is only used by the bizhawk exe itself:
cp output/$exeName $bizhawkDir/$exeName &&
cp output/dll/* $bizhawkDir/dll &&
cp output/gamedb/* $bizhawkDir/gamedb
