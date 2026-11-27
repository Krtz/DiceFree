param(
    [string]$Project='T:\TEMP\DiceFree-Options',
    [string]$QaProject='T:\TEMP\DiceFree-Roadwarden-QA',
    [string]$Build='T:\TEMP\DiceFree-Roadwarden-QA\Builds\FireMage-20261127-160557'
)
$ErrorActionPreference='Stop'
$dest=Join-Path $Project 'docs\validation\fire-mage-staff-2026-11-27'
New-Item $dest -ItemType Directory -Force | Out-Null
foreach($pair in @(
    @("$QaProject\Logs\FireMageStaticReview.json",'class-static-review.json'),
    @("$QaProject\Logs\FireMageMagicianStaticReview.json",'model-static-review.json'),
    @("$QaProject\Logs\FireMageStaffGripReview.json",'staff-grip-review.json'),
    @("$Build\QA\fire-mage-staff.json",'runtime-review.json'),
    @("$Build\build-report.json",'build-report.json')
)){Copy-Item -LiteralPath $pair[0] -Destination (Join-Path $dest $pair[1])}
$reviews=@('class-static-review','model-static-review','staff-grip-review','runtime-review')|ForEach-Object {Get-Content "$dest\$_.json" -Raw|ConvertFrom-Json}
foreach($r in $reviews){if($r.status -ne 'passed' -or $r.error -or $r.unityErrors.Count){throw 'A review failed'}}
if($reviews[2].samples -ne 560 -or $reviews[2].minimumHeadAndHatClearance -lt .025){throw 'Staff clearance review incomplete'}
Copy-Item "$Build\QA\staff*.png" $dest
$log=Get-Content "$Project\Logs\FireMageStaffMainImport.log" -Raw
if($log -notmatch 'Application will exit with return code 0' -or $log -match 'error CS\d+'){throw 'Main import failed'}
if((Get-FileHash "$Project\ProjectSettings\PackageManagerSettings.asset").Hash -ne (Get-FileHash "$Project\Logs\FireMageStaffPackageManagerBefore.asset").Hash){throw 'Unrelated settings changed'}
$report=Get-Content "$dest\build-report.json" -Raw|ConvertFrom-Json
if($report.result -ne 'Succeeded' -or $report.errors -ne 0){throw 'Build failed'}
function Canonical([string]$path){
    $value=[IO.File]::ReadAllText($path).Replace("`r",'') -replace '[\t ]+(?=\n|$)',''
    if($path.EndsWith('.unity')){$parts=[regex]::Split($value,'(?m)(?=^--- !u!)');return $parts[0]+(($parts|Select-Object -Skip 1|Sort-Object)-join '')}
    return $value
}
$paths=(Get-Content "$Project\docs\validation\fire-mage-magician-2026-11-27\source-comparison.json" -Raw|ConvertFrom-Json).path
$receipts=@(foreach($relative in $paths){
    $main=Join-Path $Project $relative;$qa=Join-Path $QaProject $relative
    $a=(Get-FileHash -LiteralPath $main).Hash;$b=(Get-FileHash -LiteralPath $qa).Hash;$comparison='byte-identical'
    if($a -ne $b){if([IO.Path]::GetExtension($main) -in '.fbx','.glb','.png','.bin' -or (Canonical $main) -cne (Canonical $qa)){throw "Main/QA mismatch: $relative"};$comparison='same serialized content; whitespace or scene record order differs'}
    [ordered]@{path=$relative;mainSha256=$a;qaSha256=$b;comparison=$comparison}
})
$receipts|ConvertTo-Json -Depth 4|Set-Content "$dest\source-comparison.json" -Encoding utf8
[ordered]@{
    executable="$Build\DiceFree.exe";executableSha256=(Get-FileHash "$Build\DiceFree.exe").Hash
    staticChecks=$reviews[0].checks;modelChecks=$reviews[1].checks;runtimeChecks=$reviews[3].checks
    animationSamples=$reviews[2].samples;minimumHeadAndHatClearanceMeters=$reviews[2].minimumHeadAndHatClearance
    sourceComparisons=$receipts.Count;mainImportExitCode=0;mainCompileErrors=0;buildErrors=$report.errors;buildWarnings=$report.warnings
    limitations='Automated disposable-profile checks; batch captures omit overlay UI. Clearance checks sample seven motion states at twenty times and four headings. No new manual balance or live co-op acceptance.'
}|ConvertTo-Json -Depth 5|Set-Content "$dest\delivery.json" -Encoding utf8
Write-Output "Packaged 560 holding-pose samples, $($reviews[3].checks) player checks and $($receipts.Count) source comparisons"