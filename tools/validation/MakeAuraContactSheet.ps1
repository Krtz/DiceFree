param([Parameter(Mandatory=$true)][string]$QaDirectory,[Parameter(Mandatory=$true)][string]$OutputPath)
Add-Type -AssemblyName System.Drawing
$entries=@(
 @('Mana Attunement','mana-attunement.png','#49b9ff'),
 @('Elemental Weaving','elemental-weaving.png','#f3c77b'),
 @('Hold the Line','hold-the-line.png','#efbb50'),
 @('Speed Presence','speed-presence.png','#b58bff'),
 @('Empowered Ice Aegis','empowered-ice-aegis.png','#92e3ff'),
 @('Radiation','radiation.png','#a5ef42')
)
$canvas=[Drawing.Bitmap]::new(1416,860)
$graphics=[Drawing.Graphics]::FromImage($canvas)
$font=[Drawing.Font]::new('Segoe UI',18,[Drawing.FontStyle]::Bold)
$background=[Drawing.SolidBrush]::new([Drawing.ColorTranslator]::FromHtml('#10171e'))
$panel=[Drawing.SolidBrush]::new([Drawing.ColorTranslator]::FromHtml('#1b2731'))
try {
 $graphics.FillRectangle($background,0,0,$canvas.Width,$canvas.Height)
 $graphics.InterpolationMode=[Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
 for($i=0;$i -lt $entries.Count;$i++) {
  $x=($i%3)*472
  $y=[Math]::Floor($i/3)*430
  $graphics.FillRectangle($panel,$x+8,$y+8,456,414)
  $accent=[Drawing.SolidBrush]::new([Drawing.ColorTranslator]::FromHtml($entries[$i][2]))
  $source=[Drawing.Image]::FromFile((Join-Path $QaDirectory $entries[$i][1]))
  try {
   $graphics.FillRectangle($accent,$x+16,$y+20,4,24)
   $graphics.DrawString($entries[$i][0],$font,[Drawing.Brushes]::White,$x+30,$y+16)
   $destination=[Drawing.Rectangle]::new($x+16,$y+54,440,356)
   $crop=[Drawing.Rectangle]::new(850,510,220,178)
   $graphics.DrawImage($source,$destination,$crop,[Drawing.GraphicsUnit]::Pixel)
  } finally { $accent.Dispose();$source.Dispose() }
 }
 $canvas.Save([IO.Path]::GetFullPath($OutputPath),[Drawing.Imaging.ImageFormat]::Png)
} finally { $graphics.Dispose();$canvas.Dispose();$font.Dispose();$background.Dispose();$panel.Dispose() }
