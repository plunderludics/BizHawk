targetDir="../unity-hawk/Packages/org.plunderludics.UnityHawk/BizHawk/dll"

dotnet build . -c Release -p:UnityHawk=true &&
cp output/dll/* $targetDir

# We don't need these, and they cause problems for building, so delete them:
rm $targetDir/EmuHawk.odb $targetDir/SlimDx.dll $targetDir/BizHawk.Bizware.DirectX.*
# This one we need but it's registered as a dependency in the unity project:
rm $targetDir/Newtonsoft.Json.dll