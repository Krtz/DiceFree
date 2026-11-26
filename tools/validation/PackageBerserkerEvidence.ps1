param(
    [string]$Project='T:\TEMP\DiceFree-Options',
    [string]$QaProject='T:\TEMP\DiceFree-Roadwarden-QA',
    [string]$Build='T:\TEMP\DiceFree-Roadwarden-QA\Builds\Berserker-20261126-162640',
    [string]$ArtWorkspace='T:\TEMP\DiceFree-BerserkerArt'
)
$ErrorActionPreference='Stop'
$dest=Join-Path $Project 'docs\validation\berserker-2026-11-26'
New-Item -ItemType Directory -Force -Path $dest | Out-Null
foreach($pair in @(
    @("$QaProject\Logs\BerserkerStaticReview.json",'static-review.json'),
    @("$Build\QA\berserker-review.json",'combat-review.json'),
    @("$Build\QA\berserker-reload.json",'reload-review.json'),
    @("$Build\build-report.json",'build-report.json'),
    @("$QaProject\Logs\BerserkerPlacement.txt",'mentor-placement.txt'),
    @("$Build\QA\gorren-ashscar-mentor-world-only.png",'gorren-ashscar.png'),
    @("$Build\QA\berserker-delivery-world-only.png",'berserker.png'),
    @("$Build\QA\berserker-coffee-shield-world-only.png",'coffee-shield.png'),
    @("$ArtWorkspace\Prepared\source-receipt.json",'art-source.json'),
    @("$ArtWorkspace\package-verify-final.json",'canonical-verify.json'),
    @("$ArtWorkspace\validation.json",'asset-validation.json'),
    @("$ArtWorkspace\vendor-main.json",'vendor-main.json')
)){Copy-Item -LiteralPath $pair[0] -Destination (Join-Path $dest $pair[1])}
$counts=@{}
foreach($pair in @(@('static-review.json',392),@('combat-review.json',398),@('reload-review.json',25))){
    $r=Get-Content -LiteralPath "$dest\$($pair[0])" -Raw | ConvertFrom-Json
    if($r.status -ne 'passed' -or $r.error -or $r.unityErrors.Count -or $r.checks -ne $pair[1]){throw "Failed review: $($pair[0])"}
    $counts[$pair[0]]=$r.checks
}
$log=Get-Content -LiteralPath "$Project\Logs\BerserkerFinalMainImport.log" -Raw
if($log -notmatch 'Application will exit with return code 0' -or $log -match 'error CS\d+'){throw 'Main import failed'}
if((Get-FileHash "$Project\ProjectSettings\PackageManagerSettings.asset").Hash -ne (Get-FileHash "$Project\Logs\BerserkerPackageManagerBefore.asset").Hash){throw 'Unrelated settings changed'}
$buildReport=Get-Content "$Build\build-report.json" -Raw | ConvertFrom-Json
if($buildReport.result -ne 'Succeeded' -or $buildReport.errors -ne 0){throw 'Windows build failed'}
$paths=@(Get-Content "$Project\Logs\BerserkerSemanticAssets.txt")
$paths+='Assets/_DiceFree/Settings/Items/Wolf-Fang Greatsword.asset'
foreach($folder in 'Assets/_DiceFree/Settings/Berserker','Assets/_DiceFree/Art/Characters/Berserker'){
    $paths+=Get-ChildItem (Join-Path $Project $folder) -Recurse -File | ForEach-Object { $_.FullName.Substring($Project.Length+1).Replace('\','/') }
    $paths+=$folder+'.meta'
}
$paths+=@(
    'Application/CrimsonCrucibleRun.cs','Application/StoryDungeonRun.cs',
    'Combat/ActorStats.cs','Combat/BasicAttack.cs','Combat/EquipmentStats.cs','Combat/GroundAuraVisual.cs',
    'Gameplay/CodexProgression.cs','Items/Equipment.cs','Items/ItemDefinition.cs',
    'Quests/QuestGiver.cs','Quests/QuestWorldMarker.cs','UI/PlayerMotionPresentation.cs','UI/WorldAtlasHudMarkers.cs'
) | ForEach-Object { 'Assets/_DiceFree/Scripts/'+$_ }
$paths+=Get-ChildItem "$Project\Assets\_DiceFree\Scripts" -Recurse -File -Filter 'Berserker*' | ForEach-Object { $_.FullName.Substring($Project.Length+1).Replace('\','/') }
$paths+='Assets/_DiceFree/Resources/PlayerMotion/TierThree.meta','Assets/_DiceFree/Resources/PlayerMotion/TierThree/Berserker.controller','Assets/_DiceFree/Resources/PlayerMotion/TierThree/Berserker.controller.meta'
$paths+='Assets/_DiceFree/Shaders/GroundAuraSigil.shader','Assets/_DiceFree/Scenes/Cornberg.unity','Assets/_DiceFree/Scenes/DevelopmentRoom.unity'
function Canonical([string]$path){
    $value=[IO.File]::ReadAllText($path).Replace("`r",'') -replace '[\t ]+(?=\n|$)',''
    if($path.EndsWith('.unity')){
        $parts=[regex]::Split($value,'(?m)(?=^--- !u!)')
        return $parts[0]+(($parts | Select-Object -Skip 1 | Sort-Object) -join '')
    }
    return $value
}
$receipts=@(foreach($relative in ($paths | Sort-Object -Unique)){
    $main=Join-Path $Project $relative; $qa=Join-Path $QaProject $relative
    $mainHash=(Get-FileHash -LiteralPath $main).Hash;$qaHash=(Get-FileHash -LiteralPath $qa).Hash
    $comparison='byte-identical'
    if($mainHash -ne $qaHash){
        if([IO.Path]::GetExtension($main) -in '.fbx','.glb','.jpg','.png','.bin'){throw "Binary mismatch: $relative"}
        if((Canonical $main) -cne (Canonical $qa)){throw "Main / QA mismatch: $relative"}
        $comparison='same serialized content; whitespace or scene record order differs'
    }
    [ordered]@{path=$relative;mainSha256=$mainHash;qaSha256=$qaHash;comparison=$comparison}
})
$receipts | ConvertTo-Json -Depth 4 | Set-Content "$dest\source-receipt.json" -Encoding utf8
[ordered]@{
    executable="$Build\DiceFree.exe";executableSha256=(Get-FileHash "$Build\DiceFree.exe").Hash
    reviewBuild=$Build;checks=$counts;totalChecks=815;sourceComparisons=$receipts.Count
    mainImportExitCode=0;mainCompileErrors=0;buildErrors=$buildReport.errors;buildWarnings=$buildReport.warnings
    limitations='Automated disposable-profile checks and local party participation; no manual balance or live network co-op acceptance. Batch captures omit screen-space overlay UI.'
} | ConvertTo-Json -Depth 5 | Set-Content "$dest\delivery.json" -Encoding utf8
Write-Output "Packaged 815 checks and $($receipts.Count) source comparisons: $dest"
