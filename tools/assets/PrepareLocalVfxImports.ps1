param([string]$Project='T:\TEMP\DiceFree-Options',[string]$Evidence='T:\TEMP\DiceFree-AssetCatalog\2026-11-27\import-repairs')
$ErrorActionPreference='Stop'
New-Item $Evidence -ItemType Directory -Force | Out-Null
$rules=@(
 @('Assets/JMO Assets/Cartoon FX Remaster/Demo Assets','CFXRDemo','Missing optional uGUI demo dependency'),
 @('Assets/Spells Pack 2 Free Version/Demo/Environment/Scripts','DiceFree.OptionalVendorDemo.SpellsEnvironment','Demo light flicker uses removed GetInstanceID API'),
 @('Assets/NamuFX/_CommonAssets/Scripts','DiceFree.OptionalVendorDemo.Namu','Viewer is demo-only and requires uGUI'),
 @('Assets/Spells Pack 2 Free Version/Demo/Scene/Scripts','DiceFree.OptionalVendorDemo.SpellsPack','Optional demo UI requires uGUI/TMP'),
 @('Assets/VFXPACK_IMPACT_WALLCOEUR_FreeVersion/Scripts','DiceFree.OptionalVendorDemo.Wallcoeur','Optional viewer UI requires TMP'),
 @('Assets/100BestEffectPack/Script','DiceFree.OptionalLegacyVfxCode.Hundred','Legacy GUIText demo and lifetime helpers; curated effects use shared game lifetime'),
 @('Assets/48 Particle Effect Pack/Script','DiceFree.OptionalLegacyVfxCode.FortyEight','Duplicate global demo/lifetime type names and removed GUIText API')
)
$root=[IO.Path]::GetFullPath($Project).TrimEnd('\')+'\'
$receipt=@(foreach($rule in $rules){
 $directory=[IO.Path]::GetFullPath((Join-Path $Project $rule[0]));if(-not $directory.StartsWith($root,[StringComparison]::OrdinalIgnoreCase)){throw 'Target outside project'}
 if(-not(Test-Path -LiteralPath $directory)){continue}
 $defs=@(Get-ChildItem -LiteralPath $directory -Filter '*.asmdef' -File);if($defs.Count -gt 1){throw "Multiple existing definitions: $directory"}
 $path=if($defs.Count){$defs[0].FullName}else{Join-Path $directory ($rule[1]+'.asmdef')}
 if(Test-Path -LiteralPath $path){$definition=Get-Content -LiteralPath $path -Raw | ConvertFrom-Json; $backup=Join-Path $Evidence ($rule[1]+'.before.json');if(-not(Test-Path $backup)){Copy-Item -LiteralPath $path -Destination $backup}}
 else{$definition=[pscustomobject]@{name=$rule[1];references=@();autoReferenced=$false;defineConstraints=@()}}
 $definition.defineConstraints=@(@($definition.defineConstraints)+@('DICEFREE_ENABLE_VENDOR_DEMOS') | Select-Object -Unique)
 $definition | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $path -Encoding utf8
 [ordered]@{folder=$rule[0];definition=$path;reason=$rule[2];enableFlag='DICEFREE_ENABLE_VENDOR_DEMOS';sourceFilesDeleted=0;originalArchivePreserved=$true}
})
$receipt | ConvertTo-Json -Depth 6 | Set-Content (Join-Path $Evidence 'repairs.json') -Encoding utf8
Write-Output "Isolated $($receipt.Count) optional legacy/demo script folders; original asset archives preserved"