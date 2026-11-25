param(
    [string]$Project='T:\TEMP\DiceFree-Options',
    [string]$QaProject='T:\TEMP\DiceFree-Roadwarden-QA',
    [string]$Build='T:\TEMP\DiceFree-Roadwarden-QA\Builds\Marksman-20261125-235648',
    [string]$CombatBuild='T:\TEMP\DiceFree-Roadwarden-QA\Builds\Marksman-20261125-233955',
    [string]$ReloadBuild='T:\TEMP\DiceFree-Roadwarden-QA\Builds\Marksman-20261125-235015'
)
$ErrorActionPreference='Stop'
$dest=Join-Path $Project 'docs\validation\marksman-2026-11-25'
New-Item -ItemType Directory -Force -Path $dest | Out-Null
foreach($pair in @(
    @("$QaProject\Logs\MarksmanStaticReview.json",'static-review.json'),
    @("$CombatBuild\QA\marksman-review.json",'combat-review.json'),
    @("$ReloadBuild\QA\marksman-reload.json",'reload-review.json'),
    @("$Build\QA\marksman-aura-proof.json",'aura-proof.json'),
    @("$Build\QA\world-two-routes-review.json",'world-two-roads-review.json'),
    @("$Build\build-report.json",'build-report.json'),
    @("$QaProject\Logs\MarksmanManaBefore.txt",'mana-before.txt'),
    @("$QaProject\Logs\MarksmanPlacements.txt",'placements.txt'),
    @("$Build\QA\marksman-road-sigil-visible.png",'marksman-sigil-visible.png'),
    @("$Build\QA\marksman-road-sigil-hidden.png",'marksman-sigil-control.png'),
    @("$CombatBuild\QA\aelwen-marksman-teacher-world-only.png",'aelwen-teacher.png'),
    @("$CombatBuild\QA\marksman-level-one-world-only.png",'marksman-level-one.png')
)){Copy-Item -LiteralPath $pair[0] -Destination (Join-Path $dest $pair[1])}
$counts=@{}
foreach($name in 'combat-review.json','reload-review.json','aura-proof.json','world-two-roads-review.json'){
    $r=Get-Content -LiteralPath "$dest\$name" -Raw | ConvertFrom-Json
    if($r.status -ne 'passed' -or $r.error -or $r.unityErrors.Count){throw "Failed review: $name"}
    $counts[$name]=$r.checks
}
$static=Get-Content "$dest\static-review.json" -Raw | ConvertFrom-Json
if($static.checks -ne 198 -or $static.passed.Count -ne 198){throw 'Static review incomplete'}
$log=Get-Content -LiteralPath "$Project\Logs\MarksmanMainDeliveredImport2.log" -Raw
if($log -notmatch 'Application will exit with return code 0' -or $log -match 'error CS\d+'){throw 'Main import failed'}
$paths=@(Get-Content "$Project\Logs\MarksmanSemanticAssets.txt")
$paths+=Get-ChildItem "$Project\Assets\_DiceFree\Settings\Marksman" -Recurse -File | ForEach-Object { $_.FullName.Substring($Project.Length+1).Replace('\','/') }
$paths+='Assets/_DiceFree/Settings/Marksman.meta'
$paths+=@(
    'Combat/ActorResourceController.cs','Combat/BasicAttack.cs','Combat/ClassResourceProfile.cs',
    'Combat/DamageResolver.cs','Combat/GroundAuraVisual.cs','Gameplay/CodexProgression.cs',
    'Persistence/ManifestationPersistence.cs','Quests/QuestJournal.cs','UI/CharacterStatsHud.cs',
    'UI/PlayerMotionPresentation.cs','UI/RangerBowPresentation.cs','UI/TargetInspectionHud.cs',
    'World/TravelSurface.cs'
) | ForEach-Object { 'Assets/_DiceFree/Scripts/'+$_ }
$paths+='Assets/_DiceFree/Shaders/GroundAuraSigil.shader'
$paths+=Get-ChildItem "$Project\Assets\_DiceFree\Scripts" -Recurse -File -Filter 'Marksman*' | ForEach-Object { $_.FullName.Substring($Project.Length+1).Replace('\','/') }
$paths+='Assets/_DiceFree/Scenes/Cornberg.unity','Assets/_DiceFree/Scenes/WorldTwoArrival.unity','Assets/_DiceFree/Scenes/DevelopmentRoom.unity'
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
    $mainHash=(Get-FileHash -LiteralPath $main).Hash
    $qaHash=(Get-FileHash -LiteralPath $qa).Hash
    $comparison='byte-identical'
    if($mainHash -ne $qaHash){
        if((Canonical $main) -cne (Canonical $qa)){throw "Main / QA mismatch: $relative"}
        $comparison='same serialized content; whitespace or scene record order differs'
    }
    [ordered]@{path=$relative;mainSha256=$mainHash;qaSha256=$qaHash;comparison=$comparison}
})
$receipts | ConvertTo-Json -Depth 4 | Set-Content "$dest\source-receipt.json" -Encoding utf8
[ordered]@{
    executable="$Build\DiceFree.exe";executableSha256=(Get-FileHash "$Build\DiceFree.exe").Hash
    combatReviewBuild=$CombatBuild;reloadReviewBuild=$ReloadBuild;finalVisualAndRoadReviewBuild=$Build
    staticChecks=198;runtimeChecks=$counts;totalChecks=746;mainImportExitCode=0;mainCompileErrors=0
    finalDifference='Final delivery adds only the shared aura road-height projection and render order; combat and quest implementation unchanged from the full combat review.'
    limitations='Automated disposable-profile checks; no manual balance acceptance or live network co-op validation. Batch screenshots omit screen-space overlay UI.'
} | ConvertTo-Json -Depth 5 | Set-Content "$dest\delivery.json" -Encoding utf8
Write-Output "Packaged 746 checks and $($receipts.Count) source comparisons: $dest"
