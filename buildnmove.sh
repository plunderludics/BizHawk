targetDir="../unity-hawk/Packages/org.plunderludics.UnityHawk/BizHawk"

dotnet build . -c Release -p:UnityHawk=true &&
cp output/EmuHawk.exe $targetDir/EmuHawk.exe &&
cp output/dll/* $targetDir/dll

