
packageDir="../unity-hawk/Packages/org.plunderludics.UnityHawk"
bizhawkDir="$packageDir/BizHawk~"

mkdir -p $bizhawkDir/dll $bizhawkDir/gamedb

dotnet build src/BizHawk.Client.Headless/ -c Release -p:UnityHawk=true &&
# for dlls that are needed by unity, make a second copy outside of the BizHawk~ dir:
cp output/dll/Plunderludics.dll $packageDir/Plugins/Plunderludics.dll &&
cp output/dll/Plunderludics.UnityHawk.dll $packageDir/Plugins/Plunderludics.UnityHawk.dll &&
# everything else is only used by the bizhawk exe itself:
cp output/EmuHawk-Headless.exe $bizhawkDir/EmuHawk-Headless.exe &&
cp output/dll/* $bizhawkDir/dll &&
cp output/gamedb/* $bizhawkDir/gamedb
