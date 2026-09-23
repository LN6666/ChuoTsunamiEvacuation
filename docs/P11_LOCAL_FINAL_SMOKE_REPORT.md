# P11 Local Final Smoke Report

Status: passed.

Required command:

```powershell
powershell -ExecutionPolicy Bypass -File tools/release/run_p11_local_smoke.ps1
```

Result:

- EXE: `D:\UnityProjects\ChuoTsunamiEvacuation-Releases\ChuoTsunamiEvacuation_v1.0\ChuoTsunamiEvacuation.exe`
- Player.log: passed clean
- Errors: `0`
- Warnings: `0`
- Exceptions: `0`
- Runtime web requests observed: `0`
- Evacuation stamina runtime: `3500`
- Evacuation sprint runtime: `4.59 m/s`
- Tourism sprint runtime: `10.0 m/s`
- Tourism stamina enabled: `false`
- P2-P10 smoke scenarios: passed
- Memory sample count: `192`
- Max private memory bytes: `21879226368`
- Max working set bytes: `7459504128`
