param([Parameter(Mandatory=$true)][string]$BuildDirectory)
$ErrorActionPreference='Stop'
$buildRoot=(Resolve-Path -LiteralPath $BuildDirectory).Path
$executable=Join-Path $buildRoot 'DiceFree.exe'
if(-not (Test-Path -LiteralPath $executable -PathType Leaf)){throw 'Built executable missing.'}
$qa=Join-Path $buildRoot 'QA'
$profile=Join-Path $qa 'Profile'
if(Test-Path -LiteralPath $profile){throw 'Use a fresh timestamped build with a new disposable QA profile.'}
New-Item -ItemType Directory -Path $qa -Force | Out-Null
$playerProcess=Start-Process -FilePath $executable -WorkingDirectory $buildRoot -WindowStyle Hidden -ArgumentList @('-screen-fullscreen','0','-screen-width','1600','-screen-height','900','-diceFreeSaveRoot',$profile,'-logFile',(Join-Path $qa 'Player.log')) -PassThru
# Retain the native handle so the exit code remains available after normal menu Quit.
$nativeHandle=$playerProcess.Handle
$playerProcess.Id | Set-Content -LiteralPath (Join-Path $qa 'player-pid.txt')
Write-Output "Player launched: PID $($playerProcess.Id). Run WindowsMilestoneSmoke.cs against this build's runtime endpoint."
$deadline=[DateTime]::UtcNow.AddMinutes(12)
while(-not $playerProcess.WaitForExit(1000)){if([DateTime]::UtcNow -gt $deadline){throw 'Player did not exit within twelve minutes; it has been left running for diagnosis.'}}
$result=[ordered]@{pid=$playerProcess.Id;exited=$playerProcess.HasExited;exitCode=$playerProcess.ExitCode;normalQuitExpected=$true}
$result | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $qa 'process-exit.json')
$playerProcess.Dispose()
$result | ConvertTo-Json
