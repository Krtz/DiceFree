param([string]$Project='T:\TEMP\DiceFree-Options',[string]$Qa='T:\TEMP\DiceFree-Roadwarden-QA',[string]$Build)
$ErrorActionPreference='Stop'
if(-not $Build){$Build=(Get-Content "$Qa\Logs\FireMageBuildPath.txt").Trim()}
$dest="$Project\docs\validation\asset-first-vfx-2026-11-27";New-Item $dest -ItemType Directory -Force|Out-Null
$runtime=Get-Content "$Build\QA\asset-first-vfx.json" -Raw|ConvertFrom-Json
$static=Get-Content "$Qa\Logs\AssetFirstVfxReview.json" -Raw|ConvertFrom-Json
$report=Get-Content "$Build\build-report.json" -Raw|ConvertFrom-Json
if($runtime.status -ne 'passed' -or $runtime.error -or $runtime.unityErrors.Count -or $static.status -ne 'passed' -or $static.entries -ne 29 -or $report.result -ne 'Succeeded' -or $report.errors -ne 0){throw 'Native VFX validation incomplete'}
Copy-Item "$Build\QA\asset-first-vfx.json" "$dest\runtime-review.json";Copy-Item "$Qa\Logs\AssetFirstVfxReview.json" "$dest\static-review.json";Copy-Item "$Build\build-report.json" "$dest\build-report.json"
foreach($name in 'native-aura-Mana-world-only.png','native-aura-Infernal-persistent-world-only.png','native-cast-lightning-world-only.png'){Copy-Item "$Build\QA\$name" "$dest\$name"}
$paths=@('Assets/_DiceFree/Scripts/Combat/AssetPackVfxLibrary.cs','Assets/_DiceFree/Scripts/Combat/GroundAuraVisual.cs','Assets/_DiceFree/Scripts/Combat/EarlySkillVfx.cs','Assets/_DiceFree/Scripts/Combat/FireMageVfx.cs','Assets/_DiceFree/Scripts/UI/PlayerMotionPresentation.cs')
$paths+=@(Get-Content 'T:\TEMP\DiceFree-AssetCatalog\2026-11-27\vfx-dependencies.json' -Raw|ConvertFrom-Json)
$receipt=@(foreach($relative in ($paths|Sort-Object -Unique)){
 $a=(Get-FileHash -LiteralPath "$Project/$relative").Hash;$b=(Get-FileHash -LiteralPath "$Qa/$relative").Hash
 if($a -ne $b){throw "Source mismatch: $relative"}
 [ordered]@{path=$relative;sha256=$a;mainAndQaIdentical=$true}
})
$receipt|ConvertTo-Json -Depth 4|Set-Content "$dest\source-comparison.json"
[ordered]@{executable="$Build\DiceFree.exe";runtimeChecks=$runtime.checks;nativeBindings=$static.entries;koreanAuras=$static.auras;castImpactTemplates=$static.skillTemplates;classCastPaths=16;namedSkillMappings=91;sourceComparisons=$receipt.Count;buildErrors=$report.errors;buildWarnings=$report.warnings;limitations='Automated disposable-profile validation; source-based replacement suggestions are not artistic approval of every candidate. Batch captures omit overlay UI. Native casting/common-impact integration does not imply unique final choreography for every skill.'}|ConvertTo-Json -Depth 4|Set-Content "$dest\delivery.json"
Write-Output "Packaged $($runtime.checks) standalone checks and $($receipt.Count) matching source/dependency files"