# P11 Final Unity Test Results

Status: passed.

Required commands:

```powershell
powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode EditMode -LaunchMode Gui
powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode PlayMode -LaunchMode Gui
```

Results:

- EditMode: passed, `127/127`
- PlayMode: passed, `43/43`
- EditMode results: `test-results/editmode-results.xml`
- PlayMode results: `test-results/playmode-results.xml`
