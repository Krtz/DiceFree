param(
    [string]$Project='T:\TEMP\DiceFree-Options',
    [string]$QaProject='T:\TEMP\DiceFree-Roadwarden-QA',
    [string]$Build='T:\TEMP\DiceFree-Roadwarden-QA\Builds\FireMage-20261127-000732',
    [string]$CombatBuild='T:\TEMP\DiceFree-Roadwarden-QA\Builds\FireMage-20261127-000208'
)
$ErrorActionPreference='Stop'
$dest=Join-Path $Project 'docs\validation\fire-mage-2026-11-27'
New-Item -ItemType Directory -Force -Path $dest | Out-Null
foreach($pair in @(
    @("$QaProject\Logs\FireMageStaticReview.json",'static-review.json'),
    @("$CombatBuild\QA\fire-mage-review.json",'combat-and-trial-review.json'),
    @("$Build\QA\fire-mage-reload.json",'reload-review.json'),
    @("$Build\QA\fire-mage-visual.json",'visual-review.json'),
    @("$Build\build-report.json",'build-report.json'),
    @("$QaProject\Logs\FireMagePlacement.txt",'mentor-placement.txt'),
    @("$Build\QA\fire-mage-delivery-world-only.png",'fire-mage.png'),
    @("$Build\QA\maelis-final-camp-world-only.png",'maelis-cinderweave.png'),
    @("$Build\QA\ember-walk-warning-world-only.png",'ember-walk-warning.png'),
    @("$Build\QA\ember-walk-flames-world-only.png",'ember-walk-flames.png')
)){Copy-Item -LiteralPath $pair[0] -Destination (Join-Path $dest $pair[1])}
$static=Get-Content "$dest\static-review.json" -Raw|ConvertFrom-Json
$combat=Get-Content "$dest\combat-and-trial-review.json" -Raw|ConvertFrom-Json
$reload=Get-Content "$dest\reload-review.json" -Raw|ConvertFrom-Json
$visual=Get-Content "$dest\visual-review.json" -Raw|ConvertFrom-Json
foreach($review in @($static,$combat,$reload,$visual)){if($review.status -ne 'passed' -or $review.error -or $review.unityErrors.Count){throw 'A review failed'}}
if($static.checks -ne 355 -or $combat.checks -ne 242 -or $reload.checks -lt 20){throw 'Review counts incomplete'}
$log=Get-Content "$Project\Logs\FireMageFinalMainImport.log" -Raw
if($log -notmatch 'Application will exit with return code 0' -or $log -match 'error CS\d+'){throw 'Main import failed'}
if((Get-FileHash "$Project\ProjectSettings\PackageManagerSettings.asset").Hash -ne (Get-FileHash "$Project\Logs\FireMagePackageManagerBefore.asset").Hash){throw 'Unrelated settings changed'}
$report=Get-Content "$Build\build-report.json" -Raw|ConvertFrom-Json
if($report.result -ne 'Succeeded' -or $report.errors -ne 0){throw 'Windows build failed'}
$paths=@(Get-Content "$Project\Logs\FireMageSemanticAssets.txt")
foreach($folder in 'Assets/_DiceFree/Settings/FireMage','Assets/_DiceFree/Art/Characters/Dawnwarden'){
    $paths+=Get-ChildItem (Join-Path $Project $folder) -Recurse -File|ForEach-Object {$_.FullName.Substring($Project.Length+1).Replace('\','/')}
    $paths+=$folder+'.meta'
}
$paths+=Get-ChildItem "$Project\Assets\_DiceFree\Scripts" -Recurse -File|Where-Object {$_.Name -match '^(FireMage|FireEscape)'}|ForEach-Object {$_.FullName.Substring($Project.Length+1).Replace('\','/')}
$paths+=@('Application/IronpineSouthPass.cs','Combat/ActorStats.cs','Combat/GroundAuraVisual.cs','Combat/MageSkillCaster.cs','Foundation/RunLoadoutLock.cs','Gameplay/CodexProgression.cs','UI/AbilityBarSource.cs','UI/CharacterHudPanel.cs','UI/PlayerMotionPresentation.cs')|ForEach-Object {'Assets/_DiceFree/Scripts/'+$_}
$paths+='Assets/_DiceFree/Resources/AuraVfx/EmberFlame.mat','Assets/_DiceFree/Resources/AuraVfx/EmberFlame.mat.meta','Assets/_DiceFree/Resources/PlayerMotion/TierTwo/Priest.controller'
$paths+='Assets/_DiceFree/Shaders/GroundAuraSigil.shader','Assets/_DiceFree/Shaders/EmberFlame.shader','Assets/_DiceFree/Shaders/EmberFlame.shader.meta','Assets/_DiceFree/Shaders/EmberTerrain.shader','Assets/_DiceFree/Shaders/EmberTerrain.shader.meta'
$paths+='Assets/_DiceFree/Scenes/Cornberg.unity','Assets/_DiceFree/Scenes/DevelopmentRoom.unity','Assets/_DiceFree/Scenes/WorldTwoArrival.unity','Assets/_DiceFree/Scenes/EmberCrownTrial.unity','Assets/_DiceFree/Scenes/EmberCrownTrial.unity.meta','ProjectSettings/EditorBuildSettings.asset'
function Canonical([string]$path){
    $value=[IO.File]::ReadAllText($path).Replace("`r",'') -replace '[\t ]+(?=\n|$)',''
    if($path.EndsWith('.unity')){$parts=[regex]::Split($value,'(?m)(?=^--- !u!)');return $parts[0]+(($parts|Select-Object -Skip 1|Sort-Object)-join '')}
    return $value
}
$receipts=@(foreach($relative in ($paths|Sort-Object -Unique)){
    $main=Join-Path $Project $relative;$qa=Join-Path $QaProject $relative
    $a=(Get-FileHash -LiteralPath $main).Hash;$b=(Get-FileHash -LiteralPath $qa).Hash;$comparison='byte-identical'
    if($a -ne $b){if([IO.Path]::GetExtension($main) -in '.fbx','.glb','.png','.jpg','.bin'){throw "Binary mismatch: $relative"};if((Canonical $main) -cne (Canonical $qa)){throw "Main / QA mismatch: $relative"};$comparison='same serialized content; whitespace or scene record order differs'}
    [ordered]@{path=$relative;mainSha256=$a;qaSha256=$b;comparison=$comparison}
})
$receipts|ConvertTo-Json -Depth 4|Set-Content "$dest\source-receipt.json" -Encoding utf8
$assemblies=@(foreach($dll in Get-ChildItem "$Build\DiceFree_Data\Managed" -Filter 'DiceFree.*.dll'){
    $prior=Join-Path "$CombatBuild\DiceFree_Data\Managed" $dll.Name;$a=(Get-FileHash $dll.FullName).Hash;$b=(Get-FileHash $prior).Hash
    if($a -ne $b){throw "Runtime assembly changed since full combat review: $($dll.Name)"}
    [ordered]@{assembly=$dll.Name;sha256=$a;combatAndDeliveryIdentical=$true}
})
$assemblies|ConvertTo-Json -Depth 4|Set-Content "$dest\runtime-assembly-receipt.json" -Encoding utf8
[ordered]@{
    executable="$Build\DiceFree.exe";executableSha256=(Get-FileHash "$Build\DiceFree.exe").Hash
    combatBuild=$CombatBuild;reloadAndVisualBuild=$Build
    staticChecks=$static.checks;combatAndTrialChecks=$combat.checks;reloadChecks=$reload.checks;visualChecks=$visual.checks;totalChecks=($static.checks+$combat.checks+$reload.checks+$visual.checks)
    sourceComparisons=$receipts.Count;identicalRuntimeAssemblies=$assemblies.Count;mainImportExitCode=0;mainCompileErrors=0
    finalBuildErrors=$report.errors;finalBuildWarnings=$report.warnings
    finalDifference='Only authored hand attachment scale and warning-tile height changed after full combat review; all DiceFree runtime DLLs are byte-identical. Fresh-process reload and presentation checks ran on final delivery.'
    limitations='Automated disposable-profile validation. No manual balance acceptance or live network co-op acceptance. The advancement course is intentionally solo. Batch captures omit screen-space overlay UI.'
}|ConvertTo-Json -Depth 5|Set-Content "$dest\delivery.json" -Encoding utf8
Write-Output "Packaged $($static.checks+$combat.checks+$reload.checks+$visual.checks) checks and $($receipts.Count) source comparisons: $dest"
