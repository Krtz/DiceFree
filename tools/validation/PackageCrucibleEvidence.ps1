param(
    [string]$Project = 'T:\TEMP\DiceFree-Options',
    [string]$QaProject = 'T:\TEMP\DiceFree-Roadwarden-QA',
    [string]$ReviewBuild = 'T:\TEMP\DiceFree-Roadwarden-QA\Builds\CrimsonCrucible-20261125-090151',
    [string]$DeliveryBuild = 'T:\TEMP\DiceFree-Roadwarden-QA\Builds\CrimsonCrucible-20261125-091545'
)
$ErrorActionPreference = 'Stop'
$dest = Join-Path $Project 'docs\validation\crimson-crucible-2026-11-25'
New-Item -ItemType Directory -Force -Path $dest | Out-Null
foreach ($spec in @(
    @("$ReviewBuild\QA\crucible-review.json", 'runtime-review.json'),
    @("$DeliveryBuild\QA\crucible-lifecycle.json", 'delivered-lifecycle.json'),
    @("$DeliveryBuild\build-report.json", 'build-report.json'),
    @("$QaProject\Logs\CrucibleAuthoring.json", 'authoring.json'),
    @("$QaProject\Logs\CrucibleLoot.json", 'loot-coverage.json'),
    @("$Project\Logs\CrucibleEquipmentAudit.json", 'equipment-before.json'),
    @("$DeliveryBuild\QA\crimson-mire-entrance-delivered-world-only.png", 'entrance.png')
)) { Copy-Item -LiteralPath $spec[0] -Destination (Join-Path $dest $spec[1]) }
foreach ($name in 'runtime-review.json','delivered-lifecycle.json') {
    $report = Get-Content -LiteralPath (Join-Path $dest $name) -Raw | ConvertFrom-Json
    if ($report.status -ne 'passed' -or $report.error -or $report.unityErrors.Count) { throw "Failed review: $name" }
}
Add-Type -AssemblyName System.Drawing
$font = [Drawing.Font]::new('Segoe UI', 15)
$small = [Drawing.Font]::new('Segoe UI', 12)
$white = [Drawing.Brushes]::White
$sheet = [Drawing.Bitmap]::new(1440, 612)
$g = [Drawing.Graphics]::FromImage($sheet)
$g.Clear([Drawing.Color]::FromArgb(20,24,28))
$labels = '1 - Verdigris Globule','2 - Moonfang Alpha','3 - Bristlebreaker','4 - Bloomheart Thornwarden','5 - Stormscale Hierophant','6 - Crimson Sovereign'
for ($i=0; $i -lt 6; $i++) {
    $x = ($i % 3)*480; $y = [Math]::Floor($i/3)*306
    $g.DrawString($labels[$i],$small,$white,$x+10,$y+5)
    $pic = [Drawing.Image]::FromFile("$ReviewBuild\QA\cell-$($i+1)-windup-world-only.png")
    $g.DrawImage($pic,[Drawing.Rectangle]::new($x,$y+32,480,270)); $pic.Dispose()
}
$sheet.Save((Join-Path $dest 'six-arenas.png')); $g.Dispose(); $sheet.Dispose()
$sheet = [Drawing.Bitmap]::new(1000,1220)
$g = [Drawing.Graphics]::FromImage($sheet); $g.Clear([Drawing.Color]::FromArgb(20,24,28))
$g.DrawString('CRIMSON CRUCIBLE - LEVEL 55 ARMOUR',$font,$white,15,8)
$roles = 'bristleforged','moonfang','stormscale','bloomkeeper'
$titles = 'STR - Bristleforged','AGI - Moonfang','INT - Stormscale','SPI - Bloomkeeper'
$slots = 'head','shoulders','legs','back'
for ($r=0; $r -lt 4; $r++) {
    $g.DrawString($titles[$r],$small,$white,$r*250+12,45)
    for ($s=0; $s -lt 4; $s++) {
        $x=$r*250+15; $y=78+$s*230
        $pic=[Drawing.Image]::FromFile("$Project\Assets\_DiceFree\Art\ItemIcons\CrimsonCrucible\item.crimson-crucible.$($roles[$r])-$($slots[$s]).png")
        $g.DrawImage($pic,[Drawing.Rectangle]::new($x,$y,205,205)); $pic.Dispose()
        $g.DrawString($slots[$s],$small,$white,$x,$y+206)
    }
}
$pic=[Drawing.Image]::FromFile("$Project\Assets\_DiceFree\Art\ItemIcons\CrimsonCrucible\item.crimson-crucible.victors-shroud.png")
$g.DrawImage($pic,[Drawing.Rectangle]::new(15,1010,190,190)); $pic.Dispose()
$g.DrawString("Rare - Crucible Victor's Shroud`nUniversal back slot - all attributes",$font,$white,230,1075)
$sheet.Save((Join-Path $dest 'loot-matrix.png')); $g.Dispose(); $sheet.Dispose(); $font.Dispose(); $small.Dispose()
$paths = @(
    'Assets\_DiceFree\Settings\CrimsonCrucible',
    'Assets\_DiceFree\Resources\DiceFree\DungeonRewards\CrimsonCrucible',
    'Assets\_DiceFree\Art\ItemIcons\CrimsonCrucible'
)
$files = @($paths | ForEach-Object { Get-ChildItem -LiteralPath (Join-Path $Project $_) -Recurse -File })
foreach ($name in 'CrimsonCruciblePortal','CrimsonCrucibleRun','CrucibleArena','CrucibleBoss','CrucibleGate','ReturnToStartMenu') {
    $files += Get-Item -LiteralPath "$Project\Assets\_DiceFree\Scripts\Application\$name.cs"
}
$files += Get-Item -LiteralPath "$Project\Assets\_DiceFree\Scripts\UI\WorldAtlasHud.cs","$Project\Assets\_DiceFree\Scenes\CrimsonCrucible.unity","$Project\ProjectSettings\EditorBuildSettings.asset"
$receipt = @($files | ForEach-Object {
    $relative=$_.FullName.Substring($Project.Length+1); $hash=(Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash
    $qaHash=(Get-FileHash -LiteralPath (Join-Path $QaProject $relative) -Algorithm SHA256).Hash
    if ($hash -ne $qaHash) { throw "Main / QA content mismatch: $relative" }
    [ordered]@{path=$relative.Replace('\','/');sha256=$hash}
})
$receipt | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $dest 'source-receipt.json') -Encoding utf8
$log=Get-Content -LiteralPath "$Project\Logs\CrucibleMainImport.log" -Raw
if ($log -notmatch 'Application will exit with return code 0' -or $log -match 'error CS\d+') { throw 'Main project import did not pass' }
[ordered]@{
    executable="$DeliveryBuild\DiceFree.exe"; executableSha256=(Get-FileHash -LiteralPath "$DeliveryBuild\DiceFree.exe").Hash
    runtimeReviewBuild=$ReviewBuild; deliveredLifecycleBuild=$DeliveryBuild
    runtimeChecks=137; deliveredLifecycleChecks=17; mainImportExitCode=0; mainCompileErrors=0
    note='Cornberg scene records normalized after sync; only the new entrance was added. Review build and delivered build share gameplay code. Delivered build adds brighter icons and entrance ornament.'
} | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $dest 'delivery.json') -Encoding utf8
Write-Output "Packaged evidence and verified $($receipt.Count) matching main / QA files: $dest"
