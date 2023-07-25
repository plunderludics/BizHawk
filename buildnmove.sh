targetDir="../unity-hawk/Packages/org.plunderludics.UnityHawk/BizHawk~"
mkdir -p $targetDir/dll

dotnet build src/BizHawk.Client.Headless -p:UnityHawk=true &&
cp output/EmuHawk-Headless.exe $targetDir/EmuHawk-Headless.exe &&
cp output/dll/* $targetDir/dll

