param([string]$ProjectRoot='T:\TEMP\DiceFree-Options',[int]$Port=17877)
$ErrorActionPreference='Stop'
$run=Join-Path $ProjectRoot ('Library\NetworkLab-'+[DateTime]::UtcNow.ToString('yyyyMMdd-HHmmss'))
New-Item -ItemType Directory -Force $run | Out-Null
$exe=Join-Path $ProjectRoot 'Builds\World1Foundation\DiceFree.exe'
$processes=@{};$sequences=@{};$checks=New-Object 'System.Collections.Generic.List[object]'
$evidence=New-Object 'System.Collections.Generic.List[object]';$failure=$null
$states=@{}
function State([string]$r){
 try{$value=Get-Content (Join-Path $run ($r+'.json')) -Raw|ConvertFrom-Json;if($null -ne $value){$states[$r]=$value}}catch{}
 if($states.ContainsKey($r)){return $states[$r]};return $null
}
function Self([string]$r){$s=State $r;return $s.snapshot.actors|Where-Object id -eq $s.snapshot.ownId|Select-Object -First 1}
function Player([string]$id){return (State 'host').snapshot.actors|Where-Object id -eq $id|Select-Object -First 1}
function Wait-For([string]$n,[scriptblock]$c,[int]$seconds=45){
 $end=[DateTime]::UtcNow.AddSeconds($seconds)
 while([DateTime]::UtcNow -lt $end){if(&$c){return};Start-Sleep -Milliseconds 200};throw "Timeout: $n"
}
function Check([string]$n,[bool]$ok,$detail=$null){
 $checks.Add([pscustomobject]@{name=$n;passed=$ok;detail=$detail});Write-Output ("CHECK "+$n+": "+$ok)
 if(-not $ok){throw "Failed: $n"}
}
function Capture([string]$n){$evidence.Add([pscustomobject]@{name=$n;host=(State 'host');guest=(State 'guest')})}
function Send([string]$r,[hashtable]$cmd){
 if(-not $sequences.ContainsKey($r)){$sequences[$r]=0};$sequences[$r]++;$cmd.sequence=$sequences[$r]
 $p=Join-Path $run ($r+'-command.json')
 [IO.File]::WriteAllText($p+'.tmp',($cmd|ConvertTo-Json -Compress))
 $deadline=[DateTime]::UtcNow.AddSeconds(3)
 while($true){try{[IO.File]::Copy($p+'.tmp',$p,$true);break}catch [IO.IOException]{if([DateTime]::UtcNow -gt $deadline){throw};Start-Sleep -Milliseconds 30}}
}
function Start-Peer([string]$r,[string]$mode){
 $a=@('-batchmode','-nographics','-diceFreeNetwork',$mode,'-diceFreeNetworkPort',"$Port",
 '-diceFreeNetworkReport',(Join-Path $run ($r+'.json')),'-diceFreeNetworkCommands',(Join-Path $run ($r+'-command.json')),
 '-logFile',(Join-Path $run ($r+'.log')))
 $processes[$r]=Start-Process $exe -ArgumentList $a -PassThru
 Write-Output ("START "+$r+" PID "+$processes[$r].Id)
}
function Distance($a,$b){return [Math]::Sqrt([Math]::Pow($a.x-$b.x,2)+[Math]::Pow($a.z-$b.z,2))}
try{
 Start-Peer 'host' 'host'
 Wait-For 'host ready' {$s=State 'host';$s.ready -and $s.connected -and $null -ne $s.snapshot}
 Start-Peer 'guest' 'client'
 Wait-For 'guest state' {$s=State 'guest';$s.connected -and $s.snapshots -gt 3 -and @($s.snapshot.actors|Where-Object player).Count -eq 2}
 $hostId=(Self 'host').id;$guestId=(Self 'guest').id;$runtime=(Player $guestId).runtimeId
 Check 'Separate processes exchange authoritative state' ($hostId -ne $guestId)
 Capture 'joined'
 Send 'host' @{kind='move';x=-6;z=-8};Send 'guest' @{kind='move';x=6;z=-8}
 Wait-For 'both move' {(Distance (Player $hostId).position @{x=-6;z=-8}) -lt .5 -and (Distance (Player $guestId).position @{x=6;z=-8}) -lt .5}
 Check 'Independent host and guest movement' $true
 Wait-For 'position replication' {(Distance (Self 'guest').position (Player $guestId).position) -lt .3}
 Check 'Client receives authoritative position' $true
 $rejected=(State 'host').rejected;Send 'guest' @{kind='move';x=999999;z=999999}
 Wait-For 'reject bad destination' {(State 'host').rejected -gt $rejected}
 $rejected=(State 'host').rejected;Send 'guest' @{kind='attack';target='sentinel.world.test.2'}
 Wait-For 'reject cross face attack' {(State 'host').rejected -gt $rejected}
 Check 'Out-of-bounds movement and cross-face attack rejected' $true
 Send 'guest' @{kind='move';x=0;z=21}
 Wait-For 'north edge' {$p=Player $guestId;(Distance $p.position @{x=0;z=21}) -lt .7 -or $p.travelling -or $p.face -eq 'world.test.2'}
 # Crossing must be activated by walking onto the pad, without a travel command.
 Wait-For 'turning' {(Player $guestId).travelling}
 $turnTick=(State 'host').snapshot.tick
 Send 'host' @{kind='move';x=-6;z=-3}
 Wait-For 'host moves during flip' {(Player $hostId).position.z -gt -4}
 Check 'Other participant moves during cube flip' ((Player $hostId).face -eq 'world.test.1') @{startTick=$turnTick;endTick=(State 'host').snapshot.tick}
 Wait-For 'world2 presentation' {$s=State 'guest';$s.localFace -eq 'world.test.2' -and $s.flips -eq 1 -and -not $s.busy}
 Check 'Guest crosses alone while host stays on world1' ((Player $hostId).face -eq 'world.test.1' -and (Player $guestId).face -eq 'world.test.2')
 Capture 'split-worlds'
 Send 'guest' @{kind='move';x=40000;z=39990}
 Wait-For 'walk inward to rearm arrival gate' {(Distance (Player $guestId).position @{x=40000;z=39990}) -lt .5}
 Check 'Walking inward rearms the gate without arrival bounce' ((Player $guestId).face -eq 'world.test.2' -and (Player $guestId).transitions -eq 1)
 Send 'guest' @{kind='attack';target='sentinel.world.test.2'};Send 'host' @{kind='attack';target='sentinel.world.test.1'}
 Wait-For 'combat in both worlds' {(Player $guestId).hits -ge 1 -and (Player $hostId).hits -ge 1}
 Check 'Existing BasicAttack resolves in both occupied worlds' ((Player 'sentinel.world.test.1').hp -lt 2000 -and (Player 'sentinel.world.test.2').hp -lt 2000)
 Capture 'simultaneous-combat'
 Send 'guest' @{kind='move';x=40000;z=39979};Send 'host' @{kind='move';x=6;z=-3}
 Wait-For 'south edge' {$p=Player $guestId;(Distance $p.position @{x=40000;z=39979}) -lt .7 -or $p.travelling -or $p.face -eq 'world.test.1'}
 Start-Sleep -Milliseconds 800
 # Return crossing must also activate from walking alone.
 Wait-For 'return presentation' {$s=State 'guest';$s.localFace -eq 'world.test.1' -and $s.flips -eq 2 -and -not $s.busy}
 Check 'Round trip preserves server actor identity' ((Player $guestId).runtimeId -eq $runtime -and (Player $guestId).transitions -eq 2) $runtime
 Capture 'round-trip'
 Start-Peer 'late' 'client'
 Wait-For 'late join' {$s=State 'late';$s.connected -and @($s.snapshot.actors|Where-Object player).Count -eq 3}
 Check 'Late join receives damaged-world state' (((State 'late').snapshot.actors|Where-Object id -eq 'sentinel.world.test.1').hp -lt 2000)
 $lateId=(Self 'late').id
 Start-Peer 'fourth' 'client'
 Wait-For 'four player join' {$s=State 'fourth';$s.connected -and @($s.snapshot.actors|Where-Object player).Count -eq 4}
 Check 'Four players coexist' $true
 Start-Peer 'overflow' 'client'
 Wait-For 'fifth rejected' {$s=State 'overflow';$s.ready -and -not $s.connected -and $s.status -like 'Disconnected*'}
 Check 'Fifth player rejected without replacing roster' (@((State 'host').snapshot.actors|Where-Object player).Count -eq 4)
 $attackBefore=Player $guestId
 Send 'guest' @{kind='attack';target='sentinel.world.test.1'}
 Wait-For 'guest reaches enemy and takes new damage' {$p=Player $guestId;$null -ne $p -and $p.hits -gt $attackBefore.hits -and $p.hp -lt $attackBefore.hp-.5}
 $before=Player $guestId;Send 'guest' @{kind='disconnect'}
 Wait-For 'disconnected on host' {$p=Player $guestId;$null -ne $p -and -not $p.connected} 10
 Wait-For 'vulnerable actor' {$p=Player $guestId;$null -ne $p -and $p.hp -lt $before.hp-.5} 10
 $during=Player $guestId
 Check 'Disconnected actor remains vulnerable in grace' ($during.runtimeId -eq $runtime) @{before=$before.hp;during=$during.hp}
 Send 'guest' @{kind='reconnect'}
 Wait-For 'same actor reconnects' {$s=State 'guest';$s.connected -and $s.snapshot.ownId -eq $guestId -and (Player $guestId).connected}
 Check 'Reconnect keeps actor and missing HP' ((Player $guestId).runtimeId -eq $runtime -and (Player $guestId).hp -lt (Player $guestId).maximumHp)
 Capture 'reconnected'
 Send 'guest' @{kind='move';x=0;z=10};Send 'late' @{kind='disconnect'}
 Wait-For 'late reserved' {-not (Player $lateId).connected}
 Wait-For 'grace expiry' {$null -eq (Player $lateId)} 40
 Check 'Expired grace removes abandoned actor' $true
 Send 'late' @{kind='reconnect'}
 Wait-For 'expired actor fresh join' {$s=State 'late';$s.connected -and $null -ne $s.snapshot -and $s.snapshot.ownId -ne $lateId}
 Check 'Expired reconnect uses fresh join rules' ((Self 'late').face -eq 'world.test.1')
 Capture 'expired-rejoin'
 foreach($r in @('host','guest','late','fourth','overflow')){
  $log=Get-Content (Join-Path $run ($r+'.log')) -Raw
  Check ("No runtime/navigation errors: "+$r) (-not ($log -match 'NullReferenceException|InvalidOperationException|IndexOutOfRangeException|Failed to create agent|missing or unsupported'))
 }
 Check 'Both faces remain loaded on host' (@((State 'host').loadedFaces).Count -eq 2)
}catch{$failure=$_.Exception.Message;Write-Output ("FAILED: "+$failure);Capture 'failure'}
finally{
 foreach($r in @($processes.Keys)){Send $r @{kind='quit'}};Start-Sleep -Seconds 2
 foreach($p in $processes.Values){if(-not $p.HasExited){Stop-Process -Id $p.Id -ErrorAction SilentlyContinue}}
 $result=[pscustomobject]@{marker='DICEFREE_NETWORK_WORLD_LAB';passed=($null -eq $failure);failure=$failure;
 utc=[DateTime]::UtcNow.ToString('o');checks=$checks.ToArray();evidence=$evidence.ToArray();
 limits=@('Loopback only; laptop/internet untested','Basic attacks; isolated Novices padded to 500 HP','Both test maps resident in every process','No skills/progression/host migration/production-world replication')}
 [IO.File]::WriteAllText((Join-Path $run 'result.json'),($result|ConvertTo-Json -Depth 20))
 [IO.File]::WriteAllText((Join-Path $ProjectRoot 'Library\NetworkLabLatestRun.txt'),$run)
 Write-Output ("RESULT "+$run+'\result.json')
}
if($null -ne $failure){exit 1}
