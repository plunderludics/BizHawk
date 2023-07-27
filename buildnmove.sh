
packageDir="../unity-hawk/Packages/org.plunderludics.UnityHawk"
bizhawkDir="$packageDir/BizHawk~"

mkdir -p $bizhawkDir/dll $bizhawkDir/gamedb

dotnet build . -c Release -p:UnityHawk=true &&
# for dlls that are needed by unity, make a second copy outside of the BizHawk~ dir:
cp output/dll/BizHawk.Plunderludics.dll $packageDir/Plugins/BizHawk.Plunderludics.dll &&
cp output/dll/BizHawk.UnityHawk.dll $packageDir/Plugins/BizHawk.UnityHawk.dll &&
# everything else is only used by the bizhawk exe itself:
cp output/Headless.exe $bizhawkDir/Headless.exe &&
cp output/dll/* $bizhawkDir/dll &&
cp output/gamedb/* $bizhawkDir/gamedb
