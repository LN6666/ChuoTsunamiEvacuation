# P10-B++ Disk Paging Risk Checklist

P10-B++ does not modify OS pagefile settings.

## Symptoms To Watch

- Disk usage spikes to high values while FPS drops.
- Process memory approaches available physical RAM.
- The game stutters during camera movement after the scene has already loaded.
- Windows becomes sluggish outside Unity/player.
- Resource Monitor shows hard faults/sec during gameplay.

## Task Manager Checks

During P10-C built-player profiling:

- Open Task Manager.
- Watch CPU percent for the player.
- Watch Memory percent for the system.
- Watch player process memory.
- Watch Disk active time.
- Record whether disk activity spikes during load, tsunami start, green frames, light curtain, ResultPanel, or night/rain mode.

## PowerShell Checks

Optional commands:

```powershell
tasklist /fi "imagename eq ChuoTsunamiEvacuation.exe"
Get-Process ChuoTsunamiEvacuation | Select-Object ProcessName,Id,CPU,WorkingSet64,PrivateMemorySize64
```

If available:

```powershell
Get-Counter "\Memory\Available MBytes","\Memory\Pages/sec","\PhysicalDisk(_Total)\% Disk Time"
```

## Record For P10-C

- player process working set
- private memory
- available system memory
- disk active time
- pages/sec or hard faults/sec if available
- visible stutter moment
- Player.log warning/error count
