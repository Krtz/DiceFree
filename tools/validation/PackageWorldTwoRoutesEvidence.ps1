param(
    [string]$Project='T:\TEMP\DiceFree-Options',
    [string]$QaProject='T:\TEMP\DiceFree-Roadwarden-QA',
    [string]$Build='T:\TEMP\DiceFree-Roadwarden-QA\Builds\WorldTwoRoutes-20261125-094008'
)
$ErrorActionPreference='Stop'
$dest=Join-Path $Project 'docs\validation\world-two-routes-library-2026-11-25'
New-Item -ItemType Directory -Force -Path $dest | Out-Null
foreach($pair in @(
    @("$Build\QA\world-two-routes-review.json",'runtime-review.json'),
    @("$Build\QA\world-two-tree-fade-review.json",'tree-fade-review.json'),
    @("$Build\build-report.json",'build-report.json'),
    @("$QaProject\Logs\WorldTwoRoutes.json",'routes.json'),
    @("$QaProject\Logs\WorldTwoLibraryPass.json",'replacements.json'),
    @("$QaProject\Logs\WorldTwoRoutesStaticReview.json",'portable-assets.json'),
    @("$Project\Logs\WorldTwoLibraryShortlist.json",'catalog-shortlist.json'),
    @("$QaProject\Logs\WorldTwoLibrary\candidates.txt",'preview-candidates.txt'),
    @("$QaProject\Logs\WorldTwoRouteSurvey.txt",'routes-before.txt')
)){Copy-Item -LiteralPath $pair[0] -Destination (Join-Path $dest $pair[1])}
foreach($name in 'runtime-review.json','tree-fade-review.json'){
    $r=Get-Content -LiteralPath (Join-Path $dest $name) -Raw | ConvertFrom-Json
    if($r.status -ne 'passed' -or $r.error -or $r.unityErrors.Count){throw "Review did not pass: $name"}
}
foreach($name in 'anvilreach-stocked-market','anvilreach-stocked-foundry','steamroot-river-groves','deepmoss-rooted-groves','cinderwatch-road'){
    Copy-Item -LiteralPath "$Build\QA\$name-world-only.png" -Destination "$dest\$name.png"
}
$files=Get-ChildItem -LiteralPath "$Project\Assets\_DiceFree\Art\World\WorldTwoLibraryPass" -File
$hashes=@($files|ForEach-Object{
    $relative=$_.FullName.Substring($Project.Length+1)
    $mainHash=(Get-FileHash -LiteralPath $_.FullName).Hash
    $qaHash=(Get-FileHash -LiteralPath (Join-Path $QaProject $relative)).Hash
    if($mainHash -ne $qaHash){throw "Main / QA mismatch: $relative"}
    [ordered]@{path=$relative.Replace('\','/');sha256=$mainHash}
})
$hashes|ConvertTo-Json -Depth 3|Set-Content -LiteralPath "$dest\source-receipt.json" -Encoding utf8
$log=Get-Content -LiteralPath "$Project\Logs\WorldTwoRoutesMainImport.log" -Raw
if($log -notmatch 'Application will exit with return code 0' -or $log -match 'error CS\d+'){throw 'Main import failed'}
[ordered]@{
    executable="$Build\DiceFree.exe";executableSha256=(Get-FileHash -LiteralPath "$Build\DiceFree.exe").Hash
    runtimeChecks=97;actualTreeFadeChecks=2;mainImportExitCode=0;mainCompileErrors=0
    note='WorldTwoArrival scene records normalized after copying from QA. New asset hashes match QA; static authoring also verifies no private source dependencies. Existing package settings and recovery work excluded.'
}|ConvertTo-Json|Set-Content -LiteralPath "$dest\delivery.json" -Encoding utf8
Add-Type -AssemblyName System.Drawing
$sheet=[Drawing.Bitmap]::new(1600,960);$g=[Drawing.Graphics]::FromImage($sheet)
$g.Clear([Drawing.Color]::FromArgb(20,24,28));$font=[Drawing.Font]::new('Segoe UI',15)
$names='anvilreach-stocked-foundry','steamroot-river-groves','deepmoss-rooted-groves','cinderwatch-road'
$labels='Anvilreach - stocked smiths benches','Steamroot - riverbank willows','Deepmoss - rooted trees','Cinderwatch - continuous road bonus'
for($i=0;$i -lt 4;$i++){
    $x=($i%2)*800;$y=[Math]::Floor($i/2)*480
    $g.DrawString($labels[$i],$font,[Drawing.Brushes]::White,$x+12,$y+3)
    $image=[Drawing.Image]::FromFile("$dest\$($names[$i]).png")
    $g.DrawImage($image,[Drawing.Rectangle]::new($x,$y+30,800,450));$image.Dispose()
}
$sheet.Save("$dest\overview.png");$g.Dispose();$sheet.Dispose();$font.Dispose()
Write-Output "Packaged World Two road and library evidence; $($hashes.Count) matching main/QA files."
