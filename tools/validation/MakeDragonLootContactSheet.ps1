param([Parameter(Mandatory=$true)][string]$IconDirectory,[Parameter(Mandatory=$true)][string]$OutputPath)
Add-Type -AssemblyName System.Drawing
$entries=@(
 @('Wyrmfang Cuirass','wyrmfang-cuirass','AGI melee'),
 @('Roostguard Bulwark','roostguard-bulwark','STR tank'),
 @('Embermender Vestments','embermender-vestments','SPI healer'),
 @('Jadescale Arcanist Robe','jadescale-arcanist-robe','INT caster'),
 @("Rooststalker's Grips",'rooststalkers-grips','Attack speed and melee damage'),
 @('Dragontrail Greaves','dragontrail-greaves','All stats and movement speed')
)
$canvas=[Drawing.Bitmap]::new(1200,900)
$graphics=[Drawing.Graphics]::FromImage($canvas)
$title=[Drawing.Font]::new('Segoe UI',16,[Drawing.FontStyle]::Bold)
$label=[Drawing.Font]::new('Segoe UI',12)
$background=[Drawing.SolidBrush]::new([Drawing.ColorTranslator]::FromHtml('#0e191b'))
$panel=[Drawing.SolidBrush]::new([Drawing.ColorTranslator]::FromHtml('#1a292c'))
try {
 $graphics.FillRectangle($background,0,0,1200,900)
 $graphics.InterpolationMode=[Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
 for($i=0;$i -lt $entries.Count;$i++) {
  $x=($i%3)*400;$y=[Math]::Floor($i/3)*450
  $graphics.FillRectangle($panel,$x+8,$y+8,384,434)
  $graphics.DrawString($entries[$i][0],$title,[Drawing.Brushes]::White,$x+20,$y+20)
  $graphics.DrawString(('Level 55 | '+$entries[$i][2]),$label,[Drawing.Brushes]::PaleGoldenrod,$x+20,$y+52)
  $source=[Drawing.Image]::FromFile((Join-Path $IconDirectory ('item.ancient-road-dragon.'+$entries[$i][1]+'.png')))
  try{$graphics.DrawImage($source,[Drawing.Rectangle]::new($x+20,$y+82,360,350))}finally{$source.Dispose()}
 }
 $canvas.Save([IO.Path]::GetFullPath($OutputPath),[Drawing.Imaging.ImageFormat]::Png)
} finally {$graphics.Dispose();$canvas.Dispose();$title.Dispose();$label.Dispose();$background.Dispose();$panel.Dispose()}
