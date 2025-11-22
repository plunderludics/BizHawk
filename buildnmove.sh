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

# TODO we should probably clean the bizhawk dir before building so that old dlls etc don't get left in there
# But currently still use the config.ini in the bizhawkdir so won't make this change yet

mkdir -p $bizhawkDir/dll $bizhawkDir/gamedb $bizhawkDir/ExternalTools

buildProject=src/BizHawk.Client.EmuHawk/
exeName=EmuHawk.exe

echo "building $buildProject"
dotnet build $buildProject -c $buildConfig -p:UnityHawk=true || exit 1;

# also build UnityHawk tool
echo "building UnityHawk external tool"
cd ExternalToolProjects/UnityHawk/
. build_release.sh || exit 1; # TODO should config debug/release I guess
cd ../..

echo "copying dlls and assets into $packageDir"

# for dlls that are needed by unity, make a second copy outside of the BizHawk~ dir:
# TODO wonder if we could do this with automatic dependencies as part of dotnet build command or something
for fn in \
BizHawk.BizInvoke \
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
NLua \
Plunderludics.UnityHawk.Shared \
Plunderludics.UnityHawk.SharedBuffers \
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
cp output/ExternalTools/UnityHawk.dll $bizhawkDir/ExternalTools && # Need the UnityHawk external tool
cp output/gamedb/* $bizhawkDir/gamedb
